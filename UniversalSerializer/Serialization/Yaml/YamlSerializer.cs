// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Serialization.Yaml;
using ktsu.UniversalSerializer.Serialization.TypeRegistry;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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
		_discriminatorFormat = formatValue is string formatString && Enum.TryParse(formatString, out TypeDiscriminatorFormat format)
			? format
			: formatValue is TypeDiscriminatorFormat typeDiscriminatorFormat ? typeDiscriminatorFormat : TypeDiscriminatorFormat.Property;

		var serializerBuilder = new SerializerBuilder()
			.WithNamingConvention(CamelCaseNamingConvention.Instance)
			.DisableAliases();

		var deserializerBuilder = new DeserializerBuilder()
			.WithNamingConvention(CamelCaseNamingConvention.Instance);

		// Configure indentation if specified
		if (HasOption(SerializerOptionKeys.Yaml.IndentationWidth))
		{
			var indentationWidth = GetOption(SerializerOptionKeys.Yaml.IndentationWidth, 2);
			serializerBuilder.WithIndentedSequences().WithMaximumIndentationDepth(indentationWidth);
		}

		// Configure EmitDefaults
		if (HasOption(SerializerOptionKeys.Yaml.EmitDefaults))
		{
			var emitDefaults = GetOption(SerializerOptionKeys.Yaml.EmitDefaults, false);
			if (emitDefaults)
			{
				serializerBuilder.EmitDefaults();
			}
		}

		// Add polymorphic serialization support if enabled
		if (_enableTypeDiscriminator && _typeRegistry != null)
		{
			// Register the polymorphic serialization/deserialization handlers
			serializerBuilder.WithTypeConverter(new YamlPolymorphicTypeConverter((TypeRegistry.TypeRegistry)_typeRegistry, _typeDiscriminatorPropertyName, _discriminatorFormat));
			deserializerBuilder.WithNodeDeserializer(
				inner => new YamlPolymorphicNodeDeserializer((TypeRegistry.TypeRegistry)_typeRegistry, _typeDiscriminatorPropertyName, _discriminatorFormat, inner),
				s => s.OnTop());
		}

		// Allow configuring preferred file extension (.yaml or .yml)
		_fileExtension = GetOption(SerializerOptionKeys.Yaml.PreferredExtension, ".yaml");
		if (_fileExtension is not ".yaml" and not ".yml")
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
	public static string[] GetSupportedExtensions() => [".yaml", ".yml"];

	/// <inheritdoc/>
	public override string Serialize<T>(T obj) => obj == null ? string.Empty : _serializer.Serialize(obj);

	/// <inheritdoc/>
	public override string Serialize(object obj, Type type) => obj == null ? string.Empty : _serializer.Serialize(obj);

	/// <inheritdoc/>
	public override T Deserialize<T>(string serialized) => string.IsNullOrWhiteSpace(serialized) ? default! : _deserializer.Deserialize<T>(serialized);

	/// <inheritdoc/>
	public override object Deserialize(string serialized, Type type)
	{
		if (string.IsNullOrWhiteSpace(serialized))
		{
			return null!;
		}

		// YamlDotNet doesn't have a direct method to deserialize to a specific type,
		// so we need to use reflection to call the generic Deserialize<T> method
		var method = _deserializer.GetType().GetMethod("Deserialize", [typeof(string)]);
		var genericMethod = method!.MakeGenericMethod(type);
		return genericMethod.Invoke(_deserializer, [serialized])!;
	}

	/// <inheritdoc/>
	public override async Task<string> SerializeAsync<T>(T obj, CancellationToken cancellationToken = default) =>
		// YamlDotNet doesn't have async methods, so we need to use Task.Run for async operation
		await Task.Run(() => Serialize(obj), cancellationToken).ConfigureAwait(false);

	/// <inheritdoc/>
	public override async Task<T> DeserializeAsync<T>(string serialized, CancellationToken cancellationToken = default) =>
		// YamlDotNet doesn't have async methods, so we need to use Task.Run for async operation
		await Task.Run(() => Deserialize<T>(serialized), cancellationToken).ConfigureAwait(false);
}
