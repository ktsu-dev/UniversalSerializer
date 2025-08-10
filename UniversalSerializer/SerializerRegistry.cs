// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer;
using System;
using System.Collections.Concurrent;
using ktsu.UniversalSerializer.Toml;
using ktsu.UniversalSerializer.Yaml;

/// <summary>
/// Registry for managing serializers by format.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SerializerRegistry"/> class with the specified factory.
/// </remarks>
/// <param name="factory">The serializer factory to use.</param>
public class SerializerRegistry(SerializerFactory factory)
{
	private readonly ConcurrentDictionary<string, ISerializer> _serializersByFormat = new(StringComparer.OrdinalIgnoreCase);
	private readonly ConcurrentDictionary<string, ISerializer> _serializersByExtension = new(StringComparer.OrdinalIgnoreCase);
	private readonly ConcurrentDictionary<string, ISerializer> _serializersByContentType = new(StringComparer.OrdinalIgnoreCase);
	private readonly SerializerFactory _factory = factory ?? throw new ArgumentNullException(nameof(factory));

	/// <summary>
	/// Registers built-in serializers.
	/// </summary>
	/// <param name="options">Optional serializer options.</param>
	public void RegisterBuiltIn(SerializerOptions? options = null)
	{
		SerializerOptions serializerOptions = options ?? SerializerOptions.Default();

		// Ensure the factory knows how to create built-in serializers
		_factory
			.RegisterSerializer(o => new Json.JsonSerializer(o))
			.RegisterSerializer(o => new Xml.XmlSerializer(o))
			.RegisterSerializer(o => new YamlSerializer(o))
			.RegisterSerializer(o => new Toml.TomlSerializer(o))
			.RegisterSerializer(o => new MessagePack.MessagePackSerializer(o));

		// Register JSON serializer
		Json.JsonSerializer jsonSerializer = _factory.Create<Json.JsonSerializer>(serializerOptions);
		Register("json", jsonSerializer);
		RegisterFileExtensions("json", ".json");
		RegisterContentTypes("json", "application/json", "text/json");

		// Register XML serializer
		Xml.XmlSerializer xmlSerializer = _factory.Create<Xml.XmlSerializer>(serializerOptions);
		Register("xml", xmlSerializer);
		RegisterFileExtensions("xml", ".xml");
		RegisterContentTypes("xml", "application/xml", "text/xml");

		// Register YAML serializer
		YamlSerializer yamlSerializer = _factory.Create<YamlSerializer>(serializerOptions);
		Register("yaml", yamlSerializer);
		RegisterFileExtensions("yaml", ".yaml", ".yml");
		RegisterContentTypes("yaml", "application/x-yaml", "text/yaml");

		// Register TOML serializer
		TomlSerializer tomlSerializer = _factory.Create<TomlSerializer>(serializerOptions);
		Register("toml", tomlSerializer);
		RegisterFileExtensions("toml", ".toml", ".tml");
		RegisterContentTypes("toml", "application/toml");

		// Register MessagePack serializer
		MessagePack.MessagePackSerializer messagePackSerializer = _factory.Create<MessagePack.MessagePackSerializer>(serializerOptions);
		Register("messagepack", messagePackSerializer);
		RegisterFileExtensions("messagepack", ".msgpack", ".mp");
		RegisterContentTypes("messagepack", "application/x-msgpack");
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

		if (!_serializersByFormat.TryGetValue(format, out ISerializer? serializer))
		{
			throw new ArgumentException($"Format '{format}' is not registered.", nameof(format));
		}

		foreach (string extension in extensions)
		{
			string ext = extension.StartsWith('.') ? extension : $".{extension}";
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

		if (!_serializersByFormat.TryGetValue(format, out ISerializer? serializer))
		{
			throw new ArgumentException($"Format '{format}' is not registered.", nameof(format));
		}

		foreach (string contentType in contentTypes)
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

		_serializersByFormat.TryGetValue(format, out ISerializer? serializer);
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

		string ext = extension.StartsWith('.') ? extension : $".{extension}";
		_serializersByExtension.TryGetValue(ext, out ISerializer? serializer);
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

		_serializersByContentType.TryGetValue(contentType, out ISerializer? serializer);
		return serializer;
	}

	/// <summary>
	/// Checks if a format is supported.
	/// </summary>
	/// <param name="format">The format name to check.</param>
	/// <returns>True if the format is supported; otherwise, false.</returns>
	public bool IsFormatSupported(string format)
	{
		if (string.IsNullOrWhiteSpace(format))
		{
			return false;
		}

		return _serializersByFormat.ContainsKey(format);
	}

	/// <summary>
	/// Checks if a file extension is supported.
	/// </summary>
	/// <param name="extension">The file extension to check.</param>
	/// <returns>True if the extension is supported; otherwise, false.</returns>
	public bool IsExtensionSupported(string extension)
	{
		if (string.IsNullOrWhiteSpace(extension))
		{
			return false;
		}

		string ext = extension.StartsWith('.') ? extension : $".{extension}";
		return _serializersByExtension.ContainsKey(ext);
	}
}
