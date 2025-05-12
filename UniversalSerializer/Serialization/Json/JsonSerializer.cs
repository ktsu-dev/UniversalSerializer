// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace ktsu.UniversalSerializer.Serialization.Json;

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
    public JsonSerializer(SerializerOptions options) : base(options)
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
            var policy = GetOption<string>(SerializerOptionKeys.Json.PropertyNamingPolicy, null);
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
    }

    /// <inheritdoc/>
    public override string ContentType => "application/json";

    /// <inheritdoc/>
    public override string FileExtension => ".json";

    /// <inheritdoc/>
    public override string Serialize<T>(T obj)
    {
        return System.Text.Json.JsonSerializer.Serialize(obj, _jsonOptions);
    }

    /// <inheritdoc/>
    public override string Serialize(object obj, Type type)
    {
        return System.Text.Json.JsonSerializer.Serialize(obj, type, _jsonOptions);
    }

    /// <inheritdoc/>
    public override T Deserialize<T>(string serialized)
    {
        return System.Text.Json.JsonSerializer.Deserialize<T>(serialized, _jsonOptions)!;
    }

    /// <inheritdoc/>
    public override object Deserialize(string serialized, Type type)
    {
        return System.Text.Json.JsonSerializer.Deserialize(serialized, type, _jsonOptions)!;
    }

    /// <inheritdoc/>
    public override async Task<string> SerializeAsync<T>(T obj, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream();
        await System.Text.Json.JsonSerializer.SerializeAsync(stream, obj, _jsonOptions, cancellationToken);
        stream.Position = 0;
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    /// <inheritdoc/>
    public override async Task<T> DeserializeAsync<T>(string serialized, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(serialized));
        return await System.Text.Json.JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions, cancellationToken)!;
    }
}
