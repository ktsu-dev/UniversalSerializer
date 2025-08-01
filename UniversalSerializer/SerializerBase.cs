// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ktsu.UniversalSerializer.Compression;

/// <summary>
/// Provides a base implementation for serializers.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SerializerBase"/> class.
/// </remarks>
/// <param name="options">The serializer options.</param>
public abstract class SerializerBase(SerializerOptions options) : ISerializer
{
	private static readonly CompressionManager s_compressionManager = new();

	/// <summary>
	/// Gets the serializer options.
	/// </summary>
	protected SerializerOptions Options { get; } = options ?? SerializerOptions.Default();

	/// <inheritdoc/>
	public abstract string ContentType { get; }

	/// <inheritdoc/>
	public abstract string FileExtension { get; }

	/// <inheritdoc/>
	public abstract string Serialize<T>(T obj);

	/// <inheritdoc/>
	public abstract string Serialize(object obj, Type type);

	/// <inheritdoc/>
	public abstract T Deserialize<T>(string serialized);

	/// <inheritdoc/>
	public abstract object Deserialize(string serialized, Type type);

	/// <inheritdoc/>
	public virtual byte[] SerializeToBytes<T>(T obj)
	{
		string serialized = Serialize(obj);
		byte[] bytes = System.Text.Encoding.UTF8.GetBytes(serialized);

		// Apply compression if enabled
		return Options.EnableCompression && Options.CompressionType != CompressionType.None
			? s_compressionManager.Compress(bytes, Options.CompressionType, Options.CompressionLevel)
			: bytes;
	}

	/// <inheritdoc/>
	public virtual byte[] SerializeToBytes(object obj, Type type)
	{
		string serialized = Serialize(obj, type);
		byte[] bytes = System.Text.Encoding.UTF8.GetBytes(serialized);

		// Apply compression if enabled
		return Options.EnableCompression && Options.CompressionType != CompressionType.None
			? s_compressionManager.Compress(bytes, Options.CompressionType, Options.CompressionLevel)
			: bytes;
	}

	/// <inheritdoc/>
	public virtual T DeserializeFromBytes<T>(byte[] bytes)
	{
		// Apply decompression if enabled
		if (Options.EnableCompression && Options.CompressionType != CompressionType.None)
		{
			bytes = s_compressionManager.Decompress(bytes, Options.CompressionType);
		}

		string serialized = System.Text.Encoding.UTF8.GetString(bytes);
		return Deserialize<T>(serialized);
	}

	/// <inheritdoc/>
	public virtual object DeserializeFromBytes(byte[] bytes, Type type)
	{
		// Apply decompression if enabled
		if (Options.EnableCompression && Options.CompressionType != CompressionType.None)
		{
			bytes = s_compressionManager.Decompress(bytes, Options.CompressionType);
		}

		string serialized = System.Text.Encoding.UTF8.GetString(bytes);
		return Deserialize(serialized, type);
	}

	/// <inheritdoc/>
	public virtual async Task<string> SerializeAsync<T>(T obj, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();
		return await Task.FromResult(Serialize(obj)).ConfigureAwait(false);
	}

	/// <inheritdoc/>
	public virtual async Task<byte[]> SerializeToBytesAsync<T>(T obj, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();
		return await Task.FromResult(SerializeToBytes(obj)).ConfigureAwait(false);
	}

	/// <inheritdoc/>
	public virtual async Task<T> DeserializeAsync<T>(string serialized, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();
		return await Task.FromResult(Deserialize<T>(serialized)).ConfigureAwait(false);
	}

	/// <inheritdoc/>
	public virtual async Task<T> DeserializeFromBytesAsync<T>(byte[] bytes, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();
		return await Task.FromResult(DeserializeFromBytes<T>(bytes)).ConfigureAwait(false);
	}

	/// <summary>
	/// Gets an option value from the options.
	/// </summary>
	/// <typeparam name="T">The type of the option value.</typeparam>
	/// <param name="key">The option key.</param>
	/// <param name="defaultValue">The default value if the option is not found.</param>
	/// <returns>The option value or the default value.</returns>
	protected T GetOption<T>(string key, T defaultValue) => Options.GetOption(key, defaultValue);

	/// <summary>
	/// Determines whether an option exists.
	/// </summary>
	/// <param name="key">The option key.</param>
	/// <returns>true if the option exists; otherwise, false.</returns>
	protected bool HasOption(string key) => Options.HasOption(key);

	/// <inheritdoc/>
	public virtual void SerializeToStream<T>(T obj, Stream stream)
	{
		ArgumentNullException.ThrowIfNull(stream);
		string serialized = Serialize(obj);
		using StreamWriter writer = new(stream, System.Text.Encoding.UTF8, leaveOpen: true);
		writer.Write(serialized);
		writer.Flush();
	}

	/// <inheritdoc/>
	public virtual T DeserializeFromStream<T>(Stream stream)
	{
		ArgumentNullException.ThrowIfNull(stream);
		using StreamReader reader = new(stream, System.Text.Encoding.UTF8, leaveOpen: true);
		string serialized = reader.ReadToEnd();
		return Deserialize<T>(serialized);
	}

	/// <inheritdoc/>
	public virtual async Task SerializeToStreamAsync<T>(T obj, Stream stream, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(stream);
		cancellationToken.ThrowIfCancellationRequested();
		string serialized = await SerializeAsync(obj, cancellationToken).ConfigureAwait(false);
		using StreamWriter writer = new(stream, System.Text.Encoding.UTF8, leaveOpen: true);
		await writer.WriteAsync(serialized).ConfigureAwait(false);
		await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
	}

	/// <inheritdoc/>
	public virtual async Task<T> DeserializeFromStreamAsync<T>(Stream stream, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(stream);
		cancellationToken.ThrowIfCancellationRequested();
		using StreamReader reader = new(stream, System.Text.Encoding.UTF8, leaveOpen: true);
		string serialized = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
		return await DeserializeAsync<T>(serialized, cancellationToken).ConfigureAwait(false);
	}
}
