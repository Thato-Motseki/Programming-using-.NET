using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pattern_4
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int rows = 5;

            for (int i = 1; i <= rows; i++)
            {
                // Print leading spaces
                for (int j = 1; j <= rows - i; j++)
                {
                    Console.Write("  ");
                }

                // Print stars
                for (int j = 1; j <= i; j++)
                {
                    Console.Write("* ");
                }

                Console.WriteLine();
            }
        }
    }
}
