// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using ktsu.UniversalSerializer.Serialization;
using ktsu.UniversalSerializer.Serialization.Json;
using ktsu.UniversalSerializer.Serialization.Toml;
using ktsu.UniversalSerializer.Serialization.Xml;
using ktsu.UniversalSerializer.Serialization.Yaml;
using ktsu.UniversalSerializer.Serialization.MessagePack;
using ktsu.UniversalSerializer.Serialization.Protobuf;
using ktsu.UniversalSerializer.Serialization.FlatBuffers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ktsu.UniversalSerializer.Test;

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
        _registry.Register("json", json);
        _registry.Register("xml", xml);
        _registry.Register("yaml", yaml);
        _registry.Register("toml", toml);
        _registry.Register("messagepack", messagepack);
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
        };

        // Act
        var serialized = yamlSerializer.Serialize(testObject);
        var deserialized = yamlSerializer.Deserialize<TestModel>(serialized);

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
        };

        // Act
        var serialized = tomlSerializer.Serialize(testObject);
        var deserialized = tomlSerializer.Deserialize<TestModel>(serialized);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(testObject.Id, deserialized.Id);
        Assert.AreEqual(testObject.Name, deserialized.Name);
        Assert.AreEqual(testObject.CreatedAt, deserialized.CreatedAt);
        Assert.AreEqual(testObject.IsActive, deserialized.IsActive);
        CollectionAssert.AreEqual(testObject.Tags, deserialized.Tags);
    }

    /// <summary>
    /// Test MessagePack serialization.
    /// </summary>
    [TestMethod]
    public void MessagePackSerializer_SerializeAndDeserialize_ReturnsOriginalObject()
    {
        // Arrange
        var messagePackSerializer = _registry.GetSerializer("messagepack") as MessagePackSerializer;
        Assert.IsNotNull(messagePackSerializer, "MessagePack serializer should be available");

        var testObject = new TestModel
        {
            Id = 1,
            Name = "Test Object",
            CreatedAt = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            IsActive = true,
            Tags = ["test", "serialization", "messagepack"]
        };

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

    /// <summary>
    /// Test serializer registry by extension.
    /// </summary>
    [TestMethod]
    public void SerializerRegistry_GetSerializerByExtension_ReturnsCorrectSerializer()
    {
        // Act
        var jsonSerializer = _registry.GetSerializerByExtension(".json");
        var xmlSerializer = _registry.GetSerializerByExtension(".xml");
        var yamlSerializer = _registry.GetSerializerByExtension(".yaml");
        var yamlAltSerializer = _registry.GetSerializerByExtension(".yml");
        var tomlSerializer = _registry.GetSerializerByExtension(".toml");
        var tomlAltSerializer = _registry.GetSerializerByExtension(".tml");
        var messagePackSerializer = _registry.GetSerializerByExtension(".msgpack");
        var messagePackAltSerializer = _registry.GetSerializerByExtension(".mp");
        var unknownSerializer = _registry.GetSerializerByExtension(".unknown");

        // Assert
        Assert.IsNotNull(jsonSerializer, "JSON serializer should be available");
        Assert.IsNotNull(xmlSerializer, "XML serializer should be available");
        Assert.IsNotNull(yamlSerializer, "YAML serializer should be available");
        Assert.IsNotNull(yamlAltSerializer, "YAML serializer should be available for .yml");
        Assert.IsNotNull(tomlSerializer, "TOML serializer should be available");
        Assert.IsNotNull(tomlAltSerializer, "TOML serializer should be available for .tml");
        Assert.IsNotNull(messagePackSerializer, "MessagePack serializer should be available");
        Assert.IsNotNull(messagePackAltSerializer, "MessagePack serializer should be available for .mp");
        Assert.IsNull(unknownSerializer, "Unknown serializer should not be available");

        Assert.AreEqual("application/json", jsonSerializer.ContentType);
        Assert.AreEqual("application/xml", xmlSerializer.ContentType);
        Assert.AreEqual("application/yaml", yamlSerializer.ContentType);
        Assert.AreEqual("application/yaml", yamlAltSerializer.ContentType);
        Assert.AreEqual("application/toml", tomlSerializer.ContentType);
        Assert.AreEqual("application/toml", tomlAltSerializer.ContentType);
        Assert.AreEqual("application/x-msgpack", messagePackSerializer.ContentType);
        Assert.AreEqual("application/x-msgpack", messagePackAltSerializer.ContentType);
    }

    /// <summary>
    /// Test format detection from file path.
    /// </summary>
    [TestMethod]
    public void SerializerRegistry_DetectSerializer_ReturnsCorrectSerializer()
    {
        // Act
        var jsonSerializer = _registry.DetectSerializer("test.json");
        var xmlSerializer = _registry.DetectSerializer("test.xml");
        var yamlSerializer = _registry.DetectSerializer("test.yaml");
        var yamlAltSerializer = _registry.DetectSerializer("test.yml");
        var tomlSerializer = _registry.DetectSerializer("test.toml");
        var tomlAltSerializer = _registry.DetectSerializer("test.tml");
        var messagePackSerializer = _registry.DetectSerializer("test.msgpack");
        var messagePackAltSerializer = _registry.DetectSerializer("test.mp");
        var unknownSerializer = _registry.DetectSerializer("test.unknown");

        // Assert
        Assert.IsNotNull(jsonSerializer, "JSON serializer should be detected");
        Assert.IsNotNull(xmlSerializer, "XML serializer should be detected");
        Assert.IsNotNull(yamlSerializer, "YAML serializer should be detected");
        Assert.IsNotNull(yamlAltSerializer, "YAML serializer should be detected for .yml");
        Assert.IsNotNull(tomlSerializer, "TOML serializer should be detected");
        Assert.IsNotNull(tomlAltSerializer, "TOML serializer should be detected for .tml");
        Assert.IsNotNull(messagePackSerializer, "MessagePack serializer should be detected");
        Assert.IsNotNull(messagePackAltSerializer, "MessagePack serializer should be detected for .mp");
        Assert.IsNull(unknownSerializer, "Unknown serializer should not be detected");

        Assert.AreEqual("application/json", jsonSerializer.ContentType);
        Assert.AreEqual("application/xml", xmlSerializer.ContentType);
        Assert.AreEqual("application/yaml", yamlSerializer.ContentType);
        Assert.AreEqual("application/yaml", yamlAltSerializer.ContentType);
        Assert.AreEqual("application/toml", tomlSerializer.ContentType);
        Assert.AreEqual("application/toml", tomlAltSerializer.ContentType);
        Assert.AreEqual("application/x-msgpack", messagePackSerializer.ContentType);
        Assert.AreEqual("application/x-msgpack", messagePackAltSerializer.ContentType);
    }

    /// <summary>
    /// Test serializer options.
    /// </summary>
    [TestMethod]
    public void SerializerOptions_GetAndSetOptions_WorksCorrectly()
    {
        // Arrange
        var options = new SerializerOptions();

        // Act
        options.SetOption("TestKey", "TestValue");
        options.SetOption("IntKey", 42);

        // Assert
        Assert.AreEqual("TestValue", options.GetOption<string>("TestKey"));
        Assert.AreEqual(42, options.GetOption<int>("IntKey"));
        Assert.AreEqual(0, options.GetOption<int>("NonExistentKey"));
        Assert.AreEqual("Default", options.GetOption("NonExistentKey", "Default"));
        Assert.IsTrue(options.HasOption("TestKey"));
        Assert.IsFalse(options.HasOption("NonExistentKey"));
    }

    /// <summary>
    /// Tests JSON serialization.
    /// </summary>
    [TestMethod]
    public void JsonSerializerTest()
    {
        // Arrange
        var serializer = _registry.GetSerializer("json");
        Assert.IsNotNull(serializer);

        var testObject = new TestClass
        {
            Id = 1,
            Name = "Test",
            Description = "Test Description",
            Tags = new List<string> { "tag1", "tag2" }
        };

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

    /// <summary>
    /// Tests XML serialization.
    /// </summary>
    [TestMethod]
    public void XmlSerializerTest()
    {
        // Arrange
        var serializer = _registry.GetSerializer("xml");
        Assert.IsNotNull(serializer);

        var testObject = new TestClass
        {
            Id = 1,
            Name = "Test",
            Description = "Test Description",
            Tags = new List<string> { "tag1", "tag2" }
        };

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

    /// <summary>
    /// Tests YAML serialization.
    /// </summary>
    [TestMethod]
    public void YamlSerializerTest()
    {
        // Arrange
        var serializer = _registry.GetSerializer("yaml");
        Assert.IsNotNull(serializer);

        var testObject = new TestClass
        {
            Id = 1,
            Name = "Test",
            Description = "Test Description",
            Tags = new List<string> { "tag1", "tag2" }
        };

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

    /// <summary>
    /// Tests TOML serialization.
    /// </summary>
    [TestMethod]
    public void TomlSerializerTest()
    {
        // Arrange
        var serializer = _registry.GetSerializer("toml");
        Assert.IsNotNull(serializer);

        var testObject = new TestClass
        {
            Id = 1,
            Name = "Test",
            Description = "Test Description",
            Tags = new List<string> { "tag1", "tag2" }
        };

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

    /// <summary>
    /// Tests MessagePack serialization.
    /// </summary>
    [TestMethod]
    public void MessagePackSerializerTest()
    {
        // Arrange
        var serializer = _registry.GetSerializer("messagepack");
        Assert.IsNotNull(serializer);

        // Need to use a DTO with attributes for MessagePack
        var testObject = new MessagePackTestClass
        {
            Id = 1,
            Name = "Test",
            Description = "Test Description",
            Tags = new List<string> { "tag1", "tag2" }
        };

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

    /// <summary>
    /// Tests Protocol Buffers serialization.
    /// </summary>
    [TestMethod]
    public void ProtobufSerializerTest()
    {
        // Arrange
        var serializer = _registry.GetSerializer("protobuf");
        Assert.IsNotNull(serializer);

        // Need to use a DTO with attributes for Protobuf
        var testObject = new ProtobufTestClass
        {
            Id = 1,
            Name = "Test",
            Description = "Test Description",
            Tags = new List<string> { "tag1", "tag2" }
        };

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
