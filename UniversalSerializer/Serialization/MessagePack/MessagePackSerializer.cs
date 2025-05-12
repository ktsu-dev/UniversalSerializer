// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using System.Buffers;
using MessagePack.Resolvers;
using ktsu.UniversalSerializer.Serialization.TypeRegistry;

namespace ktsu.UniversalSerializer.Serialization.MessagePack;

/// <summary>
/// Serializer for MessagePack format using MessagePack-CSharp.
/// </summary>
public class MessagePackSerializer : SerializerBase
{
    private readonly global::MessagePack.MessagePackSerializerOptions _messagePackOptions;
    private readonly TypeRegistry.TypeRegistry? _typeRegistry;
    private readonly bool _enableTypeDiscriminator;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessagePackSerializer"/> class with default options.
    /// </summary>
    public MessagePackSerializer() : this(SerializerOptions.Default())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessagePackSerializer"/> class with the specified options.
    /// </summary>
    /// <param name="options">The serializer options.</param>
    /// <param name="typeRegistry">Optional registry for polymorphic types.</param>
    public MessagePackSerializer(SerializerOptions options, TypeRegistry.TypeRegistry? typeRegistry = null) : base(options)
    {
        _typeRegistry = typeRegistry;
        _enableTypeDiscriminator = GetOption(SerializerOptionKeys.TypeRegistry.EnableTypeDiscriminator, false);

        // Configure the MessagePack resolver
        var resolver = StandardResolver.Instance;

        // Support for type discrimination if needed
        if (_enableTypeDiscriminator && _typeRegistry != null)
        {
            // Create a composite resolver for polymorphic serialization
            resolver = CompositeResolver.Create(
                new global::MessagePack.Formatters.IMessagePackFormatter[] { },
                new global::MessagePack.IFormatterResolver[] { StandardResolver.Instance }
            );
        }

        // Create MessagePack options
        _messagePackOptions = global::MessagePack.MessagePackSerializerOptions.Standard
            .WithResolver(resolver);

        // Configure LZ4 compression if enabled
        if (GetOption(SerializerOptionKeys.MessagePack.EnableLz4Compression, false))
        {
            _messagePackOptions = _messagePackOptions.WithCompression(global::MessagePack.MessagePackCompression.Lz4Block);
        }
    }

    /// <inheritdoc/>
    public override string ContentType => "application/x-msgpack";

    /// <inheritdoc/>
    public override string FileExtension => ".msgpack";

    /// <summary>
    /// Gets all supported file extensions for MessagePack format.
    /// </summary>
    /// <returns>An array of supported file extensions.</returns>
    public string[] GetSupportedExtensions() => [".msgpack", ".mp"];

    /// <inheritdoc/>
    public override string Serialize<T>(T obj)
    {
        if (obj == null)
        {
            return string.Empty;
        }

        byte[] bytes = SerializeToBytes(obj);
        return Convert.ToBase64String(bytes);
    }

    /// <inheritdoc/>
    public override string Serialize(object obj, Type type)
    {
        if (obj == null)
        {
            return string.Empty;
        }

        byte[] bytes = SerializeToBytes(obj, type);
        return Convert.ToBase64String(bytes);
    }

    /// <inheritdoc/>
    public override T Deserialize<T>(string serialized)
    {
        if (string.IsNullOrWhiteSpace(serialized))
        {
            return default!;
        }

        byte[] bytes = Convert.FromBase64String(serialized);
        return DeserializeFromBytes<T>(bytes);
    }

    /// <inheritdoc/>
    public override object Deserialize(string serialized, Type type)
    {
        if (string.IsNullOrWhiteSpace(serialized))
        {
            return null!;
        }

        byte[] bytes = Convert.FromBase64String(serialized);
        return DeserializeFromBytes(bytes, type);
    }

    /// <inheritdoc/>
    public override byte[] SerializeToBytes<T>(T obj)
    {
        if (obj == null)
        {
            return Array.Empty<byte>();
        }

        return global::MessagePack.MessagePackSerializer.Serialize(obj, _messagePackOptions);
    }

    /// <inheritdoc/>
    public override byte[] SerializeToBytes(object obj, Type type)
    {
        if (obj == null)
        {
            return Array.Empty<byte>();
        }

        return global::MessagePack.MessagePackSerializer.Serialize(type, obj, _messagePackOptions);
    }

    /// <inheritdoc/>
    public override T DeserializeFromBytes<T>(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
        {
            return default!;
        }

        return global::MessagePack.MessagePackSerializer.Deserialize<T>(bytes, _messagePackOptions);
    }

    /// <inheritdoc/>
    public override object DeserializeFromBytes(byte[] bytes, Type type)
    {
        if (bytes == null || bytes.Length == 0)
        {
            return null!;
        }

        return global::MessagePack.MessagePackSerializer.Deserialize(type, bytes, _messagePackOptions);
    }

    /// <inheritdoc/>
    public override async Task<string> SerializeAsync<T>(T obj, CancellationToken cancellationToken = default)
    {
        if (obj == null)
        {
            return string.Empty;
        }

        byte[] bytes = await SerializeToBytesAsync(obj, cancellationToken);
        return Convert.ToBase64String(bytes);
    }

    /// <inheritdoc/>
    public override async Task<byte[]> SerializeToBytesAsync<T>(T obj, CancellationToken cancellationToken = default)
    {
        if (obj == null)
        {
            return Array.Empty<byte>();
        }

        using var buffer = new ArrayBufferWriter<byte>();
        await global::MessagePack.MessagePackSerializer.SerializeAsync(buffer, obj, _messagePackOptions, cancellationToken);
        return buffer.WrittenSpan.ToArray();
    }

    /// <inheritdoc/>
    public override async Task<T> DeserializeAsync<T>(string serialized, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(serialized))
        {
            return default!;
        }

        byte[] bytes = Convert.FromBase64String(serialized);
        return await DeserializeFromBytesAsync<T>(bytes, cancellationToken);
    }

    /// <inheritdoc/>
    public override async Task<T> DeserializeFromBytesAsync<T>(byte[] bytes, CancellationToken cancellationToken = default)
    {
        if (bytes == null || bytes.Length == 0)
        {
            return default!;
        }

        // Use Memory<byte> to avoid copying the array
        var memory = new ReadOnlyMemory<byte>(bytes);
        return await global::MessagePack.MessagePackSerializer.DeserializeAsync<T>(memory, _messagePackOptions, cancellationToken);
    }
}
