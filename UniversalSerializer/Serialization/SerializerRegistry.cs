// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using System.Collections.Concurrent;

namespace ktsu.UniversalSerializer.Serialization;

/// <summary>
/// Registry for managing serializers by format.
/// </summary>
public class SerializerRegistry
{
    private readonly ConcurrentDictionary<string, ISerializer> _serializersByFormat = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, ISerializer> _serializersByExtension = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, ISerializer> _serializersByContentType = new(StringComparer.OrdinalIgnoreCase);
    private readonly SerializerFactory _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="SerializerRegistry"/> class.
    /// </summary>
    /// <param name="factory">The serializer factory to use for creating serializers.</param>
    public SerializerRegistry(SerializerFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <summary>
    /// Gets a collection of all registered serializer formats.
    /// </summary>
    public IEnumerable<string> Formats => _serializersByFormat.Keys;

    /// <summary>
    /// Gets a collection of all registered serializers.
    /// </summary>
    public IEnumerable<ISerializer> Serializers => _serializersByFormat.Values;

    /// <summary>
    /// Registers a serializer with the registry.
    /// </summary>
    /// <param name="format">The format name for the serializer.</param>
    /// <param name="serializer">The serializer to register.</param>
    /// <returns>The current registry instance for method chaining.</returns>
    public SerializerRegistry Register(string format, ISerializer serializer)
    {
        if (string.IsNullOrWhiteSpace(format))
        {
            throw new ArgumentException("Format cannot be null or whitespace.", nameof(format));
        }

        if (serializer == null)
        {
            throw new ArgumentNullException(nameof(serializer));
        }

        _serializersByFormat[format] = serializer;

        if (!string.IsNullOrWhiteSpace(serializer.FileExtension))
        {
            var extension = serializer.FileExtension.StartsWith('.')
                ? serializer.FileExtension
                : "." + serializer.FileExtension;

            _serializersByExtension[extension] = serializer;
        }

        if (!string.IsNullOrWhiteSpace(serializer.ContentType))
        {
            _serializersByContentType[serializer.ContentType] = serializer;
        }

        return this;
    }

    /// <summary>
    /// Creates and registers a serializer using the factory.
    /// </summary>
    /// <typeparam name="TSerializer">The type of serializer to create and register.</typeparam>
    /// <param name="format">The format name for the serializer.</param>
    /// <param name="options">Optional serializer options to use when creating the serializer.</param>
    /// <returns>The current registry instance for method chaining.</returns>
    public SerializerRegistry Register<TSerializer>(string format, SerializerOptions? options = null) where TSerializer : ISerializer
    {
        var serializer = options != null
            ? _factory.Create<TSerializer>(options)
            : _factory.Create<TSerializer>();

        return Register(format, serializer);
    }

    /// <summary>
    /// Gets a serializer for the specified format.
    /// </summary>
    /// <param name="format">The format name.</param>
    /// <returns>The serializer for the specified format, or null if not found.</returns>
    public ISerializer? GetSerializer(string format)
    {
        if (string.IsNullOrWhiteSpace(format))
        {
            return null;
        }

        _serializersByFormat.TryGetValue(format, out var serializer);
        return serializer;
    }

    /// <summary>
    /// Gets a typed serializer for the specified format.
    /// </summary>
    /// <typeparam name="TSerializer">The type of serializer to return.</typeparam>
    /// <param name="format">The format name.</param>
    /// <returns>The serializer for the specified format, or null if not found or not of the specified type.</returns>
    public TSerializer? GetSerializer<TSerializer>(string format) where TSerializer : class, ISerializer
    {
        var serializer = GetSerializer(format);
        return serializer as TSerializer;
    }

    /// <summary>
    /// Gets a serializer for the specified file extension.
    /// </summary>
    /// <param name="fileExtension">The file extension (e.g., ".json").</param>
    /// <returns>The serializer for the specified file extension, or null if not found.</returns>
    public ISerializer? GetSerializerByExtension(string fileExtension)
    {
        if (string.IsNullOrWhiteSpace(fileExtension))
        {
            return null;
        }

        var extension = fileExtension.StartsWith('.') ? fileExtension : "." + fileExtension;
        _serializersByExtension.TryGetValue(extension, out var serializer);
        return serializer;
    }

    /// <summary>
    /// Gets a serializer for the specified content type.
    /// </summary>
    /// <param name="contentType">The content type (e.g., "application/json").</param>
    /// <returns>The serializer for the specified content type, or null if not found.</returns>
    public ISerializer? GetSerializerByContentType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return null;
        }

        _serializersByContentType.TryGetValue(contentType, out var serializer);
        return serializer;
    }

    /// <summary>
    /// Attempts to detect the serializer based on the file path.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>The serializer for the detected format, or null if not detected.</returns>
    public ISerializer? DetectSerializer(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return null;
        }

        var extension = Path.GetExtension(filePath);
        return GetSerializerByExtension(extension);
    }

    /// <summary>
    /// Determines whether a serializer is registered for the specified format.
    /// </summary>
    /// <param name="format">The format name.</param>
    /// <returns>true if a serializer is registered for the specified format; otherwise, false.</returns>
    public bool IsFormatSupported(string format)
    {
        return !string.IsNullOrWhiteSpace(format) && _serializersByFormat.ContainsKey(format);
    }

    /// <summary>
    /// Determines whether a serializer is registered for the specified file extension.
    /// </summary>
    /// <param name="fileExtension">The file extension.</param>
    /// <returns>true if a serializer is registered for the specified file extension; otherwise, false.</returns>
    public bool IsExtensionSupported(string fileExtension)
    {
        if (string.IsNullOrWhiteSpace(fileExtension))
        {
            return false;
        }

        var extension = fileExtension.StartsWith('.') ? fileExtension : "." + fileExtension;
        return _serializersByExtension.ContainsKey(extension);
    }

    /// <summary>
    /// Determines whether a serializer is registered for the specified content type.
    /// </summary>
    /// <param name="contentType">The content type.</param>
    /// <returns>true if a serializer is registered for the specified content type; otherwise, false.</returns>
    public bool IsContentTypeSupported(string contentType)
    {
        return !string.IsNullOrWhiteSpace(contentType) && _serializersByContentType.ContainsKey(contentType);
    }
}
