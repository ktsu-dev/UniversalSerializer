// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Serialization;
using System.Collections.Concurrent;

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
	public T? GetOption<T>(string key) => _options.TryGetValue(key, out var value) && value is T typedValue ? typedValue : default;

	/// <summary>
	/// Gets an option with the specified key, or a default value if not found.
	/// </summary>
	/// <typeparam name="T">The type of the option value.</typeparam>
	/// <param name="key">The option key.</param>
	/// <param name="defaultValue">The default value to return if the option is not found.</param>
	/// <returns>The option value, or the specified default value if not found.</returns>
	public T GetOption<T>(string key, T defaultValue) => _options.TryGetValue(key, out var value) && value is T typedValue ? typedValue : defaultValue;

	/// <summary>
	/// Tries to get an option with the specified key.
	/// </summary>
	/// <typeparam name="T">The type of the option value.</typeparam>
	/// <param name="key">The option key.</param>
	/// <param name="value">When this method returns, contains the value associated with the specified key, if found; otherwise, the default value for the type.</param>
	/// <returns>true if the option was found; otherwise, false.</returns>
	public bool TryGetOption<T>(string key, out T? value)
	{
		if (_options.TryGetValue(key, out var objValue) && objValue is T typedValue)
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
		var clone = new SerializerOptions();
		foreach (var kvp in _options)
		{
			clone._options[kvp.Key] = kvp.Value;
		}

		return clone;
	}
}
