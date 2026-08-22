using System;

class Program
{
    static double Calculate(double num1, double num2, char operation)
    {
        switch (operation)
        {
            case '+':
                return num1 + num2;

            case '-':
                return num1 - num2;

            case '*':
                return num1 * num2;

            case '/':
                if (num2 == 0)
                {
                    Console.WriteLine("Cannot divide by zero.");
                    return 0;
                }
                return num1 / num2;

            default:
                Console.WriteLine("Invalid operation.");
                return 0;
        }
    }

    static void Main()
    {
        Console.WriteLine("=== Simple Calculator ===");

        Console.Write("Enter first number: ");
        double num1 = Convert.ToDouble(Console.ReadLine());

        Console.Write("Enter an operation (+, -, *, /): ");
        char operation = Convert.ToChar(Console.ReadLine());

        Console.Write("Enter second number: ");
        double num2 = Convert.ToDouble(Console.ReadLine());

        double result = Calculate(num1, num2, operation);

        Console.WriteLine($"Result: {result}");
    }
}