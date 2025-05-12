// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Serialization;

/// <summary>
/// Interface for resolving serializers by format.
/// </summary>
public interface ISerializerResolver
{
    /// <summary>
    /// Resolves a serializer by format name.
    /// </summary>
    /// <param name="format">The format name.</param>
    /// <returns>The resolved serializer, or null if not found.</returns>
    public ISerializer? ResolveByFormat(string format);

    /// <summary>
    /// Resolves a serializer by file extension.
    /// </summary>
    /// <param name="extension">The file extension (with or without the leading dot).</param>
    /// <returns>The resolved serializer, or null if not found.</returns>
    public ISerializer? ResolveByExtension(string extension);

    /// <summary>
    /// Resolves a serializer by content type.
    /// </summary>
    /// <param name="contentType">The content type.</param>
    /// <returns>The resolved serializer, or null if not found.</returns>
    public ISerializer? ResolveByContentType(string contentType);
}
