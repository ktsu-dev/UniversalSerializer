using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Microsoft.Extensions.DependencyInjection;
using UniversalSerializer.Serialization;

namespace UniversalSerializer.Benchmarks;

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
                   .AddMessagePackSerializer()
                   .AddProtobufSerializer()
                   .AddFlatBuffersSerializer();
        });
        var serviceProvider = services.BuildServiceProvider();
        _serializerFactory = serviceProvider.GetRequiredService<ISerializerFactory>();

        // Create test data
        _testData = new TestData
        {
            Id = 12345,
            Name = "Test Object",
            Description = "This is a test object for benchmarking serializers",
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            Price = 99.99m,
            Tags = new List<string> { "test", "benchmark", "serialization", "performance" },
            Properties = new Dictionary<string, string>
            {
                { "Property1", "Value1" },
                { "Property2", "Value2" },
                { "Property3", "Value3" },
                { "Property4", "Value4" },
                { "Property5", "Value5" }
            }
        };

        // Create array of test data for collection serialization tests
        _testDataArray = Enumerable.Range(1, 100).Select(i => new TestData
        {
            Id = i,
            Name = $"Test Object {i}",
            Description = $"This is test object {i} for benchmarking serializers",
            CreatedAt = DateTime.UtcNow.AddDays(-i),
            IsActive = i % 2 == 0,
            Price = 99.99m + i,
            Tags = new List<string> { "test", "benchmark", $"tag-{i}" },
            Properties = new Dictionary<string, string>
            {
                { $"Property-{i}-1", $"Value-{i}-1" },
                { $"Property-{i}-2", $"Value-{i}-2" }
            }
        }).ToArray();
    }

    [Benchmark]
    public string SerializeJson()
    {
        var serializer = _serializerFactory.GetJsonSerializer();
        return serializer.Serialize(_testData);
    }

    [Benchmark]
    public TestData DeserializeJson()
    {
        var serializer = _serializerFactory.GetJsonSerializer();
        string json = serializer.Serialize(_testData);
        return serializer.Deserialize<TestData>(json);
    }

    [Benchmark]
    public string SerializeXml()
    {
        var serializer = _serializerFactory.GetXmlSerializer();
        return serializer.Serialize(_testData);
    }

    [Benchmark]
    public TestData DeserializeXml()
    {
        var serializer = _serializerFactory.GetXmlSerializer();
        string xml = serializer.Serialize(_testData);
        return serializer.Deserialize<TestData>(xml);
    }

    [Benchmark]
    public string SerializeYaml()
    {
        var serializer = _serializerFactory.GetYamlSerializer();
        return serializer.Serialize(_testData);
    }

    [Benchmark]
    public TestData DeserializeYaml()
    {
        var serializer = _serializerFactory.GetYamlSerializer();
        string yaml = serializer.Serialize(_testData);
        return serializer.Deserialize<TestData>(yaml);
    }

    [Benchmark]
    public string SerializeToml()
    {
        var serializer = _serializerFactory.GetTomlSerializer();
        return serializer.Serialize(_testData);
    }

    [Benchmark]
    public TestData DeserializeToml()
    {
        var serializer = _serializerFactory.GetTomlSerializer();
        string toml = serializer.Serialize(_testData);
        return serializer.Deserialize<TestData>(toml);
    }

    [Benchmark]
    public byte[] SerializeMessagePack()
    {
        var serializer = _serializerFactory.GetMessagePackSerializer();
        return serializer.SerializeToBytes(_testData);
    }

    [Benchmark]
    public TestData DeserializeMessagePack()
    {
        var serializer = _serializerFactory.GetMessagePackSerializer();
        byte[] data = serializer.SerializeToBytes(_testData);
        return serializer.DeserializeFromBytes<TestData>(data);
    }

    [Benchmark]
    public byte[] SerializeProtobuf()
    {
        var serializer = _serializerFactory.GetProtobufSerializer();
        return serializer.SerializeToBytes(_testData);
    }

    [Benchmark]
    public TestData DeserializeProtobuf()
    {
        var serializer = _serializerFactory.GetProtobufSerializer();
        byte[] data = serializer.SerializeToBytes(_testData);
        return serializer.DeserializeFromBytes<TestData>(data);
    }

    [Benchmark]
    public byte[] SerializeFlatBuffers()
    {
        var serializer = _serializerFactory.GetFlatBuffersSerializer();
        return serializer.SerializeToBytes(_testData);
    }

    [Benchmark]
    public TestData DeserializeFlatBuffers()
    {
        var serializer = _serializerFactory.GetFlatBuffersSerializer();
        byte[] data = serializer.SerializeToBytes(_testData);
        return serializer.DeserializeFromBytes<TestData>(data);
    }

    [Benchmark]
    public string[] SerializeJsonArray()
    {
        var serializer = _serializerFactory.GetJsonSerializer();
        return _testDataArray.Select(data => serializer.Serialize(data)).ToArray();
    }

    [Benchmark]
    public byte[][] SerializeMessagePackArray()
    {
        var serializer = _serializerFactory.GetMessagePackSerializer();
        return _testDataArray.Select(data => serializer.SerializeToBytes(data)).ToArray();
    }
}
