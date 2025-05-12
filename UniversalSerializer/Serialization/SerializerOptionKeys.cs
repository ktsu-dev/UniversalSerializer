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
    /// JSON-specific option keys.
    /// </summary>
    public static class Json
    {
        /// <summary>
        /// Option to allow comments in JSON.
        /// </summary>
        public const string AllowComments = "Json:AllowComments";

        /// <summary>
        /// Option to enable case-insensitive property matching.
        /// </summary>
        public const string CaseInsensitive = "Json:CaseInsensitive";

        /// <summary>
        /// Option to specify date time format.
        /// </summary>
        public const string DateTimeFormat = "Json:DateTimeFormat";

        /// <summary>
        /// Option to specify property naming policy.
        /// </summary>
        public const string PropertyNamingPolicy = "Json:PropertyNamingPolicy";

        /// <summary>
        /// Option to specify maximum depth for nested objects.
        /// </summary>
        public const string MaxDepth = "Json:MaxDepth";
    }

    /// <summary>
    /// XML-specific option keys.
    /// </summary>
    public static class Xml
    {
        /// <summary>
        /// Option to enable indentation in XML output.
        /// </summary>
        public const string Indent = "Xml:Indent";

        /// <summary>
        /// Option to omit XML declaration.
        /// </summary>
        public const string OmitXmlDeclaration = "Xml:OmitXmlDeclaration";

        /// <summary>
        /// Option to specify encoding.
        /// </summary>
        public const string Encoding = "Xml:Encoding";

        /// <summary>
        /// Option to specify namespace handling.
        /// </summary>
        public const string NamespaceHandling = "Xml:NamespaceHandling";
    }

    /// <summary>
    /// YAML-specific option keys.
    /// </summary>
    public static class Yaml
    {
        /// <summary>
        /// Option to emit default values.
        /// </summary>
        public const string EmitDefaults = "Yaml:EmitDefaults";

        /// <summary>
        /// Option to specify indentation width.
        /// </summary>
        public const string IndentationWidth = "Yaml:IndentationWidth";

        /// <summary>
        /// Option to specify encoding.
        /// </summary>
        public const string Encoding = "Yaml:Encoding";
    }

    /// <summary>
    /// TOML-specific option keys.
    /// </summary>
    public static class Toml
    {
        /// <summary>
        /// Option to specify date time format.
        /// </summary>
        public const string DateTimeFormat = "Toml:DateTimeFormat";

        /// <summary>
        /// Option to treat inline tables as objects.
        /// </summary>
        public const string InlineTablesAsObjects = "Toml:InlineTablesAsObjects";
    }

    /// <summary>
    /// INI-specific option keys.
    /// </summary>
    public static class Ini
    {
        /// <summary>
        /// Option to specify section name casing.
        /// </summary>
        public const string SectionNameCasing = "Ini:SectionNameCasing";

        /// <summary>
        /// Option to allow duplicate keys.
        /// </summary>
        public const string AllowDuplicateKeys = "Ini:AllowDuplicateKeys";
    }

    /// <summary>
    /// MessagePack-specific option keys.
    /// </summary>
    public static class MessagePack
    {
        /// <summary>
        /// Option to use compression.
        /// </summary>
        public const string UseCompression = "MessagePack:UseCompression";

        /// <summary>
        /// Option to specify compression level.
        /// </summary>
        public const string CompressionLevel = "MessagePack:CompressionLevel";

        /// <summary>
        /// Option to enable LZ4 compression.
        /// </summary>
        public const string EnableLz4Compression = "MessagePack:EnableLz4Compression";

        /// <summary>
        /// Option to omit assembly version.
        /// </summary>
        public const string OmitAssemblyVersion = "MessagePack:OmitAssemblyVersion";

        /// <summary>
        /// Option to allow serialization of private members.
        /// </summary>
        public const string AllowPrivateMembers = "MessagePack:AllowPrivateMembers";
    }

    /// <summary>
    /// Protobuf-specific option keys.
    /// </summary>
    public static class Protobuf
    {
        /// <summary>
        /// Option to use implicit fields.
        /// </summary>
        public const string UseImplicitFields = "Protobuf:UseImplicitFields";

        /// <summary>
        /// Option to skip default values.
        /// </summary>
        public const string SkipDefaults = "Protobuf:SkipDefaults";

        /// <summary>
        /// Option to preserve references.
        /// </summary>
        public const string PreserveReferencesHandling = "Protobuf:PreserveReferencesHandling";

        /// <summary>
        /// Option to specify type format.
        /// </summary>
        public const string TypeFormat = "Protobuf:TypeFormat";
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
    }
}
