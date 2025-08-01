// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Json;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using ktsu.UniversalSerializer.TypeConverter;

/// <summary>
/// JSON converter that uses the TypeConverterRegistry to convert types to and from strings.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="JsonStringTypeConverter"/> class.
/// </remarks>
/// <param name="typeConverterRegistry">The type converter registry.</param>
public class JsonStringTypeConverter(TypeConverterRegistry typeConverterRegistry) : JsonConverterFactory
{
	private readonly TypeConverterRegistry _typeConverterRegistry = typeConverterRegistry ?? throw new ArgumentNullException(nameof(typeConverterRegistry));

	/// <inheritdoc/>
	public override bool CanConvert(Type typeToConvert) => _typeConverterRegistry.HasConverter(typeToConvert);

	/// <inheritdoc/>
	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		Type converterType = typeof(JsonStringTypeConverterInner<>).MakeGenericType(typeToConvert);
		return (JsonConverter)Activator.CreateInstance(
			converterType,
			[_typeConverterRegistry])!;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via reflection")]
	private sealed class JsonStringTypeConverterInner<T>(TypeConverterRegistry typeConverterRegistry) : JsonConverter<T>
	{
		private readonly TypeConverterRegistry _typeConverterRegistry = typeConverterRegistry;

		public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
			{
				throw new JsonException($"Cannot convert {reader.TokenType} to {typeToConvert}. Expected string.");
			}

			string? stringValue = reader.GetString();
			return stringValue == null
				? throw new JsonException($"Cannot convert null to {typeToConvert}.")
				: (T)_typeConverterRegistry.ConvertFromString(stringValue, typeToConvert);
		}

		public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			string stringValue = _typeConverterRegistry.ConvertToString(value);
			writer.WriteStringValue(stringValue);
		}
	}
}
