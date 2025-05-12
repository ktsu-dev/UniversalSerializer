// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Serialization.TypeConverter;

/// <summary>
/// Adapter that converts a type-specific <see cref="ICustomTypeConverter{T}"/> into a generic <see cref="ITypeConverter"/>.
/// </summary>
/// <typeparam name="T">The type that the custom converter handles.</typeparam>
public class CustomTypeConverterAdapter<T> : ITypeConverter
{
    private readonly ICustomTypeConverter<T> _customConverter;
    private readonly Type _handledType;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomTypeConverterAdapter{T}"/> class.
    /// </summary>
    /// <param name="customConverter">The custom type converter to adapt.</param>
    public CustomTypeConverterAdapter(ICustomTypeConverter<T> customConverter)
    {
        _customConverter = customConverter ?? throw new ArgumentNullException(nameof(customConverter));
        _handledType = typeof(T);
    }

    /// <inheritdoc/>
    public bool CanConvert(Type type)
    {
        return type == _handledType ||
               (_handledType.IsValueType && type == typeof(Nullable<>).MakeGenericType(_handledType));
    }

    /// <inheritdoc/>
    public string ConvertToString(object value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        if (!CanConvert(value.GetType()))
        {
            throw new InvalidOperationException($"This converter cannot convert type {value.GetType().Name}");
        }

        return _customConverter.ConvertToString((T)value);
    }

    /// <inheritdoc/>
    public object ConvertFromString(string value, Type targetType)
    {
        if (!CanConvert(targetType))
        {
            throw new InvalidOperationException($"This converter cannot convert to type {targetType.Name}");
        }

        // Handle nullable types
        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
        }

        return _customConverter.ConvertFromString(value);
    }
}
