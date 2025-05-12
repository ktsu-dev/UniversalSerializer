// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using System.Text;
using System.Xml;

namespace ktsu.UniversalSerializer.Serialization.Xml;

/// <summary>
/// Serializer for XML format using System.Xml.Serialization.
/// </summary>
public class XmlSerializer : SerializerBase
{
    private readonly System.Xml.Serialization.XmlSerializer[] _cachedSerializers = [];
    private readonly Dictionary<Type, System.Xml.Serialization.XmlSerializer> _serializerCache = new();
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
    public XmlSerializer(SerializerOptions options) : base(options)
    {
        _writerSettings = new XmlWriterSettings
        {
            Indent = GetOption(SerializerOptionKeys.Xml.Indent, true),
            OmitXmlDeclaration = GetOption(SerializerOptionKeys.Xml.OmitXmlDeclaration, false)
        };

        if (HasOption(SerializerOptionKeys.Xml.Encoding))
        {
            var encodingName = GetOption<string>(SerializerOptionKeys.Xml.Encoding, "utf-8");
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

        var serializer = GetSerializer<T>();
        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, _writerSettings);

        serializer.Serialize(xmlWriter, obj);
        return stringWriter.ToString();
    }

    /// <inheritdoc/>
    public override string Serialize(object obj, Type type)
    {
        if (obj == null)
        {
            return string.Empty;
        }

        var serializer = GetSerializer(type);
        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, _writerSettings);

        serializer.Serialize(xmlWriter, obj);
        return stringWriter.ToString();
    }

    /// <inheritdoc/>
    public override T Deserialize<T>(string serialized)
    {
        if (string.IsNullOrWhiteSpace(serialized))
        {
            return default!;
        }

        var serializer = GetSerializer<T>();
        using var stringReader = new StringReader(serialized);
        using var xmlReader = XmlReader.Create(stringReader, _readerSettings);

        return (T)serializer.Deserialize(xmlReader)!;
    }

    /// <inheritdoc/>
    public override object Deserialize(string serialized, Type type)
    {
        if (string.IsNullOrWhiteSpace(serialized))
        {
            return null!;
        }

        var serializer = GetSerializer(type);
        using var stringReader = new StringReader(serialized);
        using var xmlReader = XmlReader.Create(stringReader, _readerSettings);

        return serializer.Deserialize(xmlReader)!;
    }

    /// <inheritdoc/>
    public override async Task<string> SerializeAsync<T>(T obj, CancellationToken cancellationToken = default)
    {
        if (obj == null)
        {
            return string.Empty;
        }

        var serializer = GetSerializer<T>();
        using var memoryStream = new MemoryStream();
        using var xmlWriter = XmlWriter.Create(memoryStream, _writerSettings);

        serializer.Serialize(xmlWriter, obj);
        await xmlWriter.FlushAsync();

        memoryStream.Position = 0;
        using var reader = new StreamReader(memoryStream);
        return await reader.ReadToEndAsync();
    }

    /// <inheritdoc/>
    public override async Task<T> DeserializeAsync<T>(string serialized, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(serialized))
        {
            return default!;
        }

        var serializer = GetSerializer<T>();
        using var stringReader = new StringReader(serialized);
        using var xmlReader = XmlReader.Create(stringReader, _readerSettings);

        // We have to do synchronous deserialization since XML serializer doesn't have an async API
        var result = (T)serializer.Deserialize(xmlReader)!;

        // Just to make it look like it's async, but there's no real benefit
        await Task.CompletedTask;

        return result;
    }

    private System.Xml.Serialization.XmlSerializer GetSerializer<T>()
    {
        var type = typeof(T);
        return GetSerializer(type);
    }

    private System.Xml.Serialization.XmlSerializer GetSerializer(Type type)
    {
        if (_serializerCache.TryGetValue(type, out var serializer))
        {
            return serializer;
        }

        serializer = new System.Xml.Serialization.XmlSerializer(type);
        _serializerCache[type] = serializer;
        return serializer;
    }
}
