using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Security.Principal;

namespace EcoCashSimulation
{
    class Program
    {
        static Dictionary<int, Account> accounts = new Dictionary<int, Account>();
        static Account currentAccount;

        static void Main(string[] args)
        {
            CreateSampleAccounts();

            bool running = true;

            Console.WriteLine("======================================");
            Console.WriteLine("           ECOCASH LESOTHO");
            Console.WriteLine("        MOBILE MONEY SYSTEM");
            Console.WriteLine("======================================\n");

            while (running)
            {
                try
                {
                    if (currentAccount == null)
                    {
                        ShowWelcomeMenu();
                    }
                    else
                    {
                        ShowMainMenu();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nAn unexpected error occurred.");
                    Console.WriteLine("Please try again.");
                    Logger.LogError(ex);
                }
                finally
                {
                    Console.WriteLine();
                }

                if (currentAccount == null)
                {
                    Console.WriteLine("Press ENTER to continue...");
                    Console.ReadLine();

                    Console.Clear();
                    ShowLoginMenu();

                    if (currentAccount != null)
                    {
                        Console.Clear();
                    }
                }
                else
                {
                    Console.WriteLine("\nPress ENTER to continue...");
                    Console.ReadLine();
                    Console.Clear();
                }
            }
        }

        static void CreateSampleAccounts()
        {
            Account account1 = new Account(61234567, "Thato Motseki", "1234", 2500.00m);
            Account account2 = new Account(62345678, "Mpho Mokoena", "4321", 1800.00m);
            Account account3 = new Account(63456789, "Lerato Phiri", "1111", 3200.00m);

            accounts.Add(account1.PhoneNumber, account1);
            accounts.Add(account2.PhoneNumber, account2);
            accounts.Add(account3.PhoneNumber, account3);
        }

        static void ShowWelcomeMenu()
        {
            Console.WriteLine("\n1. Login");
            Console.WriteLine("2. Register");
            Console.WriteLine("3. Exit");
            Console.Write("Choose an option: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    // Login handled by main loop
                    break;
                case "2":
                    RegisterNewAccount();
                    break;
                case "3":
                    Console.WriteLine("\nThank you for using EcoCash.");
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }
        }

        static void ShowLoginMenu()
        {
            Console.WriteLine("======================================");
            Console.WriteLine("              ECOCASH LOGIN");
            Console.WriteLine("======================================");

            Console.Write("Enter your EcoCash number: ");

            try
            {
                int phoneNumber = int.Parse(Console.ReadLine());

                if (!IsValidPhoneNumber(phoneNumber))
                {
                    Console.WriteLine("Invalid EcoCash number.");
                    Console.WriteLine("The number must contain 8 digits and start with 6.");
                    return;
                }

                if (!accounts.ContainsKey(phoneNumber))
                {
                    Console.WriteLine("Account not found.");
                    Console.Write("Would you like to register a new account? (Y/N): ");
                    string ans = Console.ReadLine();

                    if (!string.IsNullOrEmpty(ans) && (ans.Equals("Y", StringComparison.OrdinalIgnoreCase) || ans.Equals("Yes", StringComparison.OrdinalIgnoreCase)))
                    {
                        RegisterNewAccount();
                    }

                    return;
                }

                Account account = accounts[phoneNumber];

                Console.Write("Enter your PIN: ");
                string pin = Console.ReadLine();

                if (account.VerifyPin(pin))
                {
                    currentAccount = account;
                    Console.WriteLine("\nLogin successful.");
                    Console.WriteLine("Welcome, " + currentAccount.Name + "!");
                }
                else
                {
                    Console.WriteLine("Incorrect PIN.");
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Please enter numbers only.");
                Logger.LogError(new FormatException("Invalid phone number entered."));
            }
        }

        static void RegisterNewAccount()
        {
            Console.WriteLine("\n===== REGISTER NEW ECocash ACCOUNT =====");

            try
            {
                Console.Write("Enter new EcoCash number (8 digits, starts with 6): ");

                string input = Console.ReadLine();

                if (!int.TryParse(input, out int phoneNumber))
                {
                    Console.WriteLine("Please enter numbers only.");
                    return;
                }

                if (!IsValidPhoneNumber(phoneNumber))
                {
                    Console.WriteLine("Invalid EcoCash number. It must contain 8 digits and start with 6.");
                    return;
                }

                if (accounts.ContainsKey(phoneNumber))
                {
                    Console.WriteLine("That number is already registered.");
                    return;
                }

                Console.Write("Enter your full name: ");
                string name = Console.ReadLine();

                Console.Write("Choose a 4-digit PIN: ");
                string pin = Console.ReadLine();

                if (string.IsNullOrEmpty(pin) || pin.Length != 4)
                {
                    Console.WriteLine("PIN must contain exactly 4 digits.");
                    return;
                }

                Console.Write("Confirm PIN: ");
                string confirm = Console.ReadLine();

                if (pin != confirm)
                {
                    Console.WriteLine("PINs do not match.");
                    return;
                }

                Account newAccount = new Account(phoneNumber, name, pin, 0m);
                accounts.Add(newAccount.PhoneNumber, newAccount);

                currentAccount = newAccount;

                Console.WriteLine("\nRegistration successful.");
                Console.WriteLine("Welcome, " + newAccount.Name + "!");
                Console.WriteLine("Your EcoCash number: " + newAccount.PhoneNumber);
                Console.WriteLine("Current balance: M " + newAccount.Balance.ToString("0.00"));
                Console.WriteLine("To add money to your account, choose 'Deposit Money' from the menu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Registration failed. Please try again.");
                Logger.LogError(ex);
            }
        }

        static void ShowMainMenu()
        {
            Console.WriteLine("\n======================================");
            Console.WriteLine("             ECOCASH MENU");
            Console.WriteLine("======================================");
            Console.WriteLine("Account: " + currentAccount.Name);
            Console.WriteLine("Number: " + currentAccount.PhoneNumber);
            Console.WriteLine("--------------------------------------");
            Console.WriteLine("1. Check Balance");
            Console.WriteLine("2. Send Money");
            Console.WriteLine("3. Cash Out");
            Console.WriteLine("4. Deposit Money");
            Console.WriteLine("5. Transaction History");
            Console.WriteLine("6. Change PIN");
            Console.WriteLine("7. Logout");
            Console.WriteLine("8. Exit");
            Console.WriteLine("--------------------------------------\n");
            Console.Write("Choose an option: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    CheckBalance();
                    break;

                case "2":
                    SendMoney();
                    break;

                case "3":
                    CashOut();
                    break;

                case "4":
                    DepositMoney();
                    break;

                case "5":
                    ShowTransactionHistory();
                    break;

                case "6":
                    ChangePin();
                    break;

                case "7":
                    currentAccount = null;
                    Console.WriteLine("You have been logged out.");
                    break;

                case "8":
                    Console.WriteLine("\nThank you for using EcoCash.");
                    Environment.Exit(0);
                    break;

                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }
        }

        static void CheckBalance()
        {
            Console.WriteLine("\n----- BALANCE -----");
            Console.WriteLine("Available Balance: M " + currentAccount.Balance.ToString("0.00"));
        }

        static void SendMoney()
        {
            try
            {
                Console.WriteLine("\n----- SEND MONEY -----");

                Console.Write("Enter recipient EcoCash number: ");
                int recipientNumber = int.Parse(Console.ReadLine());

                if (!IsValidPhoneNumber(recipientNumber))
                {
                    Console.WriteLine("Invalid recipient number.");
                    return;
                }

                if (recipientNumber == currentAccount.PhoneNumber)
                {
                    Console.WriteLine("You cannot send money to yourself.");
                    return;
                }

                if (!accounts.ContainsKey(recipientNumber))
                {
                    Console.WriteLine("Recipient account not found.");
                    return;
                }

                Console.Write("Enter amount: M ");
                decimal amount = decimal.Parse(Console.ReadLine());

                if (amount <= 0)
                {
                    Console.WriteLine("Amount must be greater than zero.");
                    return;
                }

                Console.Write("Enter PIN to confirm: ");
                string pin = Console.ReadLine();

                if (!currentAccount.VerifyPin(pin))
                {
                    Console.WriteLine("Incorrect PIN. Transaction cancelled.");
                    return;
                }

                Account recipient = accounts[recipientNumber];

                SendMoneyTransaction transaction =
                    new SendMoneyTransaction(currentAccount, recipient, amount);

                transaction.Process();

                currentAccount.AddTransaction(transaction);
                recipient.AddTransaction(transaction);

                Console.WriteLine("\nTransaction successful.");
                Console.WriteLine("Reference: " + transaction.Reference);
                Console.WriteLine("Sent: M " + amount.ToString("0.00"));
                Console.WriteLine("Recipient: " + recipient.Name);
                Console.WriteLine("New Balance: M " +
                                  currentAccount.Balance.ToString("0.00"));
            }
            catch (FormatException ex)
            {
                Console.WriteLine("Please enter a valid number.");
                Logger.LogError(ex);
            }
            catch (InsufficientBalanceException ex)
            {
                Console.WriteLine(ex.Message);
                Logger.LogError(ex);
            }
        }

        static void CashOut()
        {
            try
            {
                Console.WriteLine("\n----- CASH OUT -----");

                Console.Write("Enter cash-out amount: M ");
                decimal amount = decimal.Parse(Console.ReadLine());

                if (amount <= 0)
                {
                    Console.WriteLine("Amount must be greater than zero.");
                    return;
                }

                Console.Write("Enter PIN to confirm: ");
                string pin = Console.ReadLine();

                if (!currentAccount.VerifyPin(pin))
                {
                    Console.WriteLine("Incorrect PIN. Transaction cancelled.");
                    return;
                }

                CashOutTransaction transaction =
                    new CashOutTransaction(currentAccount, amount);

                transaction.Process();

                currentAccount.AddTransaction(transaction);

                Console.WriteLine("\nCash out successful.");
                Console.WriteLine("Reference: " + transaction.Reference);
                Console.WriteLine("Cash Out: M " + amount.ToString("0.00"));
                Console.WriteLine("New Balance: M " +
                                  currentAccount.Balance.ToString("0.00"));
            }
            catch (FormatException ex)
            {
                Console.WriteLine("Please enter a valid amount.");
                Logger.LogError(ex);
            }
            catch (InsufficientBalanceException ex)
            {
                Console.WriteLine(ex.Message);
                Logger.LogError(ex);
            }
        }

        static void DepositMoney()
        {
            try
            {
                Console.WriteLine("\n----- DEPOSIT MONEY -----");

                Console.Write("Enter deposit amount: M ");
                decimal amount = decimal.Parse(Console.ReadLine());

                if (amount <= 0)
                {
                    Console.WriteLine("Amount must be greater than zero.");
                    return;
                }

                DepositTransaction transaction =
                    new DepositTransaction(currentAccount, amount);

                transaction.Process();

                currentAccount.AddTransaction(transaction);

                Console.WriteLine("\nDeposit successful.");
                Console.WriteLine("Reference: " + transaction.Reference);
                Console.WriteLine("Deposited: M " + amount.ToString("0.00"));
                Console.WriteLine("New Balance: M " +
                                  currentAccount.Balance.ToString("0.00"));
            }
            catch (FormatException ex)
            {
                Console.WriteLine("Please enter a valid amount.");
                Logger.LogError(ex);
            }
        }

        static void ShowTransactionHistory()
        {
            Console.WriteLine("\n----- TRANSACTION HISTORY -----");

            List<Transaction> history = currentAccount.GetTransactions();

            if (history.Count == 0)
            {
                Console.WriteLine("No transactions found.");
                return;
            }

            foreach (Transaction transaction in history)
            {
                Console.WriteLine("--------------------------------------");
                Console.WriteLine("Type: " + transaction.TransactionType);
                Console.WriteLine("Amount: M " +
                                  transaction.Amount.ToString("0.00"));
                Console.WriteLine("Reference: " + transaction.Reference);
                Console.WriteLine("Date: " +
                                  transaction.TransactionDate.ToString("dd/MM/yyyy HH:mm"));
                Console.WriteLine("Status: " + transaction.Status);
            }
        }

        static void ChangePin()
        {
            Console.WriteLine("\n----- CHANGE PIN -----");

            Console.Write("Enter current PIN: ");
            string oldPin = Console.ReadLine();

            if (!currentAccount.VerifyPin(oldPin))
            {
                Console.WriteLine("Incorrect current PIN.");
                return;
            }

            Console.Write("Enter new PIN: ");
            string newPin = Console.ReadLine();

            if (newPin.Length != 4)
            {
                Console.WriteLine("PIN must contain exactly 4 digits.");
                return;
            }

            Console.Write("Confirm new PIN: ");
            string confirmPin = Console.ReadLine();

            if (newPin != confirmPin)
            {
                Console.WriteLine("PINs do not match.");
                return;
            }

            currentAccount.ChangePin(newPin);

            Console.WriteLine("PIN changed successfully.");
        }

        static bool IsValidPhoneNumber(int phoneNumber)
        {
            return phoneNumber >= 60000000 && phoneNumber <= 69999999;
        }
    }
}