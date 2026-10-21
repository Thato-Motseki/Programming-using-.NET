namespace EcoCashSimulation
{
    public class SendMoneyTransaction : Transaction
    {
        private Account sender;
        private Account receiver;

        public SendMoneyTransaction(
            Account sender,
            Account receiver,
            decimal amount)
            : base(amount, "Send Money")
        {
            this.sender = sender;
            this.receiver = receiver;
        }

        public override void Process()
        {
            sender.RemoveMoney(Amount);
            receiver.AddMoney(Amount);

            Status = "Successful";
        }
    }
}