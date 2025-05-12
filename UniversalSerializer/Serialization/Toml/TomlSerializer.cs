// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Serialization.Toml;

using ktsu.UniversalSerializer.Serialization.TypeRegistry;
using Tomlyn;
using Tomlyn.Model;

/// <summary>
/// Serializer for TOML format using Tomlyn.
/// </summary>
public class TomlSerializer : SerializerBase
{
	private readonly TypeRegistry.TypeRegistry? _typeRegistry;
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
	public TomlSerializer(SerializerOptions options, TypeRegistry.TypeRegistry? typeRegistry = null) : base(options)
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

		// Convert the object to a TomlTable using reflection
		var tomlModel = ConvertToTomlModel(obj, typeof(T));
		return Toml.FromModel(tomlModel);
	}

	/// <inheritdoc/>
	public override string Serialize(object obj, Type type)
	{
		if (obj == null)
		{
			return string.Empty;
		}

		// Convert the object to a TomlTable using reflection
		var tomlModel = ConvertToTomlModel(obj, type);
		return Toml.FromModel(tomlModel);
	}

	/// <inheritdoc/>
	public override T Deserialize<T>(string serialized)
	{
		if (string.IsNullOrWhiteSpace(serialized))
		{
			return default!;
		}

		// Parse the TOML string into a model
		var model = Toml.ToModel(serialized);

		// Convert the model to the requested type
		return (T)ConvertFromTomlModel(model, typeof(T))!;
	}

	/// <inheritdoc/>
	public override object Deserialize(string serialized, Type type)
	{
		if (string.IsNullOrWhiteSpace(serialized))
		{
			return null!;
		}

		// Parse the TOML string into a model
		var model = Toml.ToModel(serialized);

		// Convert the model to the requested type
		return ConvertFromTomlModel(model, type)!;
	}

	/// <inheritdoc/>
	public override async Task<string> SerializeAsync<T>(T obj, CancellationToken cancellationToken = default) =>
		// Tomlyn doesn't have async methods, so we need to use Task.Run for async operation
		await Task.Run(() => Serialize(obj), cancellationToken).ConfigureAwait(false);

	/// <inheritdoc/>
	public override async Task<T> DeserializeAsync<T>(string serialized, CancellationToken cancellationToken = default) =>
		// Tomlyn doesn't have async methods, so we need to use Task.Run for async operation
		await Task.Run(() => Deserialize<T>(serialized), cancellationToken).ConfigureAwait(false);

	private TomlTable ConvertToTomlModel(object value, Type type)
	{
		var tomlTable = new TomlTable();

		// If polymorphic serialization is enabled and the type registry is available
		if (_enableTypeDiscriminator && _typeRegistry != null && value != null)
		{
			var actualType = value.GetType();

			// Only add type information if the actual type differs from the expected type
			if (actualType != type && (type.IsInterface || type.IsAbstract))
			{
				// Get the type name from the registry
				var typeName = _typeRegistry.GetTypeName(actualType);

				// Add type discriminator based on format
				switch (_discriminatorFormat)
				{
					case TypeDiscriminatorFormat.Property:
						tomlTable[_typeDiscriminatorPropertyName] = typeName;
						break;

					case TypeDiscriminatorFormat.Wrapper:
						var wrapperTable = new TomlTable
						{
							["type"] = typeName,
							["value"] = ConvertObjectToTomlValue(value, actualType)
						};
						return wrapperTable;

					case TypeDiscriminatorFormat.TypeProperty:
						// Not fully implemented for TOML yet
						break;
					default:
						break;
				}
			}
		}

		// Add all properties to the TOML table
		if (value != null)
		{
			var properties = value.GetType().GetProperties();
			foreach (var property in properties)
			{
				if (!property.CanRead)
				{
					continue;
				}

				var propertyValue = property.GetValue(value);
				if (propertyValue == null)
				{
					continue;
				}

				// Convert the property value to a TOML-compatible value
				var tomlValue = ConvertObjectToTomlValue(propertyValue, property.PropertyType);
				tomlTable[property.Name] = tomlValue;
			}
		}

		return tomlTable;
	}

	// Add placeholder methods to make the file compile
	private object ConvertObjectToTomlValue(object value, Type type) =>
		// Stub implementation - would need to be properly implemented
		value?.ToString() ?? string.Empty;

	private object ConvertFromTomlModel(TomlTable model, Type type) =>
		// Stub implementation - would need to be properly implemented
		Activator.CreateInstance(type)!;
}
