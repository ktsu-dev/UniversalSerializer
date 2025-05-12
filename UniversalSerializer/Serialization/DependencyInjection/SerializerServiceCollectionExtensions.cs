// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Serialization.DependencyInjection;

using ktsu.UniversalSerializer.Serialization.Json;
using ktsu.UniversalSerializer.Serialization.MessagePack;
using ktsu.UniversalSerializer.Serialization.Toml;
using ktsu.UniversalSerializer.Serialization.TypeConverter;
using ktsu.UniversalSerializer.Serialization.TypeRegistry;
using ktsu.UniversalSerializer.Serialization.Xml;
using ktsu.UniversalSerializer.Serialization.Yaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IServiceCollection"/> to register serializers.
/// </summary>
public static class SerializerServiceCollectionExtensions
{
	/// <summary>
	/// Adds the Universal Serializer core services to the service collection.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <param name="optionsAction">Optional action to configure options.</param>
	/// <returns>The service collection with core services added.</returns>
	public static IServiceCollection AddUniversalSerializer(
		this IServiceCollection services,
		Action<SerializerOptions>? optionsAction = null)
	{
		// Register options
		var options = SerializerOptions.Default();
		optionsAction?.Invoke(options);
		services.TryAddSingleton(options);

		// Register core services
		services.TryAddSingleton<SerializerFactory>();
		services.TryAddSingleton<SerializerRegistry>();
		services.TryAddSingleton<TypeRegistry.TypeRegistry>();
		services.TryAddSingleton<TypeConverterRegistry>();

		// Register common helper services
		services.TryAddTransient<ISerializerResolver, SerializerResolver>();

		return services;
	}

	/// <summary>
	/// Adds all built-in serializers to the service collection.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <returns>The service collection with all built-in serializers added.</returns>
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
	/// <returns>The service collection with JSON serializer added.</returns>
	public static IServiceCollection AddJsonSerializer(this IServiceCollection services)
	{
		services.TryAddTransient<Json.JsonSerializer>();
		return services;
	}

	/// <summary>
	/// Adds the XML serializer to the service collection.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <returns>The service collection with XML serializer added.</returns>
	public static IServiceCollection AddXmlSerializer(this IServiceCollection services)
	{
		services.TryAddTransient<Xml.XmlSerializer>();
		return services;
	}

	/// <summary>
	/// Adds the YAML serializer to the service collection.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <returns>The service collection with YAML serializer added.</returns>
	public static IServiceCollection AddYamlSerializer(this IServiceCollection services)
	{
		services.TryAddTransient<Yaml.YamlSerializer>();
		return services;
	}

	/// <summary>
	/// Adds the TOML serializer to the service collection.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <returns>The service collection with TOML serializer added.</returns>
	public static IServiceCollection AddTomlSerializer(this IServiceCollection services)
	{
		services.TryAddTransient<Toml.TomlSerializer>();
		return services;
	}

	/// <summary>
	/// Adds the MessagePack serializer to the service collection.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <returns>The service collection with MessagePack serializer added.</returns>
	public static IServiceCollection AddMessagePackSerializer(this IServiceCollection services)
	{
		services.TryAddTransient<MessagePack.MessagePackSerializer>();
		return services;
	}
}
