// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace UniversalSerializer.Benchmarks;
using System.Runtime.Serialization;

/// <summary>
/// Test data class used for benchmarking serializers.
/// </summary>
[DataContract]
public class TestData
{
	/// <summary>
	/// Gets or sets the unique identifier.
	/// </summary>
	[DataMember]
	public int Id { get; set; }

	/// <summary>
	/// Gets or sets the name.
	/// </summary>
	[DataMember]
	public string? Name { get; set; }

	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	[DataMember]
	public string? Description { get; set; }

	/// <summary>
	/// Gets or sets the creation date and time.
	/// </summary>
	[DataMember]
	public DateTime CreatedAt { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether this instance is active.
	/// </summary>
	[DataMember]
	public bool IsActive { get; set; }

	/// <summary>
	/// Gets or sets the price.
	/// </summary>
	[DataMember]
	public decimal Price { get; set; }

	/// <summary>
	/// Gets or sets the collection of tags.
	/// </summary>
	[DataMember]
	public IList<string>? Tags { get; set; }

	/// <summary>
	/// Gets or sets the properties dictionary.
	/// </summary>
	[DataMember]
	public Dictionary<string, string>? Properties { get; set; }
}
