// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Xml.Serialization;
using System.Xml;
using System;

namespace ktsu.UniversalSerializer.Serialization.Xml;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using ktsu.UniversalSerializer.Serialization.TypeRegistry;

/// <summary>
/// Serializer for XML format using the .NET XmlSerializer.
/// </summary>
public class XmlSerializer : SerializerBase
{
	private readonly Dictionary<Type, System.Xml.Serialization.XmlSerializer> _serializerCache = new();
	private readonly XmlWriterSettings _writerSettings;
	private readonly XmlReaderSettings _readerSettings;
	private readonly TypeRegistry? _typeRegistry;
	private readonly bool _enableTypeDiscriminator;
	private readonly string _typeDiscriminatorFormat;

	/// <summary>
	/// Initializes a new instance of the <see cref="XmlSerializer"/> class with default options.
	/// </summary>
	public XmlSerializer() : this(SerializerOptions.Default())
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="XmlSerializer"/> class with the specified options.
	/// </summary>
	/// <param name="options">The serializer options.</param>
	/// <param name="typeRegistry">Optional registry for polymorphic types.</param>
	public XmlSerializer(SerializerOptions options, TypeRegistry? typeRegistry = null) : base(options)
	{
		_typeRegistry = typeRegistry;
		_enableTypeDiscriminator = GetOption(SerializerOptionKeys.TypeRegistry.EnableTypeDiscriminator, false);
		_typeDiscriminatorFormat = GetOption(SerializerOptionKeys.TypeRegistry.TypeDiscriminatorFormat,
			TypeDiscriminatorFormat.Property.ToString());

		_writerSettings = new XmlWriterSettings
		{
			Indent = GetOption(SerializerOptionKeys.Xml.Indent, true),
			OmitXmlDeclaration = GetOption(SerializerOptionKeys.Xml.OmitXmlDeclaration, false)
		};

		if (HasOption(SerializerOptionKeys.Xml.Encoding))
		{
			var encodingName = GetOption(SerializerOptionKeys.Xml.Encoding, "utf-8");
			_writerSettings.Encoding = Encoding.GetEncoding(encodingName);
		}
		else
		{
			_writerSettings.Encoding = Encoding.UTF8;
		}

		_readerSettings = new XmlReaderSettings
		{
			IgnoreWhitespace = true,
			IgnoreComments = true,
			IgnoreProcessingInstructions = true
		};
	}

	/// <inheritdoc/>
	public override string ContentType => "application/xml";

	/// <inheritdoc/>
	public override string FileExtension => ".xml";

	/// <inheritdoc/>
	public override string Serialize<T>(T obj)
	{
		if (obj == null)
		{
			return string.Empty;
		}

		var xmlSerializer = GetXmlSerializer(typeof(T));

		using var stringWriter = new StringWriter();
		using var xmlWriter = XmlWriter.Create(stringWriter, _writerSettings);
		xmlSerializer.Serialize(xmlWriter, obj);
		return stringWriter.ToString();
	}

	/// <inheritdoc/>
	public override string Serialize(object obj, Type type)
	{
		if (obj == null)
		{
			return string.Empty;
		}

		var xmlSerializer = GetXmlSerializer(type);

		using var stringWriter = new StringWriter();
		using var xmlWriter = XmlWriter.Create(stringWriter, _writerSettings);
		xmlSerializer.Serialize(xmlWriter, obj);
		return stringWriter.ToString();
	}

	/// <inheritdoc/>
	public override T Deserialize<T>(string serialized)
	{
		if (string.IsNullOrWhiteSpace(serialized))
		{
			return default!;
		}

		var xmlSerializer = GetXmlSerializer(typeof(T));

		using var stringReader = new StringReader(serialized);
		return (T)xmlSerializer.Deserialize(stringReader)!;
	}

	/// <inheritdoc/>
	public override object Deserialize(string serialized, Type type)
	{
		if (string.IsNullOrWhiteSpace(serialized))
		{
			return null!;
		}

		var xmlSerializer = GetXmlSerializer(type);

		using var stringReader = new StringReader(serialized);
		return xmlSerializer.Deserialize(stringReader)!;
	}

	/// <inheritdoc/>
	public override async Task<string> SerializeAsync<T>(T obj, CancellationToken cancellationToken = default)
	{
		return await Task.Run(() => Serialize(obj), cancellationToken).ConfigureAwait(false);
	}

	/// <inheritdoc/>
	public override async Task<T> DeserializeAsync<T>(string serialized, CancellationToken cancellationToken = default)
	{
		return await Task.Run(() => Deserialize<T>(serialized), cancellationToken).ConfigureAwait(false);
	}

	private System.Xml.Serialization.XmlSerializer GetXmlSerializer(Type type)
	{
		// Check if we have a cached serializer
		if (_serializerCache.TryGetValue(type, out var cachedSerializer))
		{
			return cachedSerializer;
		}

		// Create a new serializer
		var newSerializer = new System.Xml.Serialization.XmlSerializer(type);

		// Cache the serializer
		_serializerCache[type] = newSerializer;

		return newSerializer;
	}
}
