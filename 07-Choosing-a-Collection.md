# Choosing the Right C# Collection

The correct collection depends on how data will be accessed and modified.

## Decision guide

### Need index-based access?

Use:

```text
List<T>
```

### Need key-value lookup?

Use:

```text
Dictionary<TKey,TValue>
```

### Need unique values?

Use:

```text
HashSet<T>
```

### Need FIFO processing?

Use:

```text
Queue<T>
```

### Need LIFO processing?

Use:

```text
Stack<T>
```

### Need linked-node insertion/removal?

Consider:

```text
LinkedList<T>
```

## Comparison

| Collection | Ordered | Duplicates | Key lookup | Indexing |
|---|---:|---:|---:|---:|
| List<T> | Yes | Yes | No | Yes |
| Dictionary<TKey,TValue> | Key-based | Keys: No | Yes | No |
| HashSet<T> | No indexing | No | Membership | No |
| Queue<T> | Yes | Yes | No | No |
| Stack<T> | Yes | Yes | No | No |
| LinkedList<T> | Yes | Yes | No | No |

The best collection is determined by the operations your program performs most often, not simply by which collection is easiest to write.
