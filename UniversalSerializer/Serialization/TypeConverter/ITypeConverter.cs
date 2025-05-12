// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Serialization.TypeConverter;

/// <summary>
/// Defines a type converter that can convert between objects and string representations.
/// </summary>
public interface ITypeConverter
{
    /// <summary>
    /// Determines whether this converter can convert the specified type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>true if this converter can convert the type; otherwise, false.</returns>
    bool CanConvert(Type type);

    /// <summary>
    /// Converts an object to its string representation.
    /// </summary>
    /// <param name="value">The object to convert.</param>
    /// <returns>The string representation of the object.</returns>
    string ConvertToString(object value);

    /// <summary>
    /// Converts a string to an object of the specified type.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="targetType">The target type.</param>
    /// <returns>The converted object.</returns>
    object ConvertFromString(string value, Type targetType);
}
