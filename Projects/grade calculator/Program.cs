using System;

class Program
{
    static void Main()
    {
        string[] subjects = { "Programming", "Database", "Networking", "Mathematics" };
        double[] marks = { 78, 65, 82, 71 };

        double total = 0;

        Console.WriteLine("=== Student Grade Report ===");

        for (int i = 0; i < subjects.Length; i++)
        {
            Console.WriteLine($"{subjects[i]}: {marks[i]}%");
            total += marks[i];
        }

        double average = total / marks.Length;

        Console.WriteLine($"\nAverage: {average:F2}%");

        if (average >= 80)
        {
            Console.WriteLine("Grade: A");
        }
        else if (average >= 70)
        {
            Console.WriteLine("Grade: B");
        }
        else if (average >= 60)
        {
            Console.WriteLine("Grade: C");
        }
        else if (average >= 50)
        {
            Console.WriteLine("Grade: D");
        }
        else
        {
            Console.WriteLine("Grade: F");
        }
    }
}