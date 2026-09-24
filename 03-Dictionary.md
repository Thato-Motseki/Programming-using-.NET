# Dictionary<TKey, TValue>

`Dictionary<TKey, TValue>` stores data as key-value pairs.

Each key must be unique.

## Creating a Dictionary

```csharp
Dictionary<int, string> students = new Dictionary<int, string>();

students.Add(101, "Thato");
students.Add(102, "Neo");
students.Add(103, "Mpho");
```

## Accessing a value

```csharp
Console.WriteLine(students[101]);
```

## Safer lookup

Using `TryGetValue` avoids an exception when the key does not exist.

```csharp
if (students.TryGetValue(101, out string name))
{
    Console.WriteLine(name);
}
```

## Checking for a key

```csharp
if (students.ContainsKey(102))
{
    Console.WriteLine("Student exists.");
}
```

## Removing an item

```csharp
students.Remove(103);
```

## Iterating

```csharp
foreach (KeyValuePair<int, string> student in students)
{
    Console.WriteLine($"{student.Key}: {student.Value}");
}
```

## When to use Dictionary<TKey,TValue>

Use a dictionary when you need to retrieve a value using a unique key.

Examples:

- Student ID → student name
- Product code → product
- Username → account information
- Country code → country name
