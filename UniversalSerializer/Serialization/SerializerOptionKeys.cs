// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.UniversalSerializer.Serialization;
/// <summary>
/// Keys for serializer options.
/// </summary>
public static class SerializerOptionKeys
{
	/// <summary>
	/// Common serializer options shared across formats.
	/// </summary>
	public static class Common
	{
		/// <summary>
		/// Option key for enum serialization format.
		/// </summary>
		public const string EnumFormat = "Common:EnumFormat";

		/// <summary>
		/// Option key for using string conversion for unsupported types.
		/// </summary>
		public const string UseStringConversionForUnsupportedTypes = "Common:UseStringConversion";
	}

	/// <summary>
	/// JSON serializer options.
	/// </summary>
	public static class Json
	{
		/// <summary>
		/// Option key for case-insensitive property name matching.
		/// </summary>
		public const string CaseInsensitive = "Json:CaseInsensitive";

		/// <summary>
		/// Option key for allowing comments in JSON.
		/// </summary>
		public const string AllowComments = "Json:AllowComments";

		/// <summary>
		/// Option key for maximum depth of nested objects.
		/// </summary>
		public const string MaxDepth = "Json:MaxDepth";

		/// <summary>
		/// Option key for property naming policy.
		/// </summary>
		public const string PropertyNamingPolicy = "Json:PropertyNamingPolicy";
	}

	/// <summary>
	/// YAML serializer options.
	/// </summary>
	public static class Yaml
	{
		/// <summary>
		/// Option key for preferred file extension (.yaml or .yml).
		/// </summary>
		public const string PreferredExtension = "Yaml:PreferredExtension";

		/// <summary>
		/// Option key for indentation width.
		/// </summary>
		public const string IndentationWidth = "Yaml:IndentationWidth";

		/// <summary>
		/// Option key for emitting default values.
		/// </summary>
		public const string EmitDefaults = "Yaml:EmitDefaults";
	}

	/// <summary>
	/// MessagePack serializer options.
	/// </summary>
	public static class MessagePack
	{
		/// <summary>
		/// Option key for enabling LZ4 compression.
		/// </summary>
		public const string EnableLz4Compression = "MessagePack:EnableLz4Compression";
	}

	/// <summary>
	/// XML serializer options.
	/// </summary>
	public static class Xml
	{
		/// <summary>
		/// Option key for indenting XML output.
		/// </summary>
		public const string Indent = "Xml:Indent";

		/// <summary>
		/// Option key for omitting XML declaration.
		/// </summary>
		public const string OmitXmlDeclaration = "Xml:OmitXmlDeclaration";

		/// <summary>
		/// Option key for XML encoding.
		/// </summary>
		public const string Encoding = "Xml:Encoding";
	}

	/// <summary>
	/// Type registry-related options.
	/// </summary>
	public static class TypeRegistry
	{
		/// <summary>
		/// Option key for enabling type discriminators for polymorphic serialization.
		/// </summary>
		public const string EnableTypeDiscriminator = "TypeRegistry:EnableTypeDiscriminator";

		/// <summary>
		/// Option key for the property name used for type discrimination.
		/// </summary>
		public const string DiscriminatorPropertyName = "TypeRegistry:DiscriminatorPropertyName";

		/// <summary>
		/// Previous option key for the property name used for type discrimination (for backward compatibility).
		/// </summary>
		public const string TypeDiscriminatorPropertyName = "TypeRegistry:TypeDiscriminatorPropertyName";

		/// <summary>
		/// Option key for the format used for type discrimination.
		/// </summary>
		public const string DiscriminatorFormat = "TypeRegistry:DiscriminatorFormat";

		/// <summary>
		/// Previous option key for the format used for type discrimination (for backward compatibility).
		/// </summary>
		public const string TypeDiscriminatorFormat = "TypeRegistry:TypeDiscriminatorFormat";
	}
}
