// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer;
/// <summary>
/// Defines formats for enum value serialization.
/// </summary>
public enum EnumSerializationFormat
{
	/// <summary>
	/// Serialize enums as their name string.
	/// </summary>
	Name,

	/// <summary>
	/// Serialize enums as their underlying numeric value.
	/// </summary>
	Value,

	/// <summary>
	/// Serialize enums as an object with both name and value properties.
	/// </summary>
	NameAndValue
}
