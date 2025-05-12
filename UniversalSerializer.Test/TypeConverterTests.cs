// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using ktsu.UniversalSerializer.Serialization.TypeConverter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ktsu.UniversalSerializer.Test;

/// <summary>
/// Tests for type converters.
/// </summary>
[TestClass]
public class TypeConverterTests
{
    private TypeConverterRegistry _registry;

    /// <summary>
    /// Initializes the test environment.
    /// </summary>
    [TestInitialize]
    public void Initialize()
    {
        _registry = new TypeConverterRegistry();
    }

    /// <summary>
    /// Test built-in string convertible type converter.
    /// </summary>
    [TestMethod]
    public void StringConvertibleTypeConverter_ConvertToAndFromString_ReturnsOriginalValue()
    {
        // Test int to string conversion
        int intValue = 42;
        string stringValue = _registry.ConvertToString(intValue);
        Assert.AreEqual("42", stringValue);
        int convertedIntValue = _registry.ConvertFromString<int>(stringValue);
        Assert.AreEqual(intValue, convertedIntValue);

        // Test bool to string conversion
        bool boolValue = true;
        stringValue = _registry.ConvertToString(boolValue);
        Assert.AreEqual("True", stringValue);
        bool convertedBoolValue = _registry.ConvertFromString<bool>(stringValue);
        Assert.AreEqual(boolValue, convertedBoolValue);

        // Test DateTime to string conversion (using built-in converter)
        DateTime dateTimeValue = new DateTime(2023, 1, 1, 12, 0, 0);
        stringValue = _registry.ConvertToString(dateTimeValue);
        DateTime convertedDateTimeValue = _registry.ConvertFromString<DateTime>(stringValue);
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
        var customConverter = new DateTimeCustomConverter(customFormat);
        _registry.RegisterConverter(customConverter);

        // Act
        DateTime dateTimeValue = new DateTime(2023, 1, 1);
        string stringValue = _registry.ConvertToString(dateTimeValue);
        DateTime convertedDateTimeValue = _registry.ConvertFromString<DateTime>(stringValue);

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
        var customConverter = new DateTimeCustomConverter("yyyy-MM-dd");

        // Act & Assert - Before Registration
        Assert.IsTrue(_registry.HasConverter(typeof(DateTime)), "DateTime should have a converter by default");

        // Register custom converter
        _registry.RegisterConverter(customConverter);

        // Act & Assert - After Registration
        var dateTimeValue = new DateTime(2023, 1, 1);
        var stringValue = _registry.ConvertToString(dateTimeValue);
        Assert.AreEqual("2023-01-01", stringValue);

        // Remove converter
        bool removed = _registry.RemoveConverter<DateTime>();

        // Act & Assert - After Removal
        Assert.IsTrue(removed, "Converter should be removed successfully");
        // The default converter should still handle DateTime after custom converter is removed
        Assert.IsTrue(_registry.HasConverter(typeof(DateTime)), "DateTime should still have a converter after removal");
    }

    /// <summary>
    /// Define a custom type for testing.
    /// </summary>
    private class CustomType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public override string ToString() => $"{Id}:{Name}";
    }

    /// <summary>
    /// Define a custom converter for the CustomType.
    /// </summary>
    private class CustomTypeConverter : ICustomTypeConverter<CustomType>
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

            var parts = value.Split('|');
            return new CustomType
            {
                Id = parts.Length > 0 && int.TryParse(parts[0], out var id) ? id : 0,
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
        var customConverter = new CustomTypeConverter();
        _registry.RegisterConverter(customConverter);

        // Act
        var customObj = new CustomType { Id = 123, Name = "Test" };
        var stringValue = _registry.ConvertToString(customObj);
        var convertedObj = _registry.ConvertFromString<CustomType>(stringValue);

        // Assert
        Assert.AreEqual("123|Test", stringValue);
        Assert.AreEqual(customObj.Id, convertedObj.Id);
        Assert.AreEqual(customObj.Name, convertedObj.Name);
    }
}
