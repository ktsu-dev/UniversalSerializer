// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

using System.Reflection;
using ktsu.UniversalSerializer.Serialization.TypeRegistry;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace ktsu.UniversalSerializer.Serialization.Yaml;

/// <summary>
/// YamlDotNet type converter for polymorphic serialization.
/// </summary>
internal class YamlPolymorphicTypeConverter : IYamlTypeConverter
{
    private readonly TypeRegistry.TypeRegistry _typeRegistry;
    private readonly string _typeDiscriminatorPropertyName;
    private readonly TypeDiscriminatorFormat _discriminatorFormat;

    /// <summary>
    /// Initializes a new instance of the <see cref="YamlPolymorphicTypeConverter"/> class.
    /// </summary>
    /// <param name="typeRegistry">The type registry.</param>
    /// <param name="typeDiscriminatorPropertyName">The name of the type discriminator property.</param>
    /// <param name="discriminatorFormat">The format to use for type discrimination.</param>
    public YamlPolymorphicTypeConverter(
        TypeRegistry.TypeRegistry typeRegistry,
        string typeDiscriminatorPropertyName,
        TypeDiscriminatorFormat discriminatorFormat)
    {
        _typeRegistry = typeRegistry ?? throw new ArgumentNullException(nameof(typeRegistry));
        _typeDiscriminatorPropertyName = typeDiscriminatorPropertyName ?? throw new ArgumentNullException(nameof(typeDiscriminatorPropertyName));
        _discriminatorFormat = discriminatorFormat;
    }

    /// <summary>
    /// Determines whether this converter accepts the specified type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is a class or interface other than string, false otherwise.</returns>
    public bool Accepts(Type type)
    {
        // Only include abstract classes and interfaces for polymorphic handling
        return (type.IsInterface || (type.IsClass && type.IsAbstract)) && type != typeof(string);
    }

    /// <summary>
    /// Reads an object of the specified type from YAML.
    /// </summary>
    /// <param name="parser">The parser to read from.</param>
    /// <param name="type">The type of the object to read.</param>
    /// <returns>The read object.</returns>
    public object ReadYaml(IParser parser, Type type)
    {
        // The deserialization is handled by YamlPolymorphicNodeDeserializer
        throw new NotImplementedException("ReadYaml is not implemented by this converter. Use YamlPolymorphicNodeDeserializer instead.");
    }

    /// <summary>
    /// Writes an object to YAML.
    /// </summary>
    /// <param name="emitter">The emitter to write to.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="type">The type of the value.</param>
    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        if (value == null)
        {
            emitter.Emit(new ScalarEventInfo(new Scalar(null, "null", ScalarStyle.Plain, true, false)));
            return;
        }

        var actualType = value.GetType();

        // If the actual type matches the expected type exactly, don't add type information
        if (actualType == type)
        {
            // Just use the default serialization
            throw new YamlDotNet.Core.YamlException("Object is exact type, no polymorphic serialization needed.");
        }

        // Get the type name from the registry
        var typeName = _typeRegistry.GetTypeName(actualType);

        switch (_discriminatorFormat)
        {
            case TypeDiscriminatorFormat.Property:
                WriteWithPropertyDiscriminator(emitter, value, typeName);
                break;

            case TypeDiscriminatorFormat.Wrapper:
                WriteWithWrapperDiscriminator(emitter, value, typeName);
                break;

            case TypeDiscriminatorFormat.TypeProperty:
                throw new NotImplementedException("TypeProperty discriminator format not implemented yet for YAML serialization.");

            default:
                throw new ArgumentOutOfRangeException(nameof(_discriminatorFormat), $"Unsupported type discriminator format: {_discriminatorFormat}");
        }
    }

    private void WriteWithPropertyDiscriminator(IEmitter emitter, object value, string typeName)
    {
        // Start mapping
        emitter.Emit(new MappingStart());

        // Write type discriminator property
        emitter.Emit(new Scalar(_typeDiscriminatorPropertyName));
        emitter.Emit(new Scalar(typeName));

        // Use reflection to get all properties of the object
        var properties = value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Emit each property
        foreach (var property in properties)
        {
            if (!property.CanRead)
                continue;

            var propertyValue = property.GetValue(value);
            if (propertyValue == null)
                continue;

            // Emit property name
            emitter.Emit(new Scalar(property.Name));

            // For complex objects, we would need recursive serialization
            // This is simplified for demonstration
            emitter.Emit(new Scalar(propertyValue.ToString() ?? string.Empty));
        }

        // End mapping
        emitter.Emit(new MappingEnd());
    }

    private void WriteWithWrapperDiscriminator(IEmitter emitter, object value, string typeName)
    {
        // Start mapping
        emitter.Emit(new MappingStart());

        // Write type property
        emitter.Emit(new Scalar("type"));
        emitter.Emit(new Scalar(typeName));

        // Write value property
        emitter.Emit(new Scalar("value"));

        // Here we would need to serialize the entire object
        // This is simplified for demonstration
        emitter.Emit(new Scalar(value.ToString() ?? string.Empty));

        // End mapping
        emitter.Emit(new MappingEnd());
    }
}
