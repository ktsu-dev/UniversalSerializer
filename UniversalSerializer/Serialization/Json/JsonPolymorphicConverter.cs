// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using System.Text.Json;
using System.Text.Json.Serialization;
using ktsu.UniversalSerializer.Serialization.TypeRegistry;

namespace ktsu.UniversalSerializer.Serialization.Json;

/// <summary>
/// JSON converter factory for polymorphic type handling.
/// </summary>
public class JsonPolymorphicConverter : JsonConverterFactory
{
    private readonly TypeRegistry.TypeRegistry _typeRegistry;
    private readonly SerializerOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonPolymorphicConverter"/> class.
    /// </summary>
    /// <param name="typeRegistry">The type registry.</param>
    /// <param name="options">The serializer options.</param>
    public JsonPolymorphicConverter(TypeRegistry.TypeRegistry typeRegistry, SerializerOptions options)
    {
        _typeRegistry = typeRegistry ?? throw new ArgumentNullException(nameof(typeRegistry));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
    {
        // Can convert abstract classes and interfaces
        return typeToConvert.IsInterface || (typeToConvert.IsClass && typeToConvert.IsAbstract);
    }

    /// <inheritdoc/>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(JsonPolymorphicConverterInner<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(
            converterType,
            new object[] { _typeRegistry, _options })!;
    }

    private class JsonPolymorphicConverterInner<T> : JsonConverter<T>
    {
        private readonly TypeRegistry.TypeRegistry _typeRegistry;
        private readonly SerializerOptions _options;
        private readonly string _discriminatorProperty;
        private readonly TypeDiscriminatorFormat _discriminatorFormat;

        public JsonPolymorphicConverterInner(TypeRegistry.TypeRegistry typeRegistry, SerializerOptions options)
        {
            _typeRegistry = typeRegistry;
            _options = options;
            _discriminatorProperty = _options.GetOption(SerializerOptionKeys.TypeRegistry.TypeDiscriminatorPropertyName, "$type");

            var formatValue = _options.GetOption(SerializerOptionKeys.TypeRegistry.TypeDiscriminatorFormat,
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
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected start of object");
            }

            using var jsonDocument = JsonDocument.ParseValue(ref reader);
            var rootElement = jsonDocument.RootElement;

            // Extract type information based on the discriminator format
            string? typeName = null;

            switch (_discriminatorFormat)
            {
                case TypeDiscriminatorFormat.Property:
                    if (rootElement.TryGetProperty(_discriminatorProperty, out var typeProperty))
                    {
                        typeName = typeProperty.GetString();
                    }
                    break;

                case TypeDiscriminatorFormat.TypeProperty:
                    // Not implemented yet
                    throw new NotImplementedException("TypeProperty discriminator format not implemented yet");

                case TypeDiscriminatorFormat.Wrapper:
                    if (rootElement.TryGetProperty("type", out var wrapperTypeProperty) &&
                        rootElement.TryGetProperty("value", out var valueProperty))
                    {
                        typeName = wrapperTypeProperty.GetString();
                        // Re-use valueProperty for deserialization below
                        rootElement = valueProperty;
                    }
                    break;
            }

            if (string.IsNullOrEmpty(typeName))
            {
                throw new JsonException($"Could not find type discriminator '{_discriminatorProperty}' in JSON");
            }

            // Resolve the type
            var type = _typeRegistry.ResolveType(typeName);
            if (type == null)
            {
                throw new JsonException($"Could not resolve type '{typeName}'");
            }

            // Verify the type is assignable to the expected type
            if (!typeof(T).IsAssignableFrom(type))
            {
                throw new JsonException($"Type '{type.FullName}' is not assignable to '{typeof(T).FullName}'");
            }

            // Deserialize to the concrete type
            var json = rootElement.GetRawText();
            return (T)JsonSerializer.Deserialize(json, type, options)!;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            var concreteType = value.GetType();

            // Skip discriminator if the concrete type is the exact type requested
            if (concreteType == typeof(T))
            {
                JsonSerializer.Serialize(writer, value, concreteType, options);
                return;
            }

            var typeName = _typeRegistry.GetTypeName(concreteType);

            switch (_discriminatorFormat)
            {
                case TypeDiscriminatorFormat.Property:
                    // Serialize the value to a document
                    var valueJson = JsonSerializer.Serialize(value, concreteType, options);

                    // Parse the document to add the type discriminator
                    using (var document = JsonDocument.Parse(valueJson))
                    {
                        writer.WriteStartObject();

                        // Write the type discriminator property
                        writer.WriteString(_discriminatorProperty, typeName);

                        // Copy all properties from the original object
                        foreach (var property in document.RootElement.EnumerateObject())
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
                    JsonSerializer.Serialize(writer, value, concreteType, options);
                    writer.WriteEndObject();
                    break;
            }
        }
    }
}
