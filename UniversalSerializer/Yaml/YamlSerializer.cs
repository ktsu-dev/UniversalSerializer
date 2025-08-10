// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Yaml;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeDeserializers;

/// <summary>
/// Serializer for YAML format using YamlDotNet.
/// </summary>
public class YamlSerializer : SerializerBase
{
	private readonly SerializerBuilder _serializerBuilder;
	private readonly DeserializerBuilder _deserializerBuilder;
	private readonly TypeRegistry? _typeRegistry;
	private readonly bool _enableTypeDiscriminator;
	private readonly string _typeDiscriminatorPropertyName;
	private readonly string _discriminatorFormat;

	/// <summary>
	/// Initializes a new instance of the <see cref="YamlSerializer"/> class.
	/// </summary>
	public YamlSerializer() : this(SerializerOptions.Default())
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="YamlSerializer"/> class.
	/// </summary>
	/// <param name="options">The serializer options.</param>
	/// <param name="typeRegistry">Optional registry for polymorphic types.</param>
	public YamlSerializer(SerializerOptions options, TypeRegistry? typeRegistry = null) : base(options)
	{
		_typeRegistry = typeRegistry;
		_enableTypeDiscriminator = GetOption(SerializerOptionKeys.TypeRegistry.EnableTypeDiscriminator, false);
		_typeDiscriminatorPropertyName = GetOption(SerializerOptionKeys.TypeRegistry.DiscriminatorPropertyName, SerializerDefaults.TypeDiscriminatorPropertyName);
		_discriminatorFormat = GetOption(SerializerOptionKeys.TypeRegistry.DiscriminatorFormat, SerializerDefaults.TypeDiscriminatorFormat);

		// Configure serializer
		_serializerBuilder = new SerializerBuilder()
			.ConfigureDefaultValuesHandling(DefaultValuesHandling.Preserve)
			.WithIndentedSequences();

		// Configure deserializer
		_deserializerBuilder = new DeserializerBuilder()
			.IgnoreUnmatchedProperties();

		// Handle enum serialization format
		object enumFormat = GetOption<object>(SerializerOptionKeys.Common.EnumFormat, EnumSerializationFormat.Name.ToString());
		if (enumFormat is string enumFormatStr &&
			Enum.TryParse(enumFormatStr, out EnumSerializationFormat enumSerializationFormat) &&
			enumSerializationFormat == EnumSerializationFormat.Name)
		{
			_serializerBuilder.WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance);
			_deserializerBuilder.WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance);
		}

		// Add polymorphic type handling if requested
		if (_enableTypeDiscriminator && _typeRegistry != null)
		{
			_serializerBuilder.WithTypeConverter(new YamlPolymorphicTypeConverter(_typeRegistry, _typeDiscriminatorPropertyName, _discriminatorFormat));
			_deserializerBuilder.WithNodeDeserializer(
				inner => new YamlPolymorphicNodeDeserializer(_typeRegistry, _typeDiscriminatorPropertyName, _discriminatorFormat, inner),
				s => s.InsteadOf<ObjectNodeDeserializer>());
		}
	}

	/// <inheritdoc/>
	public override string ContentType => "text/yaml";

	/// <inheritdoc/>
	public override string FileExtension => ".yaml";

	/// <inheritdoc/>
	public override string Serialize<T>(T obj)
	{
		if (obj == null)
		{
			return string.Empty;
		}

		ISerializer serializer = _serializerBuilder.Build();
		StringBuilder stringBuilder = new();

		using StringWriter writer = new(stringBuilder);
		serializer.Serialize(writer, obj);

		return stringBuilder.ToString();
	}

	/// <inheritdoc/>
	public override string Serialize(object obj, Type type)
	{
		if (obj == null)
		{
			return string.Empty;
		}

		ISerializer serializer = _serializerBuilder.Build();
		StringBuilder stringBuilder = new();

		using StringWriter writer = new(stringBuilder);
		serializer.Serialize(writer, obj);

		return stringBuilder.ToString();
	}

	/// <inheritdoc/>
	public override T Deserialize<T>(string serialized)
	{
		if (string.IsNullOrWhiteSpace(serialized))
		{
			return default!;
		}

		IDeserializer deserializer = _deserializerBuilder.Build();

		using StringReader reader = new(serialized);
		return deserializer.Deserialize<T>(reader);
	}

	/// <inheritdoc/>
	public override object Deserialize(string serialized, Type type)
	{
		if (string.IsNullOrWhiteSpace(serialized))
		{
			return null!;
		}

		IDeserializer deserializer = _deserializerBuilder.Build();

		using StringReader reader = new(serialized);
		return deserializer.Deserialize(reader, type)!;
	}

	/// <inheritdoc/>
	public override async Task<string> SerializeAsync<T>(T obj, CancellationToken cancellationToken = default) => await Task.Run(() => Serialize(obj), cancellationToken).ConfigureAwait(false);

	/// <inheritdoc/>
	public override async Task<T> DeserializeAsync<T>(string serialized, CancellationToken cancellationToken = default) => await Task.Run(() => Deserialize<T>(serialized), cancellationToken).ConfigureAwait(false);
}
