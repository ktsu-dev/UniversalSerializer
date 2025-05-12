// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Serialization.TypeConverter;

/// <summary>
/// Defines a type-specific converter that can convert between objects of a specific type and string representations.
/// </summary>
/// <typeparam name="T">The type that this converter handles.</typeparam>
public interface ICustomTypeConverter<T>
{
    /// <summary>
    /// Converts an object to its string representation.
    /// </summary>
    /// <param name="value">The object to convert.</param>
    /// <returns>The string representation of the object.</returns>
    string ConvertToString(T value);

    /// <summary>
    /// Converts a string to an object of the specified type.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The converted object.</returns>
    T ConvertFromString(string value);
}
