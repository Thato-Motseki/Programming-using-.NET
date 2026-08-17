using System;

class StudentGradeCalculator
{
    static void Main()
    {
        Console.WriteLine("=== Student Grade Calculator ===");

        Console.Write("Enter student name: ");
        string name = Console.ReadLine();

        Console.Write("Enter assignment mark: ");
        double assignment = Convert.ToDouble(Console.ReadLine());

        Console.Write("Enter test mark: ");
        double test = Convert.ToDouble(Console.ReadLine());

        Console.Write("Enter exam mark: ");
        double exam = Convert.ToDouble(Console.ReadLine());

        double total = assignment + test + exam;

        string grade;

        if (total >= 80)
            grade = "A";
        else if (total >= 70)
            grade = "B";
        else if (total >= 60)
            grade = "C";
        else if (total >= 50)
            grade = "D";
        else
            grade = "F";

        Console.WriteLine("\n=== Result ===");
        Console.WriteLine($"Student: {name}");
        Console.WriteLine($"Total: {total:F2}%");
        Console.WriteLine($"Grade: {grade}");
    }
}
