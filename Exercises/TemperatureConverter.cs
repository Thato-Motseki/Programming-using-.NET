using System;

class TemperatureConverter
{
    static void Main()
    {
        Console.WriteLine("=== Temperature Converter ===");

        Console.Write("Enter temperature in Celsius: ");
        double celsius = Convert.ToDouble(Console.ReadLine());

        double fahrenheit = (celsius * 9 / 5) + 32;

        Console.WriteLine($"Celsius: {celsius:F2}°C");
        Console.WriteLine($"Fahrenheit: {fahrenheit:F2}°F");
    }
}
