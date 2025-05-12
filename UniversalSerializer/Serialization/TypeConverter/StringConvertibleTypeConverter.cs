// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Serialization.TypeConverter;
using System.Reflection;

/// <summary>
/// Type converter that handles types with standard string conversion patterns like ToString/Parse, ToString/FromString, or ToString/Constructor.
/// </summary>
public class StringConvertibleTypeConverter : ITypeConverter
{
	/// <summary>
	/// Determines whether this converter can convert the specified type.
	/// </summary>
	/// <param name="type">The type to check.</param>
	/// <returns>true if this converter can convert the type; otherwise, false.</returns>
	public bool CanConvert(Type type)
	{
		// Check if type has a ToString method override and any of the supported deserialization methods
		return HasToStringOverride(type) &&
			   (HasParseMethod(type) || HasFromStringMethod(type) || HasStringConstructor(type));
	}

	/// <summary>
	/// Converts an object to its string representation.
	/// </summary>
	/// <param name="value">The object to convert.</param>
	/// <returns>The string representation of the object.</returns>
	public string ConvertToString(object value) => value.ToString() ?? string.Empty;

	/// <summary>
	/// Converts a string to an object of the specified type.
	/// </summary>
	/// <param name="value">The string to convert.</param>
	/// <param name="targetType">The target type.</param>
	/// <returns>The converted object.</returns>
	public object ConvertFromString(string value, Type targetType)
	{
		// Try Parse method first (instance.ToString / static.Parse pattern)
		if (HasParseMethod(targetType))
		{
			var parseMethod = targetType.GetMethod("Parse",
				BindingFlags.Public | BindingFlags.Static,
				null,
				[typeof(string)],
				null);

			return parseMethod?.Invoke(null, [value]) ??
				   throw new InvalidOperationException($"Parse method not found for type {targetType.Name}");
		}

		// Try FromString method next (instance.ToString / static.FromString pattern)
		if (HasFromStringMethod(targetType))
		{
			var fromStringMethod = targetType.GetMethod("FromString",
				BindingFlags.Public | BindingFlags.Static,
				null,
				[typeof(string)],
				null);

			return fromStringMethod?.Invoke(null, [value]) ??
				   throw new InvalidOperationException($"FromString method not found for type {targetType.Name}");
		}

		// Try string constructor as last resort
		if (HasStringConstructor(targetType))
		{
			var constructor = targetType.GetConstructor([typeof(string)]);
			return constructor?.Invoke([value]) ??
				   throw new InvalidOperationException($"String constructor not found for type {targetType.Name}");
		}

		throw new InvalidOperationException($"Cannot convert string to {targetType.Name}");
	}

	// Helper methods to check for conversion methods
	private static bool HasToStringOverride(Type type)
	{
		// Check if the type overrides ToString
		var toStringMethod = type.GetMethod("ToString",
			BindingFlags.Public | BindingFlags.Instance,
			null,
			Type.EmptyTypes,
			null);

		return toStringMethod != null &&
			   toStringMethod.DeclaringType != typeof(object) &&
			   toStringMethod.DeclaringType != typeof(ValueType);
	}

	private static bool HasParseMethod(Type type)
	{
		// Check for static Parse(string) method
		var parseMethod = type.GetMethod("Parse",
			BindingFlags.Public | BindingFlags.Static,
			null,
			[typeof(string)],
			null);

		return parseMethod != null && parseMethod.ReturnType == type;
	}

	private static bool HasFromStringMethod(Type type)
	{
		// Check for static FromString(string) method
		var fromStringMethod = type.GetMethod("FromString",
			BindingFlags.Public | BindingFlags.Static,
			null,
			[typeof(string)],
			null);

		return fromStringMethod != null && fromStringMethod.ReturnType == type;
	}

	private static bool HasStringConstructor(Type type)
	{
		// Check for constructor that takes a single string parameter
		var constructor = type.GetConstructor([typeof(string)]);
		return constructor != null;
	}
}
