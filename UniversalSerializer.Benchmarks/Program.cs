// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace UniversalSerializer.Benchmarks;
using BenchmarkDotNet.Running;

/// <summary>
/// Entry point for the benchmark application.
/// </summary>
public static class Program
{
	/// <summary>
	/// Main entry point for running benchmarks.
	/// </summary>
	/// <param name="args">Command line arguments.</param>
#pragma warning disable IDE0060 // Remove unused parameter
	public static void Main(string[] args)
#pragma warning restore IDE0060 // Remove unused parameter
	{
		BenchmarkDotNet.Reports.Summary summary = BenchmarkRunner.Run<SerializerBenchmarks>();
	}
}
