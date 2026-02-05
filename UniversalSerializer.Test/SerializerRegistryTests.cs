// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Test;

using System;
using ktsu.UniversalSerializer.Contracts;
using ktsu.UniversalSerializer.Models;
using ktsu.UniversalSerializer.Services;
using ktsu.UniversalSerializer.Services.Json;
using ktsu.UniversalSerializer.Services.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests for the SerializerRegistry class.
/// </summary>
[TestClass]
public class SerializerRegistryTests
{
	private SerializerFactory _factory = null!;
	private SerializerRegistry _registry = null!;
	private JsonSerializer _jsonSerializer = null!;
	private XmlSerializer _xmlSerializer = null!;

	/// <summary>
	/// Initializes the test environment.
	/// </summary>
	[TestInitialize]
	public void Initialize()
	{
		_factory = new SerializerFactory();
		_factory.RegisterSerializer(options => new JsonSerializer(options));
		_factory.RegisterSerializer(options => new XmlSerializer(options));

		_jsonSerializer = _factory.Create<JsonSerializer>();
		_xmlSerializer = _factory.Create<XmlSerializer>();

		_registry = new SerializerRegistry(_factory);
	}

	/// <summary>
	/// Tests constructor with null factory throws ArgumentNullException.
	/// </summary>
	[TestMethod]
	public void SerializerRegistry_Constructor_NullFactory_ThrowsArgumentNullException()
	{
		// Act & Assert
		Assert.ThrowsExactly<ArgumentNullException>(() => new SerializerRegistry(null!));
	}

	/// <summary>
	/// Tests Register method with valid parameters.
	/// </summary>
	[TestMethod]
	public void SerializerRegistry_Register_ValidParameters_Success()
	{
		// Act
		_registry.Register("json", _jsonSerializer);

		// Assert
		Assert.IsTrue(_registry.IsFormatSupported("json"), "JSON format should be supported after registration");
		ISerializer? retrieved = _registry.GetSerializer("json");
		Assert.AreSame(_jsonSerializer, retrieved);
	}

	/// <summary>
	/// Tests Register method with null or empty format throws ArgumentException.
	/// </summary>
	[TestMethod]
	public void SerializerRegistry_Register_NullOrEmptyFormat_ThrowsArgumentException()
	{
		// Act & Assert
		Assert.ThrowsExactly<ArgumentException>(() => _registry.Register(null!, _jsonSerializer));
		Assert.ThrowsExactly<ArgumentException>(() => _registry.Register("", _jsonSerializer));
		Assert.ThrowsExactly<ArgumentException>(() => _registry.Register("   ", _jsonSerializer));
	}

	/// <summary>
	/// Tests Register method with null serializer throws ArgumentNullException.
	/// </summary>
	[TestMethod]
	public void SerializerRegistry_Register_NullSerializer_ThrowsArgumentNullException()
	{
		// Act & Assert
		Assert.ThrowsExactly<ArgumentNullException>(() => _registry.Register("json", null!));
	}

	/// <summary>
	/// Tests RegisterFileExtensions method with valid parameters.
	/// </summary>
	[TestMethod]
	public void SerializerRegistry_RegisterFileExtensions_ValidParameters_Success()
	{
		// Arrange
		_registry.Register("json", _jsonSerializer);

		// Act
		_registry.RegisterFileExtensions("json", ".json", ".jsonl");

		// Assert
		Assert.IsTrue(_registry.IsExtensionSupported(".json"), ".json extension should be supported");
		Assert.IsTrue(_registry.IsExtensionSupported(".jsonl"), ".jsonl extension should be supported");

		ISerializer? retrieved1 = _registry.GetSerializerByExtension(".json");
		ISerializer? retrieved2 = _registry.GetSerializerByExtension(".jsonl");
		Assert.AreSame(_jsonSerializer, retrieved1);
		Assert.AreSame(_jsonSerializer, retrieved2);
	}

	/// <summary>
	/// Tests RegisterFileExtensions method with extensions without dots.
	/// </summary>
	[TestMethod]
	public void SerializerRegistry_RegisterFileExtensions_ExtensionsWithoutDots_Success()
	{
		// Arrange
		_registry.Register("json", _jsonSerializer);

		// Act
		_registry.RegisterFileExtensions("json", "json", "jsonl");

		// Assert
		Assert.IsTrue(_registry.IsExtensionSupported(".json"), ".json extension should be supported without dot in registration");
		Assert.IsTrue(_registry.IsExtensionSupported(".jsonl"), ".jsonl extension should be supported without dot in registration");
	}

	/// <summary>
	/// Tests RegisterFileExtensions method with unregistered format throws ArgumentException.
	/// </summary>
	[TestMethod]
	public void SerializerRegistry_RegisterFileExtensions_UnregisteredFormat_ThrowsArgumentException()
	{
		// Act & Assert
		Assert.ThrowsExactly<ArgumentException>(() => _registry.RegisterFileExtensions("unknown", ".json"));
	}

	/// <summary>
	/// Tests RegisterFileExtensions method with null or empty format throws ArgumentException.
	/// </summary>
	[TestMethod]
	public void SerializerRegistry_RegisterFileExtensions_NullOrEmptyFormat_ThrowsArgumentException()
	{
		// Act & Assert
		Assert.ThrowsExactly<ArgumentException>(() => _registry.RegisterFileExtensions(null!, ".json"));
		Assert.ThrowsExactly<ArgumentException>(() => _registry.RegisterFileExtensions("", ".json"));
		Assert.ThrowsExactly<ArgumentException>(() => _registry.RegisterFileExtensions("   ", ".json"));
	}

	/// <summary>
	/// Tests RegisterFileExtensions method with null or empty extensions throws ArgumentException.
	/// </summary>
	[TestMethod]
	public void SerializerRegistry_RegisterFileExtensions_NullOrEmptyExtensions_ThrowsArgumentException()
	{
		// Arrange
		_registry.Register("json", _jsonSerializer);

		// Act & Assert
		Assert.ThrowsExactly<ArgumentException>(() => _registry.RegisterFileExtensions("json", null!));
		Assert.ThrowsExactly<ArgumentException>(() => _registry.RegisterFileExtensions("json"));
	}

	/// <summary>
	/// Tests RegisterContentTypes method with valid parameters.
	/// </summary>
	[TestMethod]
	public void SerializerRegistry_RegisterContentTypes_ValidParameters_Success()
	{
		// Arrange
		_registry.Register("json", _jsonSerializer);

		// Act
		_registry.RegisterContentTypes("json", "application/json", "text/json");

		// Assert
		ISerializer? retrieved1 = _registry.GetSerializerByContentType("application/json");
		ISerializer? retrieved2 = _registry.GetSerializerByContentType("text/json");
		Assert.AreSame(_jsonSerializer, retrieved1);
		Assert.AreSame(_jsonSerializer, retrieved2);
	}

	/// <summary>
	/// Tests RegisterContentTypes method with unregistered format throws ArgumentException.
	/// </summary>
	[TestMethod]
	public void SerializerRegistry_RegisterContentTypes_UnregisteredFormat_ThrowsArgumentException()
	{
		// Act & Assert
		Assert.ThrowsExactly<ArgumentException>(() => _registry.RegisterContentTypes("unknown", "application/json"));
	}

	/// <summary>
	/// Tests RegisterContentTypes method with null or empty format throws ArgumentException.
	/// </summary>
	[TestMethod]
	public void SerializerRegistry_RegisterContentTypes_NullOrEmptyFormat_ThrowsArgumentException()
	{
		// Act & Assert
		Assert.ThrowsExactly<ArgumentException>(() => _registry.RegisterContentTypes(null!, "application/json"));
		Assert.ThrowsExactly<ArgumentException>(() => _registry.RegisterContentTypes("", "application/json"));
		Assert.ThrowsExactly<ArgumentException>(() => _registry.RegisterContentTypes("   ", "application/json"));
	}

	/// <summary>
	/// Tests RegisterContentTypes method with null or empty content types throws ArgumentException.
	/// </summary>
	[TestMethod]
	public void SerializerRegistry_RegisterContentTypes_NullOrEmptyContentTypes_ThrowsArgumentException()
	{
		// Arrange
		_registry.Register("json", _jsonSerializer);

		// Act & Assert
		Assert.ThrowsExactly<ArgumentException>(() => _registry.RegisterContentTypes("json", null!));
		Assert.ThrowsExactly<ArgumentException>(() => _registry.RegisterContentTypes("json"));
	}

	/// <summary>
	/// Tests GetSerializer method with valid format.
	/// </summary>
	[TestMethod]
	public void SerializerRegistry_GetSerializer_ValidFormat_ReturnsSerializer()
	{
		// Arrange
		_registry.Register("json", _jsonSerializer);

		// Act
		ISerializer? result = _registry.GetSerializer("json");

		// Assert
		Assert.AreSame(_jsonSerializer, result);
	}

	/// <summary>
	/// Tests GetSerializer method with unknown format returns null.
	/// </summary>
	[TestMethod]
	public void SerializerRegistry_GetSerializer_UnknownFormat_ReturnsNull()
	{
		// Act
		ISerializer? result = _registry.GetSerializer("unknown");

		// Assert
		Assert.IsNull(result);
	}

	/// <summary>
	/// Tests GetSerializer method with null or empty format throws ArgumentException.
	/// </summary>
	[TestMethod]
	public void SerializerRegistry_GetSerializer_NullOrEmptyFormat_ThrowsArgumentException()
	{
		// Act & Assert
		Assert.ThrowsExactly<ArgumentException>(() => _registry.GetSerializer(null!));
		Assert.ThrowsExactly<ArgumentException>(() => _registry.GetSerializer(""));
		Assert.ThrowsExactly<ArgumentException>(() => _registry.GetSerializer("   "));
	}

	/// <summary>
	/// Tests GetSerializerByExtension method with valid extension.
	/// </summary>
	[TestMethod]
	public void SerializerRegistry_GetSerializerByExtension_ValidExtension_ReturnsSerializer()
	{
		// Arrange
		_registry.Register("json", _jsonSerializer);
		_registry.RegisterFileExtensions("json", ".json");

		// Act
		ISerializer? result = _registry.GetSerializerByExtension(".json");

		// Assert
		Assert.AreSame(_jsonSerializer, result);
	}

	/// <summary>
	/// Tests GetSerializerByExtension method with extension without dot.
	/// </summary>
	[TestMethod]
	public void SerializerRegistry_GetSerializerByExtension_ExtensionWithoutDot_ReturnsSerializer()
	{
		// Arrange
		_registry.Register("json", _jsonSerializer);
		_registry.RegisterFileExtensions("json", ".json");

		// Act
		ISerializer? result = _registry.GetSerializerByExtension("json");

		// Assert
		Assert.AreSame(_jsonSerializer, result);
	}

	/// <summary>
	/// Tests GetSerializerByExtension method with unknown extension returns null.
	/// </summary>
	[TestMethod]
	public void SerializerRegistry_GetSerializerByExtension_UnknownExtension_ReturnsNull()
	{
		// Act
		ISerializer? result = _registry.GetSerializerByExtension(".unknown");

		// Assert
		Assert.IsNull(result);
	}

	/// <summary>
	/// Tests GetSerializerByExtension method with null or empty extension throws ArgumentException.
	/// </summary>
	[TestMethod]
	public void SerializerRegistry_GetSerializerByExtension_NullOrEmptyExtension_ThrowsArgumentException()
	{
		// Act & Assert
		Assert.ThrowsExactly<ArgumentException>(() => _registry.GetSerializerByExtension(null!));
		Assert.ThrowsExactly<ArgumentException>(() => _registry.GetSerializerByExtension(""));
		Assert.ThrowsExactly<ArgumentException>(() => _registry.GetSerializerByExtension("   "));
	}

	/// <summary>
	/// Tests GetSerializerByContentType method with valid content type.
	/// </summary>
	[TestMethod]
	public void SerializerRegistry_GetSerializerByContentType_ValidContentType_ReturnsSerializer()
	{
		// Arrange
		_registry.Register("json", _jsonSerializer);
		_registry.RegisterContentTypes("json", "application/json");

		// Act
		ISerializer? result = _registry.GetSerializerByContentType("application/json");

		// Assert
		Assert.AreSame(_jsonSerializer, result);
	}

	/// <summary>
	/// Tests GetSerializerByContentType method with unknown content type returns null.
	/// </summary>
	[TestMethod]
	public void SerializerRegistry_GetSerializerByContentType_UnknownContentType_ReturnsNull()
	{
		// Act
		ISerializer? result = _registry.GetSerializerByContentType("application/unknown");

		// Assert
		Assert.IsNull(result);
	}

	/// <summary>
	/// Tests GetSerializerByContentType method with null or empty content type throws ArgumentException.
	/// </summary>
	[TestMethod]
	public void SerializerRegistry_GetSerializerByContentType_NullOrEmptyContentType_ThrowsArgumentException()
	{
		// Act & Assert
		Assert.ThrowsExactly<ArgumentException>(() => _registry.GetSerializerByContentType(null!));
		Assert.ThrowsExactly<ArgumentException>(() => _registry.GetSerializerByContentType(""));
		Assert.ThrowsExactly<ArgumentException>(() => _registry.GetSerializerByContentType("   "));
	}

	/// <summary>
	/// Tests IsFormatSupported method.
	/// </summary>
	[TestMethod]
	public void SerializerRegistry_IsFormatSupported_ReturnsCorrectValue()
	{
		// Arrange
		_registry.Register("json", _jsonSerializer);

		// Act & Assert
		Assert.IsTrue(_registry.IsFormatSupported("json"), "Registered JSON format should be supported");
		Assert.IsFalse(_registry.IsFormatSupported("xml"), "Unregistered XML format should not be supported");
		Assert.IsFalse(_registry.IsFormatSupported(null!), "Null format should not be supported");
		Assert.IsFalse(_registry.IsFormatSupported(""), "Empty format should not be supported");
		Assert.IsFalse(_registry.IsFormatSupported("   "), "Whitespace format should not be supported");
	}

	/// <summary>
	/// Tests IsExtensionSupported method.
	/// </summary>
	[TestMethod]
	public void SerializerRegistry_IsExtensionSupported_ReturnsCorrectValue()
	{
		// Arrange
		_registry.Register("json", _jsonSerializer);
		_registry.RegisterFileExtensions("json", ".json");

		// Act & Assert
		Assert.IsTrue(_registry.IsExtensionSupported(".json"), "Registered .json extension should be supported");
		Assert.IsFalse(_registry.IsExtensionSupported(".xml"), "Unregistered .xml extension should not be supported");
		Assert.IsFalse(_registry.IsExtensionSupported(null!), "Null extension should not be supported");
		Assert.IsFalse(_registry.IsExtensionSupported(""), "Empty extension should not be supported");
		Assert.IsFalse(_registry.IsExtensionSupported("   "), "Whitespace extension should not be supported");
	}

	/// <summary>
	/// Tests case insensitive format comparison.
	/// </summary>
	[TestMethod]
	public void SerializerRegistry_CaseInsensitiveComparison_Success()
	{
		// Arrange
		_registry.Register("JSON", _jsonSerializer);
		_registry.RegisterFileExtensions("JSON", ".JSON");
		_registry.RegisterContentTypes("JSON", "APPLICATION/JSON");

		// Act & Assert
		Assert.IsTrue(_registry.IsFormatSupported("json"), "Format lookup should be case insensitive (lowercase)");
		Assert.IsTrue(_registry.IsFormatSupported("Json"), "Format lookup should be case insensitive (mixed case)");
		Assert.IsTrue(_registry.IsExtensionSupported(".json"), "Extension lookup should be case insensitive (lowercase)");
		Assert.IsTrue(_registry.IsExtensionSupported(".Json"), "Extension lookup should be case insensitive (mixed case)");

		Assert.AreSame(_jsonSerializer, _registry.GetSerializer("json"));
		Assert.AreSame(_jsonSerializer, _registry.GetSerializerByExtension(".json"));
		Assert.AreSame(_jsonSerializer, _registry.GetSerializerByContentType("application/json"));
	}

	/// <summary>
	/// Tests RegisterBuiltIn method with default options.
	/// </summary>
	[TestMethod]
	public void SerializerRegistry_RegisterBuiltIn_DefaultOptions_Success()
	{
		// Act
		_registry.RegisterBuiltIn();

		// Assert - Check that all built-in formats are registered
		Assert.IsTrue(_registry.IsFormatSupported("json"), "Built-in JSON format should be registered");
		Assert.IsTrue(_registry.IsFormatSupported("xml"), "Built-in XML format should be registered");
		Assert.IsTrue(_registry.IsFormatSupported("yaml"), "Built-in YAML format should be registered");
		Assert.IsTrue(_registry.IsFormatSupported("toml"), "Built-in TOML format should be registered");
		Assert.IsTrue(_registry.IsFormatSupported("messagepack"), "Built-in MessagePack format should be registered");

		// Check extensions
		Assert.IsTrue(_registry.IsExtensionSupported(".json"), "Built-in .json extension should be registered");
		Assert.IsTrue(_registry.IsExtensionSupported(".xml"), "Built-in .xml extension should be registered");
		Assert.IsTrue(_registry.IsExtensionSupported(".yaml"), "Built-in .yaml extension should be registered");
		Assert.IsTrue(_registry.IsExtensionSupported(".yml"), "Built-in .yml extension should be registered");
		Assert.IsTrue(_registry.IsExtensionSupported(".toml"), "Built-in .toml extension should be registered");
		Assert.IsTrue(_registry.IsExtensionSupported(".tml"), "Built-in .tml extension should be registered");
		Assert.IsTrue(_registry.IsExtensionSupported(".msgpack"), "Built-in .msgpack extension should be registered");
		Assert.IsTrue(_registry.IsExtensionSupported(".mp"), "Built-in .mp extension should be registered");

		// Check content types
		Assert.IsNotNull(_registry.GetSerializerByContentType("application/json"));
		Assert.IsNotNull(_registry.GetSerializerByContentType("text/json"));
		Assert.IsNotNull(_registry.GetSerializerByContentType("application/xml"));
		Assert.IsNotNull(_registry.GetSerializerByContentType("text/xml"));
		Assert.IsNotNull(_registry.GetSerializerByContentType("application/x-yaml"));
		Assert.IsNotNull(_registry.GetSerializerByContentType("text/yaml"));
		Assert.IsNotNull(_registry.GetSerializerByContentType("application/toml"));
		Assert.IsNotNull(_registry.GetSerializerByContentType("application/x-msgpack"));
	}

	/// <summary>
	/// Tests RegisterBuiltIn method with custom options.
	/// </summary>
	[TestMethod]
	public void SerializerRegistry_RegisterBuiltIn_CustomOptions_Success()
	{
		// Arrange
		SerializerOptions customOptions = SerializerOptions.Default();
		customOptions.EnableCompression = true;

		// Act
		_registry.RegisterBuiltIn(customOptions);

		// Assert
		Assert.IsTrue(_registry.IsFormatSupported("json"), "JSON format should be registered with custom options");
		ISerializer? jsonSerializer = _registry.GetSerializer("json");
		Assert.IsNotNull(jsonSerializer);
	}

	/// <summary>
	/// Tests that registry handles multiple serializers for different formats.
	/// </summary>
	[TestMethod]
	public void SerializerRegistry_MultipleSerializers_HandledCorrectly()
	{
		// Arrange
		_registry.Register("json", _jsonSerializer);
		_registry.Register("xml", _xmlSerializer);

		// Act & Assert
		Assert.AreSame(_jsonSerializer, _registry.GetSerializer("json"));
		Assert.AreSame(_xmlSerializer, _registry.GetSerializer("xml"));
		Assert.AreNotSame((object)_jsonSerializer, _xmlSerializer);
	}

	/// <summary>
	/// Tests that registering same format twice overwrites the previous registration.
	/// </summary>
	[TestMethod]
	public void SerializerRegistry_RegisterSameFormatTwice_OverwritesPrevious()
	{
		// Arrange
		_registry.Register("json", _jsonSerializer);
		JsonSerializer newJsonSerializer = _factory.Create<JsonSerializer>();

		// Act
		_registry.Register("json", newJsonSerializer);

		// Assert
		Assert.AreSame(newJsonSerializer, _registry.GetSerializer("json"));
		Assert.AreNotSame(_jsonSerializer, _registry.GetSerializer("json"));
	}
}
