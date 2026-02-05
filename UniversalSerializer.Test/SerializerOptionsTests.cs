// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Test;

using ktsu.UniversalSerializer.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests for the SerializerOptions class.
/// </summary>
[TestClass]
public class SerializerOptionsTests
{
	/// <summary>
	/// Tests setting and getting options.
	/// </summary>
	[TestMethod]
	public void SerializerOptions_SetAndGetOption_Success()
	{
		// Arrange
		SerializerOptions options = new();

		// Act
		options.SetOption("TestKey", "TestValue");
		string? result = options.GetOption<string>("TestKey");

		// Assert
		Assert.AreEqual("TestValue", result);
	}

	/// <summary>
	/// Tests getting option with default value.
	/// </summary>
	[TestMethod]
	public void SerializerOptions_GetOptionWithDefault_ReturnsDefault()
	{
		// Arrange
		SerializerOptions options = new();

		// Act
		string result = options.GetOption("NonExistentKey", "DefaultValue");

		// Assert
		Assert.AreEqual("DefaultValue", result);
	}

	/// <summary>
	/// Tests getting option with default value when option exists.
	/// </summary>
	[TestMethod]
	public void SerializerOptions_GetOptionWithDefault_ReturnsActualValue()
	{
		// Arrange
		SerializerOptions options = new();
		options.SetOption("TestKey", "ActualValue");

		// Act
		string result = options.GetOption("TestKey", "DefaultValue");

		// Assert
		Assert.AreEqual("ActualValue", result);
	}

	/// <summary>
	/// Tests TryGetOption method.
	/// </summary>
	[TestMethod]
	public void SerializerOptions_TryGetOption_Success()
	{
		// Arrange
		SerializerOptions options = new();
		options.SetOption("TestKey", 42);

		// Act
		bool success = options.TryGetOption("TestKey", out int value);

		// Assert
		Assert.IsTrue(success, "TryGetOption should return true for an existing key");
		Assert.AreEqual(42, value);
	}

	/// <summary>
	/// Tests TryGetOption method with non-existent key.
	/// </summary>
	[TestMethod]
	public void SerializerOptions_TryGetOption_NonExistentKey_ReturnsFalse()
	{
		// Arrange
		SerializerOptions options = new();

		// Act
		bool success = options.TryGetOption("NonExistentKey", out int value);

		// Assert
		Assert.IsFalse(success, "TryGetOption should return false for a non-existent key");
		Assert.AreEqual(0, value);
	}

	/// <summary>
	/// Tests HasOption method.
	/// </summary>
	[TestMethod]
	public void SerializerOptions_HasOption_ReturnsCorrectValue()
	{
		// Arrange
		SerializerOptions options = new();
		options.SetOption("TestKey", "TestValue");

		// Act & Assert
		Assert.IsTrue(options.HasOption("TestKey"), "HasOption should return true for an existing key");
		Assert.IsFalse(options.HasOption("NonExistentKey"), "HasOption should return false for a non-existent key");
	}

	/// <summary>
	/// Tests RemoveOption method.
	/// </summary>
	[TestMethod]
	public void SerializerOptions_RemoveOption_Success()
	{
		// Arrange
		SerializerOptions options = new();
		options.SetOption("TestKey", "TestValue");

		// Act
		bool removed = options.RemoveOption("TestKey");

		// Assert
		Assert.IsTrue(removed, "RemoveOption should return true when removing an existing key");
		Assert.IsFalse(options.HasOption("TestKey"), "HasOption should return false after key is removed");
	}

	/// <summary>
	/// Tests RemoveOption method with non-existent key.
	/// </summary>
	[TestMethod]
	public void SerializerOptions_RemoveOption_NonExistentKey_ReturnsFalse()
	{
		// Arrange
		SerializerOptions options = new();

		// Act
		bool removed = options.RemoveOption("NonExistentKey");

		// Assert
		Assert.IsFalse(removed, "RemoveOption should return false for a non-existent key");
	}

	/// <summary>
	/// Tests Clear method.
	/// </summary>
	[TestMethod]
	public void SerializerOptions_Clear_RemovesAllOptions()
	{
		// Arrange
		SerializerOptions options = new();
		options.SetOption("Key1", "Value1");
		options.SetOption("Key2", "Value2");

		// Act
		options.Clear();

		// Assert
		Assert.AreEqual(0, options.Count);
		Assert.IsFalse(options.HasOption("Key1"), "HasOption should return false for Key1 after Clear");
		Assert.IsFalse(options.HasOption("Key2"), "HasOption should return false for Key2 after Clear");
	}

	/// <summary>
	/// Tests Count property.
	/// </summary>
	[TestMethod]
	public void SerializerOptions_Count_ReturnsCorrectValue()
	{
		// Arrange
		SerializerOptions options = new();

		// Act & Assert
		Assert.AreEqual(0, options.Count);

		options.SetOption("Key1", "Value1");
		Assert.AreEqual(1, options.Count);

		options.SetOption("Key2", "Value2");
		Assert.AreEqual(2, options.Count);

		options.RemoveOption("Key1");
		Assert.AreEqual(1, options.Count);
	}

	/// <summary>
	/// Tests Clone method.
	/// </summary>
	[TestMethod]
	public void SerializerOptions_Clone_CreatesIndependentCopy()
	{
		// Arrange
		SerializerOptions original = new();
		original.SetOption("Key1", "Value1");
		original.SetOption("Key2", 42);
		original.EnableCompression = true;
		original.CompressionLevel = 9;

		// Act
		SerializerOptions clone = original.Clone();

		// Assert
		Assert.AreEqual(original.Count, clone.Count);
		Assert.AreEqual("Value1", clone.GetOption<string>("Key1"));
		Assert.AreEqual(42, clone.GetOption<int>("Key2"));
		Assert.AreEqual(original.EnableCompression, clone.EnableCompression);
		Assert.AreEqual(original.CompressionLevel, clone.CompressionLevel);

		// Verify independence
		clone.SetOption("Key3", "Value3");
		Assert.IsFalse(original.HasOption("Key3"), "Original should not have Key3 after adding it to clone");
	}

	/// <summary>
	/// Tests Default method.
	/// </summary>
	[TestMethod]
	public void SerializerOptions_Default_ReturnsNewInstance()
	{
		// Act
		SerializerOptions options1 = SerializerOptions.Default();
		SerializerOptions options2 = SerializerOptions.Default();

		// Assert
		Assert.IsNotNull(options1);
		Assert.IsNotNull(options2);
		Assert.AreNotSame(options1, options2);
	}

	/// <summary>
	/// Tests default property values.
	/// </summary>
	[TestMethod]
	public void SerializerOptions_DefaultValues_AreCorrect()
	{
		// Arrange
		SerializerOptions options = new();

		// Assert
		Assert.IsTrue(options.UseStringConversionForUnsupportedTypes, "UseStringConversionForUnsupportedTypes should default to true");
		Assert.IsFalse(options.EnableTypeDiscriminator, "EnableTypeDiscriminator should default to false");
		Assert.AreEqual(TypeDiscriminatorFormat.Property, options.TypeDiscriminatorFormat);
		Assert.AreEqual("$type", options.TypeDiscriminatorPropertyName);
		Assert.IsFalse(options.UseFullyQualifiedTypeNames, "UseFullyQualifiedTypeNames should default to false");
		Assert.IsFalse(options.EnableCompression, "EnableCompression should default to false");
		Assert.AreEqual(CompressionType.GZip, options.CompressionType);
		Assert.AreEqual(6, options.CompressionLevel);
	}

	/// <summary>
	/// Tests method chaining with SetOption.
	/// </summary>
	[TestMethod]
	public void SerializerOptions_SetOption_SupportsMethodChaining()
	{
		// Arrange
		SerializerOptions options = new();

		// Act
		SerializerOptions result = options
			.SetOption("Key1", "Value1")
			.SetOption("Key2", 42)
			.SetOption("Key3", true);

		// Assert
		Assert.AreSame(options, result);
		Assert.AreEqual(3, options.Count);
		Assert.AreEqual("Value1", options.GetOption<string>("Key1"));
		Assert.AreEqual(42, options.GetOption<int>("Key2"));
		Assert.AreEqual(true, options.GetOption<bool>("Key3"));
	}

	/// <summary>
	/// Tests type safety with wrong type casting.
	/// </summary>
	[TestMethod]
	public void SerializerOptions_GetOption_WrongType_ReturnsDefault()
	{
		// Arrange
		SerializerOptions options = new();
		options.SetOption("StringKey", "StringValue");

		// Act
		int result = options.GetOption<int>("StringKey");

		// Assert
		Assert.AreEqual(0, result); // Default value for int
	}
}
