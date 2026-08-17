using System;

class RestaurantOrderingSystem
{
    static void Main()
    {
        string[] menuItems =
        {
            "Burger",
            "Pizza",
            "Fries",
            "Chicken Wings",
            "Soft Drink"
        };

        double[] prices = { 55.00, 85.00, 30.00, 65.00, 20.00 };

        double subtotal = 0;
        bool ordering = true;

        Console.WriteLine("=== RESTAURANT MENU ===");

        for (int i = 0; i < menuItems.Length; i++)
        {
            Console.WriteLine($"{i + 1}. {menuItems[i]} - M{prices[i]:F2}");
        }

        while (ordering)
        {
            Console.Write("\nEnter item number (0 to finish): ");
            int choice = Convert.ToInt32(Console.ReadLine());

            if (choice == 0)
            {
                ordering = false;
            }
            else if (choice >= 1 && choice <= menuItems.Length)
            {
                Console.Write($"Enter quantity of {menuItems[choice - 1]}: ");
                int quantity = Convert.ToInt32(Console.ReadLine());

                if (quantity > 0)
                {
                    double itemTotal = prices[choice - 1] * quantity;
                    subtotal += itemTotal;

                    Console.WriteLine(
                        $"{quantity} x {menuItems[choice - 1]} = M{itemTotal:F2}");
                }
                else
                {
                    Console.WriteLine("Quantity must be greater than zero.");
                }
            }
            else
            {
                Console.WriteLine("Invalid menu choice.");
            }
        }

        double discount = 0;

        if (subtotal >= 300)
        {
            discount = subtotal * 0.10;
        }

        double finalTotal = subtotal - discount;

        Console.WriteLine("\n=== FINAL BILL ===");
        Console.WriteLine($"Subtotal: M{subtotal:F2}");
        Console.WriteLine($"Discount: M{discount:F2}");
        Console.WriteLine($"Total: M{finalTotal:F2}");

        if (discount > 0)
            Console.WriteLine("10% discount applied!");
        else
            Console.WriteLine("Spend M300 or more to receive a 10% discount.");
    }
}
