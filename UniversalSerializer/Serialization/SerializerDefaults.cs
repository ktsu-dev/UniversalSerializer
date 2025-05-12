// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Serialization;

/// <summary>
/// Default values for serializer options.
/// </summary>
public static class SerializerDefaults
{
	/// <summary>
	/// The default property name for type discriminators.
	/// </summary>
	public const string TypeDiscriminatorPropertyName = "$type";

	/// <summary>
	/// The default format for type discriminators.
	/// </summary>
	public const string TypeDiscriminatorFormat = "Property";
}
