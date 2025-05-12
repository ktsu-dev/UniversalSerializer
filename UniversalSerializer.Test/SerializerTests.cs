// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using ktsu.UniversalSerializer.Serialization;
using ktsu.UniversalSerializer.Serialization.Json;
using ktsu.UniversalSerializer.Serialization.Xml;
using ktsu.UniversalSerializer.Serialization.Yaml;
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
        _registry = new SerializerRegistry(_factory);
        _options = SerializerOptions.Default();

        // Register serializer creators
        _factory.RegisterSerializer<JsonSerializer>(options => new JsonSerializer(options));
        _factory.RegisterSerializer<XmlSerializer>(options => new XmlSerializer(options));
        _factory.RegisterSerializer<YamlSerializer>(options => new YamlSerializer(options));

        // Register serializers with the registry
        _registry.Register("json", _factory.Create<JsonSerializer>(_options));
        _registry.Register("xml", _factory.Create<XmlSerializer>(_options));
        _registry.Register("yaml", _factory.Create<YamlSerializer>(_options));
    }

    /// <summary>
    /// Test JSON serialization.
    /// </summary>
    [TestMethod]
    public void JsonSerializer_SerializeAndDeserialize_ReturnsOriginalObject()
    {
        // Arrange
        var jsonSerializer = _registry.GetSerializer("json") as JsonSerializer;
        Assert.IsNotNull(jsonSerializer, "JSON serializer should be available");

        var testObject = new TestModel
        {
            Id = 1,
            Name = "Test Object",
            CreatedAt = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            IsActive = true,
            Tags = ["test", "serialization", "json"]
        };

        // Act
        var serialized = jsonSerializer.Serialize(testObject);
        var deserialized = jsonSerializer.Deserialize<TestModel>(serialized);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(testObject.Id, deserialized.Id);
        Assert.AreEqual(testObject.Name, deserialized.Name);
        Assert.AreEqual(testObject.CreatedAt, deserialized.CreatedAt);
        Assert.AreEqual(testObject.IsActive, deserialized.IsActive);
        CollectionAssert.AreEqual(testObject.Tags, deserialized.Tags);
    }

    /// <summary>
    /// Test XML serialization.
    /// </summary>
    [TestMethod]
    public void XmlSerializer_SerializeAndDeserialize_ReturnsOriginalObject()
    {
        // Arrange
        var xmlSerializer = _registry.GetSerializer("xml") as XmlSerializer;
        Assert.IsNotNull(xmlSerializer, "XML serializer should be available");

        var testObject = new TestModel
        {
            Id = 1,
            Name = "Test Object",
            CreatedAt = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            IsActive = true,
            Tags = ["test", "serialization", "xml"]
        };

        // Act
        var serialized = xmlSerializer.Serialize(testObject);
        var deserialized = xmlSerializer.Deserialize<TestModel>(serialized);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(testObject.Id, deserialized.Id);
        Assert.AreEqual(testObject.Name, deserialized.Name);
        Assert.AreEqual(testObject.CreatedAt, deserialized.CreatedAt);
        Assert.AreEqual(testObject.IsActive, deserialized.IsActive);
        CollectionAssert.AreEqual(testObject.Tags, deserialized.Tags);
    }

    /// <summary>
    /// Test YAML serialization.
    /// </summary>
    [TestMethod]
    public void YamlSerializer_SerializeAndDeserialize_ReturnsOriginalObject()
    {
        // Arrange
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
        var unknownSerializer = _registry.GetSerializerByExtension(".unknown");

        // Assert
        Assert.IsNotNull(jsonSerializer, "JSON serializer should be available");
        Assert.IsNotNull(xmlSerializer, "XML serializer should be available");
        Assert.IsNotNull(yamlSerializer, "YAML serializer should be available");
        Assert.IsNotNull(yamlAltSerializer, "YAML serializer should be available for .yml");
        Assert.IsNull(unknownSerializer, "Unknown serializer should not be available");

        Assert.AreEqual("application/json", jsonSerializer.ContentType);
        Assert.AreEqual("application/xml", xmlSerializer.ContentType);
        Assert.AreEqual("application/yaml", yamlSerializer.ContentType);
        Assert.AreEqual("application/yaml", yamlAltSerializer.ContentType);
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
        var unknownSerializer = _registry.DetectSerializer("test.unknown");

        // Assert
        Assert.IsNotNull(jsonSerializer, "JSON serializer should be detected");
        Assert.IsNotNull(xmlSerializer, "XML serializer should be detected");
        Assert.IsNotNull(yamlSerializer, "YAML serializer should be detected");
        Assert.IsNotNull(yamlAltSerializer, "YAML serializer should be detected for .yml");
        Assert.IsNull(unknownSerializer, "Unknown serializer should not be detected");

        Assert.AreEqual("application/json", jsonSerializer.ContentType);
        Assert.AreEqual("application/xml", xmlSerializer.ContentType);
        Assert.AreEqual("application/yaml", yamlSerializer.ContentType);
        Assert.AreEqual("application/yaml", yamlAltSerializer.ContentType);
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
