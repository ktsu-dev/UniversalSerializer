// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace UniversalSerializer.Benchmarks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using ktsu.UniversalSerializer.Serialization;
using ktsu.UniversalSerializer.Serialization.DependencyInjection;
using ktsu.UniversalSerializer.Serialization.Json;
using ktsu.UniversalSerializer.Serialization.MessagePack;
using ktsu.UniversalSerializer.Serialization.Toml;
using ktsu.UniversalSerializer.Serialization.Xml;
using ktsu.UniversalSerializer.Serialization.Yaml;
using Microsoft.Extensions.DependencyInjection;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class SerializerBenchmarks
{
	private readonly ISerializerFactory _serializerFactory;
	private readonly TestData _testData;
	private readonly TestData[] _testDataArray;

	public SerializerBenchmarks()
	{
		ServiceCollection services = new();
		services.AddUniversalSerializer(options =>
		{
			// Configure serialization options
		});

		// Add serializers to the container
		services.AddJsonSerializer()
			   .AddXmlSerializer()
			   .AddYamlSerializer()
			   .AddTomlSerializer()
			   .AddMessagePackSerializer();

		ServiceProvider serviceProvider = services.BuildServiceProvider();
		_serializerFactory = serviceProvider.GetRequiredService<ISerializerFactory>();

		// Create test data
		_testData = new TestData
		{
			Id = 1,
			Name = "Test Object",
			Description = "This is a test object for benchmarking serializers",
			CreatedAt = DateTime.UtcNow,
			IsActive = true,
			Price = 123.45M,
			Tags = ["serialization", "benchmark", "performance", "testing"],
			Properties = new Dictionary<string, string>
			{
				["key1"] = "value1",
				["key2"] = "value2",
				["key3"] = "value3"
			}
		};

		// Create array data for collection tests
		_testDataArray = new TestData[10];
		for (int i = 0; i < 10; i++)
		{
			_testDataArray[i] = new TestData
			{
				Id = i + 1,
				Name = $"Test Object {i + 1}",
				Description = $"This is test object {i + 1} for benchmarking serializers",
				CreatedAt = DateTime.UtcNow.AddDays(-i),
				IsActive = i % 2 == 0,
				Price = 100 + (i * 10.5M),
				Tags = [$"tag{i}_1", $"tag{i}_2", $"tag{i}_3"],
				Properties = new Dictionary<string, string>
				{
					[$"key{i}_1"] = $"value{i}_1",
					[$"key{i}_2"] = $"value{i}_2"
				}
			};
		}
	}

	[Benchmark(Baseline = true)]
	public string JsonSerialize()
	{
		JsonSerializer serializer = _serializerFactory.GetSerializer<JsonSerializer>();
		return serializer.Serialize(_testData);
	}

	[Benchmark]
	public TestData JsonDeserialize()
	{
		JsonSerializer serializer = _serializerFactory.GetSerializer<JsonSerializer>();
		string json = serializer.Serialize(_testData);
		return serializer.Deserialize<TestData>(json);
	}

	[Benchmark]
	public string XmlSerialize()
	{
		XmlSerializer serializer = _serializerFactory.GetSerializer<XmlSerializer>();
		return serializer.Serialize(_testData);
	}

	[Benchmark]
	public TestData XmlDeserialize()
	{
		XmlSerializer serializer = _serializerFactory.GetSerializer<XmlSerializer>();
		string xml = serializer.Serialize(_testData);
		return serializer.Deserialize<TestData>(xml);
	}

	[Benchmark]
	public string YamlSerialize()
	{
		YamlSerializer serializer = _serializerFactory.GetSerializer<YamlSerializer>();
		return serializer.Serialize(_testData);
	}

	[Benchmark]
	public TestData YamlDeserialize()
	{
		YamlSerializer serializer = _serializerFactory.GetSerializer<YamlSerializer>();
		string yaml = serializer.Serialize(_testData);
		return serializer.Deserialize<TestData>(yaml);
	}

	[Benchmark]
	public string TomlSerialize()
	{
		TomlSerializer serializer = _serializerFactory.GetSerializer<TomlSerializer>();
		return serializer.Serialize(_testData);
	}

	[Benchmark]
	public TestData TomlDeserialize()
	{
		TomlSerializer serializer = _serializerFactory.GetSerializer<TomlSerializer>();
		string toml = serializer.Serialize(_testData);
		return serializer.Deserialize<TestData>(toml);
	}

	[Benchmark]
	public string MessagePackSerialize()
	{
		MessagePackSerializer serializer = _serializerFactory.GetSerializer<MessagePackSerializer>();
		return serializer.Serialize(_testData);
	}

	[Benchmark]
	public TestData MessagePackDeserialize()
	{
		MessagePackSerializer serializer = _serializerFactory.GetSerializer<MessagePackSerializer>();
		string msgpack = serializer.Serialize(_testData);
		return serializer.Deserialize<TestData>(msgpack);
	}

	[Benchmark]
	public string[] JsonSerializeArray()
	{
		JsonSerializer serializer = _serializerFactory.GetSerializer<JsonSerializer>();
		string[] results = new string[_testDataArray.Length];
		for (int i = 0; i < _testDataArray.Length; i++)
		{
			results[i] = serializer.Serialize(_testDataArray[i]);
		}

		return results;
	}

	[Benchmark]
	public string[] MessagePackSerializeArray()
	{
		MessagePackSerializer serializer = _serializerFactory.GetSerializer<MessagePackSerializer>();
		string[] results = new string[_testDataArray.Length];
		for (int i = 0; i < _testDataArray.Length; i++)
		{
			results[i] = serializer.Serialize(_testDataArray[i]);
		}

		return results;
	}
}
