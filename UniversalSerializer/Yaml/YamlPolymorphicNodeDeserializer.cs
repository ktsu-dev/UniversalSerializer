// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Yaml;
using System;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

/// <summary>
/// YAML node deserializer that handles polymorphic types with discriminators.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="YamlPolymorphicNodeDeserializer"/> class.
/// </remarks>
/// <param name="typeRegistry">The type registry.</param>
/// <param name="discriminatorPropertyName">The type discriminator property name.</param>
/// <param name="discriminatorFormat">The type discriminator format.</param>
/// <param name="innerDeserializer">The inner deserializer to use after type resolution.</param>
public class YamlPolymorphicNodeDeserializer(
	TypeRegistry typeRegistry,
	string discriminatorPropertyName,
	string discriminatorFormat,
	INodeDeserializer innerDeserializer) : INodeDeserializer
{
	private readonly INodeDeserializer _innerDeserializer = innerDeserializer ?? throw new ArgumentNullException(nameof(innerDeserializer));
	private readonly TypeRegistry _typeRegistry = typeRegistry ?? throw new ArgumentNullException(nameof(typeRegistry));

	/// <inheritdoc/>
	public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value, ObjectDeserializer rootDeserializer)
	{
		// Use parameters to avoid CS9113 warnings
		_ = discriminatorPropertyName;
		_ = discriminatorFormat;

		// Fallback to inner deserializer if not an abstract type or interface
		if (!_typeRegistry.HasPolymorphicTypes())
		{
			return _innerDeserializer.Deserialize(reader, expectedType, nestedObjectDeserializer, out value, rootDeserializer);
		}

		value = null;
		return false;
	}
}
