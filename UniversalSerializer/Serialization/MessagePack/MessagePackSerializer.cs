// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Serialization.MessagePack;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using global::MessagePack;
using global::MessagePack.Resolvers;
using ktsu.UniversalSerializer.Serialization.TypeRegistry;

/// <summary>
/// Serializer for MessagePack format.
/// </summary>
public class MessagePackSerializer : SerializerBase
{
	private readonly MessagePackSerializerOptions _options;

	/// <summary>
	/// Initializes a new instance of the <see cref="MessagePackSerializer"/> class.
	/// </summary>
	public MessagePackSerializer() : this(SerializerOptions.Default())
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="MessagePackSerializer"/> class.
	/// </summary>
	/// <param name="options">The serializer options.</param>
	/// <param name="typeRegistry">The type registry for polymorphic serialization.</param>
#pragma warning disable IDE0060 // Remove unused parameter
	public MessagePackSerializer(SerializerOptions options, TypeRegistry? typeRegistry = null) : base(options)
#pragma warning restore IDE0060 // Remove unused parameter
	{
		// Create MessagePack options
		StandardResolver resolver = StandardResolver.Instance;
		_options = MessagePackSerializerOptions.Standard
			.WithResolver(resolver)
			.WithSecurity(MessagePackSecurity.UntrustedData);
	}

	/// <inheritdoc/>
	public override string ContentType => "application/x-msgpack";

	/// <inheritdoc/>
	public override string FileExtension => ".msgpack";

	/// <summary>
	/// Gets all supported file extensions for MessagePack format.
	/// </summary>
	/// <returns>An array of supported file extensions.</returns>
	public static string[] GetSupportedExtensions() => [".msgpack", ".mp"];

	/// <inheritdoc/>
	public override string Serialize<T>(T obj)
	{
		byte[] bytes = global::MessagePack.MessagePackSerializer.Serialize(obj, _options);
		return Convert.ToBase64String(bytes);
	}

	/// <inheritdoc/>
	public override string Serialize(object obj, Type type)
	{
		byte[] bytes = global::MessagePack.MessagePackSerializer.Serialize(type, obj, _options);
		return Convert.ToBase64String(bytes);
	}

	/// <inheritdoc/>
	public override T Deserialize<T>(string serialized)
	{
		byte[] bytes = Convert.FromBase64String(serialized);
		return global::MessagePack.MessagePackSerializer.Deserialize<T>(bytes, _options);
	}

	/// <inheritdoc/>
	public override object Deserialize(string serialized, Type type)
	{
		byte[] bytes = Convert.FromBase64String(serialized);
		return global::MessagePack.MessagePackSerializer.Deserialize(type, bytes, _options) ?? throw new InvalidOperationException("Deserialization returned null");
	}

	/// <inheritdoc/>
	public override async Task<string> SerializeAsync<T>(T obj, CancellationToken cancellationToken = default)
	{
		using MemoryStream stream = new();
		await global::MessagePack.MessagePackSerializer.SerializeAsync(stream, obj, _options, cancellationToken).ConfigureAwait(false);
		return Convert.ToBase64String(stream.ToArray());
	}

	/// <inheritdoc/>
	public override async Task<T> DeserializeAsync<T>(string serialized, CancellationToken cancellationToken = default)
	{
		byte[] bytes = Convert.FromBase64String(serialized);
		using MemoryStream stream = new(bytes);
		return await global::MessagePack.MessagePackSerializer.DeserializeAsync<T>(stream, _options, cancellationToken).ConfigureAwait(false);
	}
}
