// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer;
using System;

/// <summary>
/// Resolver for finding serializers by format, extension, or content type.
/// </summary>
public class SerializerResolver(SerializerRegistry registry) : ISerializerResolver
{
	private readonly SerializerRegistry _registry = registry ?? throw new ArgumentNullException(nameof(registry));

	/// <inheritdoc/>
	public ISerializer? ResolveByFormat(string format)
	{
		return string.IsNullOrWhiteSpace(format)
			? throw new ArgumentException("Format cannot be null or whitespace.", nameof(format))
			: _registry.GetSerializer(format);
	}

	/// <inheritdoc/>
	public ISerializer? ResolveByExtension(string extension)
	{
		return string.IsNullOrWhiteSpace(extension)
			? throw new ArgumentException("Extension cannot be null or whitespace.", nameof(extension))
			: _registry.GetSerializerByExtension(extension);
	}

	/// <inheritdoc/>
	public ISerializer? ResolveByContentType(string contentType)
	{
		return string.IsNullOrWhiteSpace(contentType)
			? throw new ArgumentException("Content type cannot be null or whitespace.", nameof(contentType))
			: _registry.GetSerializerByContentType(contentType);
	}
}
