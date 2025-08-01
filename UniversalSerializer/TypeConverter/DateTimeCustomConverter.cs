// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.TypeConverter;
using System;

/// <summary>
/// A custom converter for DateTime types with configurable format.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DateTimeCustomConverter"/> class with a custom format.
/// </remarks>
/// <param name="format">The custom date format string.</param>
public class DateTimeCustomConverter(string format) : ICustomTypeConverter<DateTime>
{
	private readonly string _format = format ?? throw new ArgumentNullException(nameof(format));

	/// <summary>
	/// Initializes a new instance of the <see cref="DateTimeCustomConverter"/> class with the default format.
	/// </summary>
	public DateTimeCustomConverter()
		: this("yyyy-MM-dd HH:mm:ss")
	{
	}

	/// <inheritdoc/>
	public string ConvertToString(DateTime value) => value.ToString(_format);

	/// <inheritdoc/>
	public DateTime ConvertFromString(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return DateTime.MinValue;
		}

		if (DateTime.TryParse(value, out DateTime result))
		{
			return result;
		}

		try
		{
			return DateTime.ParseExact(value, _format, null);
		}
		catch (FormatException)
		{
			// Fall back to default value if parsing fails
			return DateTime.MinValue;
		}
	}
}
