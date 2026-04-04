using System.Runtime.InteropServices;
using Arch.Core;
using System.Text.Json;
using System.Text.Json.Serialization;
using Arch.Core.Extensions;

namespace Siwa.Core.Serialization;

public class WorldConverter : JsonConverter<World>
{
    // 1. Use a static ConcurrentDictionary to cache reflection lookups across all JSON reads.
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, Type> TypeCache = new();

    public override World? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        // Safety check in case the JSON is malformed
        if (!root.TryGetProperty("Entities", out var entitiesProp) || entitiesProp.ValueKind != JsonValueKind.Array)
        {
            return World.Create();
        }

        // 2. Pre-allocate the list using the exact array length to prevent resizing allocations
        int entityCount = entitiesProp.GetArrayLength();
        var orderList = new List<(int Id, object[] Components)>(entityCount);

        foreach (var entity in entitiesProp.EnumerateArray())
        {
            int id = entity.GetProperty("Id").GetInt32();

            if (!entity.TryGetProperty("Components", out var componentsProp))
                continue;

            // Pre-allocate the components list
            int componentCount = componentsProp.GetArrayLength();
            var componentsList = new List<object>(componentCount);

            foreach (var component in componentsProp.EnumerateArray())
            {
                if (!component.TryGetProperty("__type__", out var typeProp) ||
                    typeProp.GetString() is not { } typeFullName) continue;

                // 3. Cache lookup
                if (!TypeCache.TryGetValue(typeFullName, out Type? type))
                {
                    type = Type.GetType(typeFullName);
                    if (type != null)
                    {
                        TypeCache[typeFullName] = type;
                    }
                }

                if (type == null || !component.TryGetProperty("__value__", out var valueProp))
                    continue;

                var obj = valueProp.Deserialize(type, options);
                if (obj != null)
                {
                    componentsList.Add(obj);
                }
            }

            orderList.Add((id, componentsList.ToArray()));
        }

        // 4. In-place sorting: Eliminates LINQ enumerator and array allocation overhead
        orderList.Sort((a, b) => a.Id.CompareTo(b.Id));

        var world = World.Create();

        // Use CollectionsMarshal for a tiny micro-optimization if you are on .NET 6+
        // foreach (var item in CollectionsMarshal.AsSpan(orderList))
        foreach (var item in CollectionsMarshal.AsSpan(orderList))
        {
            var entity = world.Create();
            entity.AddRange(item.Components);
        }

        return world;
    }

    public override void Write(Utf8JsonWriter writer, World value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteStartArray("Entities");

        // 1. Avoid massive Garbage Collection spikes by renting the array
        var entitiesArray = System.Buffers.ArrayPool<Entity>.Shared.Rent(value.Size);
    
        try
        {
            // 2. Slice the rented array to the exact size we need
            Span<Entity> entitiesSpan = entitiesArray.AsSpan(0, value.Size);
            value.GetEntities(QueryDescription.Null, entitiesSpan);

            foreach (var entity in entitiesSpan)
            {
                writer.WriteStartObject();
                writer.WriteNumber("Id", entity.Id);
                writer.WriteStartArray("Components");

                var components = entity.GetAllComponents();
                foreach (var component in components)
                {
                    if (component is null) continue;

                    var type = component.GetType();

                    writer.WriteStartObject();
                    // GetType().FullName is reasonably fast, but you could cache this 
                    // in a ConcurrentDictionary<Type, string> if you want micro-optimizations.
                    writer.WriteString("__type__", type.FullName); 

                    // 3. Let System.Text.Json handle the reflection and serialization natively
                    writer.WritePropertyName("__value__");
                    JsonSerializer.Serialize(writer, component, type, options);
                    
                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
                writer.WriteEndObject();
            }
        }
        finally
        {
            // Always return the array to the pool, even if serialization fails
            System.Buffers.ArrayPool<Entity>.Shared.Return(entitiesArray);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }
}