// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Test;

using System.IO;
using System.Threading.Tasks;
using ktsu.UniversalSerializer.Serialization;
using ktsu.UniversalSerializer.Serialization.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Tests for the SerializerExtensions class.
/// </summary>
[TestClass]
public class SerializerExtensionsTests
{
	private JsonSerializer _serializer = null!;
	private TestData _testData = null!;
	private string _tempFilePath = null!;

	/// <summary>
	/// Initializes the test environment.
	/// </summary>
	[TestInitialize]
	public void Initialize()
	{
		SerializerFactory factory = new();
		factory.RegisterSerializer(options => new JsonSerializer(options));
		_serializer = factory.Create<JsonSerializer>(SerializerOptions.Default());

		_testData = new TestData { IntValue = 42, StringValue = "Test Value" };
		_tempFilePath = Path.GetTempFileName();
	}

	/// <summary>
	/// Cleans up test resources.
	/// </summary>
	[TestCleanup]
	public void Cleanup()
	{
		if (File.Exists(_tempFilePath))
		{
			File.Delete(_tempFilePath);
		}
	}

	/// <summary>
	/// Tests SerializeToFile extension method.
	/// </summary>
	[TestMethod]
	public void SerializerExtensions_SerializeToFile_Success()
	{
		// Act
		_serializer.SerializeToFile(_testData, _tempFilePath);

		// Assert
		Assert.IsTrue(File.Exists(_tempFilePath));
		string content = File.ReadAllText(_tempFilePath);
		Assert.IsFalse(string.IsNullOrEmpty(content));

		// Verify content by deserializing
		TestData deserialized = _serializer.Deserialize<TestData>(content);
		Assert.AreEqual(_testData.IntValue, deserialized.IntValue);
		Assert.AreEqual(_testData.StringValue, deserialized.StringValue);
	}

	/// <summary>
	/// Tests SerializeToFileAsync extension method.
	/// </summary>
	[TestMethod]
	public async Task SerializerExtensions_SerializeToFileAsync_Success()
	{
		// Act
		await _serializer.SerializeToFileAsync(_testData, _tempFilePath);

		// Assert
		Assert.IsTrue(File.Exists(_tempFilePath));
		string content = await File.ReadAllTextAsync(_tempFilePath);
		Assert.IsFalse(string.IsNullOrEmpty(content));

		// Verify content by deserializing
		TestData deserialized = _serializer.Deserialize<TestData>(content);
		Assert.AreEqual(_testData.IntValue, deserialized.IntValue);
		Assert.AreEqual(_testData.StringValue, deserialized.StringValue);
	}

	/// <summary>
	/// Tests SerializeToBinaryFile extension method.
	/// </summary>
	[TestMethod]
	public void SerializerExtensions_SerializeToBinaryFile_Success()
	{
		// Act
		_serializer.SerializeToBinaryFile(_testData, _tempFilePath);

		// Assert
		Assert.IsTrue(File.Exists(_tempFilePath));
		byte[] content = File.ReadAllBytes(_tempFilePath);
		Assert.IsTrue(content.Length > 0);

		// Verify content by deserializing
		TestData deserialized = _serializer.DeserializeFromBytes<TestData>(content);
		Assert.AreEqual(_testData.IntValue, deserialized.IntValue);
		Assert.AreEqual(_testData.StringValue, deserialized.StringValue);
	}

	/// <summary>
	/// Tests SerializeToBinaryFileAsync extension method.
	/// </summary>
	[TestMethod]
	public async Task SerializerExtensions_SerializeToBinaryFileAsync_Success()
	{
		// Act
		await _serializer.SerializeToBinaryFileAsync(_testData, _tempFilePath);

		// Assert
		Assert.IsTrue(File.Exists(_tempFilePath));
		byte[] content = await File.ReadAllBytesAsync(_tempFilePath);
		Assert.IsTrue(content.Length > 0);

		// Verify content by deserializing
		TestData deserialized = _serializer.DeserializeFromBytes<TestData>(content);
		Assert.AreEqual(_testData.IntValue, deserialized.IntValue);
		Assert.AreEqual(_testData.StringValue, deserialized.StringValue);
	}

	/// <summary>
	/// Tests DeserializeFromFile extension method.
	/// </summary>
	[TestMethod]
	public void SerializerExtensions_DeserializeFromFile_Success()
	{
		// Arrange
		string serialized = _serializer.Serialize(_testData);
		File.WriteAllText(_tempFilePath, serialized);

		// Act
		TestData result = _serializer.DeserializeFromFile<TestData>(_tempFilePath);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(_testData.IntValue, result.IntValue);
		Assert.AreEqual(_testData.StringValue, result.StringValue);
	}

	/// <summary>
	/// Tests DeserializeFromFileAsync extension method.
	/// </summary>
	[TestMethod]
	public async Task SerializerExtensions_DeserializeFromFileAsync_Success()
	{
		// Arrange
		string serialized = _serializer.Serialize(_testData);
		await File.WriteAllTextAsync(_tempFilePath, serialized);

		// Act
		TestData result = await _serializer.DeserializeFromFileAsync<TestData>(_tempFilePath);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(_testData.IntValue, result.IntValue);
		Assert.AreEqual(_testData.StringValue, result.StringValue);
	}

	/// <summary>
	/// Tests DeserializeFromBinaryFile extension method.
	/// </summary>
	[TestMethod]
	public void SerializerExtensions_DeserializeFromBinaryFile_Success()
	{
		// Arrange
		byte[] serialized = _serializer.SerializeToBytes(_testData);
		File.WriteAllBytes(_tempFilePath, serialized);

		// Act
		TestData result = _serializer.DeserializeFromBinaryFile<TestData>(_tempFilePath);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(_testData.IntValue, result.IntValue);
		Assert.AreEqual(_testData.StringValue, result.StringValue);
	}

	/// <summary>
	/// Tests DeserializeFromBinaryFileAsync extension method.
	/// </summary>
	[TestMethod]
	public async Task SerializerExtensions_DeserializeFromBinaryFileAsync_Success()
	{
		// Arrange
		byte[] serialized = _serializer.SerializeToBytes(_testData);
		await File.WriteAllBytesAsync(_tempFilePath, serialized);

		// Act
		TestData result = await _serializer.DeserializeFromBinaryFileAsync<TestData>(_tempFilePath);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(_testData.IntValue, result.IntValue);
		Assert.AreEqual(_testData.StringValue, result.StringValue);
	}

	/// <summary>
	/// Tests SerializeToStream extension method.
	/// </summary>
	[TestMethod]
	public void SerializerExtensions_SerializeToStream_Success()
	{
		// Act
		using MemoryStream stream = _serializer.SerializeToStream(_testData);

		// Assert
		Assert.IsNotNull(stream);
		Assert.IsTrue(stream.Length > 0);

		// Verify content by deserializing
		stream.Position = 0;
		TestData result = _serializer.DeserializeFromStream<TestData>(stream);
		Assert.AreEqual(_testData.IntValue, result.IntValue);
		Assert.AreEqual(_testData.StringValue, result.StringValue);
	}

	/// <summary>
	/// Tests DeserializeFromStream extension method.
	/// </summary>
	[TestMethod]
	public void SerializerExtensions_DeserializeFromStream_Success()
	{
		// Arrange
		using MemoryStream stream = _serializer.SerializeToStream(_testData);
		stream.Position = 0;

		// Act
		TestData result = _serializer.DeserializeFromStream<TestData>(stream);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(_testData.IntValue, result.IntValue);
		Assert.AreEqual(_testData.StringValue, result.StringValue);
	}

	/// <summary>
	/// Tests Clone extension method.
	/// </summary>
	[TestMethod]
	public void SerializerExtensions_Clone_Success()
	{
		// Act
		TestData clone = _serializer.Clone(_testData);

		// Assert
		Assert.IsNotNull(clone);
		Assert.AreNotSame(_testData, clone); // Different objects
		Assert.AreEqual(_testData.IntValue, clone.IntValue);
		Assert.AreEqual(_testData.StringValue, clone.StringValue);

		// Verify independence
		clone.IntValue = 999;
		Assert.AreEqual(42, _testData.IntValue); // Original unchanged
	}

	/// <summary>
	/// Tests that extension methods throw ArgumentNullException for null serializer.
	/// </summary>
	[TestMethod]
	public void SerializerExtensions_NullSerializer_ThrowsArgumentNullException()
	{
		// Arrange
		JsonSerializer? nullSerializer = null;

		// Act & Assert
		Assert.ThrowsException<ArgumentNullException>(() => nullSerializer!.SerializeToFile(_testData, _tempFilePath));
		Assert.ThrowsException<ArgumentNullException>(() => nullSerializer!.SerializeToBinaryFile(_testData, _tempFilePath));
		Assert.ThrowsException<ArgumentNullException>(() => nullSerializer!.DeserializeFromFile<TestData>(_tempFilePath));
		Assert.ThrowsException<ArgumentNullException>(() => nullSerializer!.DeserializeFromBinaryFile<TestData>(_tempFilePath));
		Assert.ThrowsException<ArgumentNullException>(() => nullSerializer!.SerializeToStream(_testData));
		Assert.ThrowsException<ArgumentNullException>(() => nullSerializer!.Clone(_testData));
	}

	/// <summary>
	/// Tests SerializeToStream with complex object.
	/// </summary>
	[TestMethod]
	public void SerializerExtensions_SerializeToStream_ComplexObject_Success()
	{
		// Arrange
		ComplexTestData complexData = new()
		{
			Id = 123,
			Name = "Complex Test",
			Items = ["Item1", "Item2", "Item3"],
			Metadata = new Dictionary<string, object>
			{
				{"key1", "value1"},
				{"key2", 42}
			}
		};

		// Act
		using MemoryStream stream = _serializer.SerializeToStream(complexData);

		// Assert
		Assert.IsNotNull(stream);
		Assert.IsTrue(stream.Length > 0);

		// Verify content by deserializing
		stream.Position = 0;
		ComplexTestData result = _serializer.DeserializeFromStream<ComplexTestData>(stream);
		Assert.AreEqual(complexData.Id, result.Id);
		Assert.AreEqual(complexData.Name, result.Name);
		CollectionAssert.AreEqual(complexData.Items, result.Items);
	}

	/// <summary>
	/// Tests that DeserializeFromStream preserves stream position.
	/// </summary>
	[TestMethod]
	public void SerializerExtensions_DeserializeFromStream_PreservesStreamPosition()
	{
		// Arrange
		using MemoryStream stream = new();
		byte[] prefix = "PREFIX"u8.ToArray();
		stream.Write(prefix, 0, prefix.Length);

		using MemoryStream serializedStream = _serializer.SerializeToStream(_testData);
		serializedStream.CopyTo(stream);

		byte[] suffix = "SUFFIX"u8.ToArray();
		stream.Write(suffix, 0, suffix.Length);

		// Position stream to start of serialized data
		stream.Position = prefix.Length;

		// Act
		TestData result = _serializer.DeserializeFromStream<TestData>(stream);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(_testData.IntValue, result.IntValue);
		Assert.AreEqual(_testData.StringValue, result.StringValue);

		// Verify stream position is at the end of serialized data
		byte[] remainingData = new byte[suffix.Length];
		int bytesRead = stream.Read(remainingData, 0, suffix.Length);
		Assert.AreEqual(suffix.Length, bytesRead);
		CollectionAssert.AreEqual(suffix, remainingData);
	}
}

/// <summary>
/// Complex test data class for testing.
/// </summary>
public class ComplexTestData
{
	public int Id { get; set; }
	public string? Name { get; set; }
	public List<string>? Items { get; set; }
	public Dictionary<string, object>? Metadata { get; set; }
}
