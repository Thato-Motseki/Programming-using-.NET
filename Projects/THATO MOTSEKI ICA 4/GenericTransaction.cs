using System;

namespace EcoCashSimulation
{
    public class GenericTransaction : Transaction
    {
        public GenericTransaction(Account account, string transactionType, decimal amount, string reference, DateTime date, string status)
            : base(account, transactionType, amount, reference, date, status)
        {
        }

        public override void Process()
        {
            Status = "Completed";
        }
    }
}
