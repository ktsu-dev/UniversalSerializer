// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Serialization;

/// <summary>
/// Specifies the compression algorithm to use for serialized data.
/// </summary>
public enum CompressionType
{
	/// <summary>
	/// No compression.
	/// </summary>
	None,

	/// <summary>
	/// GZip compression algorithm.
	/// </summary>
	GZip,

	/// <summary>
	/// Deflate compression algorithm.
	/// </summary>
	Deflate,

	/// <summary>
	/// LZ4 compression algorithm (high speed).
	/// </summary>
	LZ4
}
