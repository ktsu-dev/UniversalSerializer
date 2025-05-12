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
    
    // Use string conversion for types not natively supported by the serializer
    public bool UseStringConversionForUnsupportedTypes { get; set; } = true;
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

## Serializer Implementations

### JSON Serializer

Using System.Text.Json as the default implementation:

```csharp
public class SystemTextJsonSerializer : ISerializer
{
    private readonly JsonSerializerOptions _options;
    private readonly TypeConverterRegistry _typeConverterRegistry;
    
    public string ContentType => "application/json";
    public string FileExtension => ".json";
    
    public SystemTextJsonSerializer(JsonSerializerOptions options, TypeConverterRegistry typeConverterRegistry)
    {
        _options = options;
        _typeConverterRegistry = typeConverterRegistry;
        
        // Configure System.Text.Json to use string enum conversion
        if (options.EnumFormat == EnumSerializationFormat.Name)
        {
            _options.Converters.Add(new JsonStringEnumConverter());
        }
        
        // Add converter for types with string conversion
        if (options.UseStringConversionForUnsupportedTypes)
        {
            _options.Converters.Add(new StringBasedTypeConverter(_typeConverterRegistry));
        }
    }
    
    // Implementation of ISerializer methods
    
    // Example of a custom JsonConverter for System.Text.Json
    private class StringBasedTypeConverter : JsonConverter<object>
    {
        private readonly TypeConverterRegistry _registry;
        
        public StringBasedTypeConverter(TypeConverterRegistry registry)
        {
            _registry = registry;
        }
        
        public override bool CanConvert(Type typeToConvert)
        {
            return _registry.HasConverter(typeToConvert);
        }
        
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException($"Expected string value for {typeToConvert.Name}");
            }
            
            var stringValue = reader.GetString();
            var converter = _registry.GetConverter(typeToConvert);
            return converter.ConvertFromString(stringValue, typeToConvert);
        }
        
        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            var converter = _registry.GetConverter(value.GetType());
            var stringValue = converter.ConvertToString(value);
            writer.WriteStringValue(stringValue);
        }
    }
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

### Custom Type Converters

Developers can add custom type converters for complex types:

```csharp
public class DateOnlyConverter : ITypeConverter
{
    public bool CanConvert(Type type)
    {
        return type == typeof(DateOnly);
    }
    
    public string ConvertToString(object value)
    {
        return ((DateOnly)value).ToString("yyyy-MM-dd");
    }
    
    public object ConvertFromString(string value, Type targetType)
    {
        return DateOnly.Parse(value);
    }
}

// Registration
services.AddUniversalSerializer(builder => {
    builder.ConfigureTypeConverters(registry => {
        registry.RegisterConverter(new DateOnlyConverter());
    });
});
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
