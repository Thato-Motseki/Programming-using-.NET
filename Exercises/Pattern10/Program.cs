using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pattern_10
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int rows = 5;

            // Upper half
            for (int i = 1; i <= rows; i++)
            {
                // Print spaces
                for (int j = 1; j <= rows - i; j++)
                {
                    Console.Write(" ");
                }

                // Print stars
                for (int j = 1; j <= (2 * i - 1); j++)
                {
                    Console.Write("*");
                }

                Console.WriteLine();
            }

            // Lower half
            for (int i = rows - 1; i >= 1; i--)
            {
                // Print spaces
                for (int j = 1; j <= rows - i; j++)
                {
                    Console.Write(" ");
                }

                // Print stars
                for (int j = 1; j <= (2 * i - 1); j++)
                {
                    Console.Write("*");
                }

                Console.WriteLine();
            }
        }
    }
}
