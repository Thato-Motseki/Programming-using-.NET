namespace EcoCashSimulation
{
    public class DepositTransaction : Transaction
    {
        private Account account;

        public DepositTransaction(Account account, decimal amount)
            : base(amount, "Deposit")
        {
            this.account = account;
        }

        public override void Process()
        {
            account.AddMoney(Amount);

            Status = "Successful";
        }
    }
}