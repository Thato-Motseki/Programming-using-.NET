using System;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== Expense Calculator ===");

        Console.Write("Enter food expenses: ");
        double food = Convert.ToDouble(Console.ReadLine());

        Console.Write("Enter transport expenses: ");
        double transport = Convert.ToDouble(Console.ReadLine());

        Console.Write("Enter entertainment expenses: ");
        double entertainment = Convert.ToDouble(Console.ReadLine());

        double total = food + transport + entertainment;

        Console.WriteLine("\n--- Expense Summary ---");
        Console.WriteLine($"Food:          M{food:F2}");
        Console.WriteLine($"Transport:     M{transport:F2}");
        Console.WriteLine($"Entertainment: M{entertainment:F2}");
        Console.WriteLine($"Total:         M{total:F2}");

        if (total > 1000)
        {
            Console.WriteLine("Warning: Your expenses are above M1000.");
        }
        else
        {
            Console.WriteLine("Your expenses are within the M1000 limit.");
        }
    }
}