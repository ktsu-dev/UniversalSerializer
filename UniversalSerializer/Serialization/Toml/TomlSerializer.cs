// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using System.Text;
using Tomlyn;
using Tomlyn.Model;
using ktsu.UniversalSerializer.Serialization.TypeRegistry;

namespace ktsu.UniversalSerializer.Serialization.Toml;

/// <summary>
/// Serializer for TOML format using Tomlyn.
/// </summary>
public class TomlSerializer : SerializerBase
{
    private readonly TypeRegistry.TypeRegistry? _typeRegistry;
    private readonly bool _enableTypeDiscriminator;
    private readonly string _typeDiscriminatorPropertyName;
    private readonly TypeDiscriminatorFormat _discriminatorFormat;

    /// <summary>
    /// Initializes a new instance of the <see cref="TomlSerializer"/> class with default options.
    /// </summary>
    public TomlSerializer() : this(SerializerOptions.Default())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TomlSerializer"/> class with the specified options.
    /// </summary>
    /// <param name="options">The serializer options.</param>
    /// <param name="typeRegistry">Optional registry for polymorphic types.</param>
    public TomlSerializer(SerializerOptions options, TypeRegistry.TypeRegistry? typeRegistry = null) : base(options)
    {
        _typeRegistry = typeRegistry;
        _enableTypeDiscriminator = GetOption(SerializerOptionKeys.TypeRegistry.EnableTypeDiscriminator, false);
        _typeDiscriminatorPropertyName = GetOption(SerializerOptionKeys.TypeRegistry.TypeDiscriminatorPropertyName, "$type");

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
    public override string ContentType => "application/toml";

    /// <inheritdoc/>
    public override string FileExtension => ".toml";

    /// <summary>
    /// Gets all supported file extensions for TOML format.
    /// </summary>
    /// <returns>An array of supported file extensions.</returns>
    public string[] GetSupportedExtensions() => [".toml", ".tml"];

    /// <inheritdoc/>
    public override string Serialize<T>(T obj)
    {
        if (obj == null)
        {
            return string.Empty;
        }

        // Convert the object to a TomlTable using reflection
        var tomlModel = ConvertToTomlModel(obj, typeof(T));
        return Toml.FromModel(tomlModel);
    }

    /// <inheritdoc/>
    public override string Serialize(object obj, Type type)
    {
        if (obj == null)
        {
            return string.Empty;
        }

        // Convert the object to a TomlTable using reflection
        var tomlModel = ConvertToTomlModel(obj, type);
        return Toml.FromModel(tomlModel);
    }

    /// <inheritdoc/>
    public override T Deserialize<T>(string serialized)
    {
        if (string.IsNullOrWhiteSpace(serialized))
        {
            return default!;
        }

        // Parse the TOML string into a model
        var model = Toml.ToModel(serialized);

        // Convert the model to the requested type
        return (T)ConvertFromTomlModel(model, typeof(T))!;
    }

    /// <inheritdoc/>
    public override object Deserialize(string serialized, Type type)
    {
        if (string.IsNullOrWhiteSpace(serialized))
        {
            return null!;
        }

        // Parse the TOML string into a model
        var model = Toml.ToModel(serialized);

        // Convert the model to the requested type
        return ConvertFromTomlModel(model, type)!;
    }

    /// <inheritdoc/>
    public override async Task<string> SerializeAsync<T>(T obj, CancellationToken cancellationToken = default)
    {
        // Tomlyn doesn't have async methods, so we need to use Task.Run for async operation
        return await Task.Run(() => Serialize(obj), cancellationToken);
    }

    /// <inheritdoc/>
    public override async Task<T> DeserializeAsync<T>(string serialized, CancellationToken cancellationToken = default)
    {
        // Tomlyn doesn't have async methods, so we need to use Task.Run for async operation
        return await Task.Run(() => Deserialize<T>(serialized), cancellationToken);
    }

    private TomlTable ConvertToTomlModel(object value, Type type)
    {
        var tomlTable = new TomlTable();

        // If polymorphic serialization is enabled and the type registry is available
        if (_enableTypeDiscriminator && _typeRegistry != null && value != null)
        {
            var actualType = value.GetType();

            // Only add type information if the actual type differs from the expected type
            if (actualType != type && (type.IsInterface || type.IsAbstract))
            {
                // Get the type name from the registry
                var typeName = _typeRegistry.GetTypeName(actualType);

                // Add type discriminator based on format
                switch (_discriminatorFormat)
                {
                    case TypeDiscriminatorFormat.Property:
                        tomlTable[_typeDiscriminatorPropertyName] = typeName;
                        break;

                    case TypeDiscriminatorFormat.Wrapper:
                        var wrapperTable = new TomlTable
                        {
                            ["type"] = typeName,
                            ["value"] = ConvertObjectToTomlValue(value, actualType)
                        };
                        return wrapperTable;

                    case TypeDiscriminatorFormat.TypeProperty:
                        // Not fully implemented for TOML yet
                        break;
                }
            }
        }

        // Add all properties to the TOML table
        if (value != null)
        {
            var properties = value.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (!property.CanRead)
                    continue;

                var propertyValue = property.GetValue(value);
                if (propertyValue == null)
                    continue;

                // Convert the property value to a TOML-compatible value
                var tomlValue = ConvertObjectToTomlValue(propertyValue, property.PropertyType);
                tomlTable[property.Name] = tomlValue;
            }
        }

        return tomlTable;
    }

    private object? ConvertObjectToTomlValue(object? value, Type type)
    {
        if (value == null)
            return null;

        // Handle primitive types directly supported by TOML
        if (type == typeof(string) || type == typeof(bool) ||
            type == typeof(int) || type == typeof(long) ||
            type == typeof(float) || type == typeof(double) ||
            type == typeof(DateTime) || type == typeof(DateTimeOffset))
        {
            return value;
        }

        // Handle arrays
        if (type.IsArray)
        {
            var array = (Array)value;
            var tomlArray = new TomlArray();

            foreach (var item in array)
            {
                tomlArray.Add(ConvertObjectToTomlValue(item, item?.GetType() ?? type.GetElementType()!));
            }

            return tomlArray;
        }

        // Handle dictionaries
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            var tomlTable = new TomlTable();
            var dict = value as System.Collections.IDictionary;

            if (dict != null)
            {
                foreach (System.Collections.DictionaryEntry entry in dict)
                {
                    var keyString = entry.Key?.ToString();
                    if (keyString != null)
                    {
                        tomlTable[keyString] = ConvertObjectToTomlValue(entry.Value, entry.Value?.GetType() ?? typeof(object));
                    }
                }
            }

            return tomlTable;
        }

        // Handle collections
        if (type.IsGenericType && typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
        {
            var tomlArray = new TomlArray();
            var collection = value as System.Collections.IEnumerable;

            if (collection != null)
            {
                foreach (var item in collection)
                {
                    tomlArray.Add(ConvertObjectToTomlValue(item, item?.GetType() ?? typeof(object)));
                }
            }

            return tomlArray;
        }

        // Handle complex objects by recursively creating a new TOML table
        return ConvertToTomlModel(value, type);
    }

    private object? ConvertFromTomlModel(object? tomlValue, Type targetType)
    {
        if (tomlValue == null)
            return null;

        // Handle polymorphic deserialization
        if (_enableTypeDiscriminator && _typeRegistry != null)
        {
            // Check if this is a wrapper format
            if (tomlValue is TomlTable wrapperTable &&
                wrapperTable.ContainsKey("type") &&
                wrapperTable.ContainsKey("value") &&
                _discriminatorFormat == TypeDiscriminatorFormat.Wrapper)
            {
                var typeName = wrapperTable["type"] as string;
                if (typeName != null)
                {
                    var actualType = _typeRegistry.ResolveType(typeName);
                    if (actualType != null)
                    {
                        return ConvertFromTomlModel(wrapperTable["value"], actualType);
                    }
                }
            }

            // Check if this is a property format
            if (tomlValue is TomlTable propertyTable &&
                propertyTable.ContainsKey(_typeDiscriminatorPropertyName) &&
                _discriminatorFormat == TypeDiscriminatorFormat.Property)
            {
                var typeName = propertyTable[_typeDiscriminatorPropertyName] as string;
                if (typeName != null)
                {
                    var actualType = _typeRegistry.ResolveType(typeName);
                    if (actualType != null)
                    {
                        // Remove the type discriminator property before deserialization
                        var cleanTable = new TomlTable();
                        foreach (var kvp in propertyTable)
                        {
                            if (kvp.Key != _typeDiscriminatorPropertyName)
                            {
                                cleanTable[kvp.Key] = kvp.Value;
                            }
                        }

                        return CreateObjectFromTomlTable(cleanTable, actualType);
                    }
                }
            }
        }

        // Handle primitive types directly supported by TOML
        if (targetType == typeof(string) || targetType == typeof(bool) ||
            targetType == typeof(int) || targetType == typeof(long) ||
            targetType == typeof(float) || targetType == typeof(double) ||
            targetType == typeof(DateTime) || targetType == typeof(DateTimeOffset))
        {
            if (tomlValue.GetType() == targetType)
                return tomlValue;

            // Try to convert to the target type
            return Convert.ChangeType(tomlValue, targetType);
        }

        // Handle TomlTable conversion to objects
        if (tomlValue is TomlTable tomlTable)
        {
            return CreateObjectFromTomlTable(tomlTable, targetType);
        }

        // Handle TomlArray conversion to arrays/collections
        if (tomlValue is TomlArray tomlArray)
        {
            return CreateCollectionFromTomlArray(tomlArray, targetType);
        }

        // Default case, try direct conversion
        return Convert.ChangeType(tomlValue, targetType);
    }

    private object CreateObjectFromTomlTable(TomlTable tomlTable, Type targetType)
    {
        // Create a new instance of the target type
        var instance = Activator.CreateInstance(targetType);

        // Set properties from the TOML table
        var properties = targetType.GetProperties();
        foreach (var property in properties)
        {
            if (!property.CanWrite)
                continue;

            if (tomlTable.TryGetValue(property.Name, out var value))
            {
                // Convert the TOML value to the property type
                var convertedValue = ConvertFromTomlModel(value, property.PropertyType);
                if (convertedValue != null)
                {
                    property.SetValue(instance, convertedValue);
                }
            }
        }

        return instance!;
    }

    private object CreateCollectionFromTomlArray(TomlArray tomlArray, Type targetType)
    {
        // Handle arrays
        if (targetType.IsArray)
        {
            var elementType = targetType.GetElementType()!;
            var array = Array.CreateInstance(elementType, tomlArray.Count);

            for (int i = 0; i < tomlArray.Count; i++)
            {
                array.SetValue(ConvertFromTomlModel(tomlArray[i], elementType), i);
            }

            return array;
        }

        // Handle lists
        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
        {
            var elementType = targetType.GetGenericArguments()[0];
            var listType = typeof(List<>).MakeGenericType(elementType);
            var list = Activator.CreateInstance(listType);
            var addMethod = listType.GetMethod("Add");

            foreach (var item in tomlArray)
            {
                var convertedValue = ConvertFromTomlModel(item, elementType);
                addMethod!.Invoke(list, new[] { convertedValue });
            }

            return list!;
        }

        // Default case for other collection types (not fully implemented)
        return tomlArray;
    }
}
