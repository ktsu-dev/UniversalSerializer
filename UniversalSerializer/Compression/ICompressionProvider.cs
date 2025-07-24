// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Compression;

/// <summary>
/// Interface for compression providers that can compress and decompress data.
/// </summary>
public interface ICompressionProvider
{
	/// <summary>
	/// Gets the compression type this provider supports.
	/// </summary>
	public CompressionType CompressionType { get; }

	/// <summary>
	/// Compresses the specified data.
	/// </summary>
	/// <param name="data">The data to compress.</param>
	/// <param name="level">The compression level (1-9, where 9 is maximum compression).</param>
	/// <returns>The compressed data.</returns>
	public byte[] Compress(byte[] data, int level = 6);

	/// <summary>
	/// Decompresses the specified compressed data.
	/// </summary>
	/// <param name="compressedData">The compressed data to decompress.</param>
	/// <returns>The decompressed data.</returns>
	public byte[] Decompress(byte[] compressedData);

	/// <summary>
	/// Asynchronously compresses the specified data.
	/// </summary>
	/// <param name="data">The data to compress.</param>
	/// <param name="level">The compression level (1-9, where 9 is maximum compression).</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A task that represents the asynchronous compression operation.</returns>
	public Task<byte[]> CompressAsync(byte[] data, int level = 6, CancellationToken cancellationToken = default);

	/// <summary>
	/// Asynchronously decompresses the specified compressed data.
	/// </summary>
	/// <param name="compressedData">The compressed data to decompress.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A task that represents the asynchronous decompression operation.</returns>
	public Task<byte[]> DecompressAsync(byte[] compressedData, CancellationToken cancellationToken = default);
}
