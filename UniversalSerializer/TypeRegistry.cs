// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/// <summary>
/// A registry for managing type mappings in polymorphic serialization.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TypeRegistry"/> class.
/// </remarks>
/// <param name="options">The serializer options.</param>
public class TypeRegistry(SerializerOptions options)
{
	private readonly Dictionary<string, Type> _typeMap = new(StringComparer.Ordinal);
	private readonly Dictionary<Type, string> _nameMap = [];
	private readonly SerializerOptions _options = options ?? throw new ArgumentNullException(nameof(options));

	/// <summary>
	/// Registers a type with the registry.
	/// </summary>
	/// <typeparam name="T">The type to register.</typeparam>
	/// <param name="name">The optional name to use for the type. If null, the type name will be used.</param>
	public void RegisterType<T>(string? name = null) => RegisterType(typeof(T), name);

	/// <summary>
	/// Registers a type with the registry.
	/// </summary>
	/// <param name="type">The type to register.</param>
	/// <param name="name">The optional name to use for the type. If null, the type name will be used.</param>
	public void RegisterType(Type type, string? name = null)
	{
		ArgumentNullException.ThrowIfNull(type);

		name ??= GetTypeName(type);
		_typeMap[name] = type;
		_nameMap[type] = name;
	}

	/// <summary>
	/// Resolves a type name to a type.
	/// </summary>
	/// <param name="typeName">The type name to resolve.</param>
	/// <returns>The resolved type, or null if not found.</returns>
	public Type? ResolveType(string typeName)
	{
		if (string.IsNullOrEmpty(typeName))
		{
			throw new ArgumentException("Type name cannot be null or empty.", nameof(typeName));
		}

		if (_typeMap.TryGetValue(typeName, out Type? type))
		{
			return type;
		}

		// Try to resolve by reflection if not in map
		return Type.GetType(typeName, false);
	}

	/// <summary>
	/// Gets the name for a type.
	/// </summary>
	/// <param name="type">The type to get the name for.</param>
	/// <returns>The type name.</returns>
	public string GetTypeName(Type type)
	{
		ArgumentNullException.ThrowIfNull(type);

		if (_nameMap.TryGetValue(type, out string? name))
		{
			return name;
		}

		// Get type name based on options
		bool useFullyQualifiedName = _options.GetOption("TypeRegistry:UseFullyQualifiedTypeNames", false);
		return useFullyQualifiedName
			? type.AssemblyQualifiedName ?? type.FullName ?? type.Name
			: type.FullName ?? type.Name;
	}

	/// <summary>
	/// Checks if the registry has any polymorphic types registered.
	/// </summary>
	/// <returns>True if there are polymorphic types registered; otherwise, false.</returns>
	public bool HasPolymorphicTypes() => _typeMap.Count > 0;

	/// <summary>
	/// Registers all subtypes of a base type.
	/// </summary>
	/// <typeparam name="TBase">The base type.</typeparam>
	/// <param name="assembly">The optional assembly to search. If null, the assembly containing the base type will be used.</param>
	public void RegisterSubtypes<TBase>(Assembly? assembly = null)
	{
		assembly ??= Assembly.GetAssembly(typeof(TBase)) ?? throw new InvalidOperationException("Could not get assembly for type.");

		foreach (Type? type in assembly.GetTypes()
			.Where(t => typeof(TBase).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface))
		{
			RegisterType(type);
		}
	}

	/// <summary>
	/// Gets all registered type mappings.
	/// </summary>
	/// <returns>An enumerable of type mappings.</returns>
	public IEnumerable<(string Name, Type Type)> GetAllTypeMappings() => _typeMap.Select(kvp => (kvp.Key, kvp.Value));
}
