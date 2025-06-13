// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Serialization.Toml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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

		string formatValue = GetOption(SerializerOptionKeys.TypeRegistry.TypeDiscriminatorFormat,
			TypeDiscriminatorFormat.Property.ToString());

		// Parse the format from string - not currently used but may be needed for future features
		_ = Enum.TryParse(formatValue, out TypeDiscriminatorFormat format) ? format : TypeDiscriminatorFormat.Property;
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

		TomlTable tomlTable = ConvertObjectToToml(obj);
		return Toml.FromModel(tomlTable);
	}

	/// <inheritdoc/>
	public override string Serialize(object obj, Type type)
	{
		if (obj == null)
		{
			return string.Empty;
		}

		TomlTable tomlTable = ConvertObjectToToml(obj);
		return Toml.FromModel(tomlTable);
	}

	/// <inheritdoc/>
	public override T Deserialize<T>(string serialized)
	{
		if (string.IsNullOrWhiteSpace(serialized))
		{
			return default!;
		}

		// Parse TOML
		TomlTable model = Toml.ToModel(serialized);

		// Convert the model to the requested type
		return (T)ConvertFromTomlModel(model, typeof(T))!;
	}

	/// <inheritdoc/>
	public override object Deserialize(string serialized, Type type)
	{
		ArgumentNullException.ThrowIfNull(type);
		if (string.IsNullOrWhiteSpace(serialized))
		{
			return null!;
		}

		// Parse TOML
		TomlTable model = Toml.ToModel(serialized);

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

	private TomlTable ConvertObjectToToml(object obj)
	{
		TomlTable tomlTable = [];

		// Handle null objects
		if (obj == null)
		{
			return tomlTable;
		}

		Type type = obj.GetType();

		// For polymorphic types, include type information if enabled
		if (_enableTypeDiscriminator && _typeRegistry != null)
		{
			Type? baseType = type.BaseType;
			if ((baseType != null && baseType != typeof(object)) || type.GetInterfaces().Length > 0)
			{
				string typeName = _typeRegistry.GetTypeName(type);
				if (!string.IsNullOrEmpty(typeName))
				{
					// Create a wrapper table with type information
					TomlTable wrapperTable = new()
					{
						["type"] = typeName,
						["value"] = ConvertObjectToTomlValue(obj, type)
					};
					return wrapperTable;
				}
			}
		}

		// Handle regular object conversion
		foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
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

			object? propertyValue = property.GetValue(obj);
			if (propertyValue == null)
			{
				continue;
			}

			// Convert the property value to a TOML-compatible value
			object tomlValue = ConvertObjectToTomlValue(propertyValue, property.PropertyType);
			tomlTable[property.Name] = tomlValue;
		}

		return tomlTable;
	}

	// Add placeholder methods to make the file compile
	private static object ConvertObjectToTomlValue(object value, Type type)
	{
		if (value == null)
		{
			return string.Empty;
		}

		// Handle DateTime specially to ensure proper TOML format
		if (type == typeof(DateTime))
		{
			DateTime dateTime = (DateTime)value;
			// Convert to ISO 8601 format without timezone to preserve the original DateTime
			return dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fff");
		}

		// Handle primitive types
		if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
		{
			return value;
		}

		// Handle collections
		if (value is System.Collections.IEnumerable enumerable && type != typeof(string))
		{
			List<object> list = [];
			foreach (object? item in enumerable)
			{
				if (item != null)
				{
					list.Add(ConvertObjectToTomlValue(item, item.GetType()));
				}
			}
			return list;
		}

		// For complex objects, convert to string representation
		return value.ToString() ?? string.Empty;
	}

	private static object ConvertFromTomlModel(TomlTable model, Type type)
	{
		ArgumentNullException.ThrowIfNull(model);

		// Create instance of the target type
		object? instance = Activator.CreateInstance(type) ?? throw new InvalidOperationException($"Cannot create instance of type {type.Name}");

		// Set properties from TOML table
		foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
		{
			if (!property.CanWrite)
			{
				continue;
			}

			if (model.TryGetValue(property.Name, out object? value) && value != null)
			{
				try
				{
					object? convertedValue = ConvertTomlValueToProperty(value, property.PropertyType);
					if (convertedValue != null)
					{
						property.SetValue(instance, convertedValue);
					}
				}
				catch (ArgumentException)
				{
					// Skip properties that can't be converted due to argument issues
					continue;
				}
				catch (InvalidOperationException)
				{
					// Skip properties that can't be converted due to operation issues
					continue;
				}
				catch (NotSupportedException)
				{
					// Skip properties that can't be converted due to unsupported conversions
					continue;
				}
			}
		}

		return instance;
	}

	private static object? ConvertTomlValueToProperty(object value, Type targetType)
	{
		if (value == null)
		{
			return null;
		}

		// Handle direct assignment
		if (targetType.IsAssignableFrom(value.GetType()))
		{
			return value;
		}

		// Handle type conversion
		if (targetType == typeof(string))
		{
			return value.ToString();
		}

		// Handle DateTime conversion
		if (targetType == typeof(DateTime))
		{
			if (value is DateTime dateTime)
			{
				return dateTime;
			}
			if (value is string dateString)
			{
				// Try multiple DateTime parsing formats
				if (DateTime.TryParse(dateString, out DateTime parsedDate))
				{
					return parsedDate;
				}
				// Try ISO 8601 format without timezone
				if (DateTime.TryParseExact(dateString, "yyyy-MM-ddTHH:mm:ss.fff",
					System.Globalization.CultureInfo.InvariantCulture,
					System.Globalization.DateTimeStyles.None, out DateTime isoDate))
				{
					return isoDate;
				}
				// Try ISO 8601 format with Z suffix
				if (DateTime.TryParseExact(dateString, "yyyy-MM-ddTHH:mm:ss.fffZ",
					System.Globalization.CultureInfo.InvariantCulture,
					System.Globalization.DateTimeStyles.AssumeUniversal, out DateTime isoDateZ))
				{
					return isoDateZ;
				}
			}
			if (value is DateTimeOffset dateTimeOffset)
			{
				return dateTimeOffset.DateTime;
			}
		}

		// Handle nullable DateTime
		if (targetType == typeof(DateTime?))
		{
			if (value is DateTime dateTime)
			{
				return dateTime;
			}
			if (value is string dateString)
			{
				// Try multiple DateTime parsing formats
				if (DateTime.TryParse(dateString, out DateTime parsedDate))
				{
					return parsedDate;
				}
				// Try ISO 8601 format without timezone
				if (DateTime.TryParseExact(dateString, "yyyy-MM-ddTHH:mm:ss.fff",
					System.Globalization.CultureInfo.InvariantCulture,
					System.Globalization.DateTimeStyles.None, out DateTime isoDate))
				{
					return isoDate;
				}
				// Try ISO 8601 format with Z suffix
				if (DateTime.TryParseExact(dateString, "yyyy-MM-ddTHH:mm:ss.fffZ",
					System.Globalization.CultureInfo.InvariantCulture,
					System.Globalization.DateTimeStyles.AssumeUniversal, out DateTime isoDateZ))
				{
					return isoDateZ;
				}
			}
			if (value is DateTimeOffset dateTimeOffset)
			{
				return dateTimeOffset.DateTime;
			}
		}

		if (targetType.IsPrimitive)
		{
			return Convert.ChangeType(value, targetType);
		}

		// Handle collections
		if (value is System.Collections.IEnumerable enumerable && targetType.IsGenericType)
		{
			Type elementType = targetType.GetGenericArguments()[0];
			Type listType = typeof(List<>).MakeGenericType(elementType);
			object? list = Activator.CreateInstance(listType);
			MethodInfo? addMethod = listType.GetMethod("Add");

			foreach (object? item in enumerable)
			{
				if (item != null)
				{
					object? convertedItem = ConvertTomlValueToProperty(item, elementType);
					if (convertedItem != null)
					{
						addMethod?.Invoke(list, [convertedItem]);
					}
				}
			}
			return list;
		}

		return value;
	}
}
