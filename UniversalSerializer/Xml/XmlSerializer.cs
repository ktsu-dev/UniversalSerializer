// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using ktsu.UniversalSerializer.TypeRegistry;

/// <summary>
/// Serializer for XML format using the .NET XmlSerializer.
/// </summary>
public class XmlSerializer : SerializerBase
{
	private readonly Dictionary<Type, System.Xml.Serialization.XmlSerializer> _serializerCache = [];
	private readonly XmlWriterSettings _writerSettings;
	private readonly XmlReaderSettings _readerSettings;

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
#pragma warning disable IDE0060 // Remove unused parameter
	public XmlSerializer(SerializerOptions options, TypeRegistry? typeRegistry = null) : base(options)
#pragma warning restore IDE0060 // Remove unused parameter
	{
		_writerSettings = new XmlWriterSettings
		{
			Indent = GetOption(SerializerOptionKeys.Xml.Indent, true),
			OmitXmlDeclaration = GetOption(SerializerOptionKeys.Xml.OmitXmlDeclaration, false)
		};

		if (HasOption(SerializerOptionKeys.Xml.Encoding))
		{
			string encodingName = GetOption(SerializerOptionKeys.Xml.Encoding, "utf-8");
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
			IgnoreProcessingInstructions = true,
			DtdProcessing = DtdProcessing.Prohibit,
			XmlResolver = null
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

		System.Xml.Serialization.XmlSerializer xmlSerializer = GetXmlSerializer(typeof(T));

		using StringWriter stringWriter = new();
		using XmlWriter xmlWriter = XmlWriter.Create(stringWriter, _writerSettings);
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

		System.Xml.Serialization.XmlSerializer xmlSerializer = GetXmlSerializer(type);

		using StringWriter stringWriter = new();
		using XmlWriter xmlWriter = XmlWriter.Create(stringWriter, _writerSettings);
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

		System.Xml.Serialization.XmlSerializer xmlSerializer = GetXmlSerializer(typeof(T));

		using StringReader stringReader = new(serialized);
		using XmlReader xmlReader = XmlReader.Create(stringReader, _readerSettings);
		return (T)xmlSerializer.Deserialize(xmlReader)!;
	}

	/// <inheritdoc/>
	public override object Deserialize(string serialized, Type type)
	{
		if (string.IsNullOrWhiteSpace(serialized))
		{
			return null!;
		}

		System.Xml.Serialization.XmlSerializer xmlSerializer = GetXmlSerializer(type);

		using StringReader stringReader = new(serialized);
		using XmlReader xmlReader = XmlReader.Create(stringReader, _readerSettings);
		return xmlSerializer.Deserialize(xmlReader)!;
	}

	/// <inheritdoc/>
	public override async Task<string> SerializeAsync<T>(T obj, CancellationToken cancellationToken = default) => await Task.Run(() => Serialize(obj), cancellationToken).ConfigureAwait(false);

	/// <inheritdoc/>
	public override async Task<T> DeserializeAsync<T>(string serialized, CancellationToken cancellationToken = default) => await Task.Run(() => Deserialize<T>(serialized), cancellationToken).ConfigureAwait(false);

	private System.Xml.Serialization.XmlSerializer GetXmlSerializer(Type type)
	{
		// Check if we have a cached serializer
		if (_serializerCache.TryGetValue(type, out System.Xml.Serialization.XmlSerializer? cachedSerializer))
		{
			return cachedSerializer;
		}

		// Create a new serializer
		System.Xml.Serialization.XmlSerializer newSerializer = new(type);

		// Cache the serializer
		_serializerCache[type] = newSerializer;

		return newSerializer;
	}
}
