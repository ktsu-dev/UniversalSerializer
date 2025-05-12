// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using System.IO;
using System.Text;

namespace ktsu.UniversalSerializer.Serialization;

/// <summary>
/// Provides extension methods for <see cref="ISerializer"/>.
/// </summary>
public static class SerializerExtensions
{
    /// <summary>
    /// Serializes an object to a file.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="serializer">The serializer.</param>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="filePath">The file path to save to.</param>
    public static void SerializeToFile<T>(this ISerializer serializer, T obj, string filePath)
    {
        var serialized = serializer.Serialize(obj);
        File.WriteAllText(filePath, serialized);
    }

    /// <summary>
    /// Serializes an object to a file asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="serializer">The serializer.</param>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="filePath">The file path to save to.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task SerializeToFileAsync<T>(this ISerializer serializer, T obj, string filePath, CancellationToken cancellationToken = default)
    {
        var serialized = await serializer.SerializeAsync(obj, cancellationToken);
        await File.WriteAllTextAsync(filePath, serialized, cancellationToken);
    }

    /// <summary>
    /// Serializes an object to a file using binary format.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="serializer">The serializer.</param>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="filePath">The file path to save to.</param>
    public static void SerializeToBinaryFile<T>(this ISerializer serializer, T obj, string filePath)
    {
        var bytes = serializer.SerializeToBytes(obj);
        File.WriteAllBytes(filePath, bytes);
    }

    /// <summary>
    /// Serializes an object to a file using binary format asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="serializer">The serializer.</param>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="filePath">The file path to save to.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task SerializeToBinaryFileAsync<T>(this ISerializer serializer, T obj, string filePath, CancellationToken cancellationToken = default)
    {
        var bytes = await serializer.SerializeToBytesAsync(obj, cancellationToken);
        await File.WriteAllBytesAsync(filePath, bytes, cancellationToken);
    }

    /// <summary>
    /// Deserializes an object from a file.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="serializer">The serializer.</param>
    /// <param name="filePath">The file path to read from.</param>
    /// <returns>The deserialized object.</returns>
    public static T DeserializeFromFile<T>(this ISerializer serializer, string filePath)
    {
        var serialized = File.ReadAllText(filePath);
        return serializer.Deserialize<T>(serialized);
    }

    /// <summary>
    /// Deserializes an object from a file asynchronously.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="serializer">The serializer.</param>
    /// <param name="filePath">The file path to read from.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task<T> DeserializeFromFileAsync<T>(this ISerializer serializer, string filePath, CancellationToken cancellationToken = default)
    {
        var serialized = await File.ReadAllTextAsync(filePath, cancellationToken);
        return await serializer.DeserializeAsync<T>(serialized, cancellationToken);
    }

    /// <summary>
    /// Deserializes an object from a binary file.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="serializer">The serializer.</param>
    /// <param name="filePath">The file path to read from.</param>
    /// <returns>The deserialized object.</returns>
    public static T DeserializeFromBinaryFile<T>(this ISerializer serializer, string filePath)
    {
        var bytes = File.ReadAllBytes(filePath);
        return serializer.DeserializeFromBytes<T>(bytes);
    }

    /// <summary>
    /// Deserializes an object from a binary file asynchronously.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="serializer">The serializer.</param>
    /// <param name="filePath">The file path to read from.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task<T> DeserializeFromBinaryFileAsync<T>(this ISerializer serializer, string filePath, CancellationToken cancellationToken = default)
    {
        var bytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
        return await serializer.DeserializeFromBytesAsync<T>(bytes, cancellationToken);
    }

    /// <summary>
    /// Serializes an object to a memory stream.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="serializer">The serializer.</param>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>A memory stream containing the serialized object.</returns>
    public static MemoryStream SerializeToStream<T>(this ISerializer serializer, T obj)
    {
        var serialized = serializer.Serialize(obj);
        var bytes = Encoding.UTF8.GetBytes(serialized);
        return new MemoryStream(bytes);
    }

    /// <summary>
    /// Deserializes an object from a stream.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="serializer">The serializer.</param>
    /// <param name="stream">The stream to read from.</param>
    /// <returns>The deserialized object.</returns>
    public static T DeserializeFromStream<T>(this ISerializer serializer, Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, true, -1, true);
        var serialized = reader.ReadToEnd();
        return serializer.Deserialize<T>(serialized);
    }

    /// <summary>
    /// Creates a deep clone of an object using serialization.
    /// </summary>
    /// <typeparam name="T">The type of the object to clone.</typeparam>
    /// <param name="serializer">The serializer.</param>
    /// <param name="obj">The object to clone.</param>
    /// <returns>A deep clone of the object.</returns>
    public static T Clone<T>(this ISerializer serializer, T obj)
    {
        var serialized = serializer.Serialize(obj);
        return serializer.Deserialize<T>(serialized);
    }
}
