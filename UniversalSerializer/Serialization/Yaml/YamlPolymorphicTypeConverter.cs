// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Serialization.Yaml;
using System.Reflection;
using ktsu.UniversalSerializer.Serialization.TypeRegistry;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

/// <summary>
/// YAML type converter that handles polymorphic types with discriminators.
/// </summary>
public class YamlPolymorphicTypeConverter : IYamlTypeConverter
{
	private readonly TypeRegistry _typeRegistry;
	private readonly string _discriminatorPropertyName;
	private readonly string _discriminatorFormat;

	/// <summary>
	/// Initializes a new instance of the <see cref="YamlPolymorphicTypeConverter"/> class.
	/// </summary>
	/// <param name="typeRegistry">The type registry.</param>
	/// <param name="discriminatorPropertyName">The type discriminator property name.</param>
	/// <param name="discriminatorFormat">The type discriminator format.</param>
	public YamlPolymorphicTypeConverter(
		TypeRegistry typeRegistry,
		string discriminatorPropertyName,
		string discriminatorFormat)
	{
		_typeRegistry = typeRegistry ?? throw new ArgumentNullException(nameof(typeRegistry));
		_discriminatorPropertyName = discriminatorPropertyName ?? throw new ArgumentNullException(nameof(discriminatorPropertyName));
		_discriminatorFormat = discriminatorFormat ?? throw new ArgumentNullException(nameof(discriminatorFormat));
	}

	/// <inheritdoc/>
	public bool Accepts(Type type) => _typeRegistry.HasPolymorphicTypes(type);

	/// <inheritdoc/>
	public object ReadYaml(IParser parser, Type type, ObjectDeserializer deserializer)
	{
		// Implementation for reading YAML and handling type discriminators
		throw new NotImplementedException("YamlPolymorphicTypeConverter.ReadYaml is not implemented");
	}

	/// <inheritdoc/>
	public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
	{
		// Implementation for writing YAML with type discriminators
		throw new NotImplementedException("YamlPolymorphicTypeConverter.WriteYaml is not implemented");
	}
}
