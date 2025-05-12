// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Serialization.Yaml;
using ktsu.UniversalSerializer.Serialization.TypeRegistry;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

/// <summary>
/// YAML node deserializer that handles polymorphic types with discriminators.
/// </summary>
public class YamlPolymorphicNodeDeserializer : INodeDeserializer
{
	private readonly INodeDeserializer _innerDeserializer;
	private readonly TypeRegistry _typeRegistry;
	private readonly string _discriminatorPropertyName;
	private readonly string _discriminatorFormat;

	/// <summary>
	/// Initializes a new instance of the <see cref="YamlPolymorphicNodeDeserializer"/> class.
	/// </summary>
	/// <param name="typeRegistry">The type registry.</param>
	/// <param name="discriminatorPropertyName">The type discriminator property name.</param>
	/// <param name="discriminatorFormat">The type discriminator format.</param>
	/// <param name="innerDeserializer">The inner deserializer to use after type resolution.</param>
	public YamlPolymorphicNodeDeserializer(
		TypeRegistry typeRegistry,
		string discriminatorPropertyName,
		string discriminatorFormat,
		INodeDeserializer innerDeserializer)
	{
		_typeRegistry = typeRegistry ?? throw new ArgumentNullException(nameof(typeRegistry));
		_discriminatorPropertyName = discriminatorPropertyName ?? throw new ArgumentNullException(nameof(discriminatorPropertyName));
		_discriminatorFormat = discriminatorFormat ?? throw new ArgumentNullException(nameof(discriminatorFormat));
		_innerDeserializer = innerDeserializer ?? throw new ArgumentNullException(nameof(innerDeserializer));
	}

	/// <inheritdoc/>
	public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
	{
		// Fallback to inner deserializer if not an abstract type or interface
		if (!_typeRegistry.HasPolymorphicTypes(expectedType))
		{
			return _innerDeserializer.Deserialize(reader, expectedType, nestedObjectDeserializer, out value);
		}

		value = null;
		return false;
	}

	/// <inheritdoc/>
	public bool Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value, ObjectDeserializer objectDeserializer)
	{
		return Deserialize(parser, expectedType, nestedObjectDeserializer, out value);
	}
}
