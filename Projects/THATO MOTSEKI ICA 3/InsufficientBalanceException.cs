using System;

namespace EcoCashSimulation
{
    public class InsufficientBalanceException : Exception
    {
        public InsufficientBalanceException(string message)
            : base(message)
        {
        }
    }
}