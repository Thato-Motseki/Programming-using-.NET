using System.Collections.Generic;
using System.Data.Common;

namespace EcoCashSimulation
{
    public class Account
    {
        private decimal balance;
        private string pin;
        private List<Transaction> transactions;

        public int PhoneNumber { get; private set; }
        public string Name { get; private set; }

        public decimal Balance
        {
            get { return balance; }
        }

        public Account(int phoneNumber, string name, string pin, decimal balance)
        {
            PhoneNumber = phoneNumber;
            Name = name;
            this.pin = pin;
            this.balance = balance;

            transactions = new List<Transaction>();
        }

        public bool VerifyPin(string enteredPin)
        {
            return enteredPin == pin;
        }

        public void ChangePin(string newPin)
        {
            pin = newPin;
        }

        public void AddMoney(decimal amount)
        {
            balance += amount;
        }

        public void RemoveMoney(decimal amount)
        {
            if (amount > balance)
            {
                throw new InsufficientBalanceException(
                    "Insufficient balance. Available balance is M " +
                    balance.ToString("0.00"));
            }

            balance -= amount;
        }

        public void AddTransaction(Transaction transaction)
        {
            transactions.Add(transaction);
        }

        public List<Transaction> GetTransactions()
        {
            return transactions;
        }
    }
}