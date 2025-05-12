// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Test;
using ktsu.UniversalSerializer.Serialization;
=======
using ktsu.UniversalSerializer.Serialization.Json;
using ktsu.UniversalSerializer.Serialization.Toml;
using ktsu.UniversalSerializer.Serialization.Xml;
using ktsu.UniversalSerializer.Serialization.Yaml;
using ktsu.UniversalSerializer.Serialization.MessagePack;
using ktsu.UniversalSerializer.Serialization.Protobuf;
using ktsu.UniversalSerializer.Serialization.FlatBuffers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
>>>>>>> After
using ktsu.UniversalSerializer.Serialization.FlatBuffers;
using ktsu.UniversalSerializer.Serialization.Json;
using ktsu.UniversalSerializer.Serialization.MessagePack;
using ktsu.UniversalSerializer.Serialization.Protobuf;
using ktsu.UniversalSerializer.Serialization.Toml;
using ktsu.UniversalSerializer.Serialization.Xml;
using ktsu.UniversalSerializer.Serialization.Yaml;
<<<<<<< TODO: Unmerged change from project 'UniversalSerializer.Test(net9.0)', Before:
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
		var protobuf = _factory.Create<ProtobufSerializer>();

		// Act
		SerializerRegistry.Register("json", json);
		SerializerRegistry.Register("xml", xml);
		SerializerRegistry.Register("yaml", yaml);
		SerializerRegistry.Register("toml", toml);
		SerializerRegistry.Register("messagepack", messagepack);
		_registry.Register("protobuf", protobuf);

		// Assert
		Assert.IsTrue(_registry.IsFormatSupported("json"));
		Assert.IsTrue(_registry.IsFormatSupported("xml"));
		Assert.IsTrue(_registry.IsFormatSupported("yaml"));
		Assert.IsTrue(_registry.IsFormatSupported("toml"));
		Assert.IsTrue(_registry.IsFormatSupported("messagepack"));
		Assert.IsTrue(_registry.IsFormatSupported("protobuf"));

		Assert.IsTrue(_registry.IsExtensionSupported(".json"));
		Assert.IsTrue(_registry.IsExtensionSupported(".xml"));
		Assert.IsTrue(_registry.IsExtensionSupported(".yaml"));
		Assert.IsTrue(_registry.IsExtensionSupported(".toml"));
		Assert.IsTrue(_registry.IsExtensionSupported(".msgpack"));
		Assert.IsTrue(_registry.IsExtensionSupported(".proto"));
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
	/// Tests the YAML serializer with simple types.
	/// </summary>
	[TestMethod]
	public void YamlSerializer_SerializeDeserialize_SimpleType_Success()
	{
		// Arrange
		var serializer = _factory.Create<YamlSerializer>();
		var yamlSerializer = _registry.GetSerializer("yaml") as YamlSerializer;
		Assert.IsNotNull(yamlSerializer, "YAML serializer should be available");

		var testObject = new TestModel
		{
			Id = 1,
			Name = "Test Object",
			CreatedAt = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc),
			IsActive = true,
			Tags = ["test", "serialization", "yaml"]

<<<<<<< TODO: Unmerged change from project 'UniversalSerializer.Test(net9.0)', Before:

<<<<<<< TODO: Unmerged change from project 'UniversalSerializer.Test(net9.0)', Before:
        };

        // Act
        var serialized = yamlSerializer.Serialize(testObject);
=======
		};

		// Act
		var serialized = yamlSerializer.Serialize(testObject);
>>>>>>> After
		};
=======
		};
>>>>>>> After

<<<<<<< TODO: Unmerged change from project 'UniversalSerializer.Test(net9.0)', Before:
		var deserialized = yamlSerializer.Deserialize<TestModel>(serialized);
=======
=======
		};

		// Act
		var serialized = yamlSerializer.Serialize(testObject);
>>>>>>> After
	};

	private var deserialized = yamlSerializer.Deserialize<TestModel>(serialized);
>>>>>>> After
		};

		// Act
		var serialized = yamlSerializer.Serialize(testObject);
=======
		};

		// Act
		var serialized = yamlSerializer.Serialize(testObject);
>>>>>>> After
	};

	private var deserialized = yamlSerializer.Deserialize<TestModel>(serialized);

	// Assert
	Assert.IsNotNull(deserialized);
		Assert.AreEqual(testObject.Id, deserialized.Id);
		Assert.AreEqual(testObject.Name, deserialized.Name);
		Assert.AreEqual(testObject.CreatedAt, deserialized.CreatedAt);
		Assert.AreEqual(testObject.IsActive, deserialized.IsActive);
		CollectionAssert.AreEqual(testObject.Tags, deserialized.Tags);
	}

	/// <summary>
	/// Test TOML serialization.
	/// </summary>
	[TestMethod]
	public void TomlSerializer_SerializeAndDeserialize_ReturnsOriginalObject()
	{
		// Arrange
		var tomlSerializer = _registry.GetSerializer("toml") as TomlSerializer;
		Assert.IsNotNull(tomlSerializer, "TOML serializer should be available");

		var testObject = new TestModel
		{
			Id = 1,
			Name = "Test Object",
			CreatedAt = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc),
			IsActive = true,
			Tags = ["test", "serialization", "toml"]

<<<<<<< TODO: Unmerged change from project 'UniversalSerializer.Test(net9.0)', Before:

<<<<<<< TODO: Unmerged change from project 'UniversalSerializer.Test(net9.0)', Before:
        };

        // Act
        var serialized = tomlSerializer.Serialize(testObject);
=======
		};

		// Act
		var serialized = tomlSerializer.Serialize(testObject);
>>>>>>> After
		};
=======
		};
>>>>>>> After

<<<<<<< TODO: Unmerged change from project 'UniversalSerializer.Test(net9.0)', Before:
		var deserialized = tomlSerializer.Deserialize<TestModel>(serialized);
=======
=======
		};

		// Act
		var serialized = tomlSerializer.Serialize(testObject);
>>>>>>> After
	};

	private var deserialized = tomlSerializer.Deserialize<TestModel>(serialized);
>>>>>>> After
		};

		// Act
		var serialized = tomlSerializer.Serialize(testObject);
=======
		};

		// Act
		var serialized = tomlSerializer.Serialize(testObject);
>>>>>>> After
	};

	private var deserialized = tomlSerializer.Deserialize<TestModel>(serialized);

		// Assert
		Assert.IsNotNull(deserialized);
		Assert.AreEqual(testObject.Id, deserialized.Id);
		Assert.AreEqual(testObject.Name, deserialized.Name);
		Assert.AreEqual(testObject.CreatedAt, deserialized.CreatedAt);
		Assert.AreEqual(testObject.IsActive, deserialized.IsActive);
		CollectionAssert.AreEqual(testObject.Tags, deserialized.Tags);
	}
;

// Act
// Test string-based serialization (Base64 encoded)
var serialized = messagePackSerializer.Serialize(testObject);
var deserialized = messagePackSerializer.Deserialize<TestModel>(serialized);

// Test binary serialization
var bytesSerialized = messagePackSerializer.SerializeToBytes(testObject);
var bytesDeserialized = messagePackSerializer.DeserializeFromBytes<TestModel>(bytesSerialized);

// Assert
Assert.IsNotNull(deserialized);
Assert.AreEqual(testObject.Id, deserialized.Id);
Assert.AreEqual(testObject.Name, deserialized.Name);
Assert.AreEqual(testObject.CreatedAt, deserialized.CreatedAt);
Assert.AreEqual(testObject.IsActive, deserialized.IsActive);
CollectionAssert.AreEqual(testObject.Tags, deserialized.Tags);

Assert.IsNotNull(bytesDeserialized);
Assert.AreEqual(testObject.Id, bytesDeserialized.Id);
Assert.AreEqual(testObject.Name, bytesDeserialized.Name);
Assert.AreEqual(testObject.CreatedAt, bytesDeserialized.CreatedAt);
Assert.AreEqual(testObject.IsActive, bytesDeserialized.IsActive);
CollectionAssert.AreEqual(testObject.Tags, bytesDeserialized.Tags);
	}
;

// Act
var bytes = serializer.SerializeToBytes(testObject);
var result = serializer.DeserializeFromBytes<TestClass>(bytes);

// Assert
Assert.IsNotNull(result);
Assert.AreEqual(testObject.Id, result.Id);
Assert.AreEqual(testObject.Name, result.Name);
Assert.AreEqual(testObject.Description, result.Description);
CollectionAssert.AreEqual(testObject.Tags, result.Tags);
	}
;

// Act
var bytes = serializer.SerializeToBytes(testObject);
var result = serializer.DeserializeFromBytes<TestClass>(bytes);

// Assert
Assert.IsNotNull(result);
Assert.AreEqual(testObject.Id, result.Id);
Assert.AreEqual(testObject.Name, result.Name);
Assert.AreEqual(testObject.Description, result.Description);
CollectionAssert.AreEqual(testObject.Tags, result.Tags);
	}
;

// Act
var bytes = serializer.SerializeToBytes(testObject);
var result = serializer.DeserializeFromBytes<TestClass>(bytes);

// Assert
Assert.IsNotNull(result);
Assert.AreEqual(testObject.Id, result.Id);
Assert.AreEqual(testObject.Name, result.Name);
Assert.AreEqual(testObject.Description, result.Description);
CollectionAssert.AreEqual(testObject.Tags, result.Tags);
	}
;

// Act
var bytes = serializer.SerializeToBytes(testObject);
var result = serializer.DeserializeFromBytes<TestClass>(bytes);

// Assert
Assert.IsNotNull(result);
Assert.AreEqual(testObject.Id, result.Id);
Assert.AreEqual(testObject.Name, result.Name);
Assert.AreEqual(testObject.Description, result.Description);
CollectionAssert.AreEqual(testObject.Tags, result.Tags);
	}
;

// Act
var bytes = serializer.SerializeToBytes(testObject);
var result = serializer.DeserializeFromBytes<MessagePackTestClass>(bytes);

// Assert
Assert.IsNotNull(result);
Assert.AreEqual(testObject.Id, result.Id);
Assert.AreEqual(testObject.Name, result.Name);
Assert.AreEqual(testObject.Description, result.Description);
CollectionAssert.AreEqual(testObject.Tags, result.Tags);
	}
;

// Act
var bytes = serializer.SerializeToBytes(testObject);
var result = serializer.DeserializeFromBytes<ProtobufTestClass>(bytes);

// Assert
Assert.IsNotNull(result);
Assert.AreEqual(testObject.Id, result.Id);
Assert.AreEqual(testObject.Name, result.Name);
Assert.AreEqual(testObject.Description, result.Description);
CollectionAssert.AreEqual(testObject.Tags, result.Tags);
	}


<<<<<<< TODO: Unmerged change from project 'UniversalSerializer.Test(net9.0)', Before:
	/// <summary>
	/// Tests FlatBuffers serialization.
	/// </summary>
	[TestMethod]
	[Ignore("Requires FlatSharp schema generation from FBS files")]
	public void FlatBuffersSerializerTest()
	{
		// Arrange
		var serializer = _registry.GetSerializer("flatbuffers");
		Assert.IsNotNull(serializer);

		// FlatBuffers requires pre-generated serializers from FBS schema files
		// For testing purposes, we would need a class with the appropriate attributes
		// and generated serializers which is beyond the scope of this unit test

		// For integration tests, a proper FBS schema should be created and
		// the FlatSharp compiler should generate the required serialization code
	}

	/// <summary>
	/// A standard test class for serialization.
	/// </summary>
	public class TestClass
	{
		/// <summary>
		/// Gets or sets the ID.
		/// </summary>
		public int Id { get; set; }
=======
/// <summary>
/// A standard test class for serialization.
/// </summary>
public class TestClass
{
	/// <summary>
	/// Gets or sets the ID.
	/// </summary>
	public int Id { get; set; }
>>>>>>> After
/// <summary>
/// A standard test class for serialization.
/// </summary>
public class TestClass
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
	/// Gets or sets the description.
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Gets or sets the tags.
	/// </summary>
	public List<string>? Tags { get; set; }
}

/// <summary>
/// A test class with MessagePack attributes for serialization.
/// </summary>
[MessagePack.MessagePackObject]
public class 
<<<<<<< TODO: Unmerged change from project 'UniversalSerializer.Test(net9.0)', Before:
		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		public string? Name { get; set; }
=======
	/// <summary>
	/// Gets or sets the name.
	/// </summary>
	public string? Name { get; set; }
>>>>>>> After

<<<<<<< TODO: Unmerged change from project 'UniversalSerializer.Test(net9.0)', Before:
		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		public string? Description { get; set; }
=======
	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	public string? Description { get; set; }
>>>>>>> After

<<<<<<< TODO: Unmerged change from project 'UniversalSerializer.Test(net9.0)', Before:
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
=======
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
>>>>>>> After
MessagePackTestClass
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

/// <summary>
/// A test class with Protocol Buffers attributes for serialization.
/// </summary>
[ProtoBuf.ProtoContract]
public class ProtobufTestClass
{
	/// <summary>
	/// Gets or sets the ID.
	/// </summary>
	[ProtoBuf.ProtoMember(1)]
	public int Id { get; set; }

	/// <summary>
	/// Gets or sets the name.
	/// </summary>
	[ProtoBuf.ProtoMember(2)]
	public string? Name { get; set; }

	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	[ProtoBuf.ProtoMember(3)]
	public string? Description { get; set; }

	/// <summary>
	/// Gets or sets the tags.
	/// </summary>
	[ProtoBuf.ProtoMember(4)]
	public List<string>? Tags { get; set; }
}

/// <summary>
/// A simple FlatBuffers test class (would normally be generated from an FBS schema).
/// This is for illustrative purposes only and would not actually be used in tests as-is.
/// </summary>
[FlatSharp.FlatBufferTable]
public class FlatBuffersTestClass
{
	[FlatSharp.FlatBufferItem(0)]
	public virtual int Id { get; set; }

	[FlatSharp.FlatBufferItem(1)]
	public virtual string? Name { get; set; }

	[FlatSharp.FlatBufferItem(2)]
	public virtual string? Description { get; set; }

	[FlatSharp.FlatBufferItem(3)]
	public virtual IList<string>? Tags { get; set; }

	// FlatSharp generates a Serializer property automatically which the serializer looks for
	// via reflection when writing/reading FlatBuffers data
	// public static readonly ISerializer<FlatBuffersTestClass> Serializer = /* generated code */;
}
}

/// <summary>
/// Sample test model for serialization tests.
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
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the creation date.
	/// </summary>
	public DateTime CreatedAt { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the model is active.
	/// </summary>
	public bool IsActive { get; set; }

	/// <summary>
	/// Gets or sets the tags.
	/// </summary>
	public List<string> Tags { get; set; } = [];
}
