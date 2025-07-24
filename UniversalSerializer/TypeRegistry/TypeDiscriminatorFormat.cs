// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.TypeRegistry;
/// <summary>
/// Defines formats for type discriminators in polymorphic serialization.
/// </summary>
public enum TypeDiscriminatorFormat
{
	/// <summary>
	/// Add a property with the type name to the serialized object.
	/// </summary>
	Property,

	/// <summary>
	/// Wrap the object in a container with type and value properties.
	/// </summary>
	Wrapper,

	/// <summary>
	/// Use a designated property of the object for type information.
	/// </summary>
	TypeProperty
}
