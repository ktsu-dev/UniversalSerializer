// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Serialization;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ktsu.UniversalSerializer.Serialization.TypeRegistry;

/// <summary>
/// Provides configuration options for serializers.
/// </summary>
public class SerializerOptions
{
	private readonly ConcurrentDictionary<string, object> _options = new();

	/// <summary>
	/// Gets the number of options that have been set.
	/// </summary>
	public int Count => _options.Count;

	/// <summary>
	/// Sets an option with the specified key and value.
	/// </summary>
	/// <typeparam name="T">The type of the option value.</typeparam>
	/// <param name="key">The option key.</param>
	/// <param name="value">The option value.</param>
	/// <returns>The current options instance for method chaining.</returns>
	public SerializerOptions SetOption<T>(string key, T value)
	{
		_options[key] = value!;
		return this;
	}

	/// <summary>
	/// Gets an option with the specified key.
	/// </summary>
	/// <typeparam name="T">The type of the option value.</typeparam>
	/// <param name="key">The option key.</param>
	/// <returns>The option value, or default if not found.</returns>
	public T? GetOption<T>(string key) => _options.TryGetValue(key, out object? value) && value is T typedValue ? typedValue : default;

	/// <summary>
	/// Gets an option with the specified key, or a default value if not found.
	/// </summary>
	/// <typeparam name="T">The type of the option value.</typeparam>
	/// <param name="key">The option key.</param>
	/// <param name="defaultValue">The default value to return if the option is not found.</param>
	/// <returns>The option value, or the specified default value if not found.</returns>
	public T GetOption<T>(string key, T defaultValue) => _options.TryGetValue(key, out object? value) && value is T typedValue ? typedValue : defaultValue;

	/// <summary>
	/// Tries to get an option with the specified key.
	/// </summary>
	/// <typeparam name="T">The type of the option value.</typeparam>
	/// <param name="key">The option key.</param>
	/// <param name="value">When this method returns, contains the value associated with the specified key, if found; otherwise, the default value for the type.</param>
	/// <returns>true if the option was found; otherwise, false.</returns>
	public bool TryGetOption<T>(string key, out T? value)
	{
		if (_options.TryGetValue(key, out object? objValue) && objValue is T typedValue)
		{
			value = typedValue;
			return true;
		}

		value = default;
		return false;
	}

	/// <summary>
	/// Determines whether an option with the specified key exists.
	/// </summary>
	/// <param name="key">The option key.</param>
	/// <returns>true if the option exists; otherwise, false.</returns>
	public bool HasOption(string key) => _options.ContainsKey(key);

	/// <summary>
	/// Removes an option with the specified key.
	/// </summary>
	/// <param name="key">The option key.</param>
	/// <returns>true if the option was removed; otherwise, false.</returns>
	public bool RemoveOption(string key) => _options.TryRemove(key, out _);

	/// <summary>
	/// Clears all options.
	/// </summary>
	public void Clear() => _options.Clear();

	/// <summary>
	/// Creates a new options instance with default settings.
	/// </summary>
	/// <returns>A new options instance.</returns>
	public static SerializerOptions Default() => new();

	/// <summary>
	/// Creates a deep clone of the current options instance.
	/// </summary>
	/// <returns>A new options instance with the same values.</returns>
	public SerializerOptions Clone()
	{
		SerializerOptions clone = new();
		foreach (KeyValuePair<string, object> kvp in _options)
		{
			clone._options[kvp.Key] = kvp.Value;
		}

		return clone;
	}

	// Use string conversion for types not natively supported by the serializer
	/// <summary>
	/// Gets or sets a value indicating whether to use string conversion for types not natively supported by the serializer.
	/// </summary>
	public bool UseStringConversionForUnsupportedTypes { get; set; } = true;

	// Type discriminator settings for polymorphic serialization
	/// <summary>
	/// Gets or sets a value indicating whether to enable type discriminators for polymorphic serialization.
	/// </summary>
	public bool EnableTypeDiscriminator { get; set; }

	/// <summary>
	/// Gets or sets the format for type discriminators.
	/// </summary>
	public TypeDiscriminatorFormat TypeDiscriminatorFormat { get; set; } = TypeDiscriminatorFormat.Property;

	/// <summary>
	/// Gets or sets the property name used for type discriminators.
	/// </summary>
	public string TypeDiscriminatorPropertyName { get; set; } = "$type";

	/// <summary>
	/// Gets or sets a value indicating whether to use fully qualified type names in type discriminators.
	/// </summary>
	public bool UseFullyQualifiedTypeNames { get; set; }

	// Compression settings
	/// <summary>
	/// Gets or sets a value indicating whether compression is enabled.
	/// </summary>
	public bool EnableCompression { get; set; }

	/// <summary>
	/// Gets or sets the compression type to use.
	/// </summary>
	public CompressionType CompressionType { get; set; } = CompressionType.GZip;

	/// <summary>
	/// Gets or sets the compression level (0-9, where higher values provide better compression but slower speed).
	/// </summary>
	public int CompressionLevel { get; set; } = 6; // Default compression level

	// Format-specific settings dictionary for advanced customization
	// These will be handled internally by each serializer implementation
	internal Dictionary<string, object> FormatSpecificOptions { get; } = [];
}
