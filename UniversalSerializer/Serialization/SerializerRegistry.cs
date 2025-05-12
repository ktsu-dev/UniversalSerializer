// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using System.Collections.Concurrent;
using ktsu.UniversalSerializer.Serialization.Yaml;
using ktsu.UniversalSerializer.Serialization.Toml;
using ktsu.UniversalSerializer.Serialization.MessagePack;
using ktsu.UniversalSerializer.Serialization.Protobuf;
using ktsu.UniversalSerializer.Serialization.FlatBuffers;

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
    /// Initializes a new instance of the <see cref="SerializerRegistry"/> class with the specified factory.
    /// </summary>
    /// <param name="factory">The serializer factory to use.</param>
    public SerializerRegistry(SerializerFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <summary>
    /// Registers built-in serializers.
    /// </summary>
    /// <param name="options">Optional serializer options.</param>
    public void RegisterBuiltIn(SerializerOptions? options = null)
    {
        var serializerOptions = options ?? SerializerOptions.Default();

        // Register JSON serializer
        var jsonSerializer = _factory.Create<Json.JsonSerializer>(serializerOptions);
        Register("json", jsonSerializer);
        RegisterFileExtensions("json", ".json");
        RegisterContentTypes("json", "application/json", "text/json");

        // Register XML serializer
        var xmlSerializer = _factory.Create<Xml.XmlSerializer>(serializerOptions);
        Register("xml", xmlSerializer);
        RegisterFileExtensions("xml", ".xml");
        RegisterContentTypes("xml", "application/xml", "text/xml");

        // Register YAML serializer
        var yamlSerializer = _factory.Create<YamlSerializer>(serializerOptions);
        Register("yaml", yamlSerializer);
        RegisterFileExtensions("yaml", ".yaml", ".yml");
        RegisterContentTypes("yaml", "application/x-yaml", "text/yaml");

        // Register TOML serializer
        var tomlSerializer = _factory.Create<TomlSerializer>(serializerOptions);
        Register("toml", tomlSerializer);
        RegisterFileExtensions("toml", ".toml", ".tml");
        RegisterContentTypes("toml", "application/toml");

        // Register MessagePack serializer
        var messagePackSerializer = _factory.Create<MessagePackSerializer>(serializerOptions);
        Register("messagepack", messagePackSerializer);
        RegisterFileExtensions("messagepack", ".msgpack", ".mp");
        RegisterContentTypes("messagepack", "application/x-msgpack");

        // Register Protocol Buffers serializer
        var protobufSerializer = _factory.Create<ProtobufSerializer>(serializerOptions);
        Register("protobuf", protobufSerializer);
        RegisterFileExtensions("protobuf", ".proto", ".pb", ".bin");
        RegisterContentTypes("protobuf", "application/protobuf", "application/x-protobuf");

        // Register FlatBuffers serializer
        var flatBuffersSerializer = _factory.Create<FlatBuffersSerializer>(serializerOptions);
        Register("flatbuffers", flatBuffersSerializer);
        RegisterFileExtensions("flatbuffers", ".fbs", ".fb", ".flatbuffer");
        RegisterContentTypes("flatbuffers", "application/flatbuffers", "application/x-flatbuffers");
    }

    /// <summary>
    /// Registers a serializer with the registry.
    /// </summary>
    /// <param name="format">The format name.</param>
    /// <param name="serializer">The serializer to register.</param>
    public void Register(string format, ISerializer serializer)
    {
        if (string.IsNullOrWhiteSpace(format))
        {
            throw new ArgumentException("Format cannot be null or whitespace.", nameof(format));
        }

        _serializersByFormat[format] = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    /// <summary>
    /// Registers file extensions for a serializer format.
    /// </summary>
    /// <param name="format">The format name.</param>
    /// <param name="extensions">The file extensions to register.</param>
    public void RegisterFileExtensions(string format, params string[] extensions)
    {
        if (string.IsNullOrWhiteSpace(format))
        {
            throw new ArgumentException("Format cannot be null or whitespace.", nameof(format));
        }

        if (extensions == null || extensions.Length == 0)
        {
            throw new ArgumentException("At least one extension must be provided.", nameof(extensions));
        }

        if (!_serializersByFormat.TryGetValue(format, out var serializer))
        {
            throw new ArgumentException($"Format '{format}' is not registered.", nameof(format));
        }

        foreach (var extension in extensions)
        {
            var ext = extension.StartsWith(".") ? extension : $".{extension}";
            _serializersByExtension[ext] = serializer;
        }
    }

    /// <summary>
    /// Registers content types for a serializer format.
    /// </summary>
    /// <param name="format">The format name.</param>
    /// <param name="contentTypes">The content types to register.</param>
    public void RegisterContentTypes(string format, params string[] contentTypes)
    {
        if (string.IsNullOrWhiteSpace(format))
        {
            throw new ArgumentException("Format cannot be null or whitespace.", nameof(format));
        }

        if (contentTypes == null || contentTypes.Length == 0)
        {
            throw new ArgumentException("At least one content type must be provided.", nameof(contentTypes));
        }

        if (!_serializersByFormat.TryGetValue(format, out var serializer))
        {
            throw new ArgumentException($"Format '{format}' is not registered.", nameof(format));
        }

        foreach (var contentType in contentTypes)
        {
            _serializersByContentType[contentType] = serializer;
        }
    }

    /// <summary>
    /// Gets a serializer by format name.
    /// </summary>
    /// <param name="format">The format name.</param>
    /// <returns>The serializer, or null if not found.</returns>
    public ISerializer? GetSerializer(string format)
    {
        if (string.IsNullOrWhiteSpace(format))
        {
            throw new ArgumentException("Format cannot be null or whitespace.", nameof(format));
        }

        _serializersByFormat.TryGetValue(format, out var serializer);
        return serializer;
    }

    /// <summary>
    /// Gets a serializer by file extension.
    /// </summary>
    /// <param name="extension">The file extension.</param>
    /// <returns>The serializer, or null if not found.</returns>
    public ISerializer? GetSerializerByExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new ArgumentException("Extension cannot be null or whitespace.", nameof(extension));
        }

        var ext = extension.StartsWith(".") ? extension : $".{extension}";
        _serializersByExtension.TryGetValue(ext, out var serializer);
        return serializer;
    }

    /// <summary>
    /// Gets a serializer by content type.
    /// </summary>
    /// <param name="contentType">The content type.</param>
    /// <returns>The serializer, or null if not found.</returns>
    public ISerializer? GetSerializerByContentType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new ArgumentException("Content type cannot be null or whitespace.", nameof(contentType));
        }

        _serializersByContentType.TryGetValue(contentType, out var serializer);
        return serializer;
    }

    /// <summary>
    /// Determines whether a format is supported.
    /// </summary>
    /// <param name="format">The format name.</param>
    /// <returns>True if the format is supported, false otherwise.</returns>
    public bool IsFormatSupported(string format)
    {
        if (string.IsNullOrWhiteSpace(format))
        {
            return false;
        }

        return _serializersByFormat.ContainsKey(format);
    }

    /// <summary>
    /// Determines whether a file extension is supported.
    /// </summary>
    /// <param name="extension">The file extension.</param>
    /// <returns>True if the extension is supported, false otherwise.</returns>
    public bool IsExtensionSupported(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return false;
        }

        var ext = extension.StartsWith(".") ? extension : $".{extension}";
        return _serializersByExtension.ContainsKey(ext);
    }

    /// <summary>
    /// Determines whether a content type is supported.
    /// </summary>
    /// <param name="contentType">The content type.</param>
    /// <returns>True if the content type is supported, false otherwise.</returns>
    public bool IsContentTypeSupported(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return false;
        }

        return _serializersByContentType.ContainsKey(contentType);
    }
}
