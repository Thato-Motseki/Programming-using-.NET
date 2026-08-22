using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        List<string> tasks = new List<string>();

        Console.WriteLine("=== My To-Do List ===");

        tasks.Add("Study C#");
        tasks.Add("Complete GitHub commit");
        tasks.Add("Review OOP");

        foreach (string task in tasks)
        {
            Console.WriteLine("- " + task);
        }

        Console.WriteLine($"\nTotal tasks: {tasks.Count}");
    }
}