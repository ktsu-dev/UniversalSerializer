// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer;
using System;

/// <summary>
/// Adapter that converts a type-specific <see cref="ICustomTypeConverter{T}"/> into a generic <see cref="ITypeConverter"/>.
/// </summary>
/// <typeparam name="T">The type that the custom converter handles.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="CustomTypeConverterAdapter{T}"/> class.
/// </remarks>
/// <param name="customConverter">The custom type converter to adapt.</param>
public class CustomTypeConverterAdapter<T>(ICustomTypeConverter<T> customConverter) : ITypeConverter
{
	private readonly ICustomTypeConverter<T> _customConverter = customConverter ?? throw new ArgumentNullException(nameof(customConverter));
	private readonly Type _handledType = typeof(T);

	/// <inheritdoc/>
	public bool CanConvert(Type type)
	{
		return type == _handledType ||
			   (_handledType.IsValueType && type == typeof(Nullable<>).MakeGenericType(_handledType));
	}

	/// <inheritdoc/>
	public string ConvertToString(object value)
	{
		return value == null
			? string.Empty
			: !CanConvert(value.GetType())
			? throw new InvalidOperationException($"This converter cannot convert type {value.GetType().Name}")
			: _customConverter.ConvertToString((T)value);
	}

	/// <inheritdoc/>
	public object ConvertFromString(string value, Type targetType)
	{
		ArgumentNullException.ThrowIfNull(targetType);
		if (!CanConvert(targetType))
		{
			throw new InvalidOperationException($"This converter cannot convert to type {targetType.Name}");
		}

		// Handle nullable types
		if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
		{
			if (string.IsNullOrEmpty(value))
			{
				return null!;
			}
		}

		return _customConverter.ConvertFromString(value) ?? throw new InvalidOperationException("Conversion returned null");
	}
}
