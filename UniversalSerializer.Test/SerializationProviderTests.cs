// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Test;

using System;
using System.Threading.Tasks;
using ktsu.SerializationProvider;
using ktsu.UniversalSerializer.DependencyInjection;
using ktsu.UniversalSerializer.Services;
using ktsu.UniversalSerializer.Services.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests for the SerializationProvider integration.
/// </summary>
[TestClass]
public class SerializationProviderTests
{
	private sealed record TestData(string Name, int Value, DateTime Timestamp);

	[TestMethod]
	public void UniversalSerializationProvider_WithJsonSerializer_ShouldSerializeAndDeserialize()
	{
		// Arrange
		JsonSerializer jsonSerializer = new();
		UniversalSerializationProvider provider = new(jsonSerializer, "Test.Json");
		TestData testData = new("Test", 42, new DateTime(2025, 6, 18, 16, 53, 53, DateTimeKind.Utc));

		// Act
		string serialized = provider.Serialize(testData);
		TestData deserialized = provider.Deserialize<TestData>(serialized);

		// Assert
		Assert.AreEqual("Test.Json", provider.ProviderName);
		Assert.AreEqual("application/json", provider.ContentType);
		Assert.AreEqual(testData.Name, deserialized.Name);
		Assert.AreEqual(testData.Value, deserialized.Value);
		Assert.AreEqual(testData.Timestamp, deserialized.Timestamp);
	}

	[TestMethod]
	public void UniversalSerializationProvider_WithTypeParameter_ShouldSerializeAndDeserialize()
	{
		// Arrange
		JsonSerializer jsonSerializer = new();
		UniversalSerializationProvider provider = new(jsonSerializer);
		TestData testData = new("Test", 42, new DateTime(2025, 6, 18, 16, 53, 53, DateTimeKind.Utc));

		// Act
		string serialized = provider.Serialize(testData);
		TestData deserialized = provider.Deserialize<TestData>(serialized);

		// Assert
		Assert.AreEqual(testData.Name, deserialized.Name);
		Assert.AreEqual(testData.Value, deserialized.Value);
		Assert.AreEqual(testData.Timestamp, deserialized.Timestamp);
	}

	[TestMethod]
	public async Task UniversalSerializationProvider_AsyncMethods_ShouldWork()
	{
		// Arrange
		JsonSerializer jsonSerializer = new();
		UniversalSerializationProvider provider = new(jsonSerializer);
		TestData testData = new("Test", 42, new DateTime(2025, 6, 18, 16, 53, 53, DateTimeKind.Utc));

		// Act
		string serialized = await provider.SerializeAsync(testData).ConfigureAwait(false);
		TestData deserialized = await provider.DeserializeAsync<TestData>(serialized).ConfigureAwait(false);

		// Assert
		Assert.AreEqual(testData.Name, deserialized.Name);
		Assert.AreEqual(testData.Value, deserialized.Value);
		Assert.AreEqual(testData.Timestamp, deserialized.Timestamp);
	}

	[TestMethod]
	public async Task UniversalSerializationProvider_AsyncMethodsWithType_ShouldWork()
	{
		// Arrange
		JsonSerializer jsonSerializer = new();
		UniversalSerializationProvider provider = new(jsonSerializer);
		TestData testData = new("Test", 42, new DateTime(2025, 6, 18, 16, 53, 53, DateTimeKind.Utc));

		// Act
		string serialized = await provider.SerializeAsync(testData).ConfigureAwait(false);
		object deserialized = await provider.DeserializeAsync(serialized, typeof(TestData)).ConfigureAwait(false);

		// Assert
		TestData typedResult = (TestData)deserialized;
		Assert.AreEqual(testData.Name, typedResult.Name);
		Assert.AreEqual(testData.Value, typedResult.Value);
		Assert.AreEqual(testData.Timestamp, typedResult.Timestamp);
	}

	[TestMethod]
	public void DependencyInjection_JsonSerializationProvider_ShouldWork()
	{
		// Arrange
		ServiceCollection services = new();
		services.AddUniversalSerializer();
		services.AddJsonSerializationProvider();

		ServiceProvider serviceProvider = services.BuildServiceProvider();

		// Act
		ISerializationProvider provider = serviceProvider.GetRequiredService<ISerializationProvider>();
		TestData testData = new("DI Test", 123, new DateTime(2025, 6, 18, 16, 53, 53, DateTimeKind.Utc));

		string serialized = provider.Serialize(testData);
		TestData deserialized = provider.Deserialize<TestData>(serialized);

		// Assert
		Assert.AreEqual("UniversalSerializer.Json", provider.ProviderName);
		Assert.AreEqual("application/json", provider.ContentType);
		Assert.AreEqual(testData.Name, deserialized.Name);
		Assert.AreEqual(testData.Value, deserialized.Value);
		Assert.AreEqual(testData.Timestamp, deserialized.Timestamp);
	}

	[TestMethod]
	public void DependencyInjection_GenericProvider_ShouldWork()
	{
		// Arrange
		ServiceCollection services = new();
		services.AddUniversalSerializer();
		services.AddUniversalSerializationProvider<JsonSerializer>("Custom.Provider");

		ServiceProvider serviceProvider = services.BuildServiceProvider();

		// Act
		ISerializationProvider provider = serviceProvider.GetRequiredService<ISerializationProvider>();

		// Assert
		Assert.AreEqual("Custom.Provider", provider.ProviderName);
		Assert.AreEqual("application/json", provider.ContentType);
	}

	[TestMethod]
	public void DependencyInjection_InstanceProvider_ShouldWork()
	{
		// Arrange
		ServiceCollection services = new();
		JsonSerializer jsonSerializer = new();
		services.AddUniversalSerializationProvider(jsonSerializer, "Instance.Provider");

		ServiceProvider serviceProvider = services.BuildServiceProvider();

		// Act
		ISerializationProvider provider = serviceProvider.GetRequiredService<ISerializationProvider>();

		// Assert
		Assert.AreEqual("Instance.Provider", provider.ProviderName);
		Assert.AreEqual("application/json", provider.ContentType);
	}

	[TestMethod]
	public void DependencyInjection_FactoryProvider_ShouldWork()
	{
		// Arrange
		ServiceCollection services = new();
		services.AddUniversalSerializer();
		services.AddUniversalSerializationProvider(sp => new JsonSerializer(), "Factory.Provider");

		ServiceProvider serviceProvider = services.BuildServiceProvider();

		// Act
		ISerializationProvider provider = serviceProvider.GetRequiredService<ISerializationProvider>();

		// Assert
		Assert.AreEqual("Factory.Provider", provider.ProviderName);
		Assert.AreEqual("application/json", provider.ContentType);
	}

	[TestMethod]
	public void UniversalSerializationProvider_EmptyData_ShouldThrow()
	{
		// Arrange
		JsonSerializer jsonSerializer = new();
		UniversalSerializationProvider provider = new(jsonSerializer);

		// Act & Assert
		Assert.ThrowsExactly<ArgumentException>(() => provider.Deserialize<TestData>(""));
	}

	[TestMethod]
	public void UniversalSerializationProvider_NullObject_ShouldThrow()
	{
		// Arrange
		JsonSerializer jsonSerializer = new();
		UniversalSerializationProvider provider = new(jsonSerializer);

		// Act & Assert
		Assert.ThrowsExactly<ArgumentNullException>(() => provider.Serialize<TestData>(null!));
	}
}
