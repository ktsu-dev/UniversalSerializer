// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Serialization;

/// <summary>
/// Constants for serializer option keys to avoid string literals.
/// </summary>
public static class SerializerOptionKeys
{
	/// <summary>
	/// Common option keys that apply to all serializers.
	/// </summary>
	public static class Common
	{
		/// <summary>
		/// Option to ignore null values during serialization.
		/// </summary>
		public const string IgnoreNullValues = "Common:IgnoreNullValues";

		/// <summary>
		/// Option to ignore read-only properties during serialization.
		/// </summary>
		public const string IgnoreReadOnlyProperties = "Common:IgnoreReadOnlyProperties";

		/// <summary>
		/// Option to use pretty printing during serialization.
		/// </summary>
		public const string PrettyPrint = "Common:PrettyPrint";

		/// <summary>
		/// Option to specify how enums are serialized (name, value, or both).
		/// </summary>
		public const string EnumFormat = "Common:EnumFormat";

		/// <summary>
		/// Option to specify the serialization format for DateTime values.
		/// </summary>
		public const string DateTimeFormat = "Common:DateTimeFormat";

		/// <summary>
		/// Option to specify the serialization format for DateTimeOffset values.
		/// </summary>
		public const string DateTimeOffsetFormat = "Common:DateTimeOffsetFormat";

		/// <summary>
		/// Option to specify the serialization format for TimeSpan values.
		/// </summary>
		public const string TimeSpanFormat = "Common:TimeSpanFormat";

		/// <summary>
		/// Option to specify string conversion for unsupported types.
		/// </summary>
		public const string UseStringConversionForUnsupportedTypes = "Common:UseStringConversionForUnsupportedTypes";

		/// <summary>
		/// Option to control the encoding used for strings (default: UTF-8).
		/// </summary>
		public const string Encoding = "Common:Encoding";

		/// <summary>
		/// Option to enable type discriminator for polymorphic serialization.
		/// </summary>
		public const string EnableTypeDiscriminator = "Common:EnableTypeDiscriminator";

		/// <summary>
		/// Option to control the discriminator format for polymorphic serialization.
		/// </summary>
		public const string DiscriminatorFormat = "Common:DiscriminatorFormat";
	}

	/// <summary>
	/// Type registry option keys for polymorphic serialization.
	/// </summary>
	public static class TypeRegistry
	{
		/// <summary>
		/// Option to use fully qualified type names.
		/// </summary>
		public const string UseFullyQualifiedTypeNames = "TypeRegistry:UseFullyQualifiedTypeNames";
	}

	/// <summary>
	/// JSON-specific option keys.
	/// </summary>
	public static class Json
	{
		/// <summary>
		/// Option to allow comments in JSON.
		/// </summary>
		public const string AllowComments = "Json:AllowComments";

		/// <summary>
		/// Option to allow trailing commas in JSON.
		/// </summary>
		public const string AllowTrailingCommas = "Json:AllowTrailingCommas";

		/// <summary>
		/// Option to use case-insensitive property matching.
		/// </summary>
		public const string CaseInsensitive = "Json:CaseInsensitive";

		/// <summary>
		/// Option to specify the maximum depth for JSON parsing.
		/// </summary>
		public const string MaxDepth = "Json:MaxDepth";

		/// <summary>
		/// Option to specify the property naming policy (camelCase, PascalCase, etc.).
		/// </summary>
		public const string PropertyNamingPolicy = "Json:PropertyNamingPolicy";

		/// <summary>
		/// Option to preserve object references in JSON serialization.
		/// </summary>
		public const string PreserveReferences = "Json:PreserveReferences";

		/// <summary>
		/// Option to control whether JSON is pretty-printed (indented).
		/// </summary>
		public const string Indent = "Json:Indent";

		/// <summary>
		/// Option to control whether property names are camel-cased in JSON.
		/// </summary>
		public const string CamelCase = "Json:CamelCase";

		/// <summary>
		/// Option to control whether to write bytes as base64 strings.
		/// </summary>
		public const string WriteBytesAsBase64 = "Json:WriteBytesAsBase64";

		/// <summary>
		/// Option to allow unquoted field names.
		/// </summary>
		public const string AllowUnquotedFieldNames = "Json:AllowUnquotedFieldNames";

		/// <summary>
		/// Option to allow single quotes for JSON strings.
		/// </summary>
		public const string AllowSingleQuotes = "Json:AllowSingleQuotes";

		/// <summary>
		/// Option to write enum values as strings.
		/// </summary>
		public const string WriteEnumsAsStrings = "Json:WriteEnumsAsStrings";

		/// <summary>
		/// Option to write dates as ISO 8601 strings.
		/// </summary>
		public const string WriteDatesAsIso8601 = "Json:WriteDatesAsIso8601";
	}

	/// <summary>
	/// XML-specific option keys.
	/// </summary>
	public static class Xml
	{
		/// <summary>
		/// Option to indent XML output.
		/// </summary>
		public const string Indent = "Xml:Indent";

		/// <summary>
		/// Option to omit the XML declaration.
		/// </summary>
		public const string OmitXmlDeclaration = "Xml:OmitXmlDeclaration";

		/// <summary>
		/// Option to specify the XML encoding.
		/// </summary>
		public const string Encoding = "Xml:Encoding";

		/// <summary>
		/// Option to specify the XML namespace handling.
		/// </summary>
		public const string NamespaceHandling = "Xml:NamespaceHandling";

		/// <summary>
		/// Option to specify the default namespace.
		/// </summary>
		public const string DefaultNamespace = "Xml:DefaultNamespace";

		/// <summary>
		/// Option to include type information in XML serialization.
		/// </summary>
		public const string IncludeTypeInformation = "Xml:IncludeTypeInformation";

		/// <summary>
		/// Option to include XML declaration.
		/// </summary>
		public const string WriteDeclaration = "Xml:WriteDeclaration";

		/// <summary>
		/// Option to omit XML namespaces.
		/// </summary>
		public const string OmitXmlNamespace = "Xml:OmitXmlNamespace";

		/// <summary>
		/// Option to use XmlSerializer (true) or XmlReader/Writer (false).
		/// </summary>
		public const string UseXmlSerializer = "Xml:UseXmlSerializer";

		/// <summary>
		/// Option to wrap collections in a root element.
		/// </summary>
		public const string WrapCollections = "Xml:WrapCollections";

		/// <summary>
		/// Option to specify the root element name for collections.
		/// </summary>
		public const string CollectionWrapperName = "Xml:CollectionWrapperName";

		/// <summary>
		/// Option to specify the item element name for collections.
		/// </summary>
		public const string CollectionItemName = "Xml:CollectionItemName";
	}

	/// <summary>
	/// YAML-specific option keys.
	/// </summary>
	public static class Yaml
	{
		/// <summary>
		/// Option to emit default values in YAML serialization.
		/// </summary>
		public const string EmitDefaults = "Yaml:EmitDefaults";

		/// <summary>
		/// Option to specify the indentation width for YAML output.
		/// </summary>
		public const string IndentationWidth = "Yaml:IndentationWidth";

		/// <summary>
		/// Option to specify the YAML encoding.
		/// </summary>
		public const string Encoding = "Yaml:Encoding";

		/// <summary>
		/// Option to specify the YAML default flow style.
		/// </summary>
		public const string DefaultFlowStyle = "Yaml:DefaultFlowStyle";

		/// <summary>
		/// Option to specify the YAML scalar style.
		/// </summary>
		public const string ScalarStyle = "Yaml:ScalarStyle";

		/// <summary>
		/// Option to specify whether to emit alias nodes.
		/// </summary>
		public const string EmitAlias = "Yaml:EmitAlias";

		/// <summary>
		/// Option to indent YAML.
		/// </summary>
		public const string Indent = "Yaml:Indent";

		/// <summary>
		/// Option to emit YAML document start.
		/// </summary>
		public const string EmitDocumentStart = "Yaml:EmitDocumentStart";

		/// <summary>
		/// Option to emit YAML document end.
		/// </summary>
		public const string EmitDocumentEnd = "Yaml:EmitDocumentEnd";

		/// <summary>
		/// Option to emit YAML schema tags.
		/// </summary>
		public const string EmitTags = "Yaml:EmitTags";

		/// <summary>
		/// Option to enable canonical output (sorted keys).
		/// </summary>
		public const string Canonical = "Yaml:Canonical";

		/// <summary>
		/// Option to use double-quoted strings.
		/// </summary>
		public const string UseDoubleQuotes = "Yaml:UseDoubleQuotes";

		/// <summary>
		/// Option to write enum values as strings.
		/// </summary>
		public const string WriteEnumsAsStrings = "Yaml:WriteEnumsAsStrings";
	}

	/// <summary>
	/// TOML-specific option keys.
	/// </summary>
	public static class Toml
	{
		/// <summary>
		/// Option to specify the date time format for TOML serialization.
		/// </summary>
		public const string DateTimeFormat = "Toml:DateTimeFormat";

		/// <summary>
		/// Option to use inline tables for complex objects.
		/// </summary>
		public const string InlineTablesAsObjects = "Toml:InlineTablesAsObjects";

		/// <summary>
		/// Option to indent nested tables.
		/// </summary>
		public const string IndentTables = "Toml:IndentTables";

		/// <summary>
		/// Option to specify the number of spaces to use for indentation.
		/// </summary>
		public const string IndentSpaces = "Toml:IndentSpaces";

		/// <summary>
		/// Option to allow multi-line values.
		/// </summary>
		public const string MultiLineStrings = "Toml:MultiLineStrings";

		/// <summary>
		/// Option to use literal strings (with single quotes).
		/// </summary>
		public const string UseLiteralStrings = "Toml:UseLiteralStrings";

		/// <summary>
		/// Option to use in-line tables for complex objects.
		/// </summary>
		public const string UseInlineTables = "Toml:UseInlineTables";

		/// <summary>
		/// Option to use multi-line strings.
		/// </summary>
		public const string UseMultiLineStrings = "Toml:UseMultiLineStrings";

		/// <summary>
		/// Option to use multi-line arrays.
		/// </summary>
		public const string UseMultiLineArrays = "Toml:UseMultiLineArrays";

		/// <summary>
		/// Option to write dates in simplified format.
		/// </summary>
		public const string SimplifiedDateFormat = "Toml:SimplifiedDateFormat";

		/// <summary>
		/// Option to indent TOML output.
		/// </summary>
		public const string Indent = "Toml:Indent";

		/// <summary>
		/// Option to write enum values as strings.
		/// </summary>
		public const string WriteEnumsAsStrings = "Toml:WriteEnumsAsStrings";
	}

	/// <summary>
	/// MessagePack-specific option keys.
	/// </summary>
	public static class MessagePack
	{
		/// <summary>
		/// Option to enable LZ4 compression for MessagePack serialization.
		/// </summary>
		public const string EnableLz4Compression = "MessagePack:EnableLz4Compression";

		/// <summary>
		/// Option to specify the compression level for MessagePack serialization.
		/// </summary>
		public const string CompressionLevel = "MessagePack:CompressionLevel";

		/// <summary>
		/// Option to omit assembly version when serializing types.
		/// </summary>
		public const string OmitAssemblyVersion = "MessagePack:OmitAssemblyVersion";

		/// <summary>
		/// Option to allow serialization of private members.
		/// </summary>
		public const string AllowPrivateMembers = "MessagePack:AllowPrivateMembers";

		/// <summary>
		/// Option to use compatibility mode for older versions.
		/// </summary>
		public const string UseCompatibilityMode = "MessagePack:UseCompatibilityMode";

		/// <summary>
		/// Option to specify the security level for MessagePack serialization.
		/// </summary>
		public const string SecurityLevel = "MessagePack:SecurityLevel";

		/// <summary>
		/// Option to compress small strings.
		/// </summary>
		public const string CompressSmallStrings = "MessagePack:CompressSmallStrings";

		/// <summary>
		/// Option to allow complex types to be serialized as strings if possible.
		/// </summary>
		public const string AllowComplexTypesAsStrings = "MessagePack:AllowComplexTypesAsStrings";

		/// <summary>
		/// Option to enable compression.
		/// </summary>
		public const string EnableCompression = "MessagePack:EnableCompression";
	}

	/// <summary>
	/// Protobuf-specific option keys.
	/// </summary>
	public static class Protobuf
	{
		/// <summary>
		/// Option to skip construction of objects during deserialization.
		/// </summary>
		public const string SkipConstructor = "Protobuf:SkipConstructor";

		/// <summary>
		/// Option to omit assembly version when serializing types.
		/// </summary>
		public const string OmitAssemblyVersion = "Protobuf:OmitAssemblyVersion";

		/// <summary>
		/// Option to allow serialization of private members.
		/// </summary>
		public const string AllowPrivateMembers = "Protobuf:AllowPrivateMembers";

		/// <summary>
		/// Option to use compatibility mode for older versions.
		/// </summary>
		public const string UseCompatibilityMode = "Protobuf:UseCompatibilityMode";

		/// <summary>
		/// Option to enable field-level default values.
		/// </summary>
		public const string UseImplicitDefaults = "Protobuf:UseImplicitDefaults";

		/// <summary>
		/// Option to specify the inference method for protobuf field numbers.
		/// </summary>
		public const string InferTagFromName = "Protobuf:InferTagFromName";

		/// <summary>
		/// Option to enable dynamic type support.
		/// </summary>
		public const string AutoAddMissingTypes = "Protobuf:AutoAddMissingTypes";

		/// <summary>
		/// Option to preserve object references.
		/// </summary>
		public const string PreserveObjectReferences = "Protobuf:PreserveObjectReferences";

		/// <summary>
		/// Option to include type information for polymorphic serialization.
		/// </summary>
		public const string IncludeTypeInfo = "Protobuf:IncludeTypeInfo";

		/// <summary>
		/// Option to support private members.
		/// </summary>
		public const string SupportPrivateMembers = "Protobuf:SupportPrivateMembers";

		/// <summary>
		/// Option to enable dynamic serialization for types without attributes.
		/// </summary>
		public const string EnableDynamicSerialization = "Protobuf:EnableDynamicSerialization";
	}

	/// <summary>
	/// FlatBuffers-specific option keys.
	/// </summary>
	public static class FlatBuffers
	{
		/// <summary>
		/// Option to force defaults.
		/// </summary>
		public const string ForceDefaults = "FlatBuffers:ForceDefaults";

		/// <summary>
		/// Option to share strings.
		/// </summary>
		public const string ShareStrings = "FlatBuffers:ShareStrings";

		/// <summary>
		/// Option to share keys.
		/// </summary>
		public const string ShareKeys = "FlatBuffers:ShareKeys";

		/// <summary>
		/// Option to use indirect strings.
		/// </summary>
		public const string IndirectStrings = "FlatBuffers:IndirectStrings";

		/// <summary>
		/// Option to enable type discriminator for polymorphic serialization.
		/// </summary>
		public const string EnableTypeDiscriminator = "FlatBuffers:EnableTypeDiscriminator";

		/// <summary>
		/// Option to set the deserialize mode for FlatBuffers objects.
		/// </summary>
		/// <remarks>
		/// Valid values: "Lazy", "Greedy", "Progressive"
		/// </remarks>
		public const string DeserializeMode = "FlatBuffers:DeserializeMode";

		/// <summary>
		/// Option to enable memory pooling for better performance.
		/// </summary>
		public const string EnableMemoryPooling = "FlatBuffers:EnableMemoryPooling";

		/// <summary>
		/// Option to enable string deduplication during serialization.
		/// </summary>
		public const string EnableStringDeduplication = "FlatBuffers:EnableStringDeduplication";

		/// <summary>
		/// Option to use vector metadata for faster access.
		/// </summary>
		public const string UseVectorMetadata = "FlatBuffers:UseVectorMetadata";
	}
}
