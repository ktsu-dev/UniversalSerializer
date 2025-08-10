// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer;
using System;
using System.Collections.Concurrent;

/// <summary>
/// Factory for creating serializer instances.
/// </summary>
public class SerializerFactory : ISerializerFactory
{
	private readonly ConcurrentDictionary<Type, Func<SerializerOptions, ISerializer>> _serializerCreators = new();
	private readonly SerializerOptions _defaultOptions = SerializerOptions.Default();

	/// <summary>
	/// Initializes a new instance of the <see cref="SerializerFactory"/> class.
	/// </summary>
	public SerializerFactory()
	{
	}

	/// <summary>
	/// Registers a serializer creator function for a specific serializer type.
	/// </summary>
	/// <typeparam name="TSerializer">The type of serializer to register.</typeparam>
	/// <param name="creator">The function to create a new instance of the serializer.</param>
	/// <returns>The current factory instance for method chaining.</returns>
	public SerializerFactory RegisterSerializer<TSerializer>(Func<SerializerOptions, TSerializer> creator) where TSerializer : ISerializer
	{
		ArgumentNullException.ThrowIfNull(creator);
		_serializerCreators[typeof(TSerializer)] = options => creator(options);
		return this;
	}

	/// <summary>
	/// Creates a new instance of a serializer.
	/// </summary>
	/// <typeparam name="TSerializer">The type of serializer to create.</typeparam>
	/// <returns>A new instance of the specified serializer type.</returns>
	/// <exception cref="InvalidOperationException">Thrown when the serializer type has not been registered.</exception>
	public TSerializer Create<TSerializer>() where TSerializer : ISerializer => Create<TSerializer>(_defaultOptions);

	/// <summary>
	/// Creates a new instance of a serializer with the specified options.
	/// </summary>
	/// <typeparam name="TSerializer">The type of serializer to create.</typeparam>
	/// <param name="options">The options to use when creating the serializer.</param>
	/// <returns>A new instance of the specified serializer type.</returns>
	/// <exception cref="InvalidOperationException">Thrown when the serializer type has not been registered.</exception>
	public TSerializer Create<TSerializer>(SerializerOptions options) where TSerializer : ISerializer
	{
		Type serializerType = typeof(TSerializer);

		if (!_serializerCreators.TryGetValue(serializerType, out Func<SerializerOptions, ISerializer>? creator))
		{
			throw new InvalidOperationException($"No creator registered for serializer type {serializerType.Name}.");
		}

		ISerializer? instance = creator(options ?? _defaultOptions);
		return instance is null
			? throw new InvalidOperationException($"Factory for {serializerType.Name} returned null.")
			: (TSerializer)instance;
	}

	/// <summary>
	/// Creates a new instance of a serializer by type.
	/// </summary>
	/// <param name="serializerType">The type of serializer to create.</param>
	/// <returns>A new instance of the specified serializer type.</returns>
	/// <exception cref="InvalidOperationException">Thrown when the serializer type has not been registered.</exception>
	public ISerializer Create(Type serializerType) => Create(serializerType, _defaultOptions);

	/// <summary>
	/// Creates a new instance of a serializer by type with the specified options.
	/// </summary>
	/// <param name="serializerType">The type of serializer to create.</param>
	/// <param name="options">The options to use when creating the serializer.</param>
	/// <returns>A new instance of the specified serializer type.</returns>
	/// <exception cref="InvalidOperationException">Thrown when the serializer type has not been registered.</exception>
	public ISerializer Create(Type serializerType, SerializerOptions options)
	{
		ArgumentNullException.ThrowIfNull(serializerType);
		return !typeof(ISerializer).IsAssignableFrom(serializerType)
			? throw new ArgumentException($"Type {serializerType.Name} does not implement ISerializer.", nameof(serializerType))
				: !_serializerCreators.TryGetValue(serializerType, out Func<SerializerOptions, ISerializer>? creator)
				? throw new InvalidOperationException($"No creator registered for serializer type {serializerType.Name}.")
				: creator(options ?? _defaultOptions) ?? throw new InvalidOperationException($"Factory for {serializerType.Name} returned null.");
	}

	/// <summary>
	/// Configures the default options for all serializers created by this factory.
	/// </summary>
	/// <param name="configure">The action to configure the options.</param>
	/// <returns>The current factory instance for method chaining.</returns>
	public SerializerFactory ConfigureDefaults(Action<SerializerOptions> configure)
	{
		configure?.Invoke(_defaultOptions);
		return this;
	}

	/// <summary>
	/// Gets a copy of the default options used by this factory.
	/// </summary>
	/// <returns>A copy of the default options.</returns>
	public SerializerOptions GetDefaultOptions() => _defaultOptions.Clone();

	/// <summary>
	/// Gets a serializer of the specified type.
	/// </summary>
	/// <typeparam name="TSerializer">The type of serializer to get.</typeparam>
	/// <returns>An instance of the specified serializer type.</returns>
	public TSerializer GetSerializer<TSerializer>() where TSerializer : ISerializer => Create<TSerializer>();
}
