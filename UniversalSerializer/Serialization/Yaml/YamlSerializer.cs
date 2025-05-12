// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using System.Text;
using ktsu.UniversalSerializer.Serialization.TypeRegistry;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ktsu.UniversalSerializer.Serialization.Yaml;

/// <summary>
/// Serializer for YAML format using YamlDotNet.
/// </summary>
public class YamlSerializer : SerializerBase
{
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;
    private readonly string _fileExtension;
    private readonly TypeRegistry.TypeRegistry? _typeRegistry;
    private readonly bool _enableTypeDiscriminator;
    private readonly string _typeDiscriminatorPropertyName;
    private readonly TypeDiscriminatorFormat _discriminatorFormat;

    /// <summary>
    /// Initializes a new instance of the <see cref="YamlSerializer"/> class with default options.
    /// </summary>
    public YamlSerializer() : this(SerializerOptions.Default())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="YamlSerializer"/> class with the specified options.
    /// </summary>
    /// <param name="options">The serializer options.</param>
    /// <param name="typeRegistry">Optional registry for polymorphic types.</param>
    public YamlSerializer(SerializerOptions options, TypeRegistry.TypeRegistry? typeRegistry = null) : base(options)
    {
        _typeRegistry = typeRegistry;
        _enableTypeDiscriminator = GetOption(SerializerOptionKeys.TypeRegistry.EnableTypeDiscriminator, false);
        _typeDiscriminatorPropertyName = GetOption(SerializerOptionKeys.TypeRegistry.TypeDiscriminatorPropertyName, "$type");

        var formatValue = GetOption(SerializerOptionKeys.TypeRegistry.TypeDiscriminatorFormat,
            TypeDiscriminatorFormat.Property.ToString());

        // Try to parse the format from string if stored as string
        if (formatValue is string formatString && Enum.TryParse(formatString, out TypeDiscriminatorFormat format))
        {
            _discriminatorFormat = format;
        }
        else if (formatValue is TypeDiscriminatorFormat typeDiscriminatorFormat)
        {
            _discriminatorFormat = typeDiscriminatorFormat;
        }
        else
        {
            _discriminatorFormat = TypeDiscriminatorFormat.Property;
        }

        var serializerBuilder = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .DisableAliases();

        var deserializerBuilder = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance);

        // Configure indentation if specified
        if (HasOption(SerializerOptionKeys.Yaml.IndentationWidth))
        {
            int indentationWidth = GetOption(SerializerOptionKeys.Yaml.IndentationWidth, 2);
            serializerBuilder.WithIndentedSequences().WithMaximumIndentationDepth(indentationWidth);
        }

        // Configure EmitDefaults
        if (HasOption(SerializerOptionKeys.Yaml.EmitDefaults))
        {
            bool emitDefaults = GetOption(SerializerOptionKeys.Yaml.EmitDefaults, false);
            if (emitDefaults)
            {
                serializerBuilder.EmitDefaults();
            }
        }

        // Add polymorphic serialization support if enabled
        if (_enableTypeDiscriminator && _typeRegistry != null)
        {
            // Register the polymorphic serialization/deserialization handlers
            serializerBuilder.WithTypeConverter(new YamlPolymorphicTypeConverter(_typeRegistry, _typeDiscriminatorPropertyName, _discriminatorFormat));
            deserializerBuilder.WithNodeDeserializer(
                inner => new YamlPolymorphicNodeDeserializer(_typeRegistry, _typeDiscriminatorPropertyName, _discriminatorFormat, inner),
                s => s.OnTop());
        }

        // Allow configuring preferred file extension (.yaml or .yml)
        _fileExtension = GetOption(SerializerOptionKeys.Yaml.PreferredExtension, ".yaml");
        if (_fileExtension != ".yaml" && _fileExtension != ".yml")
        {
            _fileExtension = ".yaml";
        }

        _serializer = serializerBuilder.Build();
        _deserializer = deserializerBuilder.Build();
    }

    /// <inheritdoc/>
    public override string ContentType => "application/yaml";

    /// <inheritdoc/>
    public override string FileExtension => _fileExtension;

    /// <summary>
    /// Gets all supported file extensions for YAML format.
    /// </summary>
    /// <returns>An array of supported file extensions.</returns>
    public string[] GetSupportedExtensions() => [".yaml", ".yml"];

    /// <inheritdoc/>
    public override string Serialize<T>(T obj)
    {
        if (obj == null)
        {
            return string.Empty;
        }

        return _serializer.Serialize(obj);
    }

    /// <inheritdoc/>
    public override string Serialize(object obj, Type type)
    {
        if (obj == null)
        {
            return string.Empty;
        }

        return _serializer.Serialize(obj);
    }

    /// <inheritdoc/>
    public override T Deserialize<T>(string serialized)
    {
        if (string.IsNullOrWhiteSpace(serialized))
        {
            return default!;
        }

        return _deserializer.Deserialize<T>(serialized);
    }

    /// <inheritdoc/>
    public override object Deserialize(string serialized, Type type)
    {
        if (string.IsNullOrWhiteSpace(serialized))
        {
            return null!;
        }

        // YamlDotNet doesn't have a direct method to deserialize to a specific type,
        // so we need to use reflection to call the generic Deserialize<T> method
        var method = _deserializer.GetType().GetMethod("Deserialize", new[] { typeof(string) });
        var genericMethod = method!.MakeGenericMethod(type);
        return genericMethod.Invoke(_deserializer, new object[] { serialized })!;
    }

    /// <inheritdoc/>
    public override async Task<string> SerializeAsync<T>(T obj, CancellationToken cancellationToken = default)
    {
        // YamlDotNet doesn't have async methods, so we need to use Task.Run for async operation
        return await Task.Run(() => Serialize(obj), cancellationToken);
    }

    /// <inheritdoc/>
    public override async Task<T> DeserializeAsync<T>(string serialized, CancellationToken cancellationToken = default)
    {
        // YamlDotNet doesn't have async methods, so we need to use Task.Run for async operation
        return await Task.Run(() => Deserialize<T>(serialized), cancellationToken);
    }
}
