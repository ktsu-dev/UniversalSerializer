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

		CompressionLevel compressionLevel = level switch
		{
			<= 3 => CompressionLevel.Fastest,
			<= 6 => CompressionLevel.Optimal,
			_ => CompressionLevel.SmallestSize
		};

		using MemoryStream output = new();
		using (DeflateStream deflate = new(output, compressionLevel))
		{
			deflate.Write(data, 0, data.Length);
		}
		return output.ToArray();
	}

	/// <inheritdoc/>
	public byte[] Decompress(byte[] compressedData)
	{
		ArgumentNullException.ThrowIfNull(compressedData);

		using MemoryStream input = new(compressedData);
		using DeflateStream deflate = new(input, CompressionMode.Decompress);
		using MemoryStream output = new();

		deflate.CopyTo(output);
		return output.ToArray();
	}

	/// <inheritdoc/>
	public async Task<byte[]> CompressAsync(byte[] data, int level = 6, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(data);
		cancellationToken.ThrowIfCancellationRequested();

		CompressionLevel compressionLevel = level switch
		{
			<= 3 => CompressionLevel.Fastest,
			<= 6 => CompressionLevel.Optimal,
			_ => CompressionLevel.SmallestSize
		};

		using MemoryStream output = new();
		using (DeflateStream deflate = new(output, compressionLevel))
		{
			await deflate.WriteAsync(data, cancellationToken).ConfigureAwait(false);
		}
		return output.ToArray();
	}

	/// <inheritdoc/>
	public async Task<byte[]> DecompressAsync(byte[] compressedData, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(compressedData);
		cancellationToken.ThrowIfCancellationRequested();

		using MemoryStream input = new(compressedData);
		using DeflateStream deflate = new(input, CompressionMode.Decompress);
		using MemoryStream output = new();

		await deflate.CopyToAsync(output, cancellationToken).ConfigureAwait(false);
		return output.ToArray();
	}
}
