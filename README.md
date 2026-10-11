# EntityMapper

A lightweight C# utility for mapping objects and collections through reflection. EntityMapper copies properties with matching names and types, then lets you customize each result with an optional `afterMap` callback.

```csharp
DestinationEntity result = EntityMapper.Map<SourceEntity, DestinationEntity>(source);

List<DestinationEntity> results = EntityMapper.Map<SourceEntity, DestinationEntity>(
    sourceEntities,
    (source, destination) =>
    {
        destination.category = categories
            .FirstOrDefault(x => x.Id == source.CategoryId);
    });
```

## Features

- Map a single object to a new destination object.
- Map an `IEnumerable<TSource>` to a `List<TDestination>`.
- Automatically copy properties with identical names and runtime property types.
- Customize mapped objects with a strongly typed callback.
- Map between different model classes without external mapping configuration.
- Use the supplied source without third-party dependencies.

## Setup

Add `EntityMapper.cs` to your application, or include it in a class library referenced by your application.

The source includes `using System.Reflection;`. If your project does not provide implicit or global usings, also add:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
```

Enable nullable annotations in your project to use the supplied `?` annotations:

```xml
<PropertyGroup>
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
</PropertyGroup>
```

### Namespace

The supplied `EntityMapper.cs` declares `EntityMapper` in the global namespace. The sample `Program.cs` imports `FdoMapper`; its model classes were not included with the sample.

If `FdoMapper` is your library namespace, place the mapper inside it:

```csharp
using System.Reflection;

namespace FdoMapper;

public static class EntityMapper
{
    // Existing mapper implementation.
}
```

Then use `using FdoMapper;` in your application. Alternatively, retain the global mapper class and remove that import if no other types require it. A project's root namespace setting does not automatically wrap C# source declarations in that namespace.

No project file or package metadata was supplied, so this README does not assume a NuGet package name or a verified target framework. The complete sample below uses top-level statements and target-typed `new()`, which require C# 9 or later. The optional file-scoped namespace shown above requires C# 10 or later.

## Complete example

This example follows the category lookup pattern in the supplied `Program.cs`. The model definitions below are illustrative definitions added to make the example self-contained.

Place this code in `Program.cs` and include the supplied `EntityMapper.cs` in the same project. It works with the mapper in its original global namespace.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

List<SourceEntity> sourceEntities = new()
{
    new()
    {
        Id = 1,
        Name = "Azure Storage Account",
        Description = "Primary storage account used for application data.",
        Type = "Storage",
        Url = "https://contoso.blob.core.windows.net",
        CategoryId = 10
    },
    new()
    {
        Id = 2,
        Name = "SharePoint Site",
        Description = "Corporate document repository.",
        Type = "Document",
        Url = "https://contoso.sharepoint.com/sites/fdo",
        CategoryId = 20
    },
    new()
    {
        Id = 3,
        Name = "Nomination API",
        Description = "Backend API for nomination management.",
        Type = "API",
        Url = "https://api.contoso.com/nomination",
        CategoryId = 30
    },
    new()
    {
        Id = 4,
        Name = "Migration Dashboard",
        Description = null,
        Type = "Dashboard",
        Url = null,
        CategoryId = 40
    },
    new()
    {
        Id = 5,
        Name = "Customer Data Warehouse",
        Description = "Centralized reporting database.",
        Type = "Database",
        Url = "https://sql.contoso.com",
        CategoryId = 50
    }
};

List<CategoryEntity> categories = new()
{
    new() { Id = 10, Name = "Storage" },
    new() { Id = 20, Name = "Documents" },
    new() { Id = 30, Name = "APIs" },
    new() { Id = 40, Name = "Dashboards" },
    new() { Id = 50, Name = "Databases" }
};

List<DestinationEntity> result =
    EntityMapper.Map<SourceEntity, DestinationEntity>(
        sourceEntities,
        (source, destination) =>
        {
            destination.category = categories
                .FirstOrDefault(x => x.Id == source.CategoryId);
        });

Console.WriteLine($"Mapped records: {result.Count}");

foreach (DestinationEntity item in result)
{
    Console.WriteLine(
        $"{item.Id}: {item.Name} | {item.category?.Name ?? "Uncategorized"}");
}

public sealed class SourceEntity
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Type { get; set; }
    public string? Url { get; set; }
    public int CategoryId { get; set; }
}

public sealed class DestinationEntity
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Type { get; set; }
    public string? Url { get; set; }
    public int CategoryId { get; set; }

    // Lowercase name retained to match the supplied Program.cs.
    public CategoryEntity? category { get; set; }
}

public sealed class CategoryEntity
{
    public int Id { get; set; }
    public string? Name { get; set; }
}
```

Expected output:

```text
Mapped records: 5
1: Azure Storage Account | Storage
2: SharePoint Site | Documents
3: Nomination API | APIs
4: Migration Dashboard | Dashboards
5: Customer Data Warehouse | Databases
```

Matching properties are copied automatically. The callback assigns `category` using `CategoryId`. When no category matches, `FirstOrDefault` returns null and the output displays `Uncategorized`.

## Usage

### Map one object

```csharp
DestinationEntity destination =
    EntityMapper.Map<SourceEntity, DestinationEntity>(sourceEntities[0]);
```

A new destination instance is created for each call.

### Map a collection

```csharp
List<DestinationEntity> destinations =
    EntityMapper.Map<SourceEntity, DestinationEntity>(sourceEntities);
```

The input is enumerated immediately, source order is preserved, and the result is always a new `List<TDestination>`.

### Override values after automatic mapping

```csharp
DestinationEntity destination =
    EntityMapper.Map<SourceEntity, DestinationEntity>(
        sourceEntities[0],
        (source, target) =>
        {
            target.Name = source.Name?.Trim();
            target.Description = source.Description ?? "No description available.";
            target.category = categories
                .FirstOrDefault(x => x.Id == source.CategoryId);
        });
```

`afterMap` runs after property copying. It can overwrite copied values, populate destination-only properties, perform explicit conversions, or attach related objects. For collections, it runs once per successfully mapped item.

### Reuse a callback

```csharp
Action<SourceEntity, DestinationEntity> enrich = (source, target) =>
{
    target.category = categories.FirstOrDefault(x => x.Id == source.CategoryId);
};

List<DestinationEntity> destinations =
    EntityMapper.Map<SourceEntity, DestinationEntity>(sourceEntities, enrich);
```

### Use a lookup for repeated category assignments

For a larger category list, construct a dictionary once instead of scanning the list for every source item:

```csharp
// Requires unique category IDs; duplicate IDs cause ToDictionary to throw.
Dictionary<int, CategoryEntity> categoryById =
    categories.ToDictionary(x => x.Id);

List<DestinationEntity> destinations =
    EntityMapper.Map<SourceEntity, DestinationEntity>(
        sourceEntities,
        (source, target) =>
        {
            target.category = categoryById.TryGetValue(source.CategoryId, out var category)
                ? category
                : null;
        });
```

The lookup object is assigned by reference; the mapper does not clone it.

## API reference

```csharp
public static TDestination Map<TSource, TDestination>(
    TSource source,
    Action<TSource, TDestination>? afterMap = null);

public static List<TDestination> Map<TSource, TDestination>(
    IEnumerable<TSource> source,
    Action<TSource, TDestination>? afterMap = null);
```

| Parameter | Description |
| --- | --- |
| `TSource` | Source type whose public properties are inspected. |
| `TDestination` | Destination type to instantiate and populate. Use a concrete class with a public parameterless constructor. |
| `source` | A source object or sequence of source objects. |
| `afterMap` | Optional synchronous callback executed after automatic mapping. |

Specify both generic type arguments explicitly, as shown in the examples. For collection mapping, `TSource` is the element type, such as `SourceEntity`, rather than `List<SourceEntity>`.

## Mapping rules

| Condition | Current behavior |
| --- | --- |
| Same property name and exactly equal property type | Copies the value if the destination property has a setter. |
| Different capitalization, such as `Name` and `name` | Does not match. Names are case-sensitive. |
| Different types, such as `int` and `long` | Does not match; no automatic conversion occurs. |
| `int` and `int?` | Does not match because these are different runtime types. |
| `string` and `string?` | Can match; reference-type nullable annotations do not create different runtime property types. |
| Source property absent from the destination | Skipped. |
| Destination-only property | Keeps its constructor or initializer value unless the callback assigns it. |
| Destination property without a setter | Skipped by automatic mapping. |
| Null source property value | Assigned when the properties match, including overwriting a destination initializer. |
| Matching reference-type property | Copies the reference, not a deep clone. |

The implementation inspects `typeof(TSource)` and `typeof(TDestination)`. It does not discover extra properties from a source object's runtime derived type when `TSource` is a base type.

## Requirements and limitations

Use ordinary model classes with public instance getters and setters. The current implementation is intentionally small and has the following boundaries:

- **Destination construction:** uses `Activator.CreateInstance<TDestination>()`. Classes need a public parameterless constructor; abstract classes, interfaces, and constructor-parameter mapping are not supported.
- **Value-type destinations:** avoid structs. Reflection writes act on boxed copies, and the `Action<TSource, TDestination>` callback receives a value copy, so property changes are not reliably retained in the returned value.
- **Shallow mapping:** nested objects and collections are not recursively mapped. References are shared when property types match exactly.
- **No assignability matching:** a derived-type property does not map to a base-type property merely because assignment would otherwise be legal in C#; runtime property types must be equal.
- **No property configuration:** ignore rules, aliases, attributes, and custom property converters are not implemented. Use `afterMap` for explicit assignments.
- **Property filtering:** `GetProperties()` can include inherited and public static properties. The implementation does not filter out static members or indexers, or verify that a source property is readable. Such models can cause unexpected side effects or exceptions.
- **Setter visibility:** `CanWrite` checks for a setter, not specifically a public setter. It should not be treated as an access-control boundary.
- **In-memory execution:** collection mapping uses `Select(...).ToList()` over `IEnumerable<TSource>`. It does not generate database projections or expression trees for a query provider.
- **Synchronous operation:** no asynchronous callbacks, cancellation, or parallel mapping are provided.

## Nulls and errors

| Situation | Outcome |
| --- | --- |
| Null source object | Throws `ArgumentNullException`. |
| Null source sequence | Throws `ArgumentNullException`. |
| Null element inside a sequence | Mapping throws when it reaches that element. Null elements are not skipped. |
| Empty sequence | Returns an empty `List<TDestination>`. |
| Missing matching property | Skipped without an error. |
| Constructor, getter, setter, or callback fails | The exception propagates; reflection may wrap accessor exceptions. |

Collection mapping stops at the first failure and does not return a partial list. Callbacks or static setters already executed for earlier items may still have produced side effects; there is no rollback.

## Performance notes

For every mapped object, the implementation discovers source and destination properties through reflection. For each source property, it scans destination properties with `FirstOrDefault`.

With `N` objects, `S` source properties, and `D` destination properties, property matching performs approximately `O(N × S × D)` work, plus construction, property access, and callback costs. The current implementation does not cache property pairs or compile accessors.

For repeated or high-volume mappings, caching matched property pairs by source/destination type would reduce repeated discovery and matching work. Benchmark with representative models before making performance claims.

## Documentation scope

This README describes the supplied `EntityMapper.cs` and the usage pattern in `Program.cs`. The example model definitions are illustrative. No project file, model definitions, test suite, or license file was provided. Compilation and runtime behavior were not tested in the documentation environment; expected output is derived from the source.
