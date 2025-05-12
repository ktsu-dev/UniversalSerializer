// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using System.Text.Json;
using System.Text.Json.Serialization;
using ktsu.UniversalSerializer.Serialization.TypeConverter;

namespace ktsu.UniversalSerializer.Serialization.Json;

/// <summary>
/// JSON converter that uses the TypeConverterRegistry to convert types to and from strings.
/// </summary>
public class JsonStringTypeConverter : JsonConverterFactory
{
    private readonly TypeConverterRegistry _typeConverterRegistry;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonStringTypeConverter"/> class.
    /// </summary>
    /// <param name="typeConverterRegistry">The type converter registry.</param>
    public JsonStringTypeConverter(TypeConverterRegistry typeConverterRegistry)
    {
        _typeConverterRegistry = typeConverterRegistry ?? throw new ArgumentNullException(nameof(typeConverterRegistry));
    }

    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
    {
        return _typeConverterRegistry.HasConverter(typeToConvert);
    }

    /// <inheritdoc/>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(JsonStringTypeConverterInner<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(
            converterType,
            new object[] { _typeConverterRegistry })!;
    }

    private class JsonStringTypeConverterInner<T> : JsonConverter<T>
    {
        private readonly TypeConverterRegistry _typeConverterRegistry;

        public JsonStringTypeConverterInner(TypeConverterRegistry typeConverterRegistry)
        {
            _typeConverterRegistry = typeConverterRegistry;
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException($"Cannot convert {reader.TokenType} to {typeToConvert}. Expected string.");
            }

            var stringValue = reader.GetString();
            if (stringValue == null)
            {
                throw new JsonException($"Cannot convert null to {typeToConvert}.");
            }

            return (T)_typeConverterRegistry.ConvertFromString(stringValue, typeToConvert);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            var stringValue = _typeConverterRegistry.ConvertToString(value);
            writer.WriteStringValue(stringValue);
        }
    }
}
