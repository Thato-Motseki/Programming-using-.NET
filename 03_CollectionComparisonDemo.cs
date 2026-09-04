using System;

class CollectionComparisonDemo
{
    static void Main()
    {
        Console.WriteLine("C# Topic 5 - Collection Selection Guide");
        Console.WriteLine("-----------------------------------------");

        Console.WriteLine("HashSet<T>");
        Console.WriteLine("Best for: unique values and fast membership checks");
        Console.WriteLine("Typical lookup/add: O(1) average");
        Console.WriteLine();

        Console.WriteLine("SortedSet<T>");
        Console.WriteLine("Best for: unique values that must remain sorted");
        Console.WriteLine("Typical lookup/add: O(log n)");
        Console.WriteLine();

        Console.WriteLine("LinkedList<T>");
        Console.WriteLine("Best for: sequential node-based operations");
        Console.WriteLine("Known-node insertion/removal: O(1)");
        Console.WriteLine("Searching: O(n)");
        Console.WriteLine();

        Console.WriteLine("SortedDictionary<TKey, TValue>");
        Console.WriteLine("Best for: key-value pairs sorted by key");
        Console.WriteLine("Typical lookup/add: O(log n)");
    }
}
