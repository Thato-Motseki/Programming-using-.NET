using System;
using System.Collections.Generic;

class HashSetDemo
{
    static void Main()
    {
        HashSet<int> groupA = new HashSet<int> { 1, 2, 3, 4 };
        HashSet<int> groupB = new HashSet<int> { 3, 4, 5, 6 };

        // Duplicate values are ignored.
        groupA.Add(4);

        Console.WriteLine("Group A: " + string.Join(", ", groupA));
        Console.WriteLine("Group B: " + string.Join(", ", groupB));

        HashSet<int> union = new HashSet<int>(groupA);
        union.UnionWith(groupB);

        HashSet<int> intersection = new HashSet<int>(groupA);
        intersection.IntersectWith(groupB);

        HashSet<int> difference = new HashSet<int>(groupA);
        difference.ExceptWith(groupB);

        Console.WriteLine("Union: " + string.Join(", ", union));
        Console.WriteLine("Intersection: " + string.Join(", ", intersection));
        Console.WriteLine("A - B: " + string.Join(", ", difference));
    }
}
