// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Serialization;

/// <summary>
/// Interface for factory creating serializer instances.
/// </summary>
public interface ISerializerFactory
{
	/// <summary>
	/// Creates a new instance of a serializer.
	/// </summary>
	/// <typeparam name="TSerializer">The type of serializer to create.</typeparam>
	/// <returns>A new instance of the specified serializer type.</returns>
	TSerializer Create<TSerializer>() where TSerializer : ISerializer;

	/// <summary>
	/// Creates a new instance of a serializer with the specified options.
	/// </summary>
	/// <typeparam name="TSerializer">The type of serializer to create.</typeparam>
	/// <param name="options">The options to use when creating the serializer.</param>
	/// <returns>A new instance of the specified serializer type.</returns>
	TSerializer Create<TSerializer>(SerializerOptions options) where TSerializer : ISerializer;

	/// <summary>
	/// Creates a new instance of a serializer by type.
	/// </summary>
	/// <param name="serializerType">The type of serializer to create.</param>
	/// <returns>A new instance of the specified serializer type.</returns>
	ISerializer Create(Type serializerType);

	/// <summary>
	/// Creates a new instance of a serializer by type with the specified options.
	/// </summary>
	/// <param name="serializerType">The type of serializer to create.</param>
	/// <param name="options">The options to use when creating the serializer.</param>
	/// <returns>A new instance of the specified serializer type.</returns>
	ISerializer Create(Type serializerType, SerializerOptions options);

	/// <summary>
	/// Gets a copy of the default options used by this factory.
	/// </summary>
	/// <returns>A copy of the default options.</returns>
	SerializerOptions GetDefaultOptions();

	/// <summary>
	/// Gets a serializer of the specified type.
	/// </summary>
	/// <typeparam name="TSerializer">The type of serializer to get.</typeparam>
	/// <returns>An instance of the specified serializer type.</returns>
	TSerializer GetSerializer<TSerializer>() where TSerializer : ISerializer;
}
