using System;

namespace MaseruGroceryBilling
{
    public class Product
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public Product(string name, decimal price, int quantity)
        {
            Name = name;
            Price = price;
            Quantity = quantity;
        }

        public decimal CalculateSubtotal()
        {
            return Price * Quantity;
        }
    }

    public static class BillingCalculator
    {
        public static decimal CalculateDiscount(decimal subtotal)
        {
            if (subtotal >= 500)
                return subtotal * 0.10m;

            if (subtotal >= 250)
                return subtotal * 0.05m;

            return 0m;
        }

        public static decimal CalculateTotal(decimal subtotal, decimal discount)
        {
            return subtotal - discount;
        }

        public static decimal CalculateChange(decimal cashTendered, decimal total)
        {
            return cashTendered - total;
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            Product[] products = new Product[5];
            int productCount = 0;
            bool running = true;

            Console.WriteLine("======================================");
            Console.WriteLine("       MASERU GROCERY BILLING");
            Console.WriteLine("======================================");

            while (running)
            {
                Console.WriteLine();
                Console.WriteLine("1. Add Product");
                Console.WriteLine("2. View Products");
                Console.WriteLine("3. Checkout");
                Console.WriteLine("4. Exit");
                Console.Write("Choose an option: ");

                int choice;

                if (!int.TryParse(Console.ReadLine(), out choice))
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        if (productCount >= products.Length)
                            Console.WriteLine("Product limit reached.");
                        else
                            AddProduct(products, ref productCount);
                        break;

                    case 2:
                        DisplayProducts(products, productCount);
                        break;

                    case 3:
                        Checkout(products, ref productCount);
                        break;

                    case 4:
                        running = false;
                        Console.WriteLine("Thank you for using Maseru Grocery Billing.");
                        break;

                    default:
                        Console.WriteLine("Please choose an option from 1 to 4.");
                        break;
                }
            }
        }

        public static void AddProduct(Product[] products, ref int productCount)
        {
            Console.WriteLine();
            Console.WriteLine("===== ADD PRODUCT =====");

            Console.Write("Enter product name: ");
            string name = Console.ReadLine();

            while (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Product name cannot be empty.");
                Console.Write("Enter product name: ");
                name = Console.ReadLine();
            }

            decimal price;

            while (true)
            {
                Console.Write("Enter price: M");

                if (decimal.TryParse(Console.ReadLine(), out price) && price > 0)
                    break;

                Console.WriteLine("Invalid price. Enter a positive number.");
            }

            int quantity;

            while (true)
            {
                Console.Write("Enter quantity: ");

                if (int.TryParse(Console.ReadLine(), out quantity) && quantity > 0)
                    break;

                Console.WriteLine("Invalid quantity. Enter a positive whole number.");
            }

            products[productCount] = new Product(name, price, quantity);
            productCount++;

            Console.WriteLine("Product added successfully.");
        }

        public static void DisplayProducts(Product[] products, int productCount)
        {
            Console.WriteLine();
            Console.WriteLine("========== PRODUCTS ==========");

            if (productCount == 0)
            {
                Console.WriteLine("No products have been added.");
                return;
            }

            decimal subtotal = 0m;

            for (int i = 0; i < products.Length; i++)
            {
                if (products[i] != null)
                {
                    decimal itemSubtotal = products[i].CalculateSubtotal();

                    Console.WriteLine(
                        (i + 1) + ". " +
                        products[i].Name +
                        " | Price: M" + products[i].Price.ToString("0.00") +
                        " | Quantity: " + products[i].Quantity +
                        " | Subtotal: M" + itemSubtotal.ToString("0.00")
                    );

                    subtotal += itemSubtotal;
                }
            }

            Console.WriteLine("-------------------------------");
            Console.WriteLine("Current Subtotal: M" + subtotal.ToString("0.00"));
        }

        public static void Checkout(Product[] products, ref int productCount)
        {
            Console.WriteLine();
            Console.WriteLine("========== CHECKOUT ==========");

            if (productCount == 0)
            {
                Console.WriteLine("There are no products to checkout.");
                return;
            }

            decimal subtotal = 0m;

            for (int i = 0; i < products.Length; i++)
            {
                if (products[i] != null)
                    subtotal += products[i].CalculateSubtotal();
            }

            decimal discount = BillingCalculator.CalculateDiscount(subtotal);
            decimal total = BillingCalculator.CalculateTotal(subtotal, discount);

            Console.WriteLine("Subtotal: M" + subtotal.ToString("0.00"));
            Console.WriteLine("Discount: M" + discount.ToString("0.00"));
            Console.WriteLine("Total:    M" + total.ToString("0.00"));

            decimal cashTendered;

            while (true)
            {
                Console.Write("Cash tendered: M");

                if (decimal.TryParse(Console.ReadLine(), out cashTendered))
                {
                    if (cashTendered >= total)
                        break;

                    Console.WriteLine(
                        "Insufficient cash. You need at least M" +
                        total.ToString("0.00") + "."
                    );
                }
                else
                {
                    Console.WriteLine("Invalid amount. Please enter a number.");
                }
            }

            decimal change = BillingCalculator.CalculateChange(
                cashTendered,
                total
            );

            Console.WriteLine();
            Console.WriteLine("========== RECEIPT ==========");
            Console.WriteLine("Subtotal:      M" + subtotal.ToString("0.00"));
            Console.WriteLine("Discount:      M" + discount.ToString("0.00"));
            Console.WriteLine("Amount Due:    M" + total.ToString("0.00"));
            Console.WriteLine("Cash Tendered: M" + cashTendered.ToString("0.00"));
            Console.WriteLine("Change:        M" + change.ToString("0.00"));
            Console.WriteLine("=============================");
            Console.WriteLine("Transaction completed successfully.");

            for (int i = 0; i < products.Length; i++)
                products[i] = null;

            productCount = 0;
        }
    }
}
