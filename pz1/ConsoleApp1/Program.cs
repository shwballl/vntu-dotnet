using System;
using System.Globalization;

namespace UnitConverterApp
{
    internal class Program
    {
        private const double KmPerMile = 1.609344;
        private const double KgPerPound = 0.45359237;
        private const double AbsoluteZeroCelsius = -273.15;
        private const double AbsoluteZeroFahrenheit = -459.67;

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            bool isRunning = true;

            while (isRunning)
            {
                PrintMenu();
                int choice = ReadInt("Ваш вибір: ", 0, 6);

                isRunning = choice switch
                {
                    1 => HandleConversion("км", "милі", KilometersToMiles, minValidValue: 0),
                    2 => HandleConversion("милі", "км", MilesToKilometers, minValidValue: 0),
                    3 => HandleConversion("кг", "фунти", KilogramsToPounds, minValidValue: 0),
                    4 => HandleConversion("фунти", "кг", PoundsToKilograms, minValidValue: 0),
                    5 => HandleConversion("°C", "°F", CelsiusToFahrenheit, minValidValue: AbsoluteZeroCelsius),
                    6 => HandleConversion("°F", "°C", FahrenheitToCelsius, minValidValue: AbsoluteZeroFahrenheit),
                    0 => false,
                    _ => true
                };
            }

            Console.WriteLine("\nПрограму завершено. Дякуємо за використання!");
        }

        static void PrintMenu()
        {
            Console.WriteLine("\n================== КОНВЕРТЕР ВЕЛИЧИН ==================");
            Console.WriteLine("1. км → милі");
            Console.WriteLine("2. милі → км");
            Console.WriteLine("3. кг → фунти");
            Console.WriteLine("4. фунти → кг");
            Console.WriteLine("5. °C → °F");
            Console.WriteLine("6. °F → °C");
            Console.WriteLine("0. Вихід");
            Console.WriteLine("=======================================================");
        }

        static bool HandleConversion(
            string fromUnit,
            string toUnit,
            Func<double, double> convertFunc,
            double minValidValue)
        {
            Console.WriteLine($"\n--- Конвертація: {fromUnit} → {toUnit} ---");
            double input = ReadDouble($"Введіть значення ({fromUnit}): ");

            if (input < minValidValue)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[Помилка] Фізично неможливе значення! Для {fromUnit} значення не може бути меншим за {minValidValue:F2}.");
                Console.ResetColor();
                return true;
            }

            double result = convertFunc(input);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Результат: {input:F2} {fromUnit} = {result:F2} {toUnit}");
            Console.ResetColor();

            return true;
        }

        static double KilometersToMiles(double km) => km / KmPerMile;
        static double MilesToKilometers(double miles) => miles * KmPerMile;

        static double KilogramsToPounds(double kg) => kg / KgPerPound;
        static double PoundsToKilograms(double pounds) => pounds * KgPerPound;

        static double CelsiusToFahrenheit(double celsius) => (celsius * 9.0 / 5.0) + 32.0;
        static double FahrenheitToCelsius(double fahrenheit) => (fahrenheit - 32.0) * 5.0 / 9.0;

        static int ReadInt(string prompt, int min = int.MinValue, int max = int.MaxValue)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();

                if (int.TryParse(input, out int value) && value >= min && value <= max)
                {
                    return value;
                }

                Console.WriteLine($"Некоректне значення. Введіть ціле число від {min} до {max}.");
            }
        }

        static double ReadDouble(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();

                if (input != null)
                {
                    input = input.Trim().Replace(',', '.');

                    if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                    {
                        return value;
                    }
                }

                Console.WriteLine("Некоректний ввід. Будь ласка, введіть дійсне число.");
            }
        }
    }
}