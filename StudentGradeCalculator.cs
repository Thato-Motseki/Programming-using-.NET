using System;

class StudentGradeCalculator
{
    static void Main()
    {
        Console.Write("Enter student name: ");
        string name = Console.ReadLine() ?? "";

        double[] marks = new double[5];
        double total = 0;

        for (int i = 0; i < marks.Length; i++)
        {
            Console.Write($"Enter mark for Subject {i + 1}: ");
            marks[i] = Convert.ToDouble(Console.ReadLine());
            total += marks[i];
        }

        double average = total / marks.Length;
        string grade;

        if (average >= 80)
            grade = "A";
        else if (average >= 70)
            grade = "B";
        else if (average >= 60)
            grade = "C";
        else if (average >= 50)
            grade = "D";
        else
            grade = "F";

        Console.WriteLine("\n--- Student Results ---");
        Console.WriteLine($"Student: {name}");
        Console.WriteLine($"Total: {total:F2}");
        Console.WriteLine($"Average: {average:F2}");
        Console.WriteLine($"Grade: {grade}");
        Console.WriteLine($"Status: {(average >= 50 ? "Pass" : "Fail")}");
    }
}
