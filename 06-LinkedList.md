# LinkedList<T>

`LinkedList<T>` is a doubly linked list. Each node contains a value and references to neighboring nodes.

## Creating a LinkedList

```csharp
LinkedList<string> names = new LinkedList<string>();

names.AddLast("Thato");
names.AddLast("Neo");
names.AddFirst("Mpho");
```

## Adding relative to a node

```csharp
LinkedListNode<string>? node = names.Find("Neo");

if (node != null)
{
    names.AddBefore(node, "Lerato");
}
```

## Removing

```csharp
names.Remove("Neo");
names.RemoveFirst();
names.RemoveLast();
```

## When to use it

`LinkedList<T>` can be useful when frequent insertions or removals occur at known nodes.

For many ordinary application scenarios, `List<T>` is simpler and often preferable.
