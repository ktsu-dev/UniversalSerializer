// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Test;
using System;
using ktsu.UniversalSerializer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests for type converters.
/// </summary>
[TestClass]
public class TypeConverterTests
{
	/// <summary>
	/// Gets or sets the type converter registry used for testing.
	/// </summary>
	public required TypeConverterRegistry Registry { get; set; }

	/// <summary>
	/// Initializes the test environment.
	/// </summary>
	[TestInitialize]
	public void Initialize()
	{
		Registry = new TypeConverterRegistry();
	}

	/// <summary>
	/// Test built-in string convertible type converter.
	/// </summary>
	[TestMethod]
	public void StringConvertibleTypeConverter_ConvertToAndFromString_ReturnsOriginalValue()
	{
		// Test int to string conversion
		int intValue = 42;
		string stringValue = Registry.ConvertToString(intValue);
		Assert.AreEqual("42", stringValue);
		int convertedIntValue = Registry.ConvertFromString<int>(stringValue);
		Assert.AreEqual(intValue, convertedIntValue);

		// Test bool to string conversion
		bool boolValue = true;
		stringValue = Registry.ConvertToString(boolValue);
		Assert.AreEqual("True", stringValue);
		bool convertedBoolValue = Registry.ConvertFromString<bool>(stringValue);
		Assert.AreEqual(boolValue, convertedBoolValue);

		// Test DateTime to string conversion (using built-in converter)
		DateTime dateTimeValue = new(2023, 1, 1, 12, 0, 0);
		stringValue = Registry.ConvertToString(dateTimeValue);
		DateTime convertedDateTimeValue = Registry.ConvertFromString<DateTime>(stringValue);
		Assert.AreEqual(dateTimeValue, convertedDateTimeValue);
	}

	/// <summary>
	/// Test custom date time converter with custom format.
	/// </summary>
	[TestMethod]
	public void CustomDateTimeConverter_ConvertToAndFromString_UsesCustomFormat()
	{
		// Arrange
		string customFormat = "yyyy/MM/dd";
		DateTimeCustomConverter customConverter = new(customFormat);
		Registry.RegisterConverter(customConverter);

		// Act
		DateTime dateTimeValue = new(2023, 1, 1);
		string stringValue = Registry.ConvertToString(dateTimeValue);
		DateTime convertedDateTimeValue = Registry.ConvertFromString<DateTime>(stringValue);

		// Assert
		Assert.AreEqual("2023/01/01", stringValue);
		Assert.AreEqual(dateTimeValue, convertedDateTimeValue);
	}

	/// <summary>
	/// Test custom converter registration and removal.
	/// </summary>
	[TestMethod]
	public void TypeConverterRegistry_RegisterAndRemoveConverter_WorksCorrectly()
	{
		// Arrange
		DateTimeCustomConverter customConverter = new("yyyy-MM-dd");

		// Act & Assert - Before Registration
		Assert.IsTrue(Registry.HasConverter(typeof(DateTime)), "DateTime should have a converter by default");

		// Register custom converter
		Registry.RegisterConverter(customConverter);

		// Act & Assert - After Registration
		DateTime dateTimeValue = new(2023, 1, 1);
		string stringValue = Registry.ConvertToString(dateTimeValue);
		Assert.AreEqual("2023-01-01", stringValue);

		// Remove converter
		bool removed = Registry.RemoveConverter<DateTime>();

		// Act & Assert - After Removal
		Assert.IsTrue(removed, "Converter should be removed successfully");
		// The default converter should still handle DateTime after custom converter is removed
		Assert.IsTrue(Registry.HasConverter(typeof(DateTime)), "DateTime should still have a converter after removal");
	}

	/// <summary>
	/// Define a custom type for testing.
	/// </summary>
	private sealed class CustomType
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;

		public override string ToString() => $"{Id}:{Name}";
	}

	/// <summary>
	/// Define a custom converter for the CustomType.
	/// </summary>
	private sealed class CustomTypeConverter : ICustomTypeConverter<CustomType>
	{
		public string ConvertToString(CustomType value)
		{
			return $"{value.Id}|{value.Name}";
		}

		public CustomType ConvertFromString(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return new CustomType();
			}

			string[] parts = value.Split('|');
			return new CustomType
			{
				Id = parts.Length > 0 && int.TryParse(parts[0], out int id) ? id : 0,
				Name = parts.Length > 1 ? parts[1] : string.Empty
			};
		}
	}

	/// <summary>
	/// Test custom converter for a custom type.
	/// </summary>
	[TestMethod]
	public void CustomConverter_ForCustomType_WorksCorrectly()
	{
		// Arrange
		CustomTypeConverter customConverter = new();
		Registry.RegisterConverter(customConverter);

		// Act
		CustomType customObj = new()
		{ Id = 123, Name = "Test" };
		string stringValue = Registry.ConvertToString(customObj);
		CustomType convertedObj = Registry.ConvertFromString<CustomType>(stringValue);

		// Assert
		Assert.AreEqual("123|Test", stringValue);
		Assert.AreEqual(customObj.Id, convertedObj.Id);
		Assert.AreEqual(customObj.Name, convertedObj.Name);
	}
}
