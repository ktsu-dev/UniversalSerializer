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

Minimal DI (SerializationProvider):

```csharp
using ktsu.SerializationProvider;
using ktsu.UniversalSerializer;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Registers a default JSON-based provider and all required core services automatically
services.AddUniversalSerializationProvider();

using var provider = services.BuildServiceProvider();
var sp = provider.GetRequiredService<ISerializationProvider>();

var data = new MyData { Id = 1, Name = "Example" };
string json = sp.Serialize(data);
var roundTrip = sp.Deserialize<MyData>(json);

public class MyData
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
```

Format-specific providers:

```csharp
services.AddJsonSerializationProvider();
services.AddYamlSerializationProvider();
services.AddTomlSerializationProvider();
services.AddXmlSerializationProvider();
services.AddMessagePackSerializationProvider();

// Or choose by format/extension/content-type at registration time
services.AddUniversalSerializationProviderForFormat("yaml");
services.AddUniversalSerializationProviderForExtension(".toml");
services.AddUniversalSerializationProviderForContentType("application/xml");
```

## Advanced Configuration

### Configuring Serializer Options (DI)

```csharp
using ktsu.UniversalSerializer;

// Registers core services and applies options
services.AddUniversalSerializer(options =>
{
    // Built-in simple properties
    options.UseStringConversionForUnsupportedTypes = true;
    options.EnableCompression = true;
    options.CompressionLevel = 9;

    // Format-specific options via keys
    options.SetOption(SerializerOptionKeys.Json.AllowComments, true);
    options.SetOption(SerializerOptionKeys.Json.CaseInsensitive, true);
    options.SetOption(SerializerOptionKeys.Xml.Indent, true);
});

// Optional: register serializer types for DI construction (e.g., to inject registries)
services.AddJsonSerializer();
services.AddXmlSerializer();
services.AddYamlSerializer();
services.AddMessagePackSerializer();
```

### Type Conversion

The library supports custom type conversion for types that aren't natively handled by serializers:

```csharp
using ktsu.UniversalSerializer;

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
services.AddUniversalSerializer(options =>
{
    options.UseStringConversionForUnsupportedTypes = true;
});
```

### Polymorphic Serialization

```csharp
using ktsu.UniversalSerializer;

// Register core and enable discriminators (Json/Yaml/Toml read option via key)
services.AddUniversalSerializer(options =>
{
    options.SetOption(SerializerOptionKeys.TypeRegistry.EnableTypeDiscriminator, true);
});

// Ensure JSON serializer is constructed with registries when used via DI
services.AddJsonSerializationProvider();

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

// Resolve and configure the type registry
using var provider = services.BuildServiceProvider();
var registry = provider.GetRequiredService<TypeRegistry>();
registry.RegisterType<Dog>("dog");
registry.RegisterType<Cat>("cat");

// Use provider (JSON)
var sp = provider.GetRequiredService<ktsu.SerializationProvider.ISerializationProvider>();
string json = sp.Serialize(animals);
var deserializedAnimals = sp.Deserialize<List<Animal>>(json);
```

## Binary Serialization

```csharp
using ktsu.UniversalSerializer;

// Option 1: Use provider
services.AddUniversalSerializationProviderForFormat("messagepack");

using var provider = services.BuildServiceProvider();
var sp = provider.GetRequiredService<ktsu.SerializationProvider.ISerializationProvider>();
byte[] bytes = sp.SerializeToBytes(data); // if you need bytes, use serializer directly instead

// Option 2: Use factory directly
var factory = new SerializerFactory();
factory.RegisterSerializer(o => new ktsu.UniversalSerializer.MessagePack.MessagePackSerializer(o));
var mp = factory.Create<ktsu.UniversalSerializer.MessagePack.MessagePackSerializer>();
byte[] binary = mp.SerializeToBytes(data);
var result = mp.DeserializeFromBytes<MyData>(binary);
```

## SerializationProvider Integration

UniversalSerializer implements the `ISerializationProvider` interface for standardized dependency injection scenarios:

```csharp
using ktsu.SerializationProvider;

// Add a default JSON provider (auto-bootstraps core)
services.AddUniversalSerializationProvider();

// Or add specific providers
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
| YAML | text/yaml (also registers application/x-yaml) | .yaml | YamlDotNet |
| TOML | application/toml | .toml | Tomlyn |
| MessagePack | application/x-msgpack | .msgpack | MessagePack |

## License

This project is licensed under the MIT License - see the LICENSE file for details.
