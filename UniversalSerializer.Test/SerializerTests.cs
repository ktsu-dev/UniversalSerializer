// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Test;

using System.Collections.Generic;
using ktsu.UniversalSerializer.Serialization;
using ktsu.UniversalSerializer.Serialization.Json;
using ktsu.UniversalSerializer.Serialization.MessagePack;
using ktsu.UniversalSerializer.Serialization.Toml;
using ktsu.UniversalSerializer.Serialization.Xml;
using ktsu.UniversalSerializer.Serialization.Yaml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests for serializers.
/// </summary>
[TestClass]
public class SerializerTests
{
	private SerializerFactory _factory = null!;
	private SerializerRegistry _registry = null!;
	private SerializerOptions _options = null!;

	/// <summary>
	/// Initializes the test environment.
	/// </summary>
	[TestInitialize]
	public void Initialize()
	{
		_factory = new SerializerFactory();
		_options = SerializerOptions.Default();
		_registry = new SerializerRegistry(_factory);
		_registry.RegisterBuiltIn(_options);
	}

	/// <summary>
	/// Tests registration of serializers in the registry.
	/// </summary>
	[TestMethod]
	public void SerializerRegistry_RegisterSerializers_Success()
	{
		// Arrange
		var json = _factory.Create<JsonSerializer>();
		var xml = _factory.Create<XmlSerializer>();
		var yaml = _factory.Create<YamlSerializer>();
		var toml = _factory.Create<TomlSerializer>();
		var messagepack = _factory.Create<MessagePackSerializer>();

		// Act
		_registry.Register("json", json);
		_registry.Register("xml", xml);
		_registry.Register("yaml", yaml);
		_registry.Register("toml", toml);
		_registry.Register("messagepack", messagepack);

		// Assert
		Assert.IsTrue(_registry.IsFormatSupported("json"));
		Assert.IsTrue(_registry.IsFormatSupported("xml"));
		Assert.IsTrue(_registry.IsFormatSupported("yaml"));
		Assert.IsTrue(_registry.IsFormatSupported("toml"));
		Assert.IsTrue(_registry.IsFormatSupported("messagepack"));

		Assert.IsTrue(_registry.IsExtensionSupported(".json"));
		Assert.IsTrue(_registry.IsExtensionSupported(".xml"));
		Assert.IsTrue(_registry.IsExtensionSupported(".yaml"));
		Assert.IsTrue(_registry.IsExtensionSupported(".toml"));
		Assert.IsTrue(_registry.IsExtensionSupported(".msgpack"));
	}

	/// <summary>
	/// Tests the JSON serializer with simple types.
	/// </summary>
	[TestMethod]
	public void JsonSerializer_SerializeDeserialize_SimpleType_Success()
	{
		// Arrange
		var serializer = _factory.Create<JsonSerializer>();
		var testData = new TestData { IntValue = 42, StringValue = "Hello, World!" };

		// Act
		var serialized = serializer.Serialize(testData);
		var deserialized = serializer.Deserialize<TestData>(serialized);

		// Assert
		Assert.IsNotNull(deserialized);
		Assert.AreEqual(testData.IntValue, deserialized.IntValue);
		Assert.AreEqual(testData.StringValue, deserialized.StringValue);
	}

	/// <summary>
	/// Tests the XML serializer with simple types.
	/// </summary>
	[TestMethod]
	public void XmlSerializer_SerializeDeserialize_SimpleType_Success()
	{
		// Arrange
		var serializer = _factory.Create<XmlSerializer>();
		var testData = new TestData { IntValue = 42, StringValue = "Hello, World!" };

		// Act
		var serialized = serializer.Serialize(testData);
		var deserialized = serializer.Deserialize<TestData>(serialized);

		// Assert
		Assert.IsNotNull(deserialized);
		Assert.AreEqual(testData.IntValue, deserialized.IntValue);
		Assert.AreEqual(testData.StringValue, deserialized.StringValue);
	}

	/// <summary>
	/// Tests the YAML serializer with simple types.
	/// </summary>
	[TestMethod]
	public void YamlSerializer_SerializeDeserialize_SimpleType_Success()
	{
		// Arrange
		var serializer = _factory.Create<YamlSerializer>();
		var testObject = new TestModel
		{
			Id = 1,
			Name = "Test Object",
			CreatedAt = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc),
			IsActive = true,
			Tags = ["test", "serialization", "yaml"]
		};

		// Act
		var serialized = serializer.Serialize(testObject);
		var deserialized = serializer.Deserialize<TestModel>(serialized);

		// Assert
		Assert.IsNotNull(deserialized);
		Assert.AreEqual(testObject.Id, deserialized.Id);
		Assert.AreEqual(testObject.Name, deserialized.Name);
		Assert.AreEqual(testObject.CreatedAt, deserialized.CreatedAt);
		Assert.AreEqual(testObject.IsActive, deserialized.IsActive);
		CollectionAssert.AreEqual(testObject.Tags, deserialized.Tags);
	}

	/// <summary>
	/// Tests the TOML serializer with simple types.
	/// </summary>
	[TestMethod]
	public void TomlSerializer_SerializeDeserialize_SimpleType_Success()
	{
		// Arrange
		var serializer = _factory.Create<TomlSerializer>();
		var testObject = new TestModel
		{
			Id = 1,
			Name = "Test Object",
			CreatedAt = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc),
			IsActive = true,
			Tags = ["test", "serialization", "toml"]
		};

		// Act
		var serialized = serializer.Serialize(testObject);
		var deserialized = serializer.Deserialize<TestModel>(serialized);

		// Assert
		Assert.IsNotNull(deserialized);
		Assert.AreEqual(testObject.Id, deserialized.Id);
		Assert.AreEqual(testObject.Name, deserialized.Name);
		Assert.AreEqual(testObject.CreatedAt, deserialized.CreatedAt);
		Assert.AreEqual(testObject.IsActive, deserialized.IsActive);
		CollectionAssert.AreEqual(testObject.Tags, deserialized.Tags);
	}

	/// <summary>
	/// Tests the MessagePack serializer with simple types.
	/// </summary>
	[TestMethod]
	public void MessagePackSerializer_SerializeDeserialize_SimpleType_Success()
	{
		// Arrange
		var serializer = _factory.Create<MessagePackSerializer>();
		var testObject = new MessagePackTestClass
		{
			Id = 1,
			Name = "Test Object",
			Description = "A test object for MessagePack serialization",
			Tags = ["test", "serialization", "messagepack"]
		};

		// Act
		var serialized = serializer.SerializeToBytes(testObject);
		var deserialized = serializer.DeserializeFromBytes<MessagePackTestClass>(serialized);

		// Assert
		Assert.IsNotNull(deserialized);
		Assert.AreEqual(testObject.Id, deserialized.Id);
		Assert.AreEqual(testObject.Name, deserialized.Name);
		Assert.AreEqual(testObject.Description, deserialized.Description);
		CollectionAssert.AreEqual(testObject.Tags, deserialized.Tags);
	}
}

/// <summary>
/// A test data class.
/// </summary>
public class TestData
{
	/// <summary>
	/// Gets or sets the integer value.
	/// </summary>
	public int IntValue { get; set; }

	/// <summary>
	/// Gets or sets the string value.
	/// </summary>
	public string? StringValue { get; set; }
}

/// <summary>
/// A test model class.
/// </summary>
public class TestModel
{
	/// <summary>
	/// Gets or sets the ID.
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Gets or sets the name.
	/// </summary>
	public string? Name { get; set; }

	/// <summary>
	/// Gets or sets the creation date.
	/// </summary>
	public DateTime CreatedAt { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether this instance is active.
	/// </summary>
	public bool IsActive { get; set; }

	/// <summary>
	/// Gets or sets the tags.
	/// </summary>
	public List<string>? Tags { get; set; }
}

/// <summary>
/// A test class with MessagePack attributes for serialization.
/// </summary>
[MessagePack.MessagePackObject]
public class MessagePackTestClass
{
	/// <summary>
	/// Gets or sets the ID.
	/// </summary>
	[MessagePack.Key(0)]
	public int Id { get; set; }

	/// <summary>
	/// Gets or sets the name.
	/// </summary>
	[MessagePack.Key(1)]
	public string? Name { get; set; }

	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	[MessagePack.Key(2)]
	public string? Description { get; set; }

	/// <summary>
	/// Gets or sets the tags.
	/// </summary>
	[MessagePack.Key(3)]
	public List<string>? Tags { get; set; }
}
