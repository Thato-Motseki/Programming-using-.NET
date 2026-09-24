# List<T>

`List<T>` is a generic collection that stores elements in an ordered sequence and allows access by index.

## Creating a List

```csharp
List<string> fruits = new List<string>();

fruits.Add("Apple");
fruits.Add("Banana");
fruits.Add("Orange");
```

You can also initialize it directly:

```csharp
List<int> numbers = new List<int> { 10, 20, 30, 40 };
```

## Accessing elements

```csharp
Console.WriteLine(numbers[0]);
```

Indexes start at `0`.

## Common operations

```csharp
numbers.Add(50);
numbers.Insert(1, 15);
numbers.Remove(30);
numbers.RemoveAt(0);
numbers.Contains(40);
numbers.Sort();
numbers.Reverse();

Console.WriteLine(numbers.Count);
```

## Iterating

```csharp
foreach (int number in numbers)
{
    Console.WriteLine(number);
}
```

## When to use List<T>

Use a `List<T>` when:

- Order matters.
- You need index-based access.
- Duplicate values are allowed.
- You frequently add items.

## Example program

```csharp
List<string> students = new List<string>
{
    "Thato",
    "Neo",
    "Mpho"
};

students.Add("Lerato");

foreach (string student in students)
{
    Console.WriteLine(student);
}
```
