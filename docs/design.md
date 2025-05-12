# UniversalSerializer - Design Document

## Overview

UniversalSerializer is a .NET library that provides a unified interface for various serialization formats. It abstracts away the implementation details of different serialization libraries, allowing developers to easily switch between formats without changing their code. The library uses dependency injection to manage the various serializer implementations.

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

Serializer options will be standardized through a base options class and specific options for each format:

```csharp
public enum EnumSerializationFormat
{
    Name,
    Value,
    NameAndValue
}

public abstract class SerializerOptions
{
    public bool IgnoreNullValues { get; set; } = false;
    public bool IgnoreReadOnlyProperties { get; set; } = false;
    public bool PreserveReferences { get; set; } = false;
    public bool PrettyPrint { get; set; } = false;
    
    // Enums are always serialized by name by default
    public EnumSerializationFormat EnumFormat { get; set; } = EnumSerializationFormat.Name;
}

public class JsonSerializerOptions : SerializerOptions
{
    public bool CaseInsensitive { get; set; } = false;
    public bool AllowComments { get; set; } = false;
    // Other JSON-specific options
}

// Similar classes for other formats
```

### ISerializerFactory

A factory interface will allow retrieval of the appropriate serializer:

```csharp
public interface ISerializerFactory
{
    ISerializer GetSerializer(string contentType);
    ISerializer GetSerializer<TOptions>() where TOptions : SerializerOptions;
    ISerializer GetJsonSerializer();
    ISerializer GetXmlSerializer();
    ISerializer GetYamlSerializer();
    ISerializer GetTomlSerializer();
    ISerializer GetIniSerializer();
    // Other formats
}
```

## Serializer Implementations

### JSON Serializer

Using System.Text.Json as the default implementation:

```csharp
public class SystemTextJsonSerializer : ISerializer
{
    private readonly JsonSerializerOptions _options;
    
    public string ContentType => "application/json";
    public string FileExtension => ".json";
    
    public SystemTextJsonSerializer(JsonSerializerOptions options)
    {
        _options = options;
        
        // Configure System.Text.Json to use string enum conversion
        if (options.EnumFormat == EnumSerializationFormat.Name)
        {
            _options.Converters.Add(new JsonStringEnumConverter());
        }
    }
    
    // Implementation of ISerializer methods
}
```

Alternative implementation using Newtonsoft.Json:

```csharp
public class NewtonsoftJsonSerializer : ISerializer
{
    private readonly Newtonsoft.Json.JsonSerializerSettings _settings;
    
    public string ContentType => "application/json";
    public string FileExtension => ".json";
    
    public NewtonsoftJsonSerializer(JsonSerializerOptions options)
    {
        _settings = new Newtonsoft.Json.JsonSerializerSettings();
        
        // Configure Newtonsoft.Json to use string enum conversion
        if (options.EnumFormat == EnumSerializationFormat.Name)
        {
            _settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        }
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

## Dependency Injection

### Registration Extensions

```csharp
public static class SerializerExtensions
{
    public static IServiceCollection AddUniversalSerializer(this IServiceCollection services)
    {
        // Register default serializers
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
    
    internal SerializerBuilder(IServiceCollection services)
    {
        _services = services;
    }
    
    public SerializerBuilder AddJsonSerializer<TSerializer>(Action<JsonSerializerOptions> configure = null)
        where TSerializer : class, ISerializer
    {
        var options = new JsonSerializerOptions();
        configure?.Invoke(options);
        
        _services.AddSingleton<ISerializer, TSerializer>(sp => 
            ActivatorUtilities.CreateInstance<TSerializer>(sp, options));
            
        return this;
    }
    
    // Similar methods for other serializer types
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
    builder.AddJsonSerializer<NewtonsoftJsonSerializer>(options => {
        options.PrettyPrint = true;
        options.IgnoreNullValues = true;
        
        // Optional override if needed, but defaults to Name
        options.EnumFormat = EnumSerializationFormat.Name;
    });
    
    builder.AddXmlSerializer(options => {
        options.PreserveReferences = true;
    });
});
```

## Extension Points

### Custom Serializers

Developers can add their own serializer implementations:

```csharp
public class CustomSerializer : ISerializer
{
    public string ContentType => "application/custom";
    public string FileExtension => ".custom";
    
    // Implementation
}

// Registration
services.AddUniversalSerializer(builder => {
    builder.AddSerializer<CustomSerializer>();
});
```

### Serializer Decorators

Advanced scenarios might require decorating serializers:

```csharp
public class CachingSerializerDecorator : ISerializer
{
    private readonly ISerializer _inner;
    private readonly IMemoryCache _cache;
    
    public CachingSerializerDecorator(ISerializer inner, IMemoryCache cache)
    {
        _inner = inner;
        _cache = cache;
    }
    
    // Implement methods with caching
}
```

## Performance Considerations

- Serializer instances should be singleton-scoped in DI
- Consider caching expensive serializer settings/options
- Use streaming APIs for large objects
- Benchmark different implementations for specific scenarios

## Dependencies

- System.Text.Json - Default JSON implementation
- Newtonsoft.Json - Alternative JSON implementation
- YamlDotNet - YAML serialization
- Tomlyn - TOML serialization
- IniParser - INI file parsing

## Future Enhancements

- MessagePack/Protocol Buffers support
- Streaming APIs for large file handling
- Schema validation integration
- Custom type converters registry
- Compression integration
- Encryption integration
