// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace UniversalSerializer.Benchmarks;
using System.Runtime.Serialization;
using ProtoBuf;

[ProtoContract]
[DataContract]
public class TestData
{
	[ProtoMember(1)]
	[DataMember]
	public int Id { get; set; }

	[ProtoMember(2)]
	[DataMember]
	public string? Name { get; set; }

	[ProtoMember(3)]
	[DataMember]
	public string? Description { get; set; }

	[ProtoMember(4)]
	[DataMember]
	public DateTime CreatedAt { get; set; }

	[ProtoMember(5)]
	[DataMember]
	public bool IsActive { get; set; }

	[ProtoMember(6)]
	[DataMember]
	public decimal Price { get; set; }

	[ProtoMember(7)]
	[DataMember]
	public List<string>? Tags { get; set; }

	[ProtoMember(8)]
	[DataMember]
	public Dictionary<string, string>? Properties { get; set; }
}
