// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace ktsu.UniversalSerializer.Serialization;

/// <summary>
/// Defines the standard interface for serialization operations across all formats.
/// </summary>
public interface ISerializer
{
	/// <summary>
	/// Gets the content type for this serializer (e.g., "application/json").
	/// </summary>
	public string ContentType { get; }

	/// <summary>
	/// Gets the file extension for this serializer (e.g., ".json").
	/// </summary>
	public string FileExtension { get; }

	/// <summary>
	/// Serializes an object to a string.
	/// </summary>
	/// <typeparam name="T">The type of the object to serialize.</typeparam>
	/// <param name="obj">The object to serialize.</param>
	/// <returns>A string representation of the serialized object.</returns>
	public string Serialize<T>(T obj);

	/// <summary>
	/// Serializes an object to a string.
	/// </summary>
	/// <param name="obj">The object to serialize.</param>
	/// <param name="type">The type of the object.</param>
	/// <returns>A string representation of the serialized object.</returns>
	public string Serialize(object obj, Type type);

	/// <summary>
	/// Deserializes a string to an object.
	/// </summary>
	/// <typeparam name="T">The type to deserialize to.</typeparam>
	/// <param name="serialized">The serialized string.</param>
	/// <returns>The deserialized object.</returns>
	public T Deserialize<T>(string serialized);

	/// <summary>
	/// Deserializes a string to an object of the specified type.
	/// </summary>
	/// <param name="serialized">The serialized string.</param>
	/// <param name="type">The type to deserialize to.</param>
	/// <returns>The deserialized object.</returns>
	public object Deserialize(string serialized, Type type);

	/// <summary>
	/// Serializes an object to a byte array.
	/// </summary>
	/// <typeparam name="T">The type of the object to serialize.</typeparam>
	/// <param name="obj">The object to serialize.</param>
	/// <returns>A byte array representation of the serialized object.</returns>
	public byte[] SerializeToBytes<T>(T obj);

	/// <summary>
	/// Serializes an object to a byte array.
	/// </summary>
	/// <param name="obj">The object to serialize.</param>
	/// <param name="type">The type of the object.</param>
	/// <returns>A byte array representation of the serialized object.</returns>
	public byte[] SerializeToBytes(object obj, Type type);

	/// <summary>
	/// Deserializes a byte array to an object.
	/// </summary>
	/// <typeparam name="T">The type to deserialize to.</typeparam>
	/// <param name="bytes">The serialized bytes.</param>
	/// <returns>The deserialized object.</returns>
	public T DeserializeFromBytes<T>(byte[] bytes);

	/// <summary>
	/// Deserializes a byte array to an object of the specified type.
	/// </summary>
	/// <param name="bytes">The serialized bytes.</param>
	/// <param name="type">The type to deserialize to.</param>
	/// <returns>The deserialized object.</returns>
	public object DeserializeFromBytes(byte[] bytes, Type type);

	/// <summary>
	/// Asynchronously serializes an object to a string.
	/// </summary>
	/// <typeparam name="T">The type of the object to serialize.</typeparam>
	/// <param name="obj">The object to serialize.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public Task<string> SerializeAsync<T>(T obj, CancellationToken cancellationToken = default);

	/// <summary>
	/// Asynchronously serializes an object to a byte array.
	/// </summary>
	/// <typeparam name="T">The type of the object to serialize.</typeparam>
	/// <param name="obj">The object to serialize.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public Task<byte[]> SerializeToBytesAsync<T>(T obj, CancellationToken cancellationToken = default);

	/// <summary>
	/// Asynchronously deserializes a string to an object.
	/// </summary>
	/// <typeparam name="T">The type to deserialize to.</typeparam>
	/// <param name="serialized">The serialized string.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public Task<T> DeserializeAsync<T>(string serialized, CancellationToken cancellationToken = default);

	/// <summary>
	/// Asynchronously deserializes a byte array to an object.
	/// </summary>
	/// <typeparam name="T">The type to deserialize to.</typeparam>
	/// <param name="bytes">The serialized bytes.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public Task<T> DeserializeFromBytesAsync<T>(byte[] bytes, CancellationToken cancellationToken = default);
}
