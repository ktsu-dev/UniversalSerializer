// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Serialization.TypeConverter;

/// <summary>
/// Registry for type converters that manages type conversion operations.
/// </summary>
public class TypeConverterRegistry
{
    private readonly List<ITypeConverter> _converters = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeConverterRegistry"/> class.
    /// </summary>
    public TypeConverterRegistry()
    {
        // Register default converters
        _converters.Add(new StringConvertibleTypeConverter());
    }

    /// <summary>
    /// Registers a converter with the registry.
    /// </summary>
    /// <param name="converter">The converter to register.</param>
    public void RegisterConverter(ITypeConverter converter)
    {
        if (converter == null)
        {
            throw new ArgumentNullException(nameof(converter));
        }

        _converters.Add(converter);
    }

    /// <summary>
    /// Gets a converter that can handle the specified type.
    /// </summary>
    /// <param name="type">The type to get a converter for.</param>
    /// <returns>The first converter that can handle the type, or null if none is found.</returns>
    public ITypeConverter? GetConverter(Type type)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        return _converters.FirstOrDefault(c => c.CanConvert(type));
    }

    /// <summary>
    /// Determines whether a converter exists for the specified type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>true if a converter exists; otherwise, false.</returns>
    public bool HasConverter(Type type)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        return _converters.Any(c => c.CanConvert(type));
    }

    /// <summary>
    /// Converts an object to its string representation.
    /// </summary>
    /// <param name="value">The object to convert.</param>
    /// <returns>The string representation of the object.</returns>
    /// <exception cref="InvalidOperationException">No converter is found for the object's type.</exception>
    public string ConvertToString(object value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        var type = value.GetType();
        var converter = GetConverter(type);

        if (converter == null)
        {
            throw new InvalidOperationException($"No converter found for type {type.Name}");
        }

        return converter.ConvertToString(value);
    }

    /// <summary>
    /// Converts a string to an object of the specified type.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="targetType">The target type.</param>
    /// <returns>The converted object.</returns>
    /// <exception cref="InvalidOperationException">No converter is found for the specified type.</exception>
    public object ConvertFromString(string value, Type targetType)
    {
        if (targetType == null)
        {
            throw new ArgumentNullException(nameof(targetType));
        }

        var converter = GetConverter(targetType);

        if (converter == null)
        {
            throw new InvalidOperationException($"No converter found for type {targetType.Name}");
        }

        return converter.ConvertFromString(value, targetType);
    }

    /// <summary>
    /// Converts a string to an object of the specified type.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="value">The string to convert.</param>
    /// <returns>The converted object.</returns>
    /// <exception cref="InvalidOperationException">No converter is found for the specified type.</exception>
    public T ConvertFromString<T>(string value)
    {
        var targetType = typeof(T);
        var result = ConvertFromString(value, targetType);

        return (T)result;
    }
}
