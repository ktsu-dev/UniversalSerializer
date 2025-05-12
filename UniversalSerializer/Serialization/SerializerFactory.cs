// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using System.Collections.Concurrent;

namespace ktsu.UniversalSerializer.Serialization;

/// <summary>
/// Factory for creating serializer instances.
/// </summary>
public class SerializerFactory
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
        _serializerCreators[typeof(TSerializer)] = options => creator(options);
        return this;
    }

    /// <summary>
    /// Creates a new instance of a serializer.
    /// </summary>
    /// <typeparam name="TSerializer">The type of serializer to create.</typeparam>
    /// <returns>A new instance of the specified serializer type.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the serializer type has not been registered.</exception>
    public TSerializer Create<TSerializer>() where TSerializer : ISerializer
    {
        return Create<TSerializer>(_defaultOptions);
    }

    /// <summary>
    /// Creates a new instance of a serializer with the specified options.
    /// </summary>
    /// <typeparam name="TSerializer">The type of serializer to create.</typeparam>
    /// <param name="options">The options to use when creating the serializer.</param>
    /// <returns>A new instance of the specified serializer type.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the serializer type has not been registered.</exception>
    public TSerializer Create<TSerializer>(SerializerOptions options) where TSerializer : ISerializer
    {
        var serializerType = typeof(TSerializer);

        if (!_serializerCreators.TryGetValue(serializerType, out var creator))
        {
            throw new InvalidOperationException($"No creator registered for serializer type {serializerType.Name}.");
        }

        return (TSerializer)creator(options ?? _defaultOptions);
    }

    /// <summary>
    /// Creates a new instance of a serializer by type.
    /// </summary>
    /// <param name="serializerType">The type of serializer to create.</param>
    /// <returns>A new instance of the specified serializer type.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the serializer type has not been registered.</exception>
    public ISerializer Create(Type serializerType)
    {
        return Create(serializerType, _defaultOptions);
    }

    /// <summary>
    /// Creates a new instance of a serializer by type with the specified options.
    /// </summary>
    /// <param name="serializerType">The type of serializer to create.</param>
    /// <param name="options">The options to use when creating the serializer.</param>
    /// <returns>A new instance of the specified serializer type.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the serializer type has not been registered.</exception>
    public ISerializer Create(Type serializerType, SerializerOptions options)
    {
        if (!typeof(ISerializer).IsAssignableFrom(serializerType))
        {
            throw new ArgumentException($"Type {serializerType.Name} does not implement ISerializer.", nameof(serializerType));
        }

        if (!_serializerCreators.TryGetValue(serializerType, out var creator))
        {
            throw new InvalidOperationException($"No creator registered for serializer type {serializerType.Name}.");
        }

        return creator(options ?? _defaultOptions);
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
    public SerializerOptions GetDefaultOptions()
    {
        return _defaultOptions.Clone();
    }
}
