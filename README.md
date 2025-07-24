# ktsu.UniversalSerializer

A unified serialization library for .NET that provides a consistent API for various serialization formats including JSON, XML, YAML, TOML, and MessagePack.

## Features

- **Unified API**: Serialize and deserialize objects with a consistent interface regardless of the format
- **Multiple Formats**: Support for common text formats (JSON, XML, YAML, TOML) and binary formats (MessagePack)
- **Type Conversion**: Built-in type conversion for non-natively supported types
- **Polymorphic Serialization**: Support for inheritance and polymorphic types
- **Dependency Injection**: First-class support for Microsoft DI with fluent configuration
- **SerializationProvider Integration**: Compatible with the `ISerializationProvider` interface for standardized DI scenarios
- **Extensible**: Easy to extend with custom serializers or type converters

## Installation

```shell
dotnet add package ktsu.UniversalSerializer
```

## Quick Start

```csharp
using ktsu.UniversalSerializer;
using ktsu.UniversalSerializer.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

// Add to your dependency injection container
services.AddUniversalSerializer();

// Use in your application
public class MyService
{
    private readonly ISerializerFactory _serializerFactory;
    
    public MyService(ISerializerFactory serializerFactory)
    {
        _serializerFactory = serializerFactory;
    }
    
    public void Process()
    {
        var data = new MyData { Id = 1, Name = "Example" };
        
        // Serialize to different formats
        var jsonSerializer = _serializerFactory.GetJsonSerializer();
        string jsonString = jsonSerializer.Serialize(data);
        
        var yamlSerializer = _serializerFactory.GetYamlSerializer();
        string yamlString = yamlSerializer.Serialize(data);
        
        var msgPackSerializer = _serializerFactory.GetMessagePackSerializer();
        byte[] binaryData = msgPackSerializer.SerializeToBytes(data);
        
        // Deserialize from string
        var deserializedJson = jsonSerializer.Deserialize<MyData>(jsonString);
        
        // Deserialize from bytes
        var deserializedBinary = msgPackSerializer.DeserializeFromBytes<MyData>(binaryData);
    }
}

public class MyData
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
```

## Advanced Configuration

### Configuring Serializer Options

```csharp
using ktsu.UniversalSerializer;
using ktsu.UniversalSerializer.DependencyInjection;

services.AddUniversalSerializer(options => {
    // Common options
    options.PrettyPrint = true;
    options.IgnoreNullValues = true;
    options.EnumFormat = EnumSerializationFormat.Name;
    
    // Format-specific options
    options.WithOption(SerializerOptionKeys.Json.AllowComments, true);
    options.WithOption(SerializerOptionKeys.Json.CaseInsensitive, true);
    options.WithOption(SerializerOptionKeys.Xml.Indent, true);
    options.WithOption(SerializerOptionKeys.MessagePack.EnableLz4Compression, true);
});

// Register only the serializers you need
services.AddJsonSerializer();
services.AddXmlSerializer();
services.AddYamlSerializer();
services.AddMessagePackSerializer();
```

### Type Conversion

The library supports custom type conversion for types that aren't natively handled by serializers:

```csharp
using ktsu.UniversalSerializer;
using ktsu.UniversalSerializer.DependencyInjection;

// Define a custom type with string conversion
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
    
    // Parse method for deserialization
    public static CustomId Parse(string value)
    {
        return new CustomId(Guid.Parse(value));
    }
}

// Enable string conversion in options
services.AddUniversalSerializer(options => {
    options.UseStringConversionForUnsupportedTypes = true;
});
```

### Polymorphic Serialization

```csharp
using ktsu.UniversalSerializer;
using ktsu.UniversalSerializer.DependencyInjection;
using ktsu.UniversalSerializer.TypeRegistry;

// Configure type registry for polymorphic serialization
services.AddUniversalSerializer(options => {
    // Enable type discriminator
    options.EnableTypeDiscriminator = true;
    options.TypeDiscriminatorPropertyName = "$type";
});

// Register types with the type registry
services.Configure<TypeRegistry>(registry => {
    registry.RegisterType<Dog>("dog");
    registry.RegisterType<Cat>("cat");
    
    // Or automatically register all subtypes of Animal
    registry.RegisterSubtypes<Animal>();
});

// Define types
public abstract class Animal
{
    public string Name { get; set; } = string.Empty;
}

public class Dog : Animal
{
    public string Breed { get; set; } = string.Empty;
}

public class Cat : Animal
{
    public int Lives { get; set; }
}

// Use polymorphic serialization
var animals = new List<Animal>
{
    new Dog { Name = "Rex", Breed = "German Shepherd" },
    new Cat { Name = "Whiskers", Lives = 9 }
};

var serializer = _serializerFactory.GetJsonSerializer();
string json = serializer.Serialize(animals);
// Results in type-aware JSON with $type property
// Deserialize back to correct types
var deserializedAnimals = serializer.Deserialize<List<Animal>>(json);
```

## Binary Serialization

```csharp
using ktsu.UniversalSerializer;
using ktsu.UniversalSerializer.DependencyInjection;

// Configure and use binary serializers
services.AddUniversalSerializer();
services.AddMessagePackSerializer();

// In your code
var messagePackSerializer = _serializerFactory.GetMessagePackSerializer();
byte[] binary = messagePackSerializer.SerializeToBytes(data);
var result = messagePackSerializer.DeserializeFromBytes<MyData>(binary);
```

## SerializationProvider Integration

UniversalSerializer implements the `ISerializationProvider` interface for standardized dependency injection scenarios:

```csharp
using ktsu.SerializationProvider;
using ktsu.UniversalSerializer.DependencyInjection;

// Add core services and serialization providers
services.AddUniversalSerializer();

// Add format-specific providers
services.AddJsonSerializationProvider();
services.AddYamlSerializationProvider();
services.AddMessagePackSerializationProvider();

// Use the provider
public class MyService
{
    private readonly ISerializationProvider _provider;
    
    public MyService(ISerializationProvider provider)
    {
        _provider = provider;
    }
    
    public async Task ProcessAsync()
    {
        var data = new MyData { Id = 1, Name = "Example" };
        
        // Serialize and deserialize
        string serialized = await _provider.SerializeAsync(data);
        var deserialized = await _provider.DeserializeAsync<MyData>(serialized);
    }
}
```

For more details, see the [SerializationProvider Integration Documentation](docs/serialization-provider-integration.md).

## Supported Formats

| Format | Content Type | File Extension | Package Dependency |
|--------|-------------|----------------|-------------------|
| JSON | application/json | .json | System.Text.Json (built-in) |
| XML | application/xml | .xml | System.Xml.Serialization (built-in) |
| YAML | application/yaml | .yaml | YamlDotNet |
| TOML | application/toml | .toml | Tomlyn |
| MessagePack | application/x-msgpack | .msgpack | MessagePack |

## License

This project is licensed under the MIT License - see the LICENSE file for details.
