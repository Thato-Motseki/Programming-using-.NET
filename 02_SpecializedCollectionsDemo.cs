using System;
using System.Collections.Generic;

class SpecializedCollectionsDemo
{
    static void Main()
    {
        // SortedSet<T>: unique values kept in sorted order.
        SortedSet<int> scores = new SortedSet<int> { 50, 10, 30, 10 };

        Console.WriteLine("SortedSet: " + string.Join(", ", scores));

        // LinkedList<T>: sequential nodes.
        LinkedList<string> tasks = new LinkedList<string>();
        tasks.AddLast("Task 1");
        tasks.AddLast("Task 2");
        tasks.AddFirst("Urgent Task");

        Console.WriteLine("LinkedList:");
        foreach (string task in tasks)
        {
            Console.WriteLine("- " + task);
        }

        // SortedDictionary<TKey, TValue>: key-value pairs sorted by key.
        SortedDictionary<int, string> students =
            new SortedDictionary<int, string>();

        students.Add(103, "Thato");
        students.Add(101, "John");
        students.Add(102, "Mary");

        Console.WriteLine("SortedDictionary:");
        foreach (KeyValuePair<int, string> student in students)
        {
            Console.WriteLine(student.Key + ": " + student.Value);
        }
    }
}
