// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Serialization.Toml;

using System.Reflection;
using ktsu.UniversalSerializer.Serialization.TypeRegistry;
using Tomlyn;
using Tomlyn.Model;

/// <summary>
/// Serializer for TOML format using Tomlyn.
/// </summary>
public class TomlSerializer : SerializerBase
{
	private readonly TypeRegistry? _typeRegistry;
	private readonly bool _enableTypeDiscriminator;
	private readonly string _typeDiscriminatorPropertyName;
	private readonly TypeDiscriminatorFormat _discriminatorFormat;

	/// <summary>
	/// Initializes a new instance of the <see cref="TomlSerializer"/> class with default options.
	/// </summary>
	public TomlSerializer() : this(SerializerOptions.Default())
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="TomlSerializer"/> class with the specified options.
	/// </summary>
	/// <param name="options">The serializer options.</param>
	/// <param name="typeRegistry">Optional registry for polymorphic types.</param>
	public TomlSerializer(SerializerOptions options, TypeRegistry? typeRegistry = null) : base(options)
	{
		_typeRegistry = typeRegistry;
		_enableTypeDiscriminator = GetOption(SerializerOptionKeys.TypeRegistry.EnableTypeDiscriminator, false);
		_typeDiscriminatorPropertyName = GetOption(SerializerOptionKeys.TypeRegistry.TypeDiscriminatorPropertyName, "$type");

		var formatValue = GetOption(SerializerOptionKeys.TypeRegistry.TypeDiscriminatorFormat,
			TypeDiscriminatorFormat.Property.ToString());

		// Try to parse the format from string if stored as string
		_discriminatorFormat = formatValue is string formatString && Enum.TryParse(formatString, out TypeDiscriminatorFormat format)
			? format
			: formatValue is TypeDiscriminatorFormat typeDiscriminatorFormat ? typeDiscriminatorFormat : TypeDiscriminatorFormat.Property;
	}

	/// <inheritdoc/>
	public override string ContentType => "application/toml";

	/// <inheritdoc/>
	public override string FileExtension => ".toml";

	/// <summary>
	/// Gets all supported file extensions for TOML format.
	/// </summary>
	/// <returns>An array of supported file extensions.</returns>
	public static string[] GetSupportedExtensions() => [".toml", ".tml"];

	/// <inheritdoc/>
	public override string Serialize<T>(T obj)
	{
		if (obj == null)
		{
			return string.Empty;
		}

		var tomlTable = ConvertObjectToToml(obj);
		return Toml.ToText(tomlTable);
	}

	/// <inheritdoc/>
	public override string Serialize(object obj, Type type)
	{
		if (obj == null)
		{
			return string.Empty;
		}

		var tomlTable = ConvertObjectToToml(obj);
		return Toml.ToText(tomlTable);
	}

	/// <inheritdoc/>
	public override T Deserialize<T>(string serialized)
	{
		if (string.IsNullOrWhiteSpace(serialized))
		{
			return default!;
		}

		// Parse TOML
		var model = Toml.ToModel(serialized);

		// Convert the model to the requested type
		return (T)TomlSerializer.ConvertFromTomlModel(model, typeof(T))!;
	}

	/// <inheritdoc/>
	public override object Deserialize(string serialized, Type type)
	{
		if (string.IsNullOrWhiteSpace(serialized))
		{
			return null!;
		}

		// Parse TOML
		var model = Toml.ToModel(serialized);

		// Convert the model to the requested type
		return TomlSerializer.ConvertFromTomlModel(model, type)!;
	}

	/// <inheritdoc/>
	public override async Task<string> SerializeAsync<T>(T obj, CancellationToken cancellationToken = default) =>
		// Tomlyn doesn't have async methods, so we need to use Task.Run for async operation
		await Task.Run(() => Serialize(obj), cancellationToken).ConfigureAwait(false);

	/// <inheritdoc/>
	public override async Task<T> DeserializeAsync<T>(string serialized, CancellationToken cancellationToken = default) =>
		// Tomlyn doesn't have async methods, so we need to use Task.Run for async operation
		await Task.Run(() => Deserialize<T>(serialized), cancellationToken).ConfigureAwait(false);

	private TomlTable ConvertObjectToToml(object obj)
	{
		var tomlTable = new TomlTable();

		// Handle null objects
		if (obj == null)
		{
			return tomlTable;
		}

		var type = obj.GetType();

		// For polymorphic types, include type information if enabled
		if (_enableTypeDiscriminator && _typeRegistry != null)
		{
			var baseType = type.BaseType;
			if (baseType != null && baseType != typeof(object) || type.GetInterfaces().Length > 0)
			{
				var typeName = _typeRegistry.GetTypeName(type);
				if (!string.IsNullOrEmpty(typeName))
				{
					// Create a wrapper table with type information
					var wrapperTable = new TomlTable
						{
							["type"] = typeName,
							["value"] = TomlSerializer.ConvertObjectToTomlValue(obj, type)
						};
					return wrapperTable;
				}
			}
		}

		// Handle regular object conversion
		foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
		{
			if (!property.CanRead)
			{
				continue;
			}

			// Skip properties with [JsonIgnore] or [XmlIgnore] attributes
			if (property.GetCustomAttributes()
				.Any(a => a.GetType().Name is "JsonIgnoreAttribute" or "XmlIgnoreAttribute" or "IgnoreDataMemberAttribute"))
			{
				continue;
			}

			var propertyValue = property.GetValue(obj);
			if (propertyValue == null)
			{
				continue;
			}

			// Convert the property value to a TOML-compatible value
			var tomlValue = TomlSerializer.ConvertObjectToTomlValue(propertyValue, property.PropertyType);
			tomlTable[property.Name] = tomlValue;
		}

		return tomlTable;
	}

	// Add placeholder methods to make the file compile
	private static object ConvertObjectToTomlValue(object value, Type type) =>
		// Stub implementation - would need to be properly implemented
		value?.ToString() ?? string.Empty;

	private static object ConvertFromTomlModel(TomlTable model, Type type) =>
		// Stub implementation - would need to be properly implemented
		Activator.CreateInstance(type)!;
}
