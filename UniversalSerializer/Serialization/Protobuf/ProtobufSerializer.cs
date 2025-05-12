// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using System.Buffers;
using ktsu.UniversalSerializer.Serialization.TypeRegistry;

namespace ktsu.UniversalSerializer.Serialization.Protobuf;

/// <summary>
/// Serializer for Protocol Buffers format using protobuf-net.
/// </summary>
public class ProtobufSerializer : SerializerBase
{
    private readonly global::ProtoBuf.Meta.RuntimeTypeModel _typeModel;
    private readonly TypeRegistry.TypeRegistry? _typeRegistry;
    private readonly bool _enableTypeDiscriminator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProtobufSerializer"/> class with default options.
    /// </summary>
    public ProtobufSerializer()
        : this(SerializerOptions.Default())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProtobufSerializer"/> class with the specified options.
    /// </summary>
    /// <param name="options">The serializer options.</param>
    public ProtobufSerializer(SerializerOptions options)
        : this(options, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProtobufSerializer"/> class with the specified options and type registry.
    /// </summary>
    /// <param name="options">The serializer options.</param>
    /// <param name="typeRegistry">The type registry for polymorphic serialization.</param>
    public ProtobufSerializer(SerializerOptions options, TypeRegistry.TypeRegistry? typeRegistry)
        : base(options)
    {
        _typeRegistry = typeRegistry;
        _typeModel = global::ProtoBuf.Meta.RuntimeTypeModel.Create();
        _enableTypeDiscriminator = options.GetOption(SerializerOptionKeys.TypeRegistry.EnableTypeDiscriminator, false);

        // Configure Protobuf options
        ConfigureTypeModel();
    }

    /// <inheritdoc />
    public override string FileExtension => "proto";

    /// <inheritdoc />
    public override string ContentType => "application/x-protobuf";

    /// <summary>
    /// Gets a list of all supported file extensions for the Protobuf serializer.
    /// </summary>
    /// <returns>A collection of supported file extensions.</returns>
    public IEnumerable<string> GetSupportedExtensions()
    {
        yield return ".proto";
        yield return ".pb";
        yield return ".bin";
    }

    /// <inheritdoc />
    public override bool CanSerialize<T>()
    {
        try
        {
            return _typeModel.CanSerialize(typeof(T));
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public override string Serialize<T>(T value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        using var stream = new MemoryStream();
        SerializeToStream(value, stream);

        // Convert to Base64 string for returning as text
        return Convert.ToBase64String(stream.ToArray());
    }

    /// <inheritdoc />
    public override T? Deserialize<T>(string serializedValue)
    {
        if (string.IsNullOrEmpty(serializedValue))
        {
            return default;
        }

        try
        {
            // Convert from Base64 string
            byte[] data = Convert.FromBase64String(serializedValue);
            using var stream = new MemoryStream(data);
            return DeserializeFromStream<T>(stream);
        }
        catch (Exception ex)
        {
            throw new SerializationException($"Error deserializing {typeof(T).Name} from Protobuf data: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public override byte[] SerializeToBytes<T>(T value)
    {
        if (value == null)
        {
            return Array.Empty<byte>();
        }

        using var stream = new MemoryStream();
        SerializeToStream(value, stream);
        return stream.ToArray();
    }

    /// <inheritdoc />
    public override T? DeserializeFromBytes<T>(byte[] data)
    {
        if (data == null || data.Length == 0)
        {
            return default;
        }

        try
        {
            using var stream = new MemoryStream(data);
            return DeserializeFromStream<T>(stream);
        }
        catch (Exception ex)
        {
            throw new SerializationException($"Error deserializing {typeof(T).Name} from Protobuf bytes: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public override void SerializeToStream<T>(T value, Stream stream)
    {
        if (value == null || stream == null)
        {
            return;
        }

        try
        {
            // Handle polymorphic serialization through type registry if enabled
            if (_enableTypeDiscriminator && _typeRegistry != null && value.GetType() != typeof(T))
            {
                string typeKey = _typeRegistry.GetTypeKey(value.GetType());
                // Register the type if needed
                if (!_typeModel.CanSerialize(value.GetType()))
                {
                    _typeModel.Add(value.GetType(), true);
                }
            }

            _typeModel.Serialize(stream, value);
        }
        catch (Exception ex)
        {
            throw new SerializationException($"Error serializing {typeof(T).Name} to Protobuf: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public override T? DeserializeFromStream<T>(Stream stream)
    {
        if (stream == null || stream.Length == 0)
        {
            return default;
        }

        try
        {
            return (T?)_typeModel.Deserialize(stream, null, typeof(T));
        }
        catch (Exception ex)
        {
            throw new SerializationException($"Error deserializing {typeof(T).Name} from Protobuf stream: {ex.Message}", ex);
        }
    }

    private void ConfigureTypeModel()
    {
        // Configure protobuf-net type model based on options
        bool skipConstructor = Options.GetOption(SerializerOptionKeys.Protobuf.SkipConstructor, false);
        bool omitAssemblyVersion = Options.GetOption(SerializerOptionKeys.Protobuf.OmitAssemblyVersion, true);
        bool allowPrivateMembers = Options.GetOption(SerializerOptionKeys.Protobuf.AllowPrivateMembers, false);
        bool useCompatibilityMode = Options.GetOption(SerializerOptionKeys.Protobuf.UseCompatibilityMode, false);

        // Apply configuration to the type model
        _typeModel.AutoCompile = true;
        _typeModel.UseImplicitZeroDefaults = true;

        if (skipConstructor)
        {
            _typeModel.AutoAddMissingTypes = true;
        }

        if (_enableTypeDiscriminator && _typeRegistry != null)
        {
            RegisterTypesFromRegistry();
        }
    }

    private void RegisterTypesFromRegistry()
    {
        if (_typeRegistry == null)
        {
            return;
        }

        foreach (var type in _typeRegistry.GetRegisteredTypes())
        {
            if (!_typeModel.CanSerialize(type.Type))
            {
                _typeModel.Add(type.Type, true);
            }
        }
    }
}
