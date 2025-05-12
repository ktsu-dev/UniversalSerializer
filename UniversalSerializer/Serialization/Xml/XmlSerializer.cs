// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using System.Text;
using System.Xml;
using ktsu.UniversalSerializer.Serialization.TypeRegistry;

namespace ktsu.UniversalSerializer.Serialization.Xml;

/// <summary>
/// Serializer for XML format using System.Xml.Serialization.
/// </summary>
public class XmlSerializer : SerializerBase
{
    private readonly Dictionary<Type, System.Xml.Serialization.XmlSerializer> _serializerCache = new();
    private readonly XmlWriterSettings _writerSettings;
    private readonly XmlReaderSettings _readerSettings;
    private readonly TypeRegistry.TypeRegistry? _typeRegistry;
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
    public XmlSerializer(SerializerOptions options, TypeRegistry.TypeRegistry? typeRegistry = null) : base(options)
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

        // Handle polymorphic serialization if enabled and object is of a derived type
        if (_enableTypeDiscriminator && _typeRegistry != null && obj.GetType() != typeof(T))
        {
            return SerializePolymorphic(obj, typeof(T));
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

        // Handle polymorphic serialization if enabled and object is of a derived type
        if (_enableTypeDiscriminator && _typeRegistry != null && obj.GetType() != type)
        {
            return SerializePolymorphic(obj, type);
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

        // Check if this contains polymorphic data
        if (_enableTypeDiscriminator && _typeRegistry != null && serialized.Contains("xsi:type"))
        {
            return (T)DeserializePolymorphic(serialized, typeof(T))!;
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

        // Check if this contains polymorphic data
        if (_enableTypeDiscriminator && _typeRegistry != null && serialized.Contains("xsi:type"))
        {
            return DeserializePolymorphic(serialized, type)!;
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

    private string SerializePolymorphic(object obj, Type expectedType)
    {
        if (_typeRegistry == null)
        {
            throw new InvalidOperationException("TypeRegistry is required for polymorphic serialization.");
        }

        var actualType = obj.GetType();
        var typeName = _typeRegistry.GetTypeName(actualType);

        // Use XmlSerializer for the actual type
        var serializer = GetSerializer(actualType);

        // Create namespace manager with xsi and xsd namespaces
        var namespaces = new XmlSerializerNamespaces();
        namespaces.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
        namespaces.Add("xsd", "http://www.w3.org/2001/XMLSchema");

        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, _writerSettings);

        // XML type discriminator is implemented using xsi:type
        serializer.Serialize(xmlWriter, obj, namespaces);

        // Get the serialized XML
        var xml = stringWriter.ToString();

        // Now we need to inject the type attribute
        // This is a bit of a hack, but XML serialization doesn't support type attributes directly
        // We need to modify the root element to include the xsi:type attribute
        if (!string.IsNullOrEmpty(typeName))
        {
            // Find the first '>' character which marks the end of the root element's opening tag
            var index = xml.IndexOf('>');
            if (index > 0)
            {
                // Check if it's a self-closing tag
                var isSelfClosing = xml[index - 1] == '/';
                var insertPosition = isSelfClosing ? index - 1 : index;

                // Insert the type attribute
                xml = xml.Insert(insertPosition, $" xsi:type=\"{typeName}\"");
            }
        }

        return xml;
    }

    private object DeserializePolymorphic(string serialized, Type expectedType)
    {
        if (_typeRegistry == null)
        {
            throw new InvalidOperationException("TypeRegistry is required for polymorphic deserialization.");
        }

        // Load the XML document
        var doc = new XmlDocument();
        doc.LoadXml(serialized);

        // Get the root element
        var root = doc.DocumentElement;
        if (root == null)
        {
            throw new InvalidOperationException("XML document has no root element.");
        }

        // Look for xsi:type attribute
        string? typeName = null;
        var typeAttr = root.Attributes["type", "http://www.w3.org/2001/XMLSchema-instance"];
        if (typeAttr != null)
        {
            typeName = typeAttr.Value;
        }

        if (string.IsNullOrEmpty(typeName))
        {
            // No type discriminator found, fall back to expected type
            return Deserialize(serialized, expectedType);
        }

        // Resolve the type
        var type = _typeRegistry.ResolveType(typeName);
        if (type == null)
        {
            throw new InvalidOperationException($"Could not resolve type '{typeName}'");
        }

        // Verify the type is assignable to the expected type
        if (!expectedType.IsAssignableFrom(type))
        {
            throw new InvalidOperationException($"Type '{type.FullName}' is not assignable to '{expectedType.FullName}'");
        }

        // Deserialize to the concrete type
        var serializer = GetSerializer(type);
        using var stringReader = new StringReader(serialized);
        using var xmlReader = XmlReader.Create(stringReader, _readerSettings);

        return serializer.Deserialize(xmlReader)!;
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
