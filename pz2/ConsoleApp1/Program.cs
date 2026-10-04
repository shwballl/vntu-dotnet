using System;
using System.Collections.Generic;

namespace CompanyEmployeesApp
{
    public class InvalidSalaryException : Exception
    {
        public InvalidSalaryException() { }
        public InvalidSalaryException(string message) : base(message) { }
        public InvalidSalaryException(string message, Exception inner) : base(message, inner) { }
    }

    public enum DeveloperLevel
    {
        Junior = 1,
        Middle = 2,
        Senior = 3
    }

    public abstract class Employee : IComparable<Employee>
    {
        private string _fullName = string.Empty;
        private double _baseSalary;

        public string FullName
        {
            get => _fullName;
            set => _fullName = string.IsNullOrWhiteSpace(value)
                ? throw new ArgumentException("Ім'я не може бути порожнім.")
                : value;
        }

        public double BaseSalary
        {
            get => _baseSalary;
            set
            {
                if (value <= 0)
                {
                    throw new InvalidSalaryException("Базова зарплата повинна бути більшою за нуль.");
                }
                _baseSalary = value;
            }
        }

        protected Employee(string fullName, double baseSalary)
        {
            FullName = fullName;
            BaseSalary = baseSalary;
        }

        public virtual double CalculateSalary() => BaseSalary;

        public virtual void PrintInfo()
        {
            Console.WriteLine($"{FullName} | Базова ставка: {BaseSalary:C2} | До виплати: {CalculateSalary():C2}");
        }

        public int CompareTo(Employee? other)
        {
            if (other == null) return 1;
            return other.CalculateSalary().CompareTo(this.CalculateSalary());
        }
    }

    public class Manager : Employee
    {
        private int _subordinatesCount;
        private const double BonusPerSubordinate = 1500.0; 

        public int SubordinatesCount
        {
            get => _subordinatesCount;
            set => _subordinatesCount = value >= 0 ? value : throw new ArgumentException("Кількість підлеглих не може бути від'ємною.");
        }

        public Manager(string fullName, double baseSalary, int subordinatesCount)
            : base(fullName, baseSalary)
        {
            SubordinatesCount = subordinatesCount;
        }

        public override double CalculateSalary()
        {
            return base.CalculateSalary() + (SubordinatesCount * BonusPerSubordinate);
        }

        public override void PrintInfo()
        {
            Console.Write("[Менеджер] ");
            base.PrintInfo();
            Console.WriteLine($"    └ Підлеглих: {SubordinatesCount} (Бонус: {SubordinatesCount * BonusPerSubordinate:C2})");
        }
    }

    public class Developer : Employee
    {
        public DeveloperLevel Level { get; set; }

        public Developer(string fullName, double baseSalary, DeveloperLevel level)
            : base(fullName, baseSalary)
        {
            Level = level;
        }

        public override double CalculateSalary()
        {
            double levelBonus = Level switch
            {
                DeveloperLevel.Junior => 0,
                DeveloperLevel.Middle => 5000,
                DeveloperLevel.Senior => 10000,
                _ => 0
            };
            return base.CalculateSalary() + levelBonus;
        }

        public override void PrintInfo()
        {
            Console.Write($"[Розробник {Level}] ");
            base.PrintInfo();
        }
    }

    internal class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            List<Employee> employees = new List<Employee>
            {
                new Manager("Іван Коваленко", 20000, 5),
                new Developer("Олена Петрівна", 18000, DeveloperLevel.Middle),
                new Developer("Олександр Сидорчук", 25000, DeveloperLevel.Senior)
            };

            bool isRunning = true;
            while (isRunning)
            {
                Console.WriteLine("\n=== Співробітники компанії ===");
                Console.WriteLine("1. Додати менеджера");
                Console.WriteLine("2. Додати розробника");
                Console.WriteLine("3. Показати список співробітників");
                Console.WriteLine("4. Сумарний фонд заробітної плати");
                Console.WriteLine("5. Відсортувати за зарплатою (спадання)");
                Console.WriteLine("0. Вихід");

                int choice = ReadInt("Ваш вибір: ", 0, 5);

                isRunning = choice switch
                {
                    1 => HandleAddManager(employees),
                    2 => HandleAddDeveloper(employees),
                    3 => HandleShowAll(employees),
                    4 => HandleShowTotalFund(employees),
                    5 => HandleSortEmployees(employees),
                    0 => false,
                    _ => true
                };
            }
            Console.WriteLine("\nРоботу завершено.");
        }

        static bool HandleAddManager(List<Employee> employees)
        {
            Console.WriteLine("\n--- Додавання менеджера ---");
            try
            {
                string name = ReadString("Введіть ПІБ: ");
                double salary = ReadDouble("Базова зарплата: ");
                int subs = ReadInt("Кількість підлеглих: ", 0, 1000);

                Manager newManager = new Manager(name, salary, subs);
                employees.Add(newManager);
                Console.WriteLine("Менеджера успішно додано.");
            }
            catch (InvalidSalaryException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Помилка валідації зарплати: {ex.Message}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Непередбачена помилка: {ex.Message}");
                Console.ResetColor();
            }
            finally
            {
                Console.WriteLine("Операцію додавання завершено (finally блок).");
            }
            return true;
        }

        static bool HandleAddDeveloper(List<Employee> employees)
        {
            Console.WriteLine("\n--- Додавання розробника ---");
            try
            {
                string name = ReadString("Введіть ПІБ: ");
                double salary = ReadDouble("Базова зарплата: ");

                Console.WriteLine("Оберіть рівень (1 - Junior, 2 - Middle, 3 - Senior):");
                int levelInt = ReadInt("Рівень: ", 1, 3);
                DeveloperLevel level = (DeveloperLevel)levelInt;

                Developer newDev = new Developer(name, salary, level);
                employees.Add(newDev);
                Console.WriteLine("Розробника успішно додано.");
            }
            catch (InvalidSalaryException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Помилка валідації зарплати: {ex.Message}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Непередбачена помилка: {ex.Message}");
                Console.ResetColor();
            }
            finally
            {
                Console.WriteLine("Операцію додавання завершено (finally блок).");
            }
            return true;
        }

        static bool HandleShowAll(List<Employee> employees)
        {
            Console.WriteLine("\n--- Список співробітників ---");
            if (employees.Count == 0)
            {
                Console.WriteLine("Список порожній.");
                return true;
            }

            foreach (Employee emp in employees)
            {
                emp.PrintInfo();
            }
            return true;
        }

        static bool HandleShowTotalFund(List<Employee> employees)
        {
            double total = 0;
            foreach (var emp in employees)
            {
                total += emp.CalculateSalary();
            }
            Console.WriteLine($"\nСумарний фонд заробітної плати: {total:C2}");
            return true;
        }

        static bool HandleSortEmployees(List<Employee> employees)
        {
            employees.Sort();
            Console.WriteLine("\nСпівробітників відсортовано за зарплатою (від найбільшої).");
            HandleShowAll(employees);
            return true;
        }
        static string ReadString(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input)) return input;
                Console.WriteLine("Рядок не може бути порожнім.");
            }
        }

        static double ReadDouble(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (double.TryParse(Console.ReadLine()?.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double value))
                    return value;
                Console.WriteLine("Введіть коректне число.");
            }
        }

        static int ReadInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int value) && value >= min && value <= max)
                    return value;
                Console.WriteLine($"Введіть ціле число від {min} до {max}.");
            }
        }
    }
}
