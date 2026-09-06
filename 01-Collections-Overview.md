# C# Collections Overview

A collection is an object used to store and manage multiple values.

Collections are useful when the number of values is not known in advance or when we need operations such as searching, sorting, adding, removing, or grouping data.

## Generic collections

Most modern C# collections use generics.

```csharp
List<string> names = new List<string>();
Dictionary<int, string> students = new Dictionary<int, string>();
```

Generics provide type safety because the collection specifies the type of data it can contain.

## Common collections

| Collection | Purpose |
|---|---|
| `List<T>` | Ordered, index-based collection |
| `Dictionary<TKey,TValue>` | Key-value lookup |
| `HashSet<T>` | Unique values |
| `Queue<T>` | First-In, First-Out (FIFO) |
| `Stack<T>` | Last-In, First-Out (LIFO) |
| `LinkedList<T>` | Doubly linked list |

## Collection interfaces

Important interfaces include:

- `IEnumerable<T>` — supports iteration.
- `ICollection<T>` — adds basic collection operations such as `Add`, `Remove`, and `Count`.
- `IList<T>` — represents an indexable list.
- `ISet<T>` — represents a set of unique values.
- `IDictionary<TKey,TValue>` — represents key-value pairs.

## Example

```csharp
List<int> numbers = new List<int> { 10, 20, 30 };

foreach (int number in numbers)
{
    Console.WriteLine(number);
}
```
