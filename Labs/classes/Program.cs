using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace classes
{
    internal class Program
    {
        static void Main(string[] args)
        {
        }
    }
    // C# Mini Project 1: Mobile Data Bundle System
// Suggested commit: feat: implement mobile data bundle purchasing

using System;
using System.Collections.Generic;

namespace MobileDataBundleSystem
    {
        class Customer
        {
            public string Name { get; private set; }
            public MobileAccount Account { get; private set; }

            public Customer(string name, string phoneNumber)
            {
                Name = name;
                Account = new MobileAccount(phoneNumber);
            }
        }

        class DataBundle
        {
            public string Name { get; private set; }
            public double Gigabytes { get; private set; }
            public decimal Price { get; private set; }

            public DataBundle(string name, double gigabytes, decimal price)
            {
                Name = name;
                Gigabytes = gigabytes;
                Price = price;
            }
        }

        class Purchase
        {
            public DataBundle Bundle { get; private set; }
            public DateTime PurchaseDate { get; private set; }

            public Purchase(DataBundle bundle)
            {
                Bundle = bundle;
                PurchaseDate = DateTime.Now;
            }
        }

        class MobileAccount
        {
            public string PhoneNumber { get; private set; }
            public decimal Balance { get; private set; }
            public double DataBalance { get; private set; }
            private List<Purchase> purchaseHistory = new List<Purchase>();

            public MobileAccount(string phoneNumber)
            {
                PhoneNumber = phoneNumber;
                Balance = 100m;
                DataBalance = 0;
            }

            public bool PurchaseBundle(DataBundle bundle)
            {
                if (Balance < bundle.Price)
                    return false;

                Balance -= bundle.Price;
                DataBalance += bundle.Gigabytes;
                purchaseHistory.Add(new Purchase(bundle));

                return true;
            }

            public bool UseData(double gigabytes)
            {
                if (gigabytes > DataBalance)
                    return false;

                DataBalance -= gigabytes;
                return true;
            }

            public void DisplayPurchaseHistory()
            {
                Console.WriteLine("\nPurchase History:");

                foreach (Purchase purchase in purchaseHistory)
                {
                    Console.WriteLine(
                        $"{purchase.Bundle.Name} - {purchase.Bundle.Gigabytes}GB - M{purchase.Bundle.Price:F2}"
                    );
                }
            }

            public void DisplayAccount()
            {
                Console.WriteLine($"\nPhone: {PhoneNumber}");
                Console.WriteLine($"Money Balance: M{Balance:F2}");
                Console.WriteLine($"Data Balance: {DataBalance:F2} GB");
            }
        }

        class Program
        {
            static void Main()
            {
                Customer customer = new Customer("Thato", "59000000");

                DataBundle bundle1 = new DataBundle("1GB Bundle", 1, 10m);
                DataBundle bundle2 = new DataBundle("5GB Bundle", 5, 40m);
                DataBundle bundle3 = new DataBundle("10GB Bundle", 10, 70m);

                customer.Account.DisplayAccount();

                if (customer.Account.PurchaseBundle(bundle2))
                    Console.WriteLine("\n5GB bundle purchased successfully.");
                else
                    Console.WriteLine("\nPurchase failed.");

                customer.Account.UseData(1.5);

                customer.Account.DisplayAccount();
                customer.Account.DisplayPurchaseHistory();
            }
        }
    }


    /*
    ===========================================================
    C# Mini Project 2: Parking Payment System
    Suggested commit:
    feat: implement parking ticket and payment calculation
    ===========================================================
    */

    namespace ParkingPaymentSystem
    {
        class ParkingTicket
        {
            public string TicketNumber { get; private set; }
            public DateTime EntryTime { get; private set; }
            public DateTime? ExitTime { get; private set; }

            public ParkingTicket(string ticketNumber)
            {
                TicketNumber = ticketNumber;
                EntryTime = DateTime.Now;
            }

            public void Exit()
            {
                ExitTime = DateTime.Now;
            }

            public double CalculateHours()
            {
                DateTime endTime = ExitTime ?? DateTime.Now;
                TimeSpan duration = endTime - EntryTime;

                return Math.Max(1, Math.Ceiling(duration.TotalHours));
            }

            public decimal CalculateFee(decimal hourlyRate)
            {
                return (decimal)CalculateHours() * hourlyRate;
            }
        }

        class Payment
        {
            public decimal Amount { get; private set; }
            public string PaymentMethod { get; private set; }
            public DateTime PaymentDate { get; private set; }

            public Payment(decimal amount, string paymentMethod)
            {
                Amount = amount;
                PaymentMethod = paymentMethod;
                PaymentDate = DateTime.Now;
            }

            public void DisplayReceipt(ParkingTicket ticket)
            {
                Console.WriteLine("\n===== PARKING RECEIPT =====");
                Console.WriteLine($"Ticket: {ticket.TicketNumber}");
                Console.WriteLine($"Entry: {ticket.EntryTime}");
                Console.WriteLine($"Exit: {ticket.ExitTime}");
                Console.WriteLine($"Hours: {ticket.CalculateHours()}");
                Console.WriteLine($"Payment Method: {PaymentMethod}");
                Console.WriteLine($"Amount Paid: M{Amount:F2}");
                Console.WriteLine("===========================");
            }
        }

        class ParkingSession
        {
            private ParkingTicket ticket;
            private decimal hourlyRate;

            public ParkingSession(string ticketNumber, decimal hourlyRate)
            {
                ticket = new ParkingTicket(ticketNumber);
                this.hourlyRate = hourlyRate;
            }

            public void EndSession()
            {
                ticket.Exit();

                decimal fee = ticket.CalculateFee(hourlyRate);

                Payment payment = new Payment(fee, "Card");
                payment.DisplayReceipt(ticket);
            }

            public ParkingTicket GetTicket()
            {
                return ticket;
            }
        }

        class Program
        {
            static void Main()
            {
                ParkingSession session =
                    new ParkingSession("PKG-1001", 10m);

                Console.WriteLine("Vehicle entered parking area.");
                Console.WriteLine($"Ticket: {session.GetTicket().TicketNumber}");

                Console.WriteLine("\nEnding parking session...");

                session.EndSession();
            }
        }
    }

}
