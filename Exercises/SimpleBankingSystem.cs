using System;

class SimpleBankingSystem
{
    static void Main()
    {
        double balance = 1000.00;
        bool running = true;

        Console.WriteLine("=== Simple Banking System ===");

        while (running)
        {
            Console.WriteLine("\n1. Check Balance");
            Console.WriteLine("2. Deposit");
            Console.WriteLine("3. Withdraw");
            Console.WriteLine("4. Exit");

            Console.Write("Choose an option: ");
            int choice = Convert.ToInt32(Console.ReadLine());

            switch (choice)
            {
                case 1:
                    Console.WriteLine($"Current balance: {balance:F2}");
                    break;

                case 2:
                    Console.Write("Enter deposit amount: ");
                    double deposit = Convert.ToDouble(Console.ReadLine());

                    if (deposit > 0)
                    {
                        balance += deposit;
                        Console.WriteLine($"Deposit successful. New balance: {balance:F2}");
                    }
                    else
                    {
                        Console.WriteLine("Deposit must be greater than zero.");
                    }
                    break;

                case 3:
                    Console.Write("Enter withdrawal amount: ");
                    double withdrawal = Convert.ToDouble(Console.ReadLine());

                    if (withdrawal > 0 && withdrawal <= balance)
                    {
                        balance -= withdrawal;
                        Console.WriteLine($"Withdrawal successful. New balance: {balance:F2}");
                    }
                    else
                    {
                        Console.WriteLine("Invalid withdrawal amount or insufficient funds.");
                    }
                    break;

                case 4:
                    running = false;
                    Console.WriteLine("Thank you for using the banking system.");
                    break;

                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }
}
