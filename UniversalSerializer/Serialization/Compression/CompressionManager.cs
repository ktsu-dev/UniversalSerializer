// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Serialization.Compression;

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Manages compression providers and provides unified compression/decompression functionality.
/// </summary>
public class CompressionManager
{
	private readonly ConcurrentDictionary<CompressionType, ICompressionProvider> _providers;

	/// <summary>
	/// Initializes a new instance of the <see cref="CompressionManager"/> class.
	/// </summary>
	public CompressionManager()
	{
		_providers = new ConcurrentDictionary<CompressionType, ICompressionProvider>();

		// Register built-in providers
		RegisterProvider(new GZipCompressionProvider());
		RegisterProvider(new DeflateCompressionProvider());
	}

	/// <summary>
	/// Registers a compression provider.
	/// </summary>
	/// <param name="provider">The compression provider to register.</param>
	public void RegisterProvider(ICompressionProvider provider)
	{
		ArgumentNullException.ThrowIfNull(provider);
		_providers[provider.CompressionType] = provider;
	}

	/// <summary>
	/// Gets a compression provider for the specified type.
	/// </summary>
	/// <param name="compressionType">The compression type.</param>
	/// <returns>The compression provider.</returns>
	/// <exception cref="NotSupportedException">Thrown when the compression type is not supported.</exception>
	public ICompressionProvider GetProvider(CompressionType compressionType)
	{
		if (compressionType == CompressionType.None)
		{
			throw new ArgumentException("Cannot get provider for CompressionType.None", nameof(compressionType));
		}

		return _providers.TryGetValue(compressionType, out var provider)
			? provider
			: throw new NotSupportedException($"Compression type {compressionType} is not supported");
	}

	/// <summary>
	/// Determines whether the specified compression type is supported.
	/// </summary>
	/// <param name="compressionType">The compression type to check.</param>
	/// <returns>true if the compression type is supported; otherwise, false.</returns>
	public bool IsSupported(CompressionType compressionType)
	{
		return compressionType == CompressionType.None || _providers.ContainsKey(compressionType);
	}

	/// <summary>
	/// Compresses data using the specified compression type.
	/// </summary>
	/// <param name="data">The data to compress.</param>
	/// <param name="compressionType">The compression type to use.</param>
	/// <param name="level">The compression level.</param>
	/// <returns>The compressed data.</returns>
	public byte[] Compress(byte[] data, CompressionType compressionType, int level = 6)
	{
		ArgumentNullException.ThrowIfNull(data);

		if (compressionType == CompressionType.None)
		{
			return data;
		}

		var provider = GetProvider(compressionType);
		return provider.Compress(data, level);
	}

	/// <summary>
	/// Decompresses data using the specified compression type.
	/// </summary>
	/// <param name="compressedData">The compressed data to decompress.</param>
	/// <param name="compressionType">The compression type that was used.</param>
	/// <returns>The decompressed data.</returns>
	public byte[] Decompress(byte[] compressedData, CompressionType compressionType)
	{
		ArgumentNullException.ThrowIfNull(compressedData);

		if (compressionType == CompressionType.None)
		{
			return compressedData;
		}

		var provider = GetProvider(compressionType);
		return provider.Decompress(compressedData);
	}

	/// <summary>
	/// Asynchronously compresses data using the specified compression type.
	/// </summary>
	/// <param name="data">The data to compress.</param>
	/// <param name="compressionType">The compression type to use.</param>
	/// <param name="level">The compression level.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A task that represents the asynchronous compression operation.</returns>
	public async Task<byte[]> CompressAsync(byte[] data, CompressionType compressionType, int level = 6, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(data);
		cancellationToken.ThrowIfCancellationRequested();

		if (compressionType == CompressionType.None)
		{
			return data;
		}

		var provider = GetProvider(compressionType);
		return await provider.CompressAsync(data, level, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Asynchronously decompresses data using the specified compression type.
	/// </summary>
	/// <param name="compressedData">The compressed data to decompress.</param>
	/// <param name="compressionType">The compression type that was used.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A task that represents the asynchronous decompression operation.</returns>
	public async Task<byte[]> DecompressAsync(byte[] compressedData, CompressionType compressionType, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(compressedData);
		cancellationToken.ThrowIfCancellationRequested();

		if (compressionType == CompressionType.None)
		{
			return compressedData;
		}

		var provider = GetProvider(compressionType);
		return await provider.DecompressAsync(compressedData, cancellationToken).ConfigureAwait(false);
	}
}
