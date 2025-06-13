// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace UniversalSerializer.Benchmarks;
using BenchmarkDotNet.Running;

public static class Program
{
	public static void Main(string[] args)
	{
		BenchmarkDotNet.Reports.Summary summary = BenchmarkRunner.Run<SerializerBenchmarks>();
	}
}
