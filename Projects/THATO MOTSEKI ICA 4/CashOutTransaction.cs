namespace EcoCashSimulation
{
    public class CashOutTransaction : Transaction
    {
        private Account account;

        public CashOutTransaction(Account account, decimal amount)
            : base(amount, "Cash Out")
        {
            this.account = account;
        }

        public override void Process()
        {
            account.RemoveMoney(Amount);

            Status = "Successful";
        }
    }
}