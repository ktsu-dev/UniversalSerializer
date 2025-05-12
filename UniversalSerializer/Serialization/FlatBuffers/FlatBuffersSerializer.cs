// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using System.Buffers;
using FlatSharp;
using ktsu.UniversalSerializer.Serialization.TypeRegistry;

namespace ktsu.UniversalSerializer.Serialization.FlatBuffers;

/// <summary>
/// Serializer for FlatBuffers format using FlatSharp.
/// </summary>
public class FlatBuffersSerializer : SerializerBase
{
    private readonly TypeRegistry.TypeRegistry? _typeRegistry;
    private readonly bool _enableTypeDiscriminator;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlatBuffersSerializer"/> class with default options.
    /// </summary>
    public FlatBuffersSerializer()
        : this(SerializerOptions.Default())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FlatBuffersSerializer"/> class with the specified options.
    /// </summary>
    /// <param name="options">The serializer options.</param>
    /// <param name="typeRegistry">Optional type registry for polymorphic serialization.</param>
    public FlatBuffersSerializer(SerializerOptions options, TypeRegistry.TypeRegistry? typeRegistry = null)
        : base(options)
    {
        _typeRegistry = typeRegistry;
        _enableTypeDiscriminator = options.GetValueOrDefault<bool>(SerializerOptionKeys.FlatBuffers.EnableTypeDiscriminator, false);
    }

    /// <inheritdoc />
    public override bool CanSerialize(Type type)
    {
        return type.GetCustomAttributes(typeof(FlatBufferTableAttribute), true).Length > 0
            || type.GetCustomAttributes(typeof(FlatBufferStructAttribute), true).Length > 0
            || (_typeRegistry != null && _enableTypeDiscriminator && _typeRegistry.IsTypeRegistered(type));
    }

    /// <inheritdoc />
    public override byte[] SerializeToBytes<T>(T obj)
    {
        if (obj == null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        var type = obj.GetType();

        // Find the appropriate serializer for the type
        var maxSize = GetMaxSize(obj);
        var buffer = new byte[maxSize];
        var bytesWritten = SerializeToBuffer(obj, buffer);

        if (bytesWritten < buffer.Length)
        {
            // If we didn't use the entire buffer, create a new one with just the data
            var result = new byte[bytesWritten];
            Buffer.BlockCopy(buffer, 0, result, 0, bytesWritten);
            return result;
        }

        return buffer;
    }

    /// <inheritdoc />
    public override int SerializeToBuffer<T>(T obj, byte[] buffer, int offset = 0, int? count = null)
    {
        if (obj == null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        int maxLength = count ?? buffer.Length - offset;
        var memory = new Memory<byte>(buffer, offset, maxLength);

        // Use the appropriate FlatSharp serializer
        Type objType = obj.GetType();

        try
        {
            // Use reflection to get the Serializer property from the type
            var serializerProperty = objType.GetProperty("Serializer");
            if (serializerProperty != null)
            {
                var serializer = serializerProperty.GetValue(null);
                if (serializer != null)
                {
                    // Find and call the Write method on the serializer
                    var writeMethod = serializer.GetType().GetMethod("Write", new[] { typeof(Span<byte>), objType });
                    if (writeMethod != null)
                    {
                        var span = memory.Span;
                        var result = writeMethod.Invoke(serializer, new object[] { span, obj });
                        if (result != null)
                        {
                            return (int)result;
                        }
                    }
                }
            }

            throw new SerializationException($"Type {objType.Name} does not appear to be a valid FlatSharp type with a Serializer property.");
        }
        catch (Exception ex)
        {
            throw new SerializationException($"Failed to serialize object of type {objType.Name} to FlatBuffers: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public override T DeserializeFromBytes<T>(byte[] data, int offset = 0, int? count = null)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        int length = count ?? data.Length - offset;
        var memory = new ReadOnlyMemory<byte>(data, offset, length);

        try
        {
            // Use reflection to get the Serializer property from the type
            var serializerProperty = typeof(T).GetProperty("Serializer");
            if (serializerProperty != null)
            {
                var serializer = serializerProperty.GetValue(null);
                if (serializer != null)
                {
                    // Find and call the Parse method on the serializer
                    var parseMethod = serializer.GetType().GetMethod("Parse", new[] { typeof(ReadOnlyMemory<byte>) });
                    if (parseMethod != null)
                    {
                        var result = parseMethod.Invoke(serializer, new object[] { memory });
                        if (result != null && result is T typedResult)
                        {
                            return typedResult;
                        }
                    }
                }
            }

            throw new SerializationException($"Type {typeof(T).Name} does not appear to be a valid FlatSharp type with a Serializer property.");
        }
        catch (Exception ex)
        {
            throw new SerializationException($"Failed to deserialize data to type {typeof(T).Name} from FlatBuffers: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public override int GetMaxSize<T>(T obj)
    {
        if (obj == null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        var type = obj.GetType();

        try
        {
            // Use reflection to get the Serializer property from the type
            var serializerProperty = type.GetProperty("Serializer");
            if (serializerProperty != null)
            {
                var serializer = serializerProperty.GetValue(null);
                if (serializer != null)
                {
                    // Find and call the GetMaxSize method on the serializer
                    var maxSizeMethod = serializer.GetType().GetMethod("GetMaxSize", new[] { type });
                    if (maxSizeMethod != null)
                    {
                        var result = maxSizeMethod.Invoke(serializer, new object[] { obj });
                        if (result != null)
                        {
                            return (int)result;
                        }
                    }
                }
            }

            throw new SerializationException($"Type {type.Name} does not appear to be a valid FlatSharp type with a Serializer property.");
        }
        catch (Exception ex)
        {
            throw new SerializationException($"Failed to calculate max size for type {type.Name}: {ex.Message}", ex);
        }
    }
}
