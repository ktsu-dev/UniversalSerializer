// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Test;

using System;
using ktsu.UniversalSerializer.Json;
using ktsu.UniversalSerializer.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests for the SerializerFactory class.
/// </summary>
[TestClass]
public class SerializerFactoryTests
{
	private SerializerFactory _factory = null!;

	/// <summary>
	/// Initializes the test environment.
	/// </summary>
	[TestInitialize]
	public void Initialize()
	{
		_factory = new SerializerFactory();
	}

	/// <summary>
	/// Tests RegisterSerializer method with valid factory function.
	/// </summary>
	[TestMethod]
	public void SerializerFactory_RegisterSerializer_ValidFactory_Success()
	{
		// Act
		_factory.RegisterSerializer(options => new JsonSerializer(options));

		// Assert - No exception should be thrown
		JsonSerializer serializer = _factory.Create<JsonSerializer>();
		Assert.IsNotNull(serializer);
	}

	/// <summary>
	/// Tests RegisterSerializer method with null factory throws ArgumentNullException.
	/// </summary>
	[TestMethod]
	public void SerializerFactory_RegisterSerializer_NullFactory_ThrowsArgumentNullException()
	{
		// Act & Assert
		Assert.ThrowsException<ArgumentNullException>(() =>
			_factory.RegisterSerializer<JsonSerializer>(null!));
	}

	/// <summary>
	/// Tests Create method with default options.
	/// </summary>
	[TestMethod]
	public void SerializerFactory_Create_DefaultOptions_Success()
	{
		// Arrange
		_factory.RegisterSerializer(options => new JsonSerializer(options));

		// Act
		JsonSerializer serializer = _factory.Create<JsonSerializer>();

		// Assert
		Assert.IsNotNull(serializer);
	}

	/// <summary>
	/// Tests Create method with custom options.
	/// </summary>
	[TestMethod]
	public void SerializerFactory_Create_CustomOptions_Success()
	{
		// Arrange
		_factory.RegisterSerializer(options => new JsonSerializer(options));
		SerializerOptions customOptions = SerializerOptions.Default();
		customOptions.EnableCompression = true;

		// Act
		JsonSerializer serializer = _factory.Create<JsonSerializer>(customOptions);

		// Assert
		Assert.IsNotNull(serializer);
	}

	/// <summary>
	/// Tests Create method with unregistered serializer type throws InvalidOperationException.
	/// </summary>
	[TestMethod]
	public void SerializerFactory_Create_UnregisteredType_ThrowsInvalidOperationException()
	{
		// Act & Assert
		Assert.ThrowsException<InvalidOperationException>(_factory.Create<JsonSerializer>);
	}

	/// <summary>
	/// Tests Create method with null options uses default options.
	/// </summary>
	[TestMethod]
	public void SerializerFactory_Create_NullOptions_UsesDefault()
	{
		// Arrange
		_factory.RegisterSerializer(options => new JsonSerializer(options));

		// Act
		JsonSerializer serializer = _factory.Create<JsonSerializer>(null!);

		// Assert
		Assert.IsNotNull(serializer);
	}

	/// <summary>
	/// Tests that Create method returns new instances each time.
	/// </summary>
	[TestMethod]
	public void SerializerFactory_Create_ReturnsNewInstances()
	{
		// Arrange
		_factory.RegisterSerializer(options => new JsonSerializer(options));

		// Act
		JsonSerializer serializer1 = _factory.Create<JsonSerializer>();
		JsonSerializer serializer2 = _factory.Create<JsonSerializer>();

		// Assert
		Assert.IsNotNull(serializer1);
		Assert.IsNotNull(serializer2);
		Assert.AreNotSame(serializer1, serializer2);
	}

	/// <summary>
	/// Tests registering multiple serializer types.
	/// </summary>
	[TestMethod]
	public void SerializerFactory_RegisterMultipleTypes_Success()
	{
		// Arrange
		_factory.RegisterSerializer(options => new JsonSerializer(options));
		_factory.RegisterSerializer(options => new XmlSerializer(options));

		// Act
		JsonSerializer jsonSerializer = _factory.Create<JsonSerializer>();
		XmlSerializer xmlSerializer = _factory.Create<XmlSerializer>();

		// Assert
		Assert.IsNotNull(jsonSerializer);
		Assert.IsNotNull(xmlSerializer);
		Assert.AreNotEqual(jsonSerializer.GetType(), xmlSerializer.GetType());
	}

	/// <summary>
	/// Tests Create method with unregistered type to verify registration status.
	/// </summary>
	[TestMethod]
	public void SerializerFactory_Create_VerifiesRegistrationStatus()
	{
		// Act & Assert - Before registration
		Assert.ThrowsException<InvalidOperationException>(_factory.Create<JsonSerializer>);

		// Register and test again
		_factory.RegisterSerializer(options => new JsonSerializer(options));
		JsonSerializer serializer = _factory.Create<JsonSerializer>();
		Assert.IsNotNull(serializer);

		// Unregistered type should still throw
		Assert.ThrowsException<InvalidOperationException>(_factory.Create<XmlSerializer>);
	}

	/// <summary>
	/// Tests registering same type twice overwrites previous registration.
	/// </summary>
	[TestMethod]
	public void SerializerFactory_RegisterSameTypeTwice_OverwritesPrevious()
	{
		// Arrange
		bool firstFactoryCalled = false;
		bool secondFactoryCalled = false;

		_factory.RegisterSerializer(options =>
		{
			firstFactoryCalled = true;
			return new JsonSerializer(options);
		});

		_factory.RegisterSerializer(options =>
		{
			secondFactoryCalled = true;
			return new JsonSerializer(options);
		});

		// Act
		JsonSerializer serializer = _factory.Create<JsonSerializer>();

		// Assert
		Assert.IsNotNull(serializer);
		Assert.IsFalse(firstFactoryCalled);
		Assert.IsTrue(secondFactoryCalled);
	}

	/// <summary>
	/// Tests that factory function receives correct options.
	/// </summary>
	[TestMethod]
	public void SerializerFactory_Create_PassesCorrectOptions()
	{
		// Arrange
		SerializerOptions receivedOptions = null!;
		SerializerOptions expectedOptions = SerializerOptions.Default();
		expectedOptions.EnableCompression = true;

		_factory.RegisterSerializer(options =>
		{
			receivedOptions = options;
			return new JsonSerializer(options);
		});

		// Act
		JsonSerializer serializer = _factory.Create<JsonSerializer>(expectedOptions);

		// Assert
		Assert.IsNotNull(serializer);
		Assert.AreSame(expectedOptions, receivedOptions);
	}

	/// <summary>
	/// Tests factory function that returns null throws InvalidOperationException.
	/// </summary>
	[TestMethod]
	public void SerializerFactory_FactoryReturnsNull_ThrowsInvalidOperationException()
	{
		// Arrange
		_factory.RegisterSerializer<JsonSerializer>(options => null!);

		// Act & Assert
		Assert.ThrowsException<InvalidOperationException>(_factory.Create<JsonSerializer>);
	}

	/// <summary>
	/// Tests factory function that throws exception propagates the exception.
	/// </summary>
	[TestMethod]
	public void SerializerFactory_FactoryThrowsException_PropagatesException()
	{
		// Arrange
		ArgumentException expectedException = new("Test exception");
		_factory.RegisterSerializer<JsonSerializer>(options => throw expectedException);

		// Act & Assert
		ArgumentException actualException = Assert.ThrowsException<ArgumentException>(_factory.Create<JsonSerializer>);
		Assert.AreSame(expectedException, actualException);
	}

	/// <summary>
	/// Tests Create method with interface type.
	/// </summary>
	[TestMethod]
	public void SerializerFactory_Create_InterfaceType_ThrowsInvalidOperationException()
	{
		// Act & Assert
		Assert.ThrowsException<InvalidOperationException>(_factory.Create<ISerializer>);
	}

	/// <summary>
	/// Tests ConfigureDefaults method.
	/// </summary>
	[TestMethod]
	public void SerializerFactory_ConfigureDefaults_Success()
	{
		// Arrange
		_factory.RegisterSerializer(options => new JsonSerializer(options));

		// Act
		SerializerFactory result = _factory.ConfigureDefaults(options =>
		{
			options.EnableCompression = true;
			options.CompressionLevel = 9;
		});

		// Assert
		Assert.AreSame(_factory, result); // Should return same instance for chaining

		// Verify defaults are applied to new serializers
		SerializerOptions defaultOptions = _factory.GetDefaultOptions();
		Assert.IsTrue(defaultOptions.EnableCompression);
		Assert.AreEqual(9, defaultOptions.CompressionLevel);
	}

	/// <summary>
	/// Tests GetDefaultOptions method.
	/// </summary>
	[TestMethod]
	public void SerializerFactory_GetDefaultOptions_ReturnsClone()
	{
		// Act
		SerializerOptions options1 = _factory.GetDefaultOptions();
		SerializerOptions options2 = _factory.GetDefaultOptions();

		// Assert
		Assert.IsNotNull(options1);
		Assert.IsNotNull(options2);
		Assert.AreNotSame(options1, options2); // Should be different instances
	}

	/// <summary>
	/// Tests GetSerializer method.
	/// </summary>
	[TestMethod]
	public void SerializerFactory_GetSerializer_Success()
	{
		// Arrange
		_factory.RegisterSerializer(options => new JsonSerializer(options));

		// Act
		JsonSerializer serializer = _factory.GetSerializer<JsonSerializer>();

		// Assert
		Assert.IsNotNull(serializer);
	}

	/// <summary>
	/// Tests Create method with Type parameter.
	/// </summary>
	[TestMethod]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2263:Prefer generic overload when type is known", Justification = "Test")]
	public void SerializerFactory_Create_WithType_Success()
	{
		// Arrange
		_factory.RegisterSerializer(options => new JsonSerializer(options));

		// Act
		ISerializer serializer = _factory.Create(typeof(JsonSerializer));

		// Assert
		Assert.IsNotNull(serializer);
		Assert.IsInstanceOfType<JsonSerializer>(serializer);
	}

	/// <summary>
	/// Tests Create method with Type parameter and options.
	/// </summary>
	[TestMethod]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2263:Prefer generic overload when type is known", Justification = "Test")]
	public void SerializerFactory_Create_WithTypeAndOptions_Success()
	{
		// Arrange
		_factory.RegisterSerializer(options => new JsonSerializer(options));
		SerializerOptions customOptions = SerializerOptions.Default();
		customOptions.EnableCompression = true;

		// Act
		ISerializer serializer = _factory.Create(typeof(JsonSerializer), customOptions);

		// Assert
		Assert.IsNotNull(serializer);
		Assert.IsInstanceOfType(serializer, typeof(JsonSerializer));
	}

	/// <summary>
	/// Tests Create method with null type throws ArgumentNullException.
	/// </summary>
	[TestMethod]
	public void SerializerFactory_Create_NullType_ThrowsArgumentNullException()
	{
		// Act & Assert
		Assert.ThrowsException<ArgumentNullException>(() => _factory.Create(null!));
	}

	/// <summary>
	/// Tests Create method with non-ISerializer type throws ArgumentException.
	/// </summary>
	[TestMethod]
	public void SerializerFactory_Create_NonSerializerType_ThrowsArgumentException()
	{
		// Act & Assert
		Assert.ThrowsException<ArgumentException>(() => _factory.Create(typeof(string)));
	}
}
