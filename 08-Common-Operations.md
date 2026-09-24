# Common Collection Operations

Many generic collections provide common operations through interfaces such as `ICollection<T>` and `IEnumerable<T>`.

## Count

```csharp
Console.WriteLine(collection.Count);
```

## Add

```csharp
list.Add("Item");
```

## Remove

```csharp
list.Remove("Item");
```

## Contains

```csharp
if (list.Contains("Item"))
{
    Console.WriteLine("Found");
}
```

## foreach

```csharp
foreach (string item in list)
{
    Console.WriteLine(item);
}
```

## LINQ

Collections can be queried using LINQ.

```csharp
using System.Linq;

List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6 };

List<int> evenNumbers = numbers
    .Where(n => n % 2 == 0)
    .ToList();
```

LINQ is especially useful for filtering, sorting, projection, grouping, and aggregation.
