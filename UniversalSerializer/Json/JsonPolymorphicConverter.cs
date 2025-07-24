// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Json;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using ktsu.UniversalSerializer.TypeRegistry;

/// <summary>
/// A JSON converter factory for polymorphic serialization that uses type discriminators.
/// </summary>
/// <remarks>
/// Creates a polymorphic type converter that uses type discriminators.
/// </remarks>
/// <param name="typeRegistry">The type registry to use.</param>
/// <param name="options">The serializer options.</param>
public class JsonPolymorphicConverter(TypeRegistry typeRegistry, SerializerOptions options) : JsonConverterFactory
{
	private readonly TypeRegistry _typeRegistry = typeRegistry ?? throw new ArgumentNullException(nameof(typeRegistry));
	private readonly string _typeDiscriminatorPropertyName = options.GetOption(SerializerOptionKeys.TypeRegistry.DiscriminatorPropertyName, SerializerDefaults.TypeDiscriminatorPropertyName);
	private readonly TypeDiscriminatorFormat _discriminatorFormat = Enum.TryParse(options.GetOption(SerializerOptionKeys.TypeRegistry.DiscriminatorFormat, SerializerDefaults.TypeDiscriminatorFormat), out TypeDiscriminatorFormat format) ? format : TypeDiscriminatorFormat.Property;

	/// <inheritdoc/>
	public override bool CanConvert(Type typeToConvert) => _typeRegistry.HasPolymorphicTypes();

	/// <inheritdoc/>
	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		Type converterType = typeof(JsonPolymorphicConverterInner<>).MakeGenericType(typeToConvert);
		return (JsonConverter?)Activator.CreateInstance(converterType, _typeRegistry, _typeDiscriminatorPropertyName, _discriminatorFormat);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via reflection")]
	private sealed class JsonPolymorphicConverterInner<T>(TypeRegistry typeRegistry, string typeDiscriminatorPropertyName, TypeDiscriminatorFormat discriminatorFormat) : JsonConverter<T>
	{
		private readonly TypeRegistry _typeRegistry = typeRegistry ?? throw new ArgumentNullException(nameof(typeRegistry));
		private readonly string _typeDiscriminatorPropertyName = typeDiscriminatorPropertyName;
		private readonly TypeDiscriminatorFormat _discriminatorFormat = discriminatorFormat;

		public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
			{
				throw new JsonException("Expected start of object");
			}

			using JsonDocument jsonDocument = JsonDocument.ParseValue(ref reader);
			JsonElement rootElement = jsonDocument.RootElement;

			// Extract type information based on the discriminator format
			string? typeName = null;

			switch (_discriminatorFormat)
			{
				case TypeDiscriminatorFormat.Property:
					if (rootElement.TryGetProperty(_typeDiscriminatorPropertyName, out JsonElement typeProperty))
					{
						typeName = typeProperty.GetString();
					}

					break;

				case TypeDiscriminatorFormat.TypeProperty:
					// Not implemented yet
					throw new NotImplementedException("TypeProperty discriminator format not implemented yet");

				case TypeDiscriminatorFormat.Wrapper:
					if (rootElement.TryGetProperty("type", out JsonElement wrapperTypeProperty) &&
						rootElement.TryGetProperty("value", out JsonElement valueProperty))
					{
						typeName = wrapperTypeProperty.GetString();
						// Re-use valueProperty for deserialization below
						rootElement = valueProperty;
					}

					break;
				default:
					break;
			}

			if (string.IsNullOrEmpty(typeName))
			{
				throw new JsonException($"Could not find type discriminator '{_typeDiscriminatorPropertyName}' in JSON");
			}

			// Resolve the type
			Type type = _typeRegistry.ResolveType(typeName) ?? throw new JsonException($"Could not resolve type '{typeName}'");

			// Verify the type is assignable to the expected type
			if (!typeof(T).IsAssignableFrom(type))
			{
				throw new JsonException($"Type '{type.FullName}' is not assignable to '{typeof(T).FullName}'");
			}

			// Deserialize to the concrete type
			string json = rootElement.GetRawText();
			return (T)System.Text.Json.JsonSerializer.Deserialize(json, type, options)!;
		}

		public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			Type concreteType = value.GetType();

			// Skip discriminator if the concrete type is the exact type requested
			if (concreteType == typeof(T))
			{
				System.Text.Json.JsonSerializer.Serialize(writer, value, options);
				return;
			}

			string typeName = _typeRegistry.GetTypeName(concreteType);

			switch (_discriminatorFormat)
			{
				case TypeDiscriminatorFormat.Property:
					// Serialize the value to a document
					string valueJson = System.Text.Json.JsonSerializer.Serialize(value, concreteType, options);

					// Parse the document to add the type discriminator
					using (JsonDocument document = JsonDocument.Parse(valueJson))
					{
						writer.WriteStartObject();

						// Write the type discriminator property
						writer.WriteString(_typeDiscriminatorPropertyName, typeName);

						// Copy all properties from the original object
						foreach (JsonProperty property in document.RootElement.EnumerateObject())
						{
							property.WriteTo(writer);
						}

						writer.WriteEndObject();
					}

					break;

				case TypeDiscriminatorFormat.TypeProperty:
					// Not implemented yet
					throw new NotImplementedException("TypeProperty discriminator format not implemented yet");

				case TypeDiscriminatorFormat.Wrapper:
					writer.WriteStartObject();
					writer.WriteString("type", typeName);
					writer.WritePropertyName("value");
					System.Text.Json.JsonSerializer.Serialize(writer, value, concreteType, options);
					writer.WriteEndObject();
					break;
				default:
					break;
			}
		}
	}
}
