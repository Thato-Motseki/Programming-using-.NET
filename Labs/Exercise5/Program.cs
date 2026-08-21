

using System;

namespace RoutePulseMaseru
{
    // A. Passenger Class
    public class Passenger
    {
        public string Name { get; set; }
        public string DestinationZone { get; set; }
        public decimal CashTendered { get; set; }
        public bool IsStudent { get; set; }

        public Passenger(string name, string destinationZone, decimal cashTendered, bool isStudent)
        {
            Name = name;
            DestinationZone = destinationZone;
            CashTendered = cashTendered;
            IsStudent = isStudent;
        }
    }

    // B. TaxiRoute Class
    public class TaxiRoute
    {
        public string RouteName { get; set; }
        public decimal BaseFare { get; set; }
        public int MaxCapacity { get; set; }
        public Passenger[] PassengerManifest { get; set; }
        public decimal TotalShiftEarnings { get; set; }

        public TaxiRoute(string routeName, decimal baseFare)
        {
            RouteName = routeName;
            BaseFare = baseFare;
            MaxCapacity = 4;
            PassengerManifest = new Passenger[MaxCapacity];
            TotalShiftEarnings = 0m;
        }

        public bool AddPassenger(Passenger commuter)
        {
            for (int i = 0; i < PassengerManifest.Length; i++)
            {
                if (PassengerManifest[i] == null)
                {
                    PassengerManifest[i] = commuter;
                    return true;
                }
            }

            return false;
        }

        public void DisplayManifest()
        {
            Console.WriteLine();
            Console.WriteLine("===== TAXI MANIFEST =====");
            Console.WriteLine("Route: " + RouteName);
            Console.WriteLine("Base Fare: M" + BaseFare.ToString("0.00"));
            Console.WriteLine();

            for (int i = 0; i < PassengerManifest.Length; i++)
            {
                Console.Write("Seat " + (i + 1) + ": ");

                if (PassengerManifest[i] != null)
                {
                    Passenger passenger = PassengerManifest[i];

                    decimal finalFare = FareCalculator.CalculateFare(
                        BaseFare,
                        passenger.IsStudent
                    );

                    Console.WriteLine(
                        passenger.Name +
                        " | " +
                        passenger.DestinationZone +
                        " | Fare Paid: M" +
                        finalFare.ToString("0.00")
                    );
                }
                else
                {
                    Console.WriteLine("[EMPTY]");
                }
            }

            Console.WriteLine();
            Console.WriteLine("Current Shift Earnings: M" +
                              TotalShiftEarnings.ToString("0.00"));
            Console.WriteLine("=========================");
        }
    }

    // C. FareCalculator Class
    public static class FareCalculator
    {
        public static decimal CalculateFare(decimal baseFare, bool isStudent)
        {
            if (isStudent)
            {
                return baseFare * 0.85m;
            }

            return baseFare;
        }

        public static decimal CalculateChange(decimal cashTendered, decimal finalFare)
        {
            return cashTendered - finalFare;
        }

        public static bool ProcessPayment(
            string cashInput,
            decimal requiredFare,
            out decimal cashTendered)
        {
            if (!decimal.TryParse(cashInput, out cashTendered))
            {
                Console.WriteLine("Invalid amount. Please enter a valid number.");
                return false;
            }

            if (cashTendered < requiredFare)
            {
                Console.WriteLine(
                    "Insufficient cash. Required: M" +
                    requiredFare.ToString("0.00") +
                    " | Tendered: M" +
                    cashTendered.ToString("0.00")
                );

                return false;
            }

            return true;
        }
    }

    // D. Program Class
    public class Program
    {
        public static void Main(string[] args)
        {
            TaxiRoute taxi = new TaxiRoute(
                "Maseru Mall to Roma (NUL Campus)",
                15.50m
            );

            bool running = true;

            Console.WriteLine("==================================");
            Console.WriteLine("       ROUTE-PULSE MASERU");
            Console.WriteLine("   Taxi Fare & Passenger Tracker");
            Console.WriteLine("==================================");

            while (running)
            {
                Console.WriteLine();
                Console.WriteLine("========== MAIN MENU ==========");
                Console.WriteLine("1. Board New Passenger");
                Console.WriteLine("2. View Current Taxi Manifest");
                Console.WriteLine("3. Dispatch Taxi & Calculate Earnings");
                Console.WriteLine("4. Exit System");
                Console.WriteLine("===============================");

                Console.Write("Enter your choice: ");
                string choiceInput = Console.ReadLine();

                int choice;

                if (!int.TryParse(choiceInput, out choice))
                {
                    Console.WriteLine("Invalid choice. Please enter a number from 1 to 4.");
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        BoardPassenger(taxi);
                        break;

                    case 2:
                        taxi.DisplayManifest();
                        break;

                    case 3:
                        DispatchTaxi(taxi);
                        break;

                    case 4:
                        Console.WriteLine("Exiting Route-Pulse Maseru...");
                        running = false;
                        break;

                    default:
                        Console.WriteLine("Invalid choice. Please select 1, 2, 3, or 4.");
                        break;
                }
            }

            Console.WriteLine("Thank you for using Route-Pulse Maseru.");
        }

        public static void BoardPassenger(TaxiRoute taxi)
        {
            Console.WriteLine();
            Console.WriteLine("===== BOARD PASSENGER =====");

            // Check whether the taxi is already full before collecting passenger data.
            bool taxiFull = true;

            for (int i = 0; i < taxi.PassengerManifest.Length; i++)
            {
                if (taxi.PassengerManifest[i] == null)
                {
                    taxiFull = false;
                    break;
                }
            }

            if (taxiFull)
            {
                Console.WriteLine("Taxi is full. No more passengers can be boarded.");
                return;
            }

            Console.Write("Enter passenger name: ");
            string name = Console.ReadLine();

            while (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Name cannot be empty.");
                Console.Write("Enter passenger name: ");
                name = Console.ReadLine();
            }

            Console.WriteLine();
            Console.WriteLine("Destination Zones:");
            Console.WriteLine("1. Zone 1 - Local");
            Console.WriteLine("2. Zone 2 - Inter-District");

            Console.Write("Select destination zone: ");
            string zoneInput = Console.ReadLine();

            int zoneChoice;

            while (!int.TryParse(zoneInput, out zoneChoice) ||
                   (zoneChoice != 1 && zoneChoice != 2))
            {
                Console.WriteLine("Invalid zone. Please select 1 or 2.");
                Console.Write("Select destination zone: ");
                zoneInput = Console.ReadLine();
            }

            string destinationZone;

            if (zoneChoice == 1)
            {
                destinationZone = "Zone 1 - Local";
            }
            else
            {
                destinationZone = "Zone 2 - Inter-District";
            }

            Console.Write("Is the passenger a student? (yes/no): ");
            string studentInput = Console.ReadLine();

            bool isStudent;

            while (!TryConvertToBoolean(studentInput, out isStudent))
            {
                Console.WriteLine("Invalid response. Please enter yes or no.");
                Console.Write("Is the passenger a student? (yes/no): ");
                studentInput = Console.ReadLine();
            }

            decimal finalFare = FareCalculator.CalculateFare(
                taxi.BaseFare,
                isStudent
            );

            Console.WriteLine();
            Console.WriteLine("Required Fare: M" + finalFare.ToString("0.00"));

            decimal cashTendered;
            bool paymentSuccessful = false;

            do
            {
                Console.Write("Enter cash tendered: ");
                string cashInput = Console.ReadLine();

                paymentSuccessful = FareCalculator.ProcessPayment(
                    cashInput,
                    finalFare,
                    out cashTendered
                );
            }
            while (!paymentSuccessful);

            decimal change = FareCalculator.CalculateChange(
                cashTendered,
                finalFare
            );

            Passenger passenger = new Passenger(
                name,
                destinationZone,
                cashTendered,
                isStudent
            );

            bool added = taxi.AddPassenger(passenger);

            if (added)
            {
                taxi.TotalShiftEarnings += finalFare;

                Console.WriteLine();
                Console.WriteLine("Passenger boarded successfully.");
                Console.WriteLine("Passenger: " + name);
                Console.WriteLine("Destination: " + destinationZone);
                Console.WriteLine("Student Fare: " + (isStudent ? "Yes" : "No"));
                Console.WriteLine("Fare: M" + finalFare.ToString("0.00"));
                Console.WriteLine("Cash Tendered: M" + cashTendered.ToString("0.00"));
                Console.WriteLine("Change: M" + change.ToString("0.00"));
            }
            else
            {
                Console.WriteLine("Taxi is full. Passenger could not be boarded.");
            }
        }

        public static bool TryConvertToBoolean(string input, out bool result)
        {
            if (input != null)
            {
                if (input.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase))
                {
                    result = true;
                    return true;
                }

                if (input.Trim().Equals("no", StringComparison.OrdinalIgnoreCase))
                {
                    result = false;
                    return true;
                }
            }

            result = false;
            return false;
        }

        public static void DispatchTaxi(TaxiRoute taxi)
        {
            Console.WriteLine();
            Console.WriteLine("===== DISPATCH TAXI =====");

            int passengerCount = 0;

            for (int i = 0; i < taxi.PassengerManifest.Length; i++)
            {
                if (taxi.PassengerManifest[i] != null)
                {
                    passengerCount++;
                }
            }

            if (passengerCount == 0)
            {
                Console.WriteLine("No passengers have been boarded.");
                return;
            }

            Console.WriteLine("Route: " + taxi.RouteName);
            Console.WriteLine("Passengers: " + passengerCount + "/" + taxi.MaxCapacity);
            Console.WriteLine("Total Shift Earnings: M" +
                              taxi.TotalShiftEarnings.ToString("0.00"));

            Console.WriteLine();
            Console.WriteLine("Taxi dispatched successfully.");
            Console.WriteLine("Shift earnings recorded.");

            // Reset the fixed manifest for a new shift.
            for (int i = 0; i < taxi.PassengerManifest.Length; i++)
            {
                taxi.PassengerManifest[i] = null;
            }

            taxi.TotalShiftEarnings = 0m;

            Console.WriteLine("Taxi is now ready for a new shift.");
        }
    }
}