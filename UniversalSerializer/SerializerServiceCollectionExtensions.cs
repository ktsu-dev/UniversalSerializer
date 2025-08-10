// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer;

using System;
using System.Linq;
using ktsu.SerializationProvider;
using ktsu.UniversalSerializer.Json;
using ktsu.UniversalSerializer.Toml;
using ktsu.UniversalSerializer.Yaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IServiceCollection"/> to register serializers.
/// </summary>
public static class SerializerServiceCollectionExtensions
{
	private static void EnsureCoreServices(this IServiceCollection services, Action<SerializerOptions>? optionsAction)
	{
		if (!services.Any(sd => sd.ServiceType == typeof(SerializerOptions)))
		{
			SerializerOptions options = SerializerOptions.Default();
			optionsAction?.Invoke(options);
			services.TryAddSingleton(options);
		}

		services.TryAddSingleton<SerializerFactory>();
		services.TryAddSingleton<ISerializerFactory>(sp => sp.GetRequiredService<SerializerFactory>());
		services.TryAddSingleton<SerializerRegistry>();
		services.TryAddSingleton<TypeRegistry>();
		services.TryAddSingleton<TypeConverterRegistry>();
		services.TryAddTransient<ISerializerResolver, SerializerResolver>();
	}
	/// <summary>
	/// Adds the Universal Serializer core services to the service collection.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <param name="optionsAction">Optional action to configure options.</param>
	/// <returns>The service collection with core services added.</returns>
	public static IServiceCollection AddUniversalSerializer(this IServiceCollection services, Action<SerializerOptions>? optionsAction = null)
	{
		services.EnsureCoreServices(optionsAction);

		return services;
	}

	/// <summary>
	/// Adds all supported serializers to the service collection.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <returns>The service collection with serializers added.</returns>
	public static IServiceCollection AddAllSerializers(this IServiceCollection services)
	{
		return services
			.AddJsonSerializer()
			.AddXmlSerializer()
			.AddYamlSerializer()
			.AddTomlSerializer()
			.AddMessagePackSerializer();
	}

	/// <summary>
	/// Adds the JSON serializer to the service collection.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <returns>The service collection.</returns>
	public static IServiceCollection AddJsonSerializer(this IServiceCollection services)
	{
		services.TryAddTransient<JsonSerializer>();
		services.TryAddTransient<JsonPolymorphicConverter>();
		return services;
	}

	/// <summary>
	/// Adds the XML serializer to the service collection.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <returns>The service collection.</returns>
	public static IServiceCollection AddXmlSerializer(this IServiceCollection services)
	{
		services.TryAddTransient<Xml.XmlSerializer>();
		return services;
	}

	/// <summary>
	/// Adds the YAML serializer to the service collection.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <returns>The service collection.</returns>
	public static IServiceCollection AddYamlSerializer(this IServiceCollection services)
	{
		services.TryAddTransient<YamlSerializer>();
		services.TryAddTransient<YamlPolymorphicTypeConverter>();
		services.TryAddTransient<YamlPolymorphicNodeDeserializer>();
		return services;
	}

	/// <summary>
	/// Adds the TOML serializer to the service collection.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <returns>The service collection.</returns>
	public static IServiceCollection AddTomlSerializer(this IServiceCollection services)
	{
		services.TryAddTransient<TomlSerializer>();
		return services;
	}

	/// <summary>
	/// Adds the MessagePack serializer to the service collection.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <returns>The service collection.</returns>
	public static IServiceCollection AddMessagePackSerializer(this IServiceCollection services)
	{
		services.TryAddTransient<MessagePack.MessagePackSerializer>();
		return services;
	}

	/// <summary>
	/// Adds a UniversalSerializationProvider to the service collection that internally instantiates its serializer(s).
	/// Defaults to JSON unless otherwise configured.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <param name="providerName">Optional custom provider name.</param>
	/// <returns>The service collection.</returns>
	public static IServiceCollection AddUniversalSerializationProvider(
		this IServiceCollection services,
		string? providerName = null)
	{
		services.EnsureCoreServices(null);
		services.TryAddTransient<ISerializationProvider>(serviceProvider =>
		{
			SerializerOptions options = serviceProvider.GetRequiredService<SerializerOptions>();
			SerializerRegistry registry = serviceProvider.GetRequiredService<SerializerRegistry>();
			SerializerFactory factory = serviceProvider.GetRequiredService<SerializerFactory>();

			// Ensure built-ins are available in the factory
			registry.RegisterBuiltIn(options);

			// Default to JSON unless caller chooses otherwise via specialized methods
			ISerializer serializer = factory.Create<JsonSerializer>(options);
			return new UniversalSerializationProvider(serializer, providerName ?? "UniversalSerializer.Json");
		});

		return services;
	}

	/// <summary>
	/// Adds a UniversalSerializationProvider which selects the serializer by logical format name (e.g., "json", "xml", "yaml", "toml", "messagepack").
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <param name="format">The logical format name.</param>
	/// <param name="providerName">Optional custom provider name.</param>
	/// <returns>The service collection.</returns>
	public static IServiceCollection AddUniversalSerializationProviderForFormat(
		this IServiceCollection services,
		string format,
		string? providerName = null)
	{
		services.EnsureCoreServices(null);
		services.TryAddTransient<ISerializationProvider>(serviceProvider =>
		{
			SerializerOptions options = serviceProvider.GetRequiredService<SerializerOptions>();
			SerializerRegistry registry = serviceProvider.GetRequiredService<SerializerRegistry>();

			registry.RegisterBuiltIn(options);
			ISerializer serializer = registry.GetSerializer(format) ?? throw new InvalidOperationException($"No serializer registered for format '{format}'.");
			return new UniversalSerializationProvider(serializer, providerName ?? $"UniversalSerializer.{format}");
		});

		return services;
	}

	/// <summary>
	/// Adds a UniversalSerializationProvider which selects the serializer by file extension (e.g., ".json", "json").
	/// </summary>
	public static IServiceCollection AddUniversalSerializationProviderForExtension(
		this IServiceCollection services,
		string extension,
		string? providerName = null)
	{
		services.EnsureCoreServices(null);
		services.TryAddTransient<ISerializationProvider>(serviceProvider =>
		{
			SerializerOptions options = serviceProvider.GetRequiredService<SerializerOptions>();
			SerializerRegistry registry = serviceProvider.GetRequiredService<SerializerRegistry>();

			registry.RegisterBuiltIn(options);
			ISerializer serializer = registry.GetSerializerByExtension(extension) ?? throw new InvalidOperationException($"No serializer registered for extension '{extension}'.");
			string normalized = extension.StartsWith('.') ? extension[1..] : extension;
			return new UniversalSerializationProvider(serializer, providerName ?? $"UniversalSerializer.{normalized}");
		});

		return services;
	}

	/// <summary>
	/// Adds a UniversalSerializationProvider which selects the serializer by content type (e.g., "application/json").
	/// </summary>
	public static IServiceCollection AddUniversalSerializationProviderForContentType(
		this IServiceCollection services,
		string contentType,
		string? providerName = null)
	{
		services.EnsureCoreServices(null);
		services.TryAddTransient<ISerializationProvider>(serviceProvider =>
		{
			SerializerOptions options = serviceProvider.GetRequiredService<SerializerOptions>();
			SerializerRegistry registry = serviceProvider.GetRequiredService<SerializerRegistry>();

			registry.RegisterBuiltIn(options);
			ISerializer serializer = registry.GetSerializerByContentType(contentType) ?? throw new InvalidOperationException($"No serializer registered for content type '{contentType}'.");
			return new UniversalSerializationProvider(serializer, providerName ?? $"UniversalSerializer.{contentType}");
		});

		return services;
	}

	/// <summary>
	/// Adds a UniversalSerializationProvider to the service collection using the specified serializer type.
	/// </summary>
	/// <typeparam name="TSerializer">The type of serializer to use.</typeparam>
	/// <param name="services">The service collection.</param>
	/// <param name="providerName">Optional custom provider name.</param>
	/// <returns>The service collection.</returns>
	public static IServiceCollection AddUniversalSerializationProvider<TSerializer>(
		this IServiceCollection services,
		string? providerName = null)
		where TSerializer : class, ISerializer
	{
		services.TryAddTransient<TSerializer>();
		services.TryAddTransient<ISerializationProvider>(serviceProvider =>
		{
			TSerializer serializer = serviceProvider.GetRequiredService<TSerializer>();
			return new UniversalSerializationProvider(serializer, providerName);
		});
		return services;
	}

	/// <summary>
	/// Adds a UniversalSerializationProvider to the service collection using a factory function.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <param name="serializerFactory">Factory function to create the serializer.</param>
	/// <param name="providerName">Optional custom provider name.</param>
	/// <returns>The service collection.</returns>
	public static IServiceCollection AddUniversalSerializationProvider(
		this IServiceCollection services,
		Func<IServiceProvider, ISerializer> serializerFactory,
		string? providerName = null)
	{
		services.TryAddTransient<ISerializationProvider>(serviceProvider =>
		{
			ISerializer serializer = serializerFactory(serviceProvider);
			return new UniversalSerializationProvider(serializer, providerName);
		});
		return services;
	}

	/// <summary>
	/// Adds a UniversalSerializationProvider to the service collection using a specific serializer instance.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <param name="serializer">The serializer instance to use.</param>
	/// <param name="providerName">Optional custom provider name.</param>
	/// <returns>The service collection.</returns>
	public static IServiceCollection AddUniversalSerializationProvider(
		this IServiceCollection services,
		ISerializer serializer,
		string? providerName = null)
	{
		services.TryAddSingleton<ISerializationProvider>(new UniversalSerializationProvider(serializer, providerName));
		return services;
	}

	/// <summary>
	/// Adds a JSON-based UniversalSerializationProvider to the service collection.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <param name="providerName">Optional custom provider name.</param>
	/// <returns>The service collection.</returns>
	public static IServiceCollection AddJsonSerializationProvider(
		this IServiceCollection services,
		string? providerName = null) => services.AddUniversalSerializationProvider<JsonSerializer>(providerName ?? "UniversalSerializer.Json");

	/// <summary>
	/// Adds a YAML-based UniversalSerializationProvider to the service collection.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <param name="providerName">Optional custom provider name.</param>
	/// <returns>The service collection.</returns>
	public static IServiceCollection AddYamlSerializationProvider(
		this IServiceCollection services,
		string? providerName = null) => services.AddUniversalSerializationProvider<YamlSerializer>(providerName ?? "UniversalSerializer.Yaml");

	/// <summary>
	/// Adds a TOML-based UniversalSerializationProvider to the service collection.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <param name="providerName">Optional custom provider name.</param>
	/// <returns>The service collection.</returns>
	public static IServiceCollection AddTomlSerializationProvider(
		this IServiceCollection services,
		string? providerName = null) => services.AddUniversalSerializationProvider<TomlSerializer>(providerName ?? "UniversalSerializer.Toml");

	/// <summary>
	/// Adds an XML-based UniversalSerializationProvider to the service collection.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <param name="providerName">Optional custom provider name.</param>
	/// <returns>The service collection.</returns>
	public static IServiceCollection AddXmlSerializationProvider(
		this IServiceCollection services,
		string? providerName = null) => services.AddUniversalSerializationProvider<Xml.XmlSerializer>(providerName ?? "UniversalSerializer.Xml");

	/// <summary>
	/// Adds a MessagePack-based UniversalSerializationProvider to the service collection.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <param name="providerName">Optional custom provider name.</param>
	/// <returns>The service collection.</returns>
	public static IServiceCollection AddMessagePackSerializationProvider(
		this IServiceCollection services,
		string? providerName = null) => services.AddUniversalSerializationProvider<MessagePack.MessagePackSerializer>(providerName ?? "UniversalSerializer.MessagePack");
}
