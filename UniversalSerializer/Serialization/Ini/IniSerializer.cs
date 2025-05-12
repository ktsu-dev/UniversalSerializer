// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using System.Text;
using IniParser;
using IniParser.Model;
using ktsu.UniversalSerializer.Serialization.TypeRegistry;

namespace ktsu.UniversalSerializer.Serialization.Ini;

/// <summary>
/// Serializer for INI format using INIFileParser.
/// </summary>
public class IniSerializer : SerializerBase
{
    private readonly TypeRegistry.TypeRegistry? _typeRegistry;
    private readonly bool _enableTypeDiscriminator;
    private readonly string _typeDiscriminatorPropertyName;
    private readonly TypeDiscriminatorFormat _discriminatorFormat;
    private readonly string _metadataSectionName;
    private readonly string _dataSectionPrefix;

    /// <summary>
    /// Initializes a new instance of the <see cref="IniSerializer"/> class with default options.
    /// </summary>
    public IniSerializer() : this(SerializerOptions.Default())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IniSerializer"/> class with the specified options.
    /// </summary>
    /// <param name="options">The serializer options.</param>
    /// <param name="typeRegistry">Optional registry for polymorphic types.</param>
    public IniSerializer(SerializerOptions options, TypeRegistry.TypeRegistry? typeRegistry = null) : base(options)
    {
        _typeRegistry = typeRegistry;
        _enableTypeDiscriminator = GetOption(SerializerOptionKeys.TypeRegistry.EnableTypeDiscriminator, false);
        _typeDiscriminatorPropertyName = GetOption(SerializerOptionKeys.TypeRegistry.TypeDiscriminatorPropertyName, "$type");
        _metadataSectionName = GetOption(SerializerOptionKeys.Ini.MetadataSectionName, "Metadata");
        _dataSectionPrefix = GetOption(SerializerOptionKeys.Ini.DataSectionPrefix, "Data");

        var formatValue = GetOption(SerializerOptionKeys.TypeRegistry.TypeDiscriminatorFormat,
            TypeDiscriminatorFormat.Property.ToString());

        // Try to parse the format from string if stored as string
        if (formatValue is string formatString && Enum.TryParse(formatString, out TypeDiscriminatorFormat format))
        {
            _discriminatorFormat = format;
        }
        else if (formatValue is TypeDiscriminatorFormat typeDiscriminatorFormat)
        {
            _discriminatorFormat = typeDiscriminatorFormat;
        }
        else
        {
            _discriminatorFormat = TypeDiscriminatorFormat.Property;
        }
    }

    /// <inheritdoc/>
    public override string ContentType => "application/ini";

    /// <inheritdoc/>
    public override string FileExtension => ".ini";

    /// <summary>
    /// Gets all supported file extensions for INI format.
    /// </summary>
    /// <returns>An array of supported file extensions.</returns>
    public string[] GetSupportedExtensions() => [".ini", ".conf", ".cfg"];

    /// <inheritdoc/>
    public override string Serialize<T>(T obj)
    {
        if (obj == null)
        {
            return string.Empty;
        }

        // Convert the object to INI format
        var iniData = new IniData();
        SerializeObject(obj, typeof(T), iniData);

        var parser = new FileIniDataParser();
        using var stringWriter = new StringWriter();
        parser.WriteData(stringWriter, iniData);
        return stringWriter.ToString();
    }

    /// <inheritdoc/>
    public override string Serialize(object obj, Type type)
    {
        if (obj == null)
        {
            return string.Empty;
        }

        // Convert the object to INI format
        var iniData = new IniData();
        SerializeObject(obj, type, iniData);

        var parser = new FileIniDataParser();
        using var stringWriter = new StringWriter();
        parser.WriteData(stringWriter, iniData);
        return stringWriter.ToString();
    }

    /// <inheritdoc/>
    public override T Deserialize<T>(string serialized)
    {
        if (string.IsNullOrWhiteSpace(serialized))
        {
            return default!;
        }

        // Parse the INI string
        var parser = new FileIniDataParser();
        var iniData = parser.ReadData(new StringReader(serialized));

        // Convert to the requested type
        return (T)DeserializeObject(iniData, typeof(T))!;
    }

    /// <inheritdoc/>
    public override object Deserialize(string serialized, Type type)
    {
        if (string.IsNullOrWhiteSpace(serialized))
        {
            return null!;
        }

        // Parse the INI string
        var parser = new FileIniDataParser();
        var iniData = parser.ReadData(new StringReader(serialized));

        // Convert to the requested type
        return DeserializeObject(iniData, type)!;
    }

    /// <inheritdoc/>
    public override async Task<string> SerializeAsync<T>(T obj, CancellationToken cancellationToken = default)
    {
        // INIFileParser doesn't have async methods, so we need to use Task.Run for async operation
        return await Task.Run(() => Serialize(obj), cancellationToken);
    }

    /// <inheritdoc/>
    public override async Task<T> DeserializeAsync<T>(string serialized, CancellationToken cancellationToken = default)
    {
        // INIFileParser doesn't have async methods, so we need to use Task.Run for async operation
        return await Task.Run(() => Deserialize<T>(serialized), cancellationToken);
    }

    private void SerializeObject(object obj, Type type, IniData iniData)
    {
        // Skip null objects
        if (obj == null)
        {
            return;
        }

        // Handle polymorphic serialization
        if (_enableTypeDiscriminator && _typeRegistry != null)
        {
            var actualType = obj.GetType();

            // Only add type information if the actual type differs from the expected type
            if (actualType != type && (type.IsInterface || type.IsAbstract || type == typeof(object)))
            {
                // Get the type name from the registry
                var typeName = _typeRegistry.GetTypeName(actualType);

                // Add to metadata section
                if (_discriminatorFormat == TypeDiscriminatorFormat.Property)
                {
                    iniData[_metadataSectionName][_typeDiscriminatorPropertyName] = typeName;
                }
            }
        }

        // For complex objects, serialize properties as sections
        var properties = obj.GetType().GetProperties();
        foreach (var property in properties)
        {
            if (!property.CanRead)
                continue;

            var value = property.GetValue(obj);
            if (value == null)
                continue;

            var propertyType = property.PropertyType;

            // Handle primitive types and strings directly in metadata section
            if (IsPrimitiveOrString(propertyType))
            {
                iniData[_metadataSectionName][property.Name] = ConvertToString(value);
            }
            // Handle complex nested objects as separate sections
            else if (!propertyType.IsArray && !IsEnumerableType(propertyType))
            {
                var sectionName = $"{_dataSectionPrefix}.{property.Name}";
                SerializeComplexProperty(value, propertyType, iniData, sectionName);
            }
            // Handle collections as indexed sections
            else
            {
                SerializeCollection(value, property.Name, iniData);
            }
        }
    }

    private void SerializeComplexProperty(object obj, Type type, IniData iniData, string sectionName)
    {
        if (obj == null)
            return;

        var properties = obj.GetType().GetProperties();
        foreach (var property in properties)
        {
            if (!property.CanRead)
                continue;

            var value = property.GetValue(obj);
            if (value == null)
                continue;

            var propertyType = property.PropertyType;

            // Add primitive properties to the section
            if (IsPrimitiveOrString(propertyType))
            {
                iniData[sectionName][property.Name] = ConvertToString(value);
            }
            // For complex properties, create nested sections
            else if (!propertyType.IsArray && !IsEnumerableType(propertyType))
            {
                var nestedSectionName = $"{sectionName}.{property.Name}";
                SerializeComplexProperty(value, propertyType, iniData, nestedSectionName);
            }
            // Handle collections as indexed sections within this section
            else
            {
                SerializeCollection(value, $"{sectionName}.{property.Name}", iniData);
            }
        }
    }

    private void SerializeCollection(object collection, string baseName, IniData iniData)
    {
        if (collection is IEnumerable<object> enumerable)
        {
            int index = 0;
            foreach (var item in enumerable)
            {
                if (item == null)
                    continue;

                var itemType = item.GetType();

                // For primitive items, store with index in parent section
                if (IsPrimitiveOrString(itemType))
                {
                    iniData[baseName][$"Item{index}"] = ConvertToString(item);
                }
                // For complex items, create indexed sections
                else
                {
                    var sectionName = $"{baseName}[{index}]";
                    SerializeComplexProperty(item, itemType, iniData, sectionName);
                }

                index++;
            }

            // Store the count for deserialization
            iniData[baseName]["Count"] = index.ToString();
        }
    }

    private object? DeserializeObject(IniData iniData, Type targetType)
    {
        // Check for polymorphic type information
        Type? actualType = targetType;
        if (_enableTypeDiscriminator && _typeRegistry != null &&
            iniData.Sections.ContainsSection(_metadataSectionName) &&
            iniData[_metadataSectionName].ContainsKey(_typeDiscriminatorPropertyName))
        {
            var typeName = iniData[_metadataSectionName][_typeDiscriminatorPropertyName];
            actualType = _typeRegistry.ResolveType(typeName) ?? targetType;
        }

        // Create instance of the target type
        var instance = Activator.CreateInstance(actualType);
        if (instance == null)
            return null;

        // Populate metadata properties
        if (iniData.Sections.ContainsSection(_metadataSectionName))
        {
            foreach (var key in iniData[_metadataSectionName].Keys)
            {
                // Skip type discriminator property
                if (key == _typeDiscriminatorPropertyName)
                    continue;

                var property = actualType.GetProperty(key);
                if (property != null && property.CanWrite)
                {
                    var value = iniData[_metadataSectionName][key];
                    SetPropertyValue(instance, property, value);
                }
            }
        }

        // Process data sections
        foreach (var section in iniData.Sections)
        {
            // Skip metadata section, we already processed it
            if (section.SectionName == _metadataSectionName)
                continue;

            // Handle property sections (Data.PropertyName)
            if (section.SectionName.StartsWith($"{_dataSectionPrefix}."))
            {
                var parts = section.SectionName.Split('.');
                if (parts.Length < 2)
                    continue;

                var propertyName = parts[1];
                var property = actualType.GetProperty(propertyName);

                // Skip if property doesn't exist or can't be written to
                if (property == null || !property.CanWrite)
                    continue;

                // Check if this is a complex property
                if (!IsPrimitiveOrString(property.PropertyType) &&
                    !property.PropertyType.IsArray &&
                    !IsEnumerableType(property.PropertyType))
                {
                    var nestedObject = DeserializeComplexProperty(iniData, section.SectionName, property.PropertyType);
                    property.SetValue(instance, nestedObject);
                }
            }
            // Handle collection sections
            else if (iniData[section.SectionName].ContainsKey("Count"))
            {
                var propertyName = section.SectionName;
                var property = actualType.GetProperty(propertyName);

                // Skip if property doesn't exist or can't be written to
                if (property == null || !property.CanWrite)
                    continue;

                var collection = DeserializeCollection(iniData, section.SectionName, property.PropertyType);
                property.SetValue(instance, collection);
            }
        }

        return instance;
    }

    private object? DeserializeComplexProperty(IniData iniData, string sectionName, Type targetType)
    {
        var instance = Activator.CreateInstance(targetType);
        if (instance == null)
            return null;

        // Load direct properties from this section
        foreach (var key in iniData[sectionName].Keys)
        {
            var property = targetType.GetProperty(key);
            if (property != null && property.CanWrite)
            {
                var value = iniData[sectionName][key];
                SetPropertyValue(instance, property, value);
            }
        }

        // Check for nested sections
        var sectionPrefix = sectionName + ".";
        var nestedSections = iniData.Sections
            .Where(s => s.SectionName.StartsWith(sectionPrefix))
            .ToList();

        foreach (var nestedSection in nestedSections)
        {
            // Extract property name from the section name
            var propertyName = nestedSection.SectionName.Substring(sectionPrefix.Length).Split('.')[0];

            // If it contains array index notation, handle separately
            if (propertyName.Contains('['))
                continue;

            var property = targetType.GetProperty(propertyName);
            if (property == null || !property.CanWrite)
                continue;

            // Handle nested complex property
            if (!IsPrimitiveOrString(property.PropertyType) &&
                !property.PropertyType.IsArray &&
                !IsEnumerableType(property.PropertyType))
            {
                var nestedObject = DeserializeComplexProperty(iniData, nestedSection.SectionName, property.PropertyType);
                property.SetValue(instance, nestedObject);
            }
            // Handle collection
            else if (iniData[nestedSection.SectionName].ContainsKey("Count"))
            {
                var collection = DeserializeCollection(iniData, nestedSection.SectionName, property.PropertyType);
                property.SetValue(instance, collection);
            }
        }

        return instance;
    }

    private object? DeserializeCollection(IniData iniData, string sectionName, Type collectionType)
    {
        // Get count of items
        if (!int.TryParse(iniData[sectionName]["Count"], out int count))
            return null;

        // Handle primitive types collection
        if (iniData.Sections[sectionName].Keys.Any(k => k.StartsWith("Item")))
        {
            // Get element type for the collection
            Type elementType;
            if (collectionType.IsArray)
            {
                elementType = collectionType.GetElementType()!;
            }
            else if (collectionType.IsGenericType)
            {
                elementType = collectionType.GetGenericArguments()[0];
            }
            else
            {
                return null;
            }

            // Create list of elements
            var listType = typeof(List<>).MakeGenericType(elementType);
            var list = Activator.CreateInstance(listType);
            var addMethod = listType.GetMethod("Add");

            // Add items to the list
            for (int i = 0; i < count; i++)
            {
                var key = $"Item{i}";
                if (iniData[sectionName].ContainsKey(key))
                {
                    var value = iniData[sectionName][key];
                    var convertedValue = ConvertFromString(value, elementType);
                    addMethod?.Invoke(list, new[] { convertedValue });
                }
            }

            // If array is needed, convert list to array
            if (collectionType.IsArray)
            {
                var toArrayMethod = listType.GetMethod("ToArray");
                return toArrayMethod?.Invoke(list, null);
            }

            return list;
        }
        // Handle complex type collection
        else
        {
            // Get element type for the collection
            Type elementType;
            if (collectionType.IsArray)
            {
                elementType = collectionType.GetElementType()!;
            }
            else if (collectionType.IsGenericType)
            {
                elementType = collectionType.GetGenericArguments()[0];
            }
            else
            {
                return null;
            }

            // Create list of elements
            var listType = typeof(List<>).MakeGenericType(elementType);
            var list = Activator.CreateInstance(listType);
            var addMethod = listType.GetMethod("Add");

            // Add items to the list by processing indexed sections
            for (int i = 0; i < count; i++)
            {
                var itemSectionName = $"{sectionName}[{i}]";
                if (iniData.Sections.ContainsSection(itemSectionName))
                {
                    var itemObject = DeserializeComplexProperty(iniData, itemSectionName, elementType);
                    addMethod?.Invoke(list, new[] { itemObject });
                }
            }

            // If array is needed, convert list to array
            if (collectionType.IsArray)
            {
                var toArrayMethod = listType.GetMethod("ToArray");
                return toArrayMethod?.Invoke(list, null);
            }

            return list;
        }
    }

    private static void SetPropertyValue(object instance, System.Reflection.PropertyInfo property, string value)
    {
        if (string.IsNullOrEmpty(value))
            return;

        try
        {
            var convertedValue = ConvertFromString(value, property.PropertyType);
            property.SetValue(instance, convertedValue);
        }
        catch (Exception)
        {
            // Skip properties that can't be converted
        }
    }

    private static bool IsPrimitiveOrString(Type type)
    {
        return type.IsPrimitive || type.IsEnum || type == typeof(string) ||
               type == typeof(decimal) || type == typeof(DateTime) ||
               type == typeof(TimeSpan) || type == typeof(Guid) ||
               (Nullable.GetUnderlyingType(type)?.IsPrimitive ?? false);
    }

    private static bool IsEnumerableType(Type type)
    {
        return type != typeof(string) &&
               typeof(System.Collections.IEnumerable).IsAssignableFrom(type);
    }

    private static string ConvertToString(object value)
    {
        if (value == null)
            return string.Empty;

        if (value is DateTime dateTime)
            return dateTime.ToString("o");

        if (value is DateTimeOffset dateTimeOffset)
            return dateTimeOffset.ToString("o");

        if (value is TimeSpan timeSpan)
            return timeSpan.ToString("c");

        if (value is Guid guid)
            return guid.ToString("D");

        if (value is Enum enumValue)
            return enumValue.ToString();

        return value.ToString() ?? string.Empty;
    }

    private static object? ConvertFromString(string value, Type targetType)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(targetType);
        if (underlyingType != null)
        {
            targetType = underlyingType;
        }

        if (targetType == typeof(string))
            return value;

        if (targetType == typeof(bool))
            return bool.TryParse(value, out var boolResult) ? boolResult : default;

        if (targetType == typeof(int))
            return int.TryParse(value, out var intResult) ? intResult : default;

        if (targetType == typeof(long))
            return long.TryParse(value, out var longResult) ? longResult : default;

        if (targetType == typeof(float))
            return float.TryParse(value, out var floatResult) ? floatResult : default;

        if (targetType == typeof(double))
            return double.TryParse(value, out var doubleResult) ? doubleResult : default;

        if (targetType == typeof(decimal))
            return decimal.TryParse(value, out var decimalResult) ? decimalResult : default;

        if (targetType == typeof(DateTime))
            return DateTime.TryParse(value, out var dateTimeResult) ? dateTimeResult : default;

        if (targetType == typeof(DateTimeOffset))
            return DateTimeOffset.TryParse(value, out var dateTimeOffsetResult) ? dateTimeOffsetResult : default;

        if (targetType == typeof(TimeSpan))
            return TimeSpan.TryParse(value, out var timeSpanResult) ? timeSpanResult : default;

        if (targetType == typeof(Guid))
            return Guid.TryParse(value, out var guidResult) ? guidResult : default;

        if (targetType.IsEnum)
            return Enum.TryParse(targetType, value, true, out var enumResult) ? enumResult : default;

        return null;
    }
}
