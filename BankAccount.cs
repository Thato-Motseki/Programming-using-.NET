using System;

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
