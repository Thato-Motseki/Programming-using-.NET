# Queue<T> and Stack<T>

## Queue<T>

A queue follows FIFO:

**First In, First Out**

The first item added is the first item removed.

```csharp
Queue<string> queue = new Queue<string>();

queue.Enqueue("A");
queue.Enqueue("B");
queue.Enqueue("C");

Console.WriteLine(queue.Dequeue()); // A
```

Useful methods:

```csharp
queue.Enqueue("D");
queue.Dequeue();
queue.Peek();
queue.Contains("B");
```

Typical uses:

- Printer queues
- Customer service systems
- Task processing
- Breadth-first search

## Stack<T>

A stack follows LIFO:

**Last In, First Out**

The last item added is the first item removed.

```csharp
Stack<string> stack = new Stack<string>();

stack.Push("A");
stack.Push("B");
stack.Push("C");

Console.WriteLine(stack.Pop()); // C
```

Useful methods:

```csharp
stack.Push("D");
stack.Pop();
stack.Peek();
stack.Contains("A");
```

Typical uses:

- Undo functionality
- Browser history
- Expression evaluation
- Depth-first search

## Queue vs Stack

| Feature | Queue | Stack |
|---|---|---|
| Rule | FIFO | LIFO |
| Add | `Enqueue()` | `Push()` |
| Remove | `Dequeue()` | `Pop()` |
| Inspect next | `Peek()` | `Peek()` |
