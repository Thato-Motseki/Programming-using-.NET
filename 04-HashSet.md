# HashSet<T>

`HashSet<T>` stores unique values.

Adding the same value more than once does not create a duplicate entry.

## Creating a HashSet

```csharp
HashSet<string> languages = new HashSet<string>
{
    "C#",
    "Java",
    "Python"
};
```

## Adding values

```csharp
languages.Add("JavaScript");
languages.Add("C#");
```

The second `C#` is ignored because it already exists.

`Add` returns a Boolean indicating whether the value was actually added.

```csharp
bool added = languages.Add("C++");
```

## Checking for values

```csharp
if (languages.Contains("Java"))
{
    Console.WriteLine("Java exists.");
}
```

## Set operations

```csharp
HashSet<int> a = new HashSet<int> { 1, 2, 3, 4 };
HashSet<int> b = new HashSet<int> { 3, 4, 5, 6 };

a.IntersectWith(b); // 3, 4
```

Other useful operations include:

```csharp
a.UnionWith(b);
a.ExceptWith(b);
a.SymmetricExceptWith(b);
```

## When to use HashSet<T>

Use a `HashSet<T>` when uniqueness is important and you mainly need fast membership testing rather than index-based access.
