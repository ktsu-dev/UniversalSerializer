// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Serialization.Compression;

using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Compression provider for Deflate compression using System.IO.Compression.
/// </summary>
public class DeflateCompressionProvider : ICompressionProvider
{
	/// <inheritdoc/>
	public CompressionType CompressionType => CompressionType.Deflate;

	/// <inheritdoc/>
	public byte[] Compress(byte[] data, int level = 6)
	{
		ArgumentNullException.ThrowIfNull(data);

		var compressionLevel = level switch
		{
			<= 3 => System.IO.Compression.CompressionLevel.Fastest,
			<= 6 => System.IO.Compression.CompressionLevel.Optimal,
			_ => System.IO.Compression.CompressionLevel.SmallestSize
		};

		using var output = new MemoryStream();
		using (var deflate = new DeflateStream(output, compressionLevel))
		{
			deflate.Write(data, 0, data.Length);
		}
		return output.ToArray();
	}

	/// <inheritdoc/>
	public byte[] Decompress(byte[] compressedData)
	{
		ArgumentNullException.ThrowIfNull(compressedData);

		using var input = new MemoryStream(compressedData);
		using var deflate = new DeflateStream(input, CompressionMode.Decompress);
		using var output = new MemoryStream();

		deflate.CopyTo(output);
		return output.ToArray();
	}

	/// <inheritdoc/>
	public async Task<byte[]> CompressAsync(byte[] data, int level = 6, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(data);
		cancellationToken.ThrowIfCancellationRequested();

		var compressionLevel = level switch
		{
			<= 3 => System.IO.Compression.CompressionLevel.Fastest,
			<= 6 => System.IO.Compression.CompressionLevel.Optimal,
			_ => System.IO.Compression.CompressionLevel.SmallestSize
		};

		using var output = new MemoryStream();
		using (var deflate = new DeflateStream(output, compressionLevel))
		{
			await deflate.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);
		}
		return output.ToArray();
	}

	/// <inheritdoc/>
	public async Task<byte[]> DecompressAsync(byte[] compressedData, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(compressedData);
		cancellationToken.ThrowIfCancellationRequested();

		using var input = new MemoryStream(compressedData);
		using var deflate = new DeflateStream(input, CompressionMode.Decompress);
		using var output = new MemoryStream();

		await deflate.CopyToAsync(output, cancellationToken).ConfigureAwait(false);
		return output.ToArray();
	}
}
