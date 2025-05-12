// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace UniversalSerializer.Benchmarks;
using System.Runtime.Serialization;

[DataContract]
public class TestData
{
	[DataMember]
	public int Id { get; set; }

	[DataMember]
	public string? Name { get; set; }

	[DataMember]
	public string? Description { get; set; }

	[DataMember]
	public DateTime CreatedAt { get; set; }

	[DataMember]
	public bool IsActive { get; set; }

	[DataMember]
	public decimal Price { get; set; }

	[DataMember]
	public List<string>? Tags { get; set; }

	[DataMember]
	public Dictionary<string, string>? Properties { get; set; }
}
