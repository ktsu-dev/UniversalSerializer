---
title: SerializationProvider Integration
status: draft
---

# SerializationProvider Integration

The UniversalSerializer library now includes support for the `ISerializationProvider` interface, making it easy to use with dependency injection scenarios.

## Overview

The `UniversalSerializationProvider` class implements the `ISerializationProvider` interface and acts as an adapter between the UniversalSerializer and the standard serialization provider contract.

## Requirements

To use the SerializationProvider integration, you need to add the `ktsu.SerializationProvider` package to your project:

```xml
<PackageReference Include="ktsu.SerializationProvider" Version="1.0.1" />
```

Or via the .NET CLI:
```bash
dotnet add package ktsu.SerializationProvider
```

## Basic Usage

### Direct Usage

```csharp
using ktsu.SerializationProvider;
using ktsu.UniversalSerializer;
using ktsu.UniversalSerializer.Json;

// Create a JSON serializer
var jsonSerializer = new JsonSerializer();

// Wrap it in a UniversalSerializationProvider
var provider = new UniversalSerializationProvider(jsonSerializer, "MyJsonProvider");

// Use the provider
var data = new TestData { Name = "Test", Value = 42 };
string serialized = provider.Serialize(data);
var deserialized = provider.Deserialize<TestData>(serialized);

// Define your data type
public class TestData
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
}
```

### Dependency Injection

The library provides several extension methods to register serialization providers with DI containers:

```csharp
using Microsoft.Extensions.DependencyInjection;
using ktsu.UniversalSerializer.DependencyInjection;

var services = new ServiceCollection();

// Add core UniversalSerializer services
services.AddUniversalSerializer();

// Add a JSON serialization provider
services.AddJsonSerializationProvider();

// Or add other format providers
services.AddYamlSerializationProvider();
services.AddTomlSerializationProvider();
services.AddXmlSerializationProvider();
services.AddMessagePackSerializationProvider();

var serviceProvider = services.BuildServiceProvider();

// Use the provider
var provider = serviceProvider.GetRequiredService<ISerializationProvider>();
```

## Available Extension Methods

### Generic Provider Registration

```csharp
// Register any serializer type as a provider
services.AddUniversalSerializationProvider<JsonSerializer>("Custom.Provider");
```

### Factory-Based Registration

```csharp
// Register using a factory function
services.AddUniversalSerializationProvider(
    sp => new JsonSerializer(/* custom options */), 
    "Factory.Provider"
);
```

### Instance-Based Registration

```csharp
// Register a specific serializer instance
var serializer = new JsonSerializer();
services.AddUniversalSerializationProvider(serializer, "Instance.Provider");
```

### Format-Specific Providers

```csharp
// Add specific format providers with default names
services.AddJsonSerializationProvider();        // "UniversalSerializer.Json"
services.AddYamlSerializationProvider();        // "UniversalSerializer.Yaml"
services.AddTomlSerializationProvider();        // "UniversalSerializer.Toml"
services.AddXmlSerializationProvider();         // "UniversalSerializer.Xml"
services.AddMessagePackSerializationProvider(); // "UniversalSerializer.MessagePack"
```

## Provider Properties

Each provider exposes:

- `ProviderName`: A unique identifier for the provider
- `ContentType`: The MIME type for the serialization format (e.g., "application/json")

## Interface Methods

The `ISerializationProvider` interface provides the following methods:

### Synchronous Methods
- `string Serialize<T>(T obj)` - Serialize an object of type T
- `string Serialize(object obj, Type type)` - Serialize an object with explicit type
- `T Deserialize<T>(string data)` - Deserialize to type T
- `object Deserialize(string data, Type type)` - Deserialize with explicit type

### Asynchronous Methods
- `Task<string> SerializeAsync<T>(T obj, CancellationToken cancellationToken = default)` - Async serialize generic
- `Task<string> SerializeAsync(object obj, Type type, CancellationToken cancellationToken = default)` - Async serialize with type
- `Task<T> DeserializeAsync<T>(string data, CancellationToken cancellationToken = default)` - Async deserialize generic
- `Task<object> DeserializeAsync(string data, Type type, CancellationToken cancellationToken = default)` - Async deserialize with type

## Error Handling

The provider includes proper error handling:

- `ArgumentNullException` is thrown for null objects or types
- `ArgumentException` is thrown for null, empty, or whitespace data strings
- Underlying serializer exceptions are propagated appropriately

## Async Support

All providers support both synchronous and asynchronous operations:

```csharp
// Synchronous
string serialized = provider.Serialize(data);
var deserialized = provider.Deserialize<MyType>(serialized);

// Asynchronous
string serialized = await provider.SerializeAsync(data);
var deserialized = await provider.DeserializeAsync<MyType>(serialized);

// Async with type parameter
string serialized = await provider.SerializeAsync(data, typeof(MyType));
object deserialized = await provider.DeserializeAsync(serialized, typeof(MyType));

// With cancellation token
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
string serialized = await provider.SerializeAsync(data, cts.Token);
var deserialized = await provider.DeserializeAsync<MyType>(serialized, cts.Token);
```

## Custom Provider Names

You can specify custom provider names when registering:

```csharp
services.AddJsonSerializationProvider("MyCustomJsonProvider");
```

This is useful when you need multiple providers of the same type with different configurations.

## Default Provider Names

When no custom name is provided, the provider generates a default name using the pattern `"UniversalSerializer.{SerializerTypeName}"`:

- `JsonSerializer` → `"UniversalSerializer.JsonSerializer"`
- `YamlSerializer` → `"UniversalSerializer.YamlSerializer"`
- etc.

The format-specific registration methods use cleaner default names:

- `AddJsonSerializationProvider()` → `"UniversalSerializer.Json"`
- `AddYamlSerializationProvider()` → `"UniversalSerializer.Yaml"`
- etc.

## Practical Examples

### Multiple JSON Providers with Different Configurations

```csharp
services.AddUniversalSerializationProvider(
    sp => new JsonSerializer(/* config 1 */), 
    "JsonProvider.Compact"
);

services.AddUniversalSerializationProvider(
    sp => new JsonSerializer(/* config 2 */), 
    "JsonProvider.Pretty"
);
```

### Mixed Format Providers

```csharp
services.AddJsonSerializationProvider("API.Json");
services.AddYamlSerializationProvider("Config.Yaml");
services.AddTomlSerializationProvider("Settings.Toml");
```

## Integration with Other Libraries

The `ISerializationProvider` interface is designed to be framework-agnostic and can be used with any dependency injection container or framework that supports the interface. 