// C# Practice Exercises\n// Exercises are separated by file-name markers.\n\n// ===== StudentGradeCalculator.cs =====\nusing System;

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
\n\n// ===== BankAccount.cs =====\nusing System;

class BankAccount
{
    private string accountHolder;
    private double balance;

    public BankAccount(string accountHolder, double initialBalance)
    {
        this.accountHolder = accountHolder;
        balance = initialBalance;
    }

    public void Deposit(double amount)
    {
        if (amount > 0)
        {
            balance += amount;
            Console.WriteLine($"Deposit successful. New balance: {balance:F2}");
        }
        else
        {
            Console.WriteLine("Deposit amount must be greater than zero.");
        }
    }

    public void Withdraw(double amount)
    {
        if (amount <= 0)
        {
            Console.WriteLine("Withdrawal amount must be greater than zero.");
        }
        else if (amount > balance)
        {
            Console.WriteLine("Insufficient funds.");
        }
        else
        {
            balance -= amount;
            Console.WriteLine($"Withdrawal successful. New balance: {balance:F2}");
        }
    }

    public void DisplayAccount()
    {
        Console.WriteLine("\n--- Account Information ---");
        Console.WriteLine($"Account Holder: {accountHolder}");
        Console.WriteLine($"Balance: {balance:F2}");
    }

    static void Main()
    {
        Console.Write("Enter account holder name: ");
        string name = Console.ReadLine() ?? "";

        Console.Write("Enter initial balance: ");
        double initialBalance = Convert.ToDouble(Console.ReadLine());

        BankAccount account = new BankAccount(name, initialBalance);

        account.DisplayAccount();

        Console.Write("\nEnter amount to deposit: ");
        double deposit = Convert.ToDouble(Console.ReadLine());
        account.Deposit(deposit);

        Console.Write("\nEnter amount to withdraw: ");
        double withdrawal = Convert.ToDouble(Console.ReadLine());
        account.Withdraw(withdrawal);

        account.DisplayAccount();
    }
}
\n\n// ===== RestaurantOrderingSystem.cs =====\nusing System;

class RestaurantOrderingSystem
{
    static void Main()
    {
        string[] menuItems =
        {
            "Burger",
            "Pizza",
            "Fries",
            "Chicken Wings",
            "Soft Drink"
        };

        double[] prices = { 55.00, 85.00, 30.00, 65.00, 20.00 };

        double subtotal = 0;
        bool ordering = true;

        Console.WriteLine("=== RESTAURANT MENU ===");

        for (int i = 0; i < menuItems.Length; i++)
        {
            Console.WriteLine($"{i + 1}. {menuItems[i]} - M{prices[i]:F2}");
        }

        while (ordering)
        {
            Console.Write("\nEnter item number (0 to finish): ");
            int choice = Convert.ToInt32(Console.ReadLine());

            if (choice == 0)
            {
                ordering = false;
            }
            else if (choice >= 1 && choice <= menuItems.Length)
            {
                Console.Write($"Enter quantity of {menuItems[choice - 1]}: ");
                int quantity = Convert.ToInt32(Console.ReadLine());

                if (quantity > 0)
                {
                    double itemTotal = prices[choice - 1] * quantity;
                    subtotal += itemTotal;

                    Console.WriteLine(
                        $"{quantity} x {menuItems[choice - 1]} = M{itemTotal:F2}");
                }
                else
                {
                    Console.WriteLine("Quantity must be greater than zero.");
                }
            }
            else
            {
                Console.WriteLine("Invalid menu choice.");
            }
        }

        double discount = 0;

        if (subtotal >= 300)
        {
            discount = subtotal * 0.10;
        }

        double finalTotal = subtotal - discount;

        Console.WriteLine("\n=== FINAL BILL ===");
        Console.WriteLine($"Subtotal: M{subtotal:F2}");
        Console.WriteLine($"Discount: M{discount:F2}");
        Console.WriteLine($"Total: M{finalTotal:F2}");

        if (discount > 0)
            Console.WriteLine("10% discount applied!");
        else
            Console.WriteLine("Spend M300 or more to receive a 10% discount.");
    }
}
\n