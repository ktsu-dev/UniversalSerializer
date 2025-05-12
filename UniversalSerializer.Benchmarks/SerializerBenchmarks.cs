// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace UniversalSerializer.Benchmarks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using ktsu.UniversalSerializer.Serialization;
using ktsu.UniversalSerializer.Serialization.Json;
using ktsu.UniversalSerializer.Serialization.Xml;
using ktsu.UniversalSerializer.Serialization.Yaml;
using ktsu.UniversalSerializer.Serialization.Toml;
using ktsu.UniversalSerializer.Serialization.MessagePack;
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
		var services = new ServiceCollection();
		services.AddUniversalSerializer(builder =>
		{
			builder.AddJsonSerializer()
			       .AddXmlSerializer()
			       .AddYamlSerializer()
			       .AddTomlSerializer()
			       .AddMessagePackSerializer();
		});
		var serviceProvider = services.BuildServiceProvider();
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
		for (var i = 0; i < 10; i++)
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
	public string Json_Serialize()
	{
		var serializer = _serializerFactory.GetSerializer<JsonSerializer>();
		return serializer.Serialize(_testData);
	}

	[Benchmark]
	public TestData Json_Deserialize()
	{
		var serializer = _serializerFactory.GetSerializer<JsonSerializer>();
		var json = serializer.Serialize(_testData);
		return serializer.Deserialize<TestData>(json);
	}

	[Benchmark]
	public string Xml_Serialize()
	{
		var serializer = _serializerFactory.GetSerializer<XmlSerializer>();
		return serializer.Serialize(_testData);
	}

	[Benchmark]
	public TestData Xml_Deserialize()
	{
		var serializer = _serializerFactory.GetSerializer<XmlSerializer>();
		var xml = serializer.Serialize(_testData);
		return serializer.Deserialize<TestData>(xml);
	}

	[Benchmark]
	public string Yaml_Serialize()
	{
		var serializer = _serializerFactory.GetSerializer<YamlSerializer>();
		return serializer.Serialize(_testData);
	}

	[Benchmark]
	public TestData Yaml_Deserialize()
	{
		var serializer = _serializerFactory.GetSerializer<YamlSerializer>();
		var yaml = serializer.Serialize(_testData);
		return serializer.Deserialize<TestData>(yaml);
	}

	[Benchmark]
	public string Toml_Serialize()
	{
		var serializer = _serializerFactory.GetSerializer<TomlSerializer>();
		return serializer.Serialize(_testData);
	}

	[Benchmark]
	public TestData Toml_Deserialize()
	{
		var serializer = _serializerFactory.GetSerializer<TomlSerializer>();
		var toml = serializer.Serialize(_testData);
		return serializer.Deserialize<TestData>(toml);
	}

	[Benchmark]
	public string MessagePack_Serialize()
	{
		var serializer = _serializerFactory.GetSerializer<MessagePackSerializer>();
		return serializer.Serialize(_testData);
	}

	[Benchmark]
	public TestData MessagePack_Deserialize()
	{
		var serializer = _serializerFactory.GetSerializer<MessagePackSerializer>();
		var msgpack = serializer.Serialize(_testData);
		return serializer.Deserialize<TestData>(msgpack);
	}

	[Benchmark]
	public string[] Json_SerializeArray()
	{
		var serializer = _serializerFactory.GetSerializer<JsonSerializer>();
		var results = new string[_testDataArray.Length];
		for (var i = 0; i < _testDataArray.Length; i++)
		{
			results[i] = serializer.Serialize(_testDataArray[i]);
		}
		return results;
	}

	[Benchmark]
	public string[] MessagePack_SerializeArray()
	{
		var serializer = _serializerFactory.GetSerializer<MessagePackSerializer>();
		var results = new string[_testDataArray.Length];
		for (var i = 0; i < _testDataArray.Length; i++)
		{
			results[i] = serializer.Serialize(_testDataArray[i]);
		}
		return results;
	}
}
