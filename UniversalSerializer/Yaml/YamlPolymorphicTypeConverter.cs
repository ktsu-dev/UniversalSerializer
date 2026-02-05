// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Services.Yaml;

using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using ktsu.UniversalSerializer.Services;

/// <summary>
/// YAML type converter that handles polymorphic types with discriminators.
/// </summary>
public class YamlPolymorphicTypeConverter : IYamlTypeConverter
{
	private readonly TypeRegistry _typeRegistry;

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
		_typeRegistry = Ensure.NotNull(typeRegistry);
		// Parameters are stored for future use but not currently implemented
		_ = Ensure.NotNull(discriminatorPropertyName);
		_ = Ensure.NotNull(discriminatorFormat);
	}

	/// <inheritdoc/>
	public bool Accepts(Type type)
	{
		Ensure.NotNull(type);
		return _typeRegistry.HasPolymorphicTypes() && type.IsClass && !type.IsSealed;
	}

	/// <inheritdoc/>
	public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
	{
		// For now, provide a minimal implementation
		// A full implementation would parse type discriminators and handle polymorphic types
		Scalar scalar = parser.Consume<Scalar>();
		return scalar?.Value ?? throw new InvalidOperationException("Unable to deserialize YAML scalar");
	}

	/// <inheritdoc/>
	public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
	{
		Ensure.NotNull(emitter);
		if (value == null)
		{
			emitter.Emit(new Scalar(null, null, "null", ScalarStyle.Plain, true, false));
			return;
		}

		// For now, emit as string representation
		// A full implementation would add type discriminators for polymorphic types
		string stringValue = value.ToString() ?? string.Empty;
		emitter.Emit(new Scalar(null, null, stringValue, ScalarStyle.Plain, true, false));
	}
}
