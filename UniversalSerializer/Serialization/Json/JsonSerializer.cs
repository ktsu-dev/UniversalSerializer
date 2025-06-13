// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Serialization.Json;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using ktsu.UniversalSerializer.Serialization.TypeConverter;
using ktsu.UniversalSerializer.Serialization.TypeRegistry;

/// <summary>
/// Serializer for JSON format using System.Text.Json.
/// </summary>
public class JsonSerializer : SerializerBase
{
	private readonly JsonSerializerOptions _jsonOptions;

	/// <summary>
	/// Initializes a new instance of the <see cref="JsonSerializer"/> class with default options.
	/// </summary>
	public JsonSerializer() : this(SerializerOptions.Default())
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="JsonSerializer"/> class with the specified options.
	/// </summary>
	/// <param name="options">The serializer options.</param>
	/// <param name="typeConverterRegistry">Optional registry for type converters.</param>
	/// <param name="typeRegistry">Optional registry for polymorphic types.</param>
	public JsonSerializer(
		SerializerOptions options,
		TypeConverterRegistry? typeConverterRegistry = null,
		TypeRegistry? typeRegistry = null)
		: base(options)
	{
		_jsonOptions = new JsonSerializerOptions
		{
			WriteIndented = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			PropertyNameCaseInsensitive = GetOption(SerializerOptionKeys.Json.CaseInsensitive, false)
		};

		if (HasOption(SerializerOptionKeys.Json.AllowComments))
		{
			_jsonOptions.ReadCommentHandling = GetOption(SerializerOptionKeys.Json.AllowComments, false)
				? JsonCommentHandling.Skip
				: JsonCommentHandling.Disallow;
		}

		if (HasOption(SerializerOptionKeys.Json.MaxDepth))
		{
			_jsonOptions.MaxDepth = GetOption(SerializerOptionKeys.Json.MaxDepth, 64);
		}

		if (HasOption(SerializerOptionKeys.Json.PropertyNamingPolicy))
		{
			string? policy = GetOption<string?>(SerializerOptionKeys.Json.PropertyNamingPolicy, null);
			if (policy != null)
			{
				_jsonOptions.PropertyNamingPolicy = policy.ToLowerInvariant() switch
				{
					"camelcase" => JsonNamingPolicy.CamelCase,
					"lowercase" => JsonNamingPolicy.SnakeCaseLower,
					"uppercase" => JsonNamingPolicy.SnakeCaseUpper,
					"kebabcase" => JsonNamingPolicy.KebabCaseLower,
					"kebabcaseupper" => JsonNamingPolicy.KebabCaseUpper,
					_ => null
				};
			}
		}

		// Add enum converter if specified
		object enumFormat = GetOption<object>(SerializerOptionKeys.Common.EnumFormat, EnumSerializationFormat.Name.ToString());
		if (enumFormat is string enumFormatStr &&
			Enum.TryParse(enumFormatStr, out EnumSerializationFormat enumSerializationFormat) &&
			enumSerializationFormat == EnumSerializationFormat.Name)
		{
			_jsonOptions.Converters.Add(new JsonStringEnumConverter());
		}
		else if (enumFormat is EnumSerializationFormat format && format == EnumSerializationFormat.Name)
		{
			_jsonOptions.Converters.Add(new JsonStringEnumConverter());
		}

		// Add string type converter if specified and registry provided
		if (GetOption(SerializerOptionKeys.Common.UseStringConversionForUnsupportedTypes, true) && typeConverterRegistry != null)
		{
			_jsonOptions.Converters.Add(new JsonStringTypeConverter(typeConverterRegistry));
		}

		// Add polymorphic type handling if specified and registry provided
		if (GetOption(SerializerOptionKeys.TypeRegistry.EnableTypeDiscriminator, false) && typeRegistry != null)
		{
			_jsonOptions.Converters.Add(new JsonPolymorphicConverter(typeRegistry, Options));
		}
	}

	/// <inheritdoc/>
	public override string ContentType => "application/json";

	/// <inheritdoc/>
	public override string FileExtension => ".json";

	/// <inheritdoc/>
	public override string Serialize<T>(T obj) => System.Text.Json.JsonSerializer.Serialize(obj, _jsonOptions);

	/// <inheritdoc/>
	public override string Serialize(object obj, Type type) => System.Text.Json.JsonSerializer.Serialize(obj, type, _jsonOptions);

	/// <inheritdoc/>
	public override T Deserialize<T>(string serialized) => System.Text.Json.JsonSerializer.Deserialize<T>(serialized, _jsonOptions)!;

	/// <inheritdoc/>
	public override object Deserialize(string serialized, Type type) => System.Text.Json.JsonSerializer.Deserialize(serialized, type, _jsonOptions) ?? throw new InvalidOperationException("Deserialization returned null");

	/// <inheritdoc/>
	public override async Task<string> SerializeAsync<T>(T obj, CancellationToken cancellationToken = default)
	{
		using MemoryStream stream = new();
		await System.Text.Json.JsonSerializer.SerializeAsync(stream, obj, _jsonOptions, cancellationToken).ConfigureAwait(false);
		stream.Position = 0;
		using StreamReader reader = new(stream);
		return await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
	}

	/// <inheritdoc/>
	public override async Task<T> DeserializeAsync<T>(string serialized, CancellationToken cancellationToken = default)
	{
		using MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(serialized));
		return await System.Text.Json.JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions, cancellationToken).ConfigureAwait(false) ?? throw new InvalidOperationException("Deserialization returned null");
	}
}
