using BenchmarkDotNet.Running;
using UniversalSerializer.Benchmarks;

namespace UniversalSerializer.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<SerializerBenchmarks>();
    }
}
