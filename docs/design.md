# UniversalSerializer - Design Document

## Overview

UniversalSerializer is a .NET library that provides a unified interface for various serialization formats. It abstracts away the implementation details of different serialization libraries, allowing developers to easily switch between formats without changing their code. The library uses dependency injection to manage the various serializer implementations and implements the `ISerializationProvider` interface for standardized DI scenarios.

## Goals

- Provide a consistent API for serializing and deserializing objects
- Support multiple serialization formats (JSON, XML, YAML, TOML, INI, etc.)
- Allow easy configuration through dependency injection
- Maintain extensibility for adding new serialization formats
- Minimize dependencies by using specialized libraries for each format
- Optimize performance by proper configuration and caching

## Core Architecture

### ISerializer Interface

The core of the library is the `ISerializer` interface, which defines the standard operations:

```csharp
public interface ISerializer
{
    string ContentType { get; }
    string FileExtension { get; }
    
    string Serialize<T>(T obj);
    string Serialize(object obj, Type type);
    
    T Deserialize<T>(string serialized);
    object Deserialize(string serialized, Type type);
    
    byte[] SerializeToBytes<T>(T obj);
    byte[] SerializeToBytes(object obj, Type type);
    
    T DeserializeFromBytes<T>(byte[] bytes);
    object DeserializeFromBytes(byte[] bytes, Type type);
    
    Task<string> SerializeAsync<T>(T obj, CancellationToken cancellationToken = default);
    Task<byte[]> SerializeToBytesAsync<T>(T obj, CancellationToken cancellationToken = default);
    
    Task<T> DeserializeAsync<T>(string serialized, CancellationToken cancellationToken = default);
    Task<T> DeserializeFromBytesAsync<T>(byte[] bytes, CancellationToken cancellationToken = default);
}
```

### SerializerOptions

Serializer options will be standardized through a base options class that abstracts away format-specific details:

```csharp
// Constants for option names to avoid string literals
public static class SerializerOptionKeys
{
    // JSON-specific option keys
    public static class Json
    {
        public const string AllowComments = "Json:AllowComments";
        public const string CaseInsensitive = "Json:CaseInsensitive";
        public const string DateTimeFormat = "Json:DateTimeFormat";
        public const string PropertyNamingPolicy = "Json:PropertyNamingPolicy";
        public const string MaxDepth = "Json:MaxDepth";
    }
    
    // XML-specific option keys
    public static class Xml
    {
        public const string Indent = "Xml:Indent";
        public const string OmitXmlDeclaration = "Xml:OmitXmlDeclaration";
        public const string Encoding = "Xml:Encoding";
        public const string NamespaceHandling = "Xml:NamespaceHandling";
    }
    
    // YAML-specific option keys
    public static class Yaml
    {
        public const string EmitDefaults = "Yaml:EmitDefaults";
        public const string IndentationWidth = "Yaml:IndentationWidth";
        public const string Encoding = "Yaml:Encoding";
    }
    
    // TOML-specific option keys
    public static class Toml
    {
        public const string DateTimeFormat = "Toml:DateTimeFormat";
        public const string InlineTablesAsObjects = "Toml:InlineTablesAsObjects";
    }
    
    // INI-specific option keys
    public static class Ini
    {
        public const string SectionNameCasing = "Ini:SectionNameCasing";
        public const string AllowDuplicateKeys = "Ini:AllowDuplicateKeys";
    }
    
    // MessagePack-specific option keys
    public static class MessagePack
    {
        public const string UseCompression = "MessagePack:UseCompression";
        public const string CompressionLevel = "MessagePack:CompressionLevel";
        public const string EnableLz4Compression = "MessagePack:EnableLz4Compression";
        public const string OmitAssemblyVersion = "MessagePack:OmitAssemblyVersion";
        public const string AllowPrivateMembers = "MessagePack:AllowPrivateMembers";
    }
    
    // Protobuf-specific option keys
    public static class Protobuf
    {
        public const string UseImplicitFields = "Protobuf:UseImplicitFields";
        public const string SkipDefaults = "Protobuf:SkipDefaults";
        public const string PreserveReferencesHandling = "Protobuf:PreserveReferencesHandling";
        public const string TypeFormat = "Protobuf:TypeFormat";
    }
    
    // FlatBuffers-specific option keys
    public static class FlatBuffers
    {
        public const string ForceDefaults = "FlatBuffers:ForceDefaults";
        public const string ShareStrings = "FlatBuffers:ShareStrings";
        public const string ShareKeys = "FlatBuffers:ShareKeys";
        public const string IndirectStrings = "FlatBuffers:IndirectStrings";
    }
}

public enum EnumSerializationFormat
{
    Name,
    Value,
    NameAndValue
}

public enum TypeDiscriminatorFormat
{
    Property,     // Add a property with the type name
    Wrapper,      // Wrap object in a container with type and value
    TypeProperty  // Use a designated property for type information
}

public class SerializerOptions
{
    // Common options for all serialization formats
    public bool IgnoreNullValues { get; set; } = false;
    public bool IgnoreReadOnlyProperties { get; set; } = false;
    public bool PreserveReferences { get; set; } = false;
    public bool PrettyPrint { get; set; } = false;
    
    // Enums are always serialized by name by default
    public EnumSerializationFormat EnumFormat { get; set; } = EnumSerializationFormat.Name;
    
    // Use string conversion for types not natively supported by the serializer
    public bool UseStringConversionForUnsupportedTypes { get; set; } = true;
    
    // Type discriminator settings for polymorphic serialization
    public bool EnableTypeDiscriminator { get; set; } = false;
    public TypeDiscriminatorFormat TypeDiscriminatorFormat { get; set; } = TypeDiscriminatorFormat.Property;
    public string TypeDiscriminatorPropertyName { get; set; } = "$type";
    public bool UseFullyQualifiedTypeNames { get; set; } = false;
    
    // Format-specific settings dictionary for advanced customization
    // These will be handled internally by each serializer implementation
    internal Dictionary<string, object> FormatSpecificOptions { get; } = new Dictionary<string, object>();

    // Helper methods to set format-specific options without exposing their details
    public SerializerOptions WithOption(string key, object value)
    {
        FormatSpecificOptions[key] = value;
        return this;
    }
    
    public T GetOption<T>(string key, T defaultValue = default)
    {
        if (FormatSpecificOptions.TryGetValue(key, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return defaultValue;
    }
    
    // Extension methods for specific formats for better discoverability and type safety
    public SerializerOptions WithJsonOption(string key, object value) => WithOption(key, value);
    public SerializerOptions WithXmlOption(string key, object value) => WithOption(key, value);
    public SerializerOptions WithYamlOption(string key, object value) => WithOption(key, value);
    public SerializerOptions WithTomlOption(string key, object value) => WithOption(key, value);
    public SerializerOptions WithIniOption(string key, object value) => WithOption(key, value);
    public SerializerOptions WithMessagePackOption(string key, object value) => WithOption(key, value);
    public SerializerOptions WithProtobufOption(string key, object value) => WithOption(key, value);
    public SerializerOptions WithFlatBuffersOption(string key, object value) => WithOption(key, value);
}
```
### ISerializerFactory

A factory interface will allow retrieval of the appropriate serializer:

```csharp
public interface ISerializerFactory
{
    ISerializer GetSerializer(string contentType);
    ISerializer GetSerializer(SerializerOptions options = null);
    ISerializer GetJsonSerializer(SerializerOptions options = null);
    ISerializer GetXmlSerializer(SerializerOptions options = null);
    ISerializer GetYamlSerializer(SerializerOptions options = null);
    ISerializer GetTomlSerializer(SerializerOptions options = null);
    ISerializer GetIniSerializer(SerializerOptions options = null);
    ISerializer GetMessagePackSerializer(SerializerOptions options = null);
    ISerializer GetProtobufSerializer(SerializerOptions options = null);
    ISerializer GetFlatBuffersSerializer(SerializerOptions options = null);
}
```

### TypeConverter System

To support types that aren't natively handled by serialization formats but have string conversion methods:

```csharp
public interface ITypeConverter
{
    bool CanConvert(Type type);
    string ConvertToString(object value);
    object ConvertFromString(string value, Type targetType);
}

public class StringConvertibleTypeConverter : ITypeConverter
{
    public bool CanConvert(Type type)
    {
        // Check if type has a ToString method override and any of the supported deserialization methods
        return HasToStringOverride(type) && 
               (HasParseMethod(type) || HasFromStringMethod(type) || HasStringConstructor(type));
    }
    
    public string ConvertToString(object value)
    {
        return value.ToString();
    }
    
    public object ConvertFromString(string value, Type targetType)
    {
        // Try Parse method first (instance.ToString / static.Parse pattern)
        if (HasParseMethod(targetType))
        {
            var parseMethod = targetType.GetMethod("Parse", 
                BindingFlags.Public | BindingFlags.Static, 
                null, 
                new[] { typeof(string) }, 
                null);
            
            return parseMethod.Invoke(null, new object[] { value });
        }
        
        // Try FromString method next (instance.ToString / static.FromString pattern)
        if (HasFromStringMethod(targetType))
        {
            var fromStringMethod = targetType.GetMethod("FromString", 
                BindingFlags.Public | BindingFlags.Static, 
                null, 
                new[] { typeof(string) }, 
                null);
            
            return fromStringMethod.Invoke(null, new object[] { value });
        }
        
        // Try string constructor as last resort
        if (HasStringConstructor(targetType))
        {
            var constructor = targetType.GetConstructor(new[] { typeof(string) });
            return constructor.Invoke(new object[] { value });
        }
        
        throw new InvalidOperationException($"Cannot convert string to {targetType.Name}");
    }
    
    // Helper methods to check for conversion methods
    private bool HasToStringOverride(Type type) 
    {
        // Check if the type overrides ToString
        var toStringMethod = type.GetMethod("ToString", 
            BindingFlags.Public | BindingFlags.Instance, 
            null, 
            Type.EmptyTypes, 
            null);
            
        return toStringMethod != null && 
               toStringMethod.DeclaringType != typeof(object) && 
               toStringMethod.DeclaringType != typeof(ValueType);
    }
    
    private bool HasParseMethod(Type type) 
    {
        // Check for static Parse(string) method
        var parseMethod = type.GetMethod("Parse", 
            BindingFlags.Public | BindingFlags.Static, 
            null, 
            new[] { typeof(string) }, 
            null);
            
        return parseMethod != null && parseMethod.ReturnType == type;
    }
    
    private bool HasFromStringMethod(Type type) 
    {
        // Check for static FromString(string) method
        var fromStringMethod = type.GetMethod("FromString", 
            BindingFlags.Public | BindingFlags.Static, 
            null, 
            new[] { typeof(string) }, 
            null);
            
        return fromStringMethod != null && fromStringMethod.ReturnType == type;
    }
    
    private bool HasStringConstructor(Type type) 
    {
        // Check for constructor that takes a single string parameter
        var constructor = type.GetConstructor(new[] { typeof(string) });
        return constructor != null;
    }
}

public class TypeConverterRegistry
{
    private readonly List<ITypeConverter> _converters = new List<ITypeConverter>();
    
    public TypeConverterRegistry()
    {
        // Register default converters
        _converters.Add(new StringConvertibleTypeConverter());
        // Other built-in converters
    }
    
    public void RegisterConverter(ITypeConverter converter)
    {
        _converters.Add(converter);
    }
    
    public ITypeConverter GetConverter(Type type)
    {
        return _converters.FirstOrDefault(c => c.CanConvert(type));
    }
    
    public bool HasConverter(Type type)
    {
        return _converters.Any(c => c.CanConvert(type));
    }
}
```

### TypeRegistry System

A registry system for managing types during polymorphic serialization:

```csharp
public class TypeRegistry
{
    private readonly Dictionary<string, Type> _typeMap = new Dictionary<string, Type>();
    private readonly Dictionary<Type, string> _nameMap = new Dictionary<Type, string>();
    private readonly SerializerOptions _options;
    
    public TypeRegistry(SerializerOptions options)
    {
        _options = options;
    }
    
    public void RegisterType<T>(string name = null)
    {
        RegisterType(typeof(T), name);
    }
    
    public void RegisterType(Type type, string name = null)
    {
        name ??= GetTypeName(type);
        _typeMap[name] = type;
        _nameMap[type] = name;
    }
    
    public Type ResolveType(string typeName)
    {
        if (_typeMap.TryGetValue(typeName, out var type))
        {
            return type;
        }
        
        // Try to resolve by reflection if not in map
        return Type.GetType(typeName, false);
    }
    
    public string GetTypeName(Type type)
    {
        if (_nameMap.TryGetValue(type, out var name))
        {
            return name;
        }
        
        return _options.UseFullyQualifiedTypeNames 
            ? type.AssemblyQualifiedName 
            : type.FullName;
    }
    
    public void RegisterSubtypes<TBase>(Assembly assembly = null)
    {
        assembly ??= Assembly.GetAssembly(typeof(TBase));
        
        foreach (var type in assembly.GetTypes()
            .Where(t => typeof(TBase).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface))
        {
            RegisterType(type);
        }
    }
}
```

## Serializer Implementations

### JSON Serializer

Using System.Text.Json as the default implementation:

```csharp
public class SystemTextJsonSerializer : ISerializer
{
    private readonly System.Text.Json.JsonSerializerOptions _jsonOptions;
    private readonly SerializerOptions _options;
    private readonly TypeConverterRegistry _typeConverterRegistry;
    private readonly TypeRegistry _typeRegistry;
    
    public string ContentType => "application/json";
    public string FileExtension => ".json";
    
    public SystemTextJsonSerializer(SerializerOptions options, TypeConverterRegistry typeConverterRegistry, TypeRegistry typeRegistry)
    {
        _options = options;
        _typeConverterRegistry = typeConverterRegistry;
        _typeRegistry = typeRegistry;
        
        // Map our abstract options to System.Text.Json specific options
        _jsonOptions = new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = _options.PrettyPrint,
            IgnoreNullValues = _options.IgnoreNullValues,
            // Other mappings...
        };
        
        // Apply any format-specific options
        ApplyFormatSpecificOptions();
        
        // Configure System.Text.Json to use string enum conversion
        if (_options.EnumFormat == EnumSerializationFormat.Name)
        {
            _jsonOptions.Converters.Add(new JsonStringEnumConverter());
        }
        
        // Add converter for types with string conversion
        if (_options.UseStringConversionForUnsupportedTypes)
        {
            _jsonOptions.Converters.Add(new StringBasedTypeConverter(_typeConverterRegistry));
        }
        
        // Add polymorphic type handling if enabled
        if (_options.EnableTypeDiscriminator)
        {
            _jsonOptions.Converters.Add(new PolymorphicJsonConverter(_typeRegistry, _options));
        }
    }
    
    private void ApplyFormatSpecificOptions()
    {
        // Handle format-specific options without exposing them in the public API
        if (_options.GetOption<bool>(SerializerOptionKeys.Json.AllowComments, false))
        {
            _jsonOptions.ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip;
        }
        
        if (_options.GetOption<bool>(SerializerOptionKeys.Json.CaseInsensitive, false))
        {
            _jsonOptions.PropertyNameCaseInsensitive = true;
        }
        
        // Get datetime format if specified
        var dateTimeFormat = _options.GetOption<string>(SerializerOptionKeys.Json.DateTimeFormat, null);
        if (!string.IsNullOrEmpty(dateTimeFormat))
        {
            // Apply custom date time converter with the specified format
        }
        
        // Apply max depth if specified
        var maxDepth = _options.GetOption<int>(SerializerOptionKeys.Json.MaxDepth, 0);
        if (maxDepth > 0)
        {
            _jsonOptions.MaxDepth = maxDepth;
        }
        
        // Other format-specific options...
    }
    
    // Implementation of ISerializer methods using _jsonOptions
    
    // Private converter implementations...
}
```

Alternative implementation using Newtonsoft.Json:

```csharp
public class NewtonsoftJsonSerializer : ISerializer
{
    private readonly Newtonsoft.Json.JsonSerializerSettings _jsonSettings;
    private readonly SerializerOptions _options;
    
    public string ContentType => "application/json";
    public string FileExtension => ".json";
    
    public NewtonsoftJsonSerializer(SerializerOptions options, TypeConverterRegistry typeConverterRegistry, TypeRegistry typeRegistry)
    {
        _options = options;
        
        // Map our abstract options to Newtonsoft.Json specific settings
        _jsonSettings = new Newtonsoft.Json.JsonSerializerSettings
        {
            Formatting = _options.PrettyPrint ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None,
            NullValueHandling = _options.IgnoreNullValues ? Newtonsoft.Json.NullValueHandling.Ignore : Newtonsoft.Json.NullValueHandling.Include,
            // Other mappings...
        };
        
        // Apply any format-specific options
        ApplyFormatSpecificOptions();
        
        // Configure Newtonsoft.Json to use string enum conversion
        if (_options.EnumFormat == EnumSerializationFormat.Name)
        {
            _jsonSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        }
        
        // Other converters...
    }
    
    private void ApplyFormatSpecificOptions()
    {
        // Handle format-specific options without exposing them in the public API
        // ...
    }
    
    // Implementation
}
```

### XML Serializer

Using System.Xml.Serialization:

```csharp
public class XmlSerializer : ISerializer
{
    private readonly XmlSerializerOptions _options;
    
    public string ContentType => "application/xml";
    public string FileExtension => ".xml";
    
    // Implementation
}
```

### YAML Serializer

Using YamlDotNet:

```csharp
public class YamlSerializer : ISerializer
{
    private readonly YamlSerializerOptions _options;
    
    public string ContentType => "application/yaml";
    public string FileExtension => ".yaml";
    
    // Implementation
}
```

### TOML Serializer

Using Tomlyn:

```csharp
public class TomlSerializer : ISerializer
{
    private readonly TomlSerializerOptions _options;
    
    public string ContentType => "application/toml";
    public string FileExtension => ".toml";
    
    // Implementation
}
```

### INI Serializer

Using IniParser:

```csharp
public class IniSerializer : ISerializer
{
    private readonly IniSerializerOptions _options;
    
    public string ContentType => "text/plain";
    public string FileExtension => ".ini";
    
    // Implementation
}
```

### MessagePack Serializer

Using MessagePack-CSharp as the implementation:

```csharp
public class MessagePackSerializer : ISerializer
{
    private readonly global::MessagePack.MessagePackSerializerOptions _messagePackOptions;
    private readonly SerializerOptions _options;
    private readonly TypeRegistry _typeRegistry;
    
    public string ContentType => "application/x-msgpack";
    public string FileExtension => ".msgpack";
    
    public MessagePackSerializer(SerializerOptions options, TypeRegistry typeRegistry)
    {
        _options = options;
        _typeRegistry = typeRegistry;
        
        // Create MessagePack serializer options
        var resolver = global::MessagePack.Resolvers.StandardResolver.Instance;
        
        // Add polymorphic serialization if needed
        if (_options.EnableTypeDiscriminator)
        {
            resolver = global::MessagePack.Resolvers.CompositeResolver.Create(
                new global::MessagePack.Formatters.IMessagePackFormatter[] 
                {
                    // Custom polymorphic formatter
                },
                new global::MessagePack.IFormatterResolver[] 
                {
                    resolver
                });
        }
        
        _messagePackOptions = global::MessagePack.MessagePackSerializerOptions.Standard
            .WithResolver(resolver);
            
        // Apply format-specific options
        ApplyFormatSpecificOptions();
    }
    
    private void ApplyFormatSpecificOptions()
    {
        // Use LZ4 compression if specified
        if (_options.GetOption<bool>(SerializerOptionKeys.MessagePack.EnableLz4Compression, false))
        {
            _messagePackOptions = _messagePackOptions.WithCompression(
                global::MessagePack.MessagePackCompression.Lz4Block);
        }
        
        // Other MessagePack-specific options...
    }
    
    public string Serialize<T>(T obj)
    {
        var bytes = SerializeToBytes(obj);
        return Convert.ToBase64String(bytes);
    }
    
    public T Deserialize<T>(string serialized)
    {
        var bytes = Convert.FromBase64String(serialized);
        return DeserializeFromBytes<T>(bytes);
    }
    
    public byte[] SerializeToBytes<T>(T obj)
    {
        return global::MessagePack.MessagePackSerializer.Serialize(obj, _messagePackOptions);
    }
    
    public T DeserializeFromBytes<T>(byte[] bytes)
    {
        return global::MessagePack.MessagePackSerializer.Deserialize<T>(bytes, _messagePackOptions);
    }
    
    // Other ISerializer methods...
}
```

### Protocol Buffers Serializer

Using protobuf-net as the implementation:

```csharp
public class ProtobufSerializer : ISerializer
{
    private readonly SerializerOptions _options;
    private readonly global::ProtoBuf.Meta.RuntimeTypeModel _model;
    
    public string ContentType => "application/x-protobuf";
    public string FileExtension => ".proto";
    
    public ProtobufSerializer(SerializerOptions options, TypeRegistry typeRegistry)
    {
        _options = options;
        
        // Create and configure the model
        _model = global::ProtoBuf.Meta.RuntimeTypeModel.Create();
        _model.UseImplicitZeroDefaults = !_options.GetOption<bool>(
            SerializerOptionKeys.Protobuf.SkipDefaults, false);
            
        // Configure type handling for inheritance
        if (_options.EnableTypeDiscriminator)
        {
            // Register known types from the type registry
            foreach (var typePair in typeRegistry.GetAllTypeMappings())
            {
                if (typePair.Type.IsClass && !typePair.Type.IsAbstract)
                {
                    RegisterSubType(typePair.Type);
                }
            }
        }
        
        // Apply other format-specific options
        ApplyFormatSpecificOptions();
    }
    
    private void RegisterSubType(Type type)
    {
        // Find the base type
        var baseType = type.BaseType;
        if (baseType != null && baseType != typeof(object))
        {
            var metaType = _model[baseType];
            if (metaType != null)
            {
                metaType.AddSubType(metaType.GetNextFieldNumber(), type);
            }
        }
    }
    
    private void ApplyFormatSpecificOptions()
    {
        // Apply protobuf-specific options
    }
    
    public string Serialize<T>(T obj)
    {
        var bytes = SerializeToBytes(obj);
        return Convert.ToBase64String(bytes);
    }
    
    public T Deserialize<T>(string serialized)
    {
        var bytes = Convert.FromBase64String(serialized);
        return DeserializeFromBytes<T>(bytes);
    }
    
    public byte[] SerializeToBytes<T>(T obj)
    {
        using (var stream = new MemoryStream())
        {
            _model.Serialize(stream, obj);
            return stream.ToArray();
        }
    }
    
    public T DeserializeFromBytes<T>(byte[] bytes)
    {
        using (var stream = new MemoryStream(bytes))
        {
            return (T)_model.Deserialize(stream, null, typeof(T));
        }
    }
    
    // Other ISerializer methods...
}
```

### FlatBuffers Serializer

Using FlatBuffers for .NET:

```csharp
public class FlatBuffersSerializer : ISerializer
{
    private readonly SerializerOptions _options;
    
    public string ContentType => "application/x-flatbuffers";
    public string FileExtension => ".fbs";
    
    public FlatBuffersSerializer(SerializerOptions options)
    {
        _options = options;
        
        // FlatBuffers typically requires pre-generated code
        // This implementation would need to use runtime reflection or code generation
    }
    
    // Note: FlatBuffers typically requires pre-generated serialization code
    // A pure reflection-based approach may have limitations
    
    public string Serialize<T>(T obj)
    {
        var bytes = SerializeToBytes(obj);
        return Convert.ToBase64String(bytes);
    }
    
    public T Deserialize<T>(string serialized)
    {
        var bytes = Convert.FromBase64String(serialized);
        return DeserializeFromBytes<T>(bytes);
    }
    
    public byte[] SerializeToBytes<T>(T obj)
    {
        // Implementation would depend on whether we're using pre-generated code
        // or a runtime reflection-based approach
        throw new NotImplementedException("FlatBuffers serialization requires implementation");
    }
    
    public T DeserializeFromBytes<T>(byte[] bytes)
    {
        // Implementation would depend on whether we're using pre-generated code
        // or a runtime reflection-based approach
        throw new NotImplementedException("FlatBuffers deserialization requires implementation");
    }
    
    // Other ISerializer methods...
}
```

## Dependency Injection

### Registration Extensions

```csharp
public static class SerializerExtensions
{
    public static IServiceCollection AddUniversalSerializer(this IServiceCollection services)
    {
        // Register default serializers with default options
        services.AddSingleton<SerializerOptions>();
        services.AddSingleton<TypeConverterRegistry>();
        services.AddSingleton<TypeRegistry>();
        
        services.AddSingleton<ISerializer, SystemTextJsonSerializer>();
        services.AddSingleton<ISerializer, XmlSerializer>();
        // Other default serializers
        
        services.AddSingleton<ISerializerFactory, SerializerFactory>();
        
        return services;
    }
    
    public static IServiceCollection AddUniversalSerializer(this IServiceCollection services, Action<SerializerBuilder> configure)
    {
        var builder = new SerializerBuilder(services);
        configure(builder);
        return services;
    }
}
```

### Builder Pattern for Configuration

```csharp
public class SerializerBuilder
{
    private readonly IServiceCollection _services;
    private readonly SerializerOptions _options = new SerializerOptions();
    private readonly TypeRegistry _typeRegistry;
    private readonly TypeConverterRegistry _typeConverterRegistry;
    
    internal SerializerBuilder(IServiceCollection services)
    {
        _services = services;
        _typeRegistry = new TypeRegistry(_options);
        _typeConverterRegistry = new TypeConverterRegistry();
        
        // Register the core components
        _services.AddSingleton(_options);
        _services.AddSingleton(_typeRegistry);
        _services.AddSingleton(_typeConverterRegistry);
    }
    
    public SerializerBuilder ConfigureOptions(Action<SerializerOptions> configure)
    {
        configure?.Invoke(_options);
        return this;
    }
    
    public SerializerBuilder ConfigureTypeRegistry(Action<TypeRegistry> configure)
    {
        configure?.Invoke(_typeRegistry);
        return this;
    }
    
    public SerializerBuilder ConfigureTypeConverters(Action<TypeConverterRegistry> configure)
    {
        configure?.Invoke(_typeConverterRegistry);
        return this;
    }
    
    public SerializerBuilder AddJsonSerializer()
    {
        _services.AddSingleton<ISerializer, SystemTextJsonSerializer>();
        return this;
    }
    
    public SerializerBuilder AddXmlSerializer()
    {
        _services.AddSingleton<ISerializer, XmlSerializer>();
        return this;
    }
    
    public SerializerBuilder AddMessagePackSerializer()
    {
        _services.AddSingleton<ISerializer, MessagePackSerializer>();
        return this;
    }
    
    public SerializerBuilder AddProtobufSerializer()
    {
        _services.AddSingleton<ISerializer, ProtobufSerializer>();
        return this;
    }
    
    public SerializerBuilder AddFlatBuffersSerializer()
    {
        _services.AddSingleton<ISerializer, FlatBuffersSerializer>();
        return this;
    }
    
    // Other serializer methods...
}
```

## Usage Examples

### Basic Usage

```csharp
// Program.cs
services.AddUniversalSerializer();

// In a service
public class MyService
{
    private readonly ISerializerFactory _serializerFactory;
    
    public MyService(ISerializerFactory serializerFactory)
    {
        _serializerFactory = serializerFactory;
    }
    
    public void Process()
    {
        var serializer = _serializerFactory.GetJsonSerializer();
        var data = serializer.Serialize(new { Name = "Test" });
        
        // Or by content type
        var yamlSerializer = _serializerFactory.GetSerializer("application/yaml");
        var yamlData = yamlSerializer.Serialize(new { Name = "Test" });
    }
}
```

### Advanced Configuration

```csharp
services.AddUniversalSerializer(builder => {
    builder.ConfigureOptions(options => {
        options.PrettyPrint = true;
        options.IgnoreNullValues = true;
        
        // Format-specific options using constants for type safety
        options.WithOption(SerializerOptionKeys.Json.AllowComments, true)
               .WithOption(SerializerOptionKeys.Json.CaseInsensitive, true)
               .WithOption(SerializerOptionKeys.Json.DateTimeFormat, "yyyy-MM-dd")
               .WithOption(SerializerOptionKeys.Xml.Indent, true)
               .WithOption(SerializerOptionKeys.Xml.OmitXmlDeclaration, true);
    });
    
    // Add specific serializers
    builder.AddJsonSerializer()
           .AddXmlSerializer()
           .AddMessagePackSerializer()
           .AddProtobufSerializer()
           .AddFlatBuffersSerializer();
});
```

### Format-Specific Extensions

The design could be extended with format-specific helper methods to make the API more discoverable:

```csharp
// Extension methods for SerializerOptions
public static class SerializerOptionsExtensions
{
    public static SerializerOptions WithJsonAllowComments(this SerializerOptions options, bool value = true)
    {
        return options.WithOption(SerializerOptionKeys.Json.AllowComments, value);
    }
    
    public static SerializerOptions WithJsonCaseInsensitive(this SerializerOptions options, bool value = true)
    {
        return options.WithOption(SerializerOptionKeys.Json.CaseInsensitive, value);
    }
    
    public static SerializerOptions WithXmlIndent(this SerializerOptions options, bool value = true)
    {
        return options.WithOption(SerializerOptionKeys.Xml.Indent, value);
    }
    
    // Other common options...
}

// Usage with extension methods
services.AddUniversalSerializer(builder => {
    builder.ConfigureOptions(options => {
        options.PrettyPrint = true
               .WithJsonAllowComments()
               .WithJsonCaseInsensitive()
               .WithXmlIndent();
    });
});
```

### Handling Custom Types

```csharp
// Example 1: ToString/Parse pattern
public class CustomId
{
    public Guid Value { get; }
    
    public CustomId(Guid value)
    {
        Value = value;
    }
    
    // ToString for serialization
    public override string ToString()
    {
        return Value.ToString("D");
    }
    
    // Static Parse method for deserialization
    public static CustomId Parse(string value)
    {
        return new CustomId(Guid.Parse(value));
    }
}

// Example 2: ToString/FromString pattern
public class ColorValue
{
    public byte R { get; }
    public byte G { get; }
    public byte B { get; }
    
    public ColorValue(byte r, byte g, byte b)
    {
        R = r;
        G = g;
        B = b;
    }
    
    // ToString for serialization
    public override string ToString()
    {
        return $"#{R:X2}{G:X2}{B:X2}";
    }
    
    // Static FromString method for deserialization
    public static ColorValue FromString(string value)
    {
        if (value.StartsWith("#") && value.Length == 7)
        {
            byte r = Convert.ToByte(value.Substring(1, 2), 16);
            byte g = Convert.ToByte(value.Substring(3, 2), 16);
            byte b = Convert.ToByte(value.Substring(5, 2), 16);
            return new ColorValue(r, g, b);
        }
        throw new FormatException("Invalid color format");
    }
}

// Example 3: ToString/Constructor pattern
public class EmailAddress
{
    public string Value { get; }
    
    // String constructor for deserialization
    public EmailAddress(string value)
    {
        if (!value.Contains("@"))
            throw new ArgumentException("Invalid email format");
        Value = value;
    }
    
    // ToString for serialization
    public override string ToString()
    {
        return Value;
    }
}

// Usage in a model
public class Document
{
    public CustomId Id { get; set; }
    public ColorValue Color { get; set; }
    public EmailAddress ContactEmail { get; set; }
    public string Title { get; set; }
}

// All types will be serialized as strings using the appropriate conversion methods
var serializer = _serializerFactory.GetJsonSerializer();
var doc = new Document
{
    Id = new CustomId(Guid.NewGuid()),
    Color = new ColorValue(255, 0, 0),
    ContactEmail = new EmailAddress("user@example.com"),
    Title = "Sample Document"
};

var json = serializer.Serialize(doc);
var deserializedDoc = serializer.Deserialize<Document>(json);
```

### Polymorphic Serialization

```csharp
// Register types
services.AddUniversalSerializer(builder => {
    builder.ConfigureTypeRegistry(registry => {
        // Register base type and all its subtypes
        registry.RegisterSubtypes<Animal>();
        
        // Or register specific types with custom names
        registry.RegisterType<Dog>("dog");
        registry.RegisterType<Cat>("cat");
    });
    
    builder.ConfigureOptions(options => {
        options.EnableTypeDiscriminator = true;
        options.TypeDiscriminatorFormat = TypeDiscriminatorFormat.Property;
        options.TypeDiscriminatorPropertyName = "$type";
    });
});

// Define types
public abstract class Animal
{
    public string Name { get; set; }
}

public class Dog : Animal
{
    public string Breed { get; set; }
    public bool GoodBoy { get; set; }
}

public class Cat : Animal
{
    public int Lives { get; set; }
    public bool LikesCatnip { get; set; }
}

// Create a mixed collection
var animals = new List<Animal>
{
    new Dog { Name = "Rex", Breed = "German Shepherd", GoodBoy = true },
    new Cat { Name = "Whiskers", Lives = 9, LikesCatnip = true }
};

// Serialize with type information
var serializer = _serializerFactory.GetJsonSerializer();
var json = serializer.Serialize(animals);

// Will produce something like:
// [
//   {
//     "$type": "dog",
//     "name": "Rex",
//     "breed": "German Shepherd",
//     "goodBoy": true
//   },
//   {
//     "$type": "cat",
//     "name": "Whiskers",
//     "lives": 9,
//     "likesCatnip": true
//   }
// ]

// Deserialize back to correct types
var deserializedAnimals = serializer.Deserialize<List<Animal>>(json);
// Each item will be the correct concrete type (Dog or Cat)
```

### Using Binary Serialization Formats

```csharp
// Configure serializers
services.AddUniversalSerializer(builder => {
    builder.ConfigureOptions(options => {
        // Common options
        options.PreserveReferences = true;
        
        // MessagePack-specific options
        options.WithOption(SerializerOptionKeys.MessagePack.EnableLz4Compression, true);
        
        // Protobuf-specific options
        options.WithOption(SerializerOptionKeys.Protobuf.SkipDefaults, true);
    });
    
    // Register binary serializers
    builder.AddMessagePackSerializer()
           .AddProtobufSerializer()
           .AddFlatBuffersSerializer();
});

// In a service
public class BinarySerializationExample
{
    private readonly ISerializerFactory _serializerFactory;
    
    public BinarySerializationExample(ISerializerFactory serializerFactory)
    {
        _serializerFactory = serializerFactory;
    }
    
    public void Process()
    {
        var data = new MyData { Id = 123, Name = "Test" };
        
        // Using MessagePack for compact binary serialization
        var messagePackSerializer = _serializerFactory.GetMessagePackSerializer();
        byte[] msgpackBytes = messagePackSerializer.SerializeToBytes(data);
        
        // Using Protobuf for schema-based serialization
        var protobufSerializer = _serializerFactory.GetProtobufSerializer();
        byte[] protobufBytes = protobufSerializer.SerializeToBytes(data);
        
        // Compare sizes for performance analysis
        Console.WriteLine($"MessagePack size: {msgpackBytes.Length} bytes");
        Console.WriteLine($"Protobuf size: {protobufBytes.Length} bytes");
        
        // Deserialize back
        var msgpackData = messagePackSerializer.DeserializeFromBytes<MyData>(msgpackBytes);
        var protobufData = protobufSerializer.DeserializeFromBytes<MyData>(protobufBytes);
    }
}

// Data class with appropriate attributes for Protobuf
[global::ProtoBuf.ProtoContract]
public class MyData
{
    [global::ProtoBuf.ProtoMember(1)]
    public int Id { get; set; }
    
    [global::ProtoBuf.ProtoMember(2)]
    public string Name { get; set; }
}
```

## Dependencies

- System.Text.Json - Default JSON implementation
- Newtonsoft.Json - Alternative JSON implementation
- YamlDotNet - YAML serialization
- Tomlyn - TOML serialization
- IniParser - INI file parsing
- MessagePack-CSharp - MessagePack binary serialization
- protobuf-net - Protocol Buffers implementation for .NET
- FlatBuffers - FlatBuffers serialization library

## Future Enhancements

- Streaming APIs for large file handling
- Schema validation integration
- Custom type converters registry
- Compression integration
- Encryption integration
