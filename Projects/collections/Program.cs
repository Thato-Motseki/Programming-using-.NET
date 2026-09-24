using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        // ==========================================
        // 1. HASHSET<T>
        // ==========================================

        Console.WriteLine("===== HASHSET =====");

        HashSet<string> students = new HashSet<string>();

        students.Add("Thato");
        students.Add("Lerato");
        students.Add("Thato"); // Duplicate

        Console.WriteLine("Students in HashSet:");

        foreach (string student in students)
        {
            Console.WriteLine(student);
        }

        Console.WriteLine($"Number of students: {students.Count}");


        // ==========================================
        // 2. SET OPERATIONS
        // ==========================================

        Console.WriteLine("\n===== SET OPERATIONS =====");

        HashSet<int> programmingStudents =
            new HashSet<int> { 101, 102, 103, 104 };

        HashSet<int> networkingStudents =
            new HashSet<int> { 103, 104, 105, 106 };

        Console.WriteLine("Programming students:");

        foreach (int student in programmingStudents)
        {
            Console.Write(student + " ");
        }

        Console.WriteLine("\nNetworking students:");

        foreach (int student in networkingStudents)
        {
            Console.Write(student + " ");
        }

        // INTERSECTION
        HashSet<int> commonStudents =
            new HashSet<int>(programmingStudents);

        commonStudents.IntersectWith(networkingStudents);

        Console.WriteLine("\n\nStudents taking both courses:");

        foreach (int student in commonStudents)
        {
            Console.Write(student + " ");
        }

        // UNION
        HashSet<int> allStudents =
            new HashSet<int>(programmingStudents);

        allStudents.UnionWith(networkingStudents);

        Console.WriteLine("\n\nStudents taking either course:");

        foreach (int student in allStudents)
        {
            Console.Write(student + " ");
        }

        // EXCEPT
        HashSet<int> programmingOnly =
            new HashSet<int>(programmingStudents);

        programmingOnly.ExceptWith(networkingStudents);

        Console.WriteLine("\n\nStudents taking Programming only:");

        foreach (int student in programmingOnly)
        {
            Console.Write(student + " ");
        }


        // ==========================================
        // 3. SORTEDSET<T>
        // ==========================================

        Console.WriteLine("\n\n===== SORTEDSET =====");

        SortedSet<int> marks =
            new SortedSet<int>();

        marks.Add(75);
        marks.Add(45);
        marks.Add(90);
        marks.Add(45); // Duplicate

        Console.WriteLine("Marks:");

        foreach (int mark in marks)
        {
            Console.WriteLine(mark);
        }


        // ==========================================
        // 4. LINKEDLIST<T>
        // ==========================================

        Console.WriteLine("\n===== LINKEDLIST =====");

        LinkedList<string> tasks =
            new LinkedList<string>();

        tasks.AddLast("Write code");
        tasks.AddLast("Test program");
        tasks.AddFirst("Start project");

        Console.WriteLine("Tasks:");

        foreach (string task in tasks)
        {
            Console.WriteLine(task);
        }


        // ==========================================
        // 5. SORTEDDICTIONARY<TKey, TValue>
        // ==========================================

        Console.WriteLine("\n===== SORTED DICTIONARY =====");

        SortedDictionary<int, string> studentNames =
            new SortedDictionary<int, string>();

        studentNames.Add(103, "Thato");
        studentNames.Add(101, "Lerato");
        studentNames.Add(102, "Mpho");

        Console.WriteLine("Students:");

        foreach (KeyValuePair<int, string> student
                 in studentNames)
        {
            Console.WriteLine(
                $"{student.Key} - {student.Value}");
        }
    }
}