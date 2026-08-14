using System;

class ExpenseTracker
{
    static void Main()
    {
        Console.WriteLine("=== Expense Tracker ===");

        Console.Write("Enter your monthly budget: ");
        double budget = Convert.ToDouble(Console.ReadLine());

        Console.Write("Enter number of expenses: ");
        int numberOfExpenses = Convert.ToInt32(Console.ReadLine());

        double totalExpenses = 0;

        for (int i = 1; i <= numberOfExpenses; i++)
        {
            Console.Write($"Enter expense {i}: ");
            double expense = Convert.ToDouble(Console.ReadLine());

            totalExpenses += expense;
        }

        double remainingBalance = budget - totalExpenses;

        Console.WriteLine("\n=== Summary ===");
        Console.WriteLine($"Budget: {budget:F2}");
        Console.WriteLine($"Total Expenses: {totalExpenses:F2}");
        Console.WriteLine($"Remaining Balance: {remainingBalance:F2}");

        if (remainingBalance > 0)
        {
            Console.WriteLine("You are within your budget.");
        }
        else if (remainingBalance == 0)
        {
            Console.WriteLine("You have used your entire budget.");
        }
        else
        {
            Console.WriteLine("Warning: You have exceeded your budget.");
        }
    }
}
