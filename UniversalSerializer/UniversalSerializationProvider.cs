// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Services;

using System;
using System.Threading;
using System.Threading.Tasks;
using ktsu.SerializationProvider;
using ktsu.UniversalSerializer.Contracts;

/// <summary>
/// Universal Serialization Provider that implements ISerializationProvider interface
/// for dependency injection scenarios. Uses a configurable serializer internally.
/// </summary>
public class UniversalSerializationProvider : ISerializationProvider
{
	private readonly ISerializer _serializer;

	/// <summary>
	/// Initializes a new instance of the <see cref="UniversalSerializationProvider"/> class
	/// with the specified serializer.
	/// </summary>
	/// <param name="serializer">The underlying serializer to use.</param>
	/// <param name="providerName">Optional custom provider name. If null, uses the serializer type name.</param>
	public UniversalSerializationProvider(ISerializer serializer, string? providerName = null)
	{
		_serializer = Ensure.NotNull(serializer);
		ProviderName = providerName ?? $"UniversalSerializer.{_serializer.GetType().Name}";
	}

	/// <inheritdoc/>
	public string ProviderName { get; }

	/// <inheritdoc/>
	public string ContentType => _serializer.ContentType;

	/// <inheritdoc/>
	public string Serialize<T>(T obj)
	{
#pragma warning disable KTSU0003 // Ensure.NotNull requires class constraint, but T is unconstrained
		ArgumentNullException.ThrowIfNull(obj);
#pragma warning restore KTSU0003
		return _serializer.Serialize(obj);
	}

	/// <inheritdoc/>
	public string Serialize(object obj, Type type)
	{
		Ensure.NotNull(obj);
		Ensure.NotNull(type);
		return _serializer.Serialize(obj, type);
	}

	/// <inheritdoc/>
	public T Deserialize<T>(string data)
	{
		if (string.IsNullOrWhiteSpace(data))
		{
			throw new ArgumentException("Data cannot be null or empty", nameof(data));
		}

		return _serializer.Deserialize<T>(data);
	}

	/// <inheritdoc/>
	public object Deserialize(string data, Type type)
	{
		if (string.IsNullOrWhiteSpace(data))
		{
			throw new ArgumentException("Data cannot be null or empty", nameof(data));
		}

		Ensure.NotNull(type);

		return _serializer.Deserialize(data, type);
	}

	/// <inheritdoc/>
	public async Task<string> SerializeAsync<T>(T obj, CancellationToken cancellationToken = default)
	{
#pragma warning disable KTSU0003 // Ensure.NotNull requires class constraint, but T is unconstrained
		ArgumentNullException.ThrowIfNull(obj);
#pragma warning restore KTSU0003
		return await _serializer.SerializeAsync(obj, cancellationToken).ConfigureAwait(false);
	}

	/// <inheritdoc/>
	public async Task<string> SerializeAsync(object obj, Type type, CancellationToken cancellationToken = default)
	{
		Ensure.NotNull(obj);
		Ensure.NotNull(type);

		// UniversalSerializer doesn't have async overload for non-generic serialize
		// We'll use Task.Run to avoid blocking the thread
		return await Task.Run(() => _serializer.Serialize(obj, type), cancellationToken).ConfigureAwait(false);
	}

	/// <inheritdoc/>
	public async Task<T> DeserializeAsync<T>(string data, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(data))
		{
			throw new ArgumentException("Data cannot be null or empty", nameof(data));
		}

		return await _serializer.DeserializeAsync<T>(data, cancellationToken).ConfigureAwait(false);
	}

	/// <inheritdoc/>
	public async Task<object> DeserializeAsync(string data, Type type, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(data))
		{
			throw new ArgumentException("Data cannot be null or empty", nameof(data));
		}

		Ensure.NotNull(type);

		// UniversalSerializer doesn't have async overload for non-generic deserialize
		// We'll use Task.Run to avoid blocking the thread
		return await Task.Run(() => _serializer.Deserialize(data, type), cancellationToken).ConfigureAwait(false);
	}
}
