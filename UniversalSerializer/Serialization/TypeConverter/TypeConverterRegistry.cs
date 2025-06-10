// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ktsu.UniversalSerializer.Serialization.TypeConverter;
using System.Collections.Concurrent;

/// <summary>
/// Registry for type converters that manages type conversion operations.
/// </summary>
public class TypeConverterRegistry
{
	private readonly List<ITypeConverter> _converters = [];
	private readonly ConcurrentDictionary<Type, ITypeConverter> _customConvertersByType = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="TypeConverterRegistry"/> class.
	/// </summary>
	public TypeConverterRegistry() =>
		// Register default converters
		_converters.Add(new StringConvertibleTypeConverter());

	/// <summary>
	/// Registers a converter with the registry.
	/// </summary>
	/// <param name="converter">The converter to register.</param>
	public void RegisterConverter(ITypeConverter converter)
	{
		ArgumentNullException.ThrowIfNull(converter);

		_converters.Add(converter);
	}

	/// <summary>
	/// Registers a type-specific custom converter with the registry.
	/// </summary>
	/// <typeparam name="T">The type that the converter handles.</typeparam>
	/// <param name="converter">The custom converter to register.</param>
	public void RegisterConverter<T>(ICustomTypeConverter<T> converter)
	{
		ArgumentNullException.ThrowIfNull(converter);

		var adapter = new CustomTypeConverterAdapter<T>(converter);
		_customConvertersByType[typeof(T)] = adapter;
	}

	/// <summary>
	/// Gets a converter that can handle the specified type.
	/// </summary>
	/// <param name="type">The type to get a converter for.</param>
	/// <returns>The first converter that can handle the type, or null if none is found.</returns>
	public ITypeConverter? GetConverter(Type type)
	{
		ArgumentNullException.ThrowIfNull(type);

		// First check for a custom converter specifically registered for this type
		if (_customConvertersByType.TryGetValue(type, out var customConverter))
		{
			return customConverter;
		}

		// Check if we have a custom converter for the underlying type of a nullable type
		if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
		{
			var underlyingType = Nullable.GetUnderlyingType(type);
			if (underlyingType != null && _customConvertersByType.TryGetValue(underlyingType, out var nullableConverter))
			{
				return nullableConverter;
			}
		}

		// Fall back to general converters
		return _converters.FirstOrDefault(c => c.CanConvert(type));
	}

	/// <summary>
	/// Determines whether a converter exists for the specified type.
	/// </summary>
	/// <param name="type">The type to check.</param>
	/// <returns>true if a converter exists; otherwise, false.</returns>
	public bool HasConverter(Type type)
	{
		ArgumentNullException.ThrowIfNull(type);

		// Check for a custom converter specifically registered for this type
		if (_customConvertersByType.ContainsKey(type))
		{
			return true;
		}

		// Check for a custom converter for the underlying type of a nullable type
		if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
		{
			var underlyingType = Nullable.GetUnderlyingType(type);
			if (underlyingType != null && _customConvertersByType.ContainsKey(underlyingType))
			{
				return true;
			}
		}

		// Check general converters
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

		return converter == null
			? throw new InvalidOperationException($"No converter found for type {type.Name}")
			: converter.ConvertToString(value);
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
		ArgumentNullException.ThrowIfNull(targetType);

		var converter = GetConverter(targetType);

		return converter == null
			? throw new InvalidOperationException($"No converter found for type {targetType.Name}")
			: converter.ConvertFromString(value, targetType);
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

	/// <summary>
	/// Removes a custom converter for the specified type.
	/// </summary>
	/// <typeparam name="T">The type whose converter should be removed.</typeparam>
	/// <returns>true if the converter was removed; otherwise, false.</returns>
	public bool RemoveConverter<T>() => _customConvertersByType.TryRemove(typeof(T), out _);

	/// <summary>
	/// Removes a converter for the specified type.
	/// </summary>
	/// <param name="type">The type whose converter should be removed.</param>
	/// <returns>true if the converter was removed; otherwise, false.</returns>
	public bool RemoveConverter(Type type) => type == null ? throw new ArgumentNullException(nameof(type)) : _customConvertersByType.TryRemove(type, out _);

	/// <summary>
	/// Clears all custom converters registered with this registry.
	/// </summary>
	public void ClearCustomConverters() => _customConvertersByType.Clear();
}
