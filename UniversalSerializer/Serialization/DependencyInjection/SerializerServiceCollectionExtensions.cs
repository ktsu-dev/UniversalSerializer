// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using ktsu.UniversalSerializer.Serialization.Json;
using ktsu.UniversalSerializer.Serialization.Toml;
using ktsu.UniversalSerializer.Serialization.TypeConverter;
using ktsu.UniversalSerializer.Serialization.TypeRegistry;
using ktsu.UniversalSerializer.Serialization.Xml;
using ktsu.UniversalSerializer.Serialization.Yaml;
using ktsu.UniversalSerializer.Serialization.Ini;
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

        // Register TypeConverterRegistry
        services.TryAddSingleton<TypeConverterRegistry>();

        // Register TypeRegistry
        services.TryAddSingleton<TypeRegistry.TypeRegistry>(sp =>
        {
            var factory = sp.GetRequiredService<SerializerFactory>();
            return new TypeRegistry.TypeRegistry(factory.GetDefaultOptions());
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

            // Get type converter registry and type registry from DI
            var typeConverterRegistry = sp.GetService<TypeConverterRegistry>();
            var typeRegistry = sp.GetService<TypeRegistry.TypeRegistry>();

            // Use the provider-configured factory to create and register the serializer
            factory.RegisterSerializer<JsonSerializer>(opts =>
                new JsonSerializer(opts, typeConverterRegistry, typeRegistry));

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

            // Get type registry from DI
            var typeRegistry = sp.GetService<TypeRegistry.TypeRegistry>();

            factory.RegisterSerializer<XmlSerializer>(opts => new XmlSerializer(opts, typeRegistry));
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
    /// Adds the YAML serializer to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional action to configure options specific to this serializer.</param>
    /// <param name="registerAsDefault">Whether to register the YAML serializer as the default ISerializer.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddYamlSerializer(
        this IServiceCollection services,
        Action<SerializerOptions>? configureOptions = null,
        bool registerAsDefault = false)
    {
        services.TryAddSingleton<YamlSerializer>(sp =>
        {
            var factory = sp.GetRequiredService<SerializerFactory>();
            var options = factory.GetDefaultOptions();
            configureOptions?.Invoke(options);

            // Get type registry from DI
            var typeRegistry = sp.GetService<TypeRegistry.TypeRegistry>();

            factory.RegisterSerializer<YamlSerializer>(opts => new YamlSerializer(opts, typeRegistry));
            var serializer = factory.Create<YamlSerializer>(options);

            // Register with the registry
            var registry = sp.GetRequiredService<SerializerRegistry>();
            registry.Register("yaml", serializer);

            return serializer;
        });

        if (registerAsDefault)
        {
            services.TryAddSingleton<ISerializer>(sp => sp.GetRequiredService<YamlSerializer>());
        }

        return services;
    }

    /// <summary>
    /// Adds the TOML serializer to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional action to configure options specific to this serializer.</param>
    /// <param name="registerAsDefault">Whether to register the TOML serializer as the default ISerializer.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddTomlSerializer(
        this IServiceCollection services,
        Action<SerializerOptions>? configureOptions = null,
        bool registerAsDefault = false)
    {
        services.TryAddSingleton<TomlSerializer>(sp =>
        {
            var factory = sp.GetRequiredService<SerializerFactory>();
            var options = factory.GetDefaultOptions();
            configureOptions?.Invoke(options);

            // Get type registry from DI
            var typeRegistry = sp.GetService<TypeRegistry.TypeRegistry>();

            factory.RegisterSerializer<TomlSerializer>(opts => new TomlSerializer(opts, typeRegistry));
            var serializer = factory.Create<TomlSerializer>(options);

            // Register with the registry
            var registry = sp.GetRequiredService<SerializerRegistry>();
            registry.Register("toml", serializer);
            registry.RegisterFileExtensions("toml", ".toml", ".tml");

            return serializer;
        });

        if (registerAsDefault)
        {
            services.TryAddSingleton<ISerializer>(sp => sp.GetRequiredService<TomlSerializer>());
        }

        return services;
    }

    /// <summary>
    /// Adds the INI serializer to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional action to configure options specific to this serializer.</param>
    /// <param name="registerAsDefault">Whether to register the INI serializer as the default ISerializer.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddIniSerializer(
        this IServiceCollection services,
        Action<SerializerOptions>? configureOptions = null,
        bool registerAsDefault = false)
    {
        services.TryAddSingleton<IniSerializer>(sp =>
        {
            var factory = sp.GetRequiredService<SerializerFactory>();
            var options = factory.GetDefaultOptions();
            configureOptions?.Invoke(options);

            // Get type registry from DI
            var typeRegistry = sp.GetService<TypeRegistry.TypeRegistry>();

            factory.RegisterSerializer<IniSerializer>(opts => new IniSerializer(opts, typeRegistry));
            var serializer = factory.Create<IniSerializer>(options);

            // Register with the registry
            var registry = sp.GetRequiredService<SerializerRegistry>();
            registry.Register("ini", serializer);
            registry.RegisterFileExtensions("ini", ".ini", ".conf", ".cfg");

            return serializer;
        });

        if (registerAsDefault)
        {
            services.TryAddSingleton<ISerializer>(sp => sp.GetRequiredService<IniSerializer>());
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
    /// Adds all built-in serializers to the service collection.
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
        services.AddYamlSerializer();
        services.AddTomlSerializer();
        services.AddIniSerializer();
        return services;
    }
}
