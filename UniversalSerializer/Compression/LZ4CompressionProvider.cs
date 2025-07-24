// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Compression;

using System;
using System.Threading;
using System.Threading.Tasks;
using K4os.Compression.LZ4;

/// <summary>
/// Compression provider for LZ4 compression using K4os.Compression.LZ4.
/// </summary>
public class LZ4CompressionProvider : ICompressionProvider
{
	/// <inheritdoc/>
	public CompressionType CompressionType => CompressionType.LZ4;

	/// <inheritdoc/>
	public byte[] Compress(byte[] data, int level = 6)
	{
		ArgumentNullException.ThrowIfNull(data);

		// LZ4 doesn't use traditional compression levels like other algorithms
		// We'll use LZ4Level.L00_FAST for levels 1-3, L04_HC for levels 4-6, and L09_HC for 7-9
		LZ4Level compressionLevel = level switch
		{
			<= 3 => LZ4Level.L00_FAST,
			<= 6 => LZ4Level.L04_HC,
			_ => LZ4Level.L09_HC
		};

		// Estimate maximum compressed size
		int maxCompressedSize = LZ4Codec.MaximumOutputSize(data.Length);
		byte[] compressedBuffer = new byte[maxCompressedSize];

		// Compress the data
		int compressedSize = LZ4Codec.Encode(
			data, 0, data.Length,
			compressedBuffer, 0, compressedBuffer.Length,
			compressionLevel);

		// Return only the actual compressed data
		byte[] result = new byte[compressedSize];
		Array.Copy(compressedBuffer, result, compressedSize);
		return result;
	}

	/// <inheritdoc/>
	public byte[] Decompress(byte[] compressedData)
	{
		ArgumentNullException.ThrowIfNull(compressedData);

		// For LZ4 decompression, we need to know the original size
		// Since we don't store it, we'll use a heuristic approach
		// In a production implementation, you might want to prepend the original size

		// Try different buffer sizes (this is a simplified approach)
		for (int multiplier = 2; multiplier <= 16; multiplier *= 2)
		{
			try
			{
				int estimatedSize = compressedData.Length * multiplier;
				byte[] decompressedBuffer = new byte[estimatedSize];

				int decompressedSize = LZ4Codec.Decode(
					compressedData, 0, compressedData.Length,
					decompressedBuffer, 0, decompressedBuffer.Length);

				if (decompressedSize > 0)
				{
					// Return only the actual decompressed data
					byte[] result = new byte[decompressedSize];
					Array.Copy(decompressedBuffer, result, decompressedSize);
					return result;
				}
			}
			catch (ArgumentException)
			{
				// Buffer size issues or decompression errors, try next buffer size
				continue;
			}
			catch (InvalidOperationException)
			{
				// Decompression errors, try next buffer size
				continue;
			}
		}

		throw new InvalidOperationException("Failed to decompress LZ4 data - unable to determine original size");
	}

	/// <inheritdoc/>
	public async Task<byte[]> CompressAsync(byte[] data, int level = 6, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(data);
		cancellationToken.ThrowIfCancellationRequested();

		// LZ4 compression is very fast, so we can run it in a Task
		return await Task.Run(() => Compress(data, level), cancellationToken).ConfigureAwait(false);
	}

	/// <inheritdoc/>
	public async Task<byte[]> DecompressAsync(byte[] compressedData, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(compressedData);
		cancellationToken.ThrowIfCancellationRequested();

		// LZ4 decompression is very fast, so we can run it in a Task
		return await Task.Run(() => Decompress(compressedData), cancellationToken).ConfigureAwait(false);
	}
}
