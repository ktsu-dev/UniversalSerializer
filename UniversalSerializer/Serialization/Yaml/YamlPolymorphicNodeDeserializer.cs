// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Serialization.Yaml;
using ktsu.UniversalSerializer.Serialization.TypeRegistry;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

/// <summary>
/// YamlDotNet node deserializer for polymorphic deserialization.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="YamlPolymorphicNodeDeserializer"/> class.
/// </remarks>
/// <param name="typeRegistry">The type registry.</param>
/// <param name="typeDiscriminatorPropertyName">The name of the type discriminator property.</param>
/// <param name="discriminatorFormat">The format to use for type discrimination.</param>
/// <param name="innerDeserializer">The inner deserializer to use.</param>
internal class YamlPolymorphicNodeDeserializer(
	TypeRegistry.TypeRegistry typeRegistry,
	string typeDiscriminatorPropertyName,
	TypeDiscriminatorFormat discriminatorFormat,
	INodeDeserializer innerDeserializer) : INodeDeserializer
{
	private readonly TypeRegistry.TypeRegistry _typeRegistry = typeRegistry ?? throw new ArgumentNullException(nameof(typeRegistry));
	private readonly string _typeDiscriminatorPropertyName = typeDiscriminatorPropertyName ?? throw new ArgumentNullException(nameof(typeDiscriminatorPropertyName));
	private readonly TypeDiscriminatorFormat _discriminatorFormat = discriminatorFormat;
	private readonly INodeDeserializer _innerDeserializer = innerDeserializer ?? throw new ArgumentNullException(nameof(innerDeserializer));

	/// <summary>
	/// Determines whether this deserializer can deserialize the specified node to the specified type.
	/// </summary>
	/// <param name="parser"></param>
	/// <param name="nodeEvent">The node event.</param>
	/// <param name="expectedType">The expected type.</param>
	/// <param name="nestedObjectDeserializer"></param>
	/// <param name="value"></param>
	/// <returns>True if this deserializer can deserialize the node to the specified type, false otherwise.</returns>
	public bool Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
	{
		value = null;

		// Only handle abstract classes and interfaces (for polymorphic cases)
		if (!expectedType.IsInterface && !(expectedType.IsClass && expectedType.IsAbstract))
		{
			return false;
		}

		// This should be a mapping node
		if (!parser.TryConsume<MappingStart>(out _))
		{
			return false;
		}

		string? typeName = null;

		// Make a copy of the current parser state so we can restart if needed
		var parserState = parser.Current;
		var shouldRestart = false;

		// Read the mapping entries to find the type discriminator
		switch (_discriminatorFormat)
		{
			case TypeDiscriminatorFormat.Property:
				// Look for the type discriminator property in the mapping
				while (parser.Current is Scalar keyScalar && !parser.TryConsume<MappingEnd>(out _))
				{
					if (keyScalar.Value == _typeDiscriminatorPropertyName)
					{
						parser.MoveNext();

						if (parser.Current is Scalar valueScalar)
						{
							typeName = valueScalar.Value;
							shouldRestart = true;
							parser.MoveNext();
							break;
						}
					}

					// Skip key/value pair
					parser.MoveNext(); // Skip key
					parser.SkipThisAndNestedEvents(); // Skip value
				}

				break;

			case TypeDiscriminatorFormat.Wrapper:
				string? innerTypeName = null;
				object? innerValue = null;

				// Look for 'type' and 'value' properties
				while (parser.Current is Scalar keyScalar && !parser.TryConsume<MappingEnd>(out _))
				{
					if (keyScalar.Value == "type")
					{
						parser.MoveNext();

						if (parser.Current is Scalar typeValueScalar)
						{
							innerTypeName = typeValueScalar.Value;
							parser.MoveNext();
						}
						else
						{
							parser.SkipThisAndNestedEvents(); // Skip non-scalar type value
						}
					}
					else if (keyScalar.Value == "value")
					{
						parser.MoveNext();

						// We'd need to parse the inner object here
						// For now, let's assume it will be handled by the inner deserializer
						if (innerTypeName != null)
						{
							var innerType = _typeRegistry.ResolveType(innerTypeName);
							if (innerType != null)
							{
								innerValue = nestedObjectDeserializer(parser, innerType);
							}
							else
							{
								parser.SkipThisAndNestedEvents(); // Skip if type not found
							}
						}
						else
						{
							parser.SkipThisAndNestedEvents(); // Skip if no type found yet
						}
					}
					else
					{
						parser.MoveNext(); // Skip unknown key
						parser.SkipThisAndNestedEvents(); // Skip value
					}
				}

				if (innerTypeName != null && innerValue != null)
				{
					typeName = innerTypeName;
					value = innerValue;
					return true;
				}

				break;

			case TypeDiscriminatorFormat.TypeProperty:
				throw new NotImplementedException("TypeProperty discriminator format not implemented yet for YAML deserialization.");
			default:
				break;
		}

		// If we need to restart (found type but need to parse the whole object)
		if (shouldRestart && typeName != null)
		{
			// Resolve the concrete type from the registry
			var concreteType = _typeRegistry.ResolveType(typeName) ?? throw new YamlException($"Could not resolve type '{typeName}'");

			// Check if the concrete type is assignable to the expected type
			if (!expectedType.IsAssignableFrom(concreteType))
			{
				throw new YamlException($"Type '{concreteType.FullName}' is not assignable to '{expectedType.FullName}'");
			}

			// Reset the parser
			while (parser.Current is not MappingStart and not null)
			{
				parser.MoveNext();
			}

			// Deserialize to the concrete type using the inner deserializer
			if (_innerDeserializer.Deserialize(parser, concreteType, nestedObjectDeserializer, out var innerResult))
			{
				value = innerResult;
				return true;
			}
		}

		// If we couldn't handle it, fall back to the inner deserializer
		return false;
	}
}
