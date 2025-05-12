// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using ktsu.UniversalSerializer.Serialization.Json;
using ktsu.UniversalSerializer.Serialization.Xml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ktsu.UniversalSerializer.Serialization.DependencyInjection;

/// <summary>
/// Provides extension methods for <see cref="IServiceCollection"/> to register serializers.
/// </summary>
public static class SerializerServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Universal Serializer factory and core services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional action to configure default options for all serializers.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddUniversalSerializer(
        this IServiceCollection services,
        Action<SerializerOptions>? configureOptions = null)
    {
        services.TryAddSingleton<SerializerFactory>(sp =>
        {
            var factory = new SerializerFactory();
            configureOptions?.Invoke(factory.GetDefaultOptions());
            return factory;
        });

        services.TryAddSingleton<SerializerRegistry>(sp =>
        {
            var factory = sp.GetRequiredService<SerializerFactory>();
            return new SerializerRegistry(factory);
        });

        return services;
    }

    /// <summary>
    /// Adds the JSON serializer to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional action to configure options specific to this serializer.</param>
    /// <param name="registerAsDefault">Whether to register the JSON serializer as the default ISerializer.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddJsonSerializer(
        this IServiceCollection services,
        Action<SerializerOptions>? configureOptions = null,
        bool registerAsDefault = true)
    {
        services.TryAddSingleton<JsonSerializer>(sp =>
        {
            var factory = sp.GetRequiredService<SerializerFactory>();
            var options = factory.GetDefaultOptions();
            configureOptions?.Invoke(options);

            factory.RegisterSerializer<JsonSerializer>(opts => new JsonSerializer(opts));
            var serializer = factory.Create<JsonSerializer>(options);

            // Register with the registry
            var registry = sp.GetRequiredService<SerializerRegistry>();
            registry.Register("json", serializer);

            return serializer;
        });

        if (registerAsDefault)
        {
            services.TryAddSingleton<ISerializer>(sp => sp.GetRequiredService<JsonSerializer>());
        }

        return services;
    }

    /// <summary>
    /// Adds the XML serializer to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional action to configure options specific to this serializer.</param>
    /// <param name="registerAsDefault">Whether to register the XML serializer as the default ISerializer.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddXmlSerializer(
        this IServiceCollection services,
        Action<SerializerOptions>? configureOptions = null,
        bool registerAsDefault = false)
    {
        services.TryAddSingleton<XmlSerializer>(sp =>
        {
            var factory = sp.GetRequiredService<SerializerFactory>();
            var options = factory.GetDefaultOptions();
            configureOptions?.Invoke(options);

            factory.RegisterSerializer<XmlSerializer>(opts => new XmlSerializer(opts));
            var serializer = factory.Create<XmlSerializer>(options);

            // Register with the registry
            var registry = sp.GetRequiredService<SerializerRegistry>();
            registry.Register("xml", serializer);

            return serializer;
        });

        if (registerAsDefault)
        {
            services.TryAddSingleton<ISerializer>(sp => sp.GetRequiredService<XmlSerializer>());
        }

        return services;
    }

    /// <summary>
    /// Adds a specific serializer type to the service collection.
    /// </summary>
    /// <typeparam name="TSerializer">The type of serializer to add.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="format">The format name to register the serializer under.</param>
    /// <param name="factory">The factory function to create the serializer.</param>
    /// <param name="configureOptions">Optional action to configure options specific to this serializer.</param>
    /// <param name="registerAsDefault">Whether to register this serializer as the default ISerializer.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddSerializer<TSerializer>(
        this IServiceCollection services,
        string format,
        Func<SerializerOptions, TSerializer> factory,
        Action<SerializerOptions>? configureOptions = null,
        bool registerAsDefault = false) where TSerializer : class, ISerializer
    {
        if (string.IsNullOrWhiteSpace(format))
        {
            throw new ArgumentException("Format cannot be null or whitespace.", nameof(format));
        }

        services.TryAddSingleton<TSerializer>(sp =>
        {
            var serializerFactory = sp.GetRequiredService<SerializerFactory>();
            var options = serializerFactory.GetDefaultOptions();
            configureOptions?.Invoke(options);

            serializerFactory.RegisterSerializer(factory);
            var serializer = serializerFactory.Create<TSerializer>(options);

            // Register with the registry
            var registry = sp.GetRequiredService<SerializerRegistry>();
            registry.Register(format, serializer);

            return serializer;
        });

        services.TryAddEnumerable(ServiceDescriptor.Singleton<ISerializer, TSerializer>(
            sp => sp.GetRequiredService<TSerializer>()));

        if (registerAsDefault)
        {
            services.TryAddSingleton<ISerializer>(sp => sp.GetRequiredService<TSerializer>());
        }

        return services;
    }

    /// <summary>
    /// Adds all default serializers to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional action to configure options for all serializers.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddAllSerializers(
        this IServiceCollection services,
        Action<SerializerOptions>? configureOptions = null)
    {
        services.AddUniversalSerializer(configureOptions);
        services.AddJsonSerializer();
        services.AddXmlSerializer();

        // Add additional serializers here as they are implemented

        return services;
    }
}
