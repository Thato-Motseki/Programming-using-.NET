using System;

namespace EcoCashSimulation
{
    public abstract class Transaction
    {
        public string Reference { get; protected set; }
        public decimal Amount { get; protected set; }
        public DateTime TransactionDate { get; protected set; }
        public string Status { get; protected set; }
        public string TransactionType { get; protected set; }
        public int DatabaseId { get; set; }

        public Transaction(decimal amount, string transactionType)
        {
            Amount = amount;
            TransactionType = transactionType;
            TransactionDate = DateTime.Now;
            Reference = GenerateReference();
            Status = "Pending";
            DatabaseId = 0;
        }

        public Transaction(Account account, string transactionType, decimal amount, string reference, DateTime date, string status)
        {
            Amount = amount;
            TransactionType = transactionType;
            TransactionDate = date;
            Reference = reference;
            Status = status;
            DatabaseId = 0;
        }

        public abstract void Process();

        private string GenerateReference()
        {
            return "EC" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }
    }
}
