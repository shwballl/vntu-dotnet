using System;
using System.Collections.Generic;

public enum ShipmentStatus { Created, InTransit, Arrived, Received }

public interface IShipment
{
    string TrackingNumber { get; }
    decimal WeightKg { get; }
    ShipmentStatus Status { get; set; }
}

public class Parcel : IShipment
{
    public string TrackingNumber { get; }
    public decimal WeightKg { get; }
    public ShipmentStatus Status { get; set; }
    public string Recipient { get; }

    public Parcel(string trackingNumber, decimal weightKg, string recipient)
    {
        TrackingNumber = trackingNumber;
        WeightKg = weightKg;
        Recipient = recipient;
        Status = ShipmentStatus.Created;
    }
}

public class StatusChangedEventArgs<T> : EventArgs
{
    public T Item { get; }
    public ShipmentStatus OldStatus { get; }
    public ShipmentStatus NewStatus { get; }

    public StatusChangedEventArgs(T item, ShipmentStatus oldStatus, ShipmentStatus newStatus)
    {
        Item = item;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }
}

public class Tracker<T> where T : class, IShipment
{
    private readonly List<T> _items = new();

    private Func<T, decimal>? _tariff;
    public event EventHandler<StatusChangedEventArgs<T>>? StatusChanged;

    public void Add(T item) => _items.Add(item);

    public bool AdvanceStatus(string trackingNumber)
    {
        var item = _items.Find(x => x.TrackingNumber == trackingNumber);
        if (item == null) return false;
        if (item.Status == ShipmentStatus.Received) return true; 

        var oldStatus = item.Status;
        item.Status = (ShipmentStatus)((int)item.Status + 1);

        OnStatusChanged(new StatusChangedEventArgs<T>(item, oldStatus, item.Status));
        return true;
    }

    public decimal GetCost(T item) => _tariff?.Invoke(item) ?? (item.WeightKg * 40m);

    public void ForEach(Action<T> action)
    {
        foreach (var item in _items) action(item);
    }

    public void SetTariff(Func<T, decimal>? tariff) => _tariff = tariff;

    protected virtual void OnStatusChanged(StatusChangedEventArgs<T> e) => StatusChanged?.Invoke(this, e);
}

class Program
{
    static List<string> history = new();

    static void Main()
    {
        var tracker = new Tracker<Parcel>();

        tracker.Add(new Parcel("UA-1001", 2.5m, "Олена"));
        tracker.Add(new Parcel("UA-1002", 0.8m, "Іван"));
        tracker.StatusChanged += LogToHistory;

        tracker.StatusChanged += (sender, e) =>
        {
            if (e.Item is Parcel p)
            {
                Console.WriteLine($"\n[SMS для {p.Recipient}]: Ваша посилка {p.TrackingNumber} змінила статус на {e.NewStatus}!");
                if (e.NewStatus == ShipmentStatus.Arrived)
                {
                    Console.WriteLine($"[SMS]: Код отримання: {Random.Shared.Next(1000, 9999)}");
                }
            }
        };

        var tariffs = new Dictionary<string, Func<Parcel, decimal>>
        {
            ["фіксований"] = p => 70m,
            ["експрес"] = p => 100m + p.WeightKg * 60m,
            ["акційний"] = p => p.WeightKg * 40m * 0.8m
        };

        bool isRunning = true;
        string currentTariffName = "стандартний";

        while (isRunning)
        {
            Console.WriteLine($"\n=== Меню (Поточний тариф: {currentTariffName}) ===");
            Console.WriteLine("0. Вихід");
            Console.WriteLine("1. Додати посилку");
            Console.WriteLine("2. Показати посилки");
            Console.WriteLine("3. Оновити статус посилки");
            Console.WriteLine("4. Обрати тариф (стандартний/фіксований/експрес/акційний)");
            Console.WriteLine("5. Показати історію статусів");

            Console.Write("Ваш вибір: ");
            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.Write("Номер (напр. UA-1003): ");
                    string num = Console.ReadLine() ?? "N/A";
                    Console.Write("Одержувач: ");
                    string rec = Console.ReadLine() ?? "N/A";
                    Console.Write("Вага (кг): ");
                    if (decimal.TryParse(Console.ReadLine(), out decimal w))
                    {
                        tracker.Add(new Parcel(num, w, rec));
                        Console.WriteLine("Посилку додано.");
                    }
                    else Console.WriteLine("Помилка вводу ваги.");
                    break;
                case "2":
                    tracker.ForEach(p => Console.WriteLine($"- {p.TrackingNumber} | Одержувач: {p.Recipient} | Вага: {p.WeightKg} кг | Статус: {p.Status} | Вартість: {tracker.GetCost(p):F2} грн"));
                    break;
                case "3":
                    Console.Write("Введіть номер посилки: ");
                    string tNum = Console.ReadLine() ?? "";
                    if (!tracker.AdvanceStatus(tNum))
                    {
                        Console.WriteLine("Посилку не знайдено, або статус вже Received.");
                    }
                    break;
                case "4":
                    Console.Write("Введіть назву тарифу: ");
                    string tName = Console.ReadLine()?.ToLower() ?? "";
                    if (tariffs.TryGetValue(tName, out var newTariff))
                    {
                        tracker.SetTariff(newTariff);
                        currentTariffName = tName;
                        Console.WriteLine($"Тариф змінено на '{tName}'.");
                    }
                    else if (tName == "стандартний")
                    {
                        tracker.SetTariff(null);
                        currentTariffName = "стандартний";
                        Console.WriteLine("Встановлено стандартний тариф.");
                    }
                    else
                    {
                        Console.WriteLine("Невідомий тариф.");
                    }
                    break;
                case "5":
                    Console.WriteLine("Історія змін:");
                    foreach (var h in history) Console.WriteLine(h);
                    break;
                case "0":
                    isRunning = false;
                    break;
                default:
                    Console.WriteLine("Некоректний вибір.");
                    break;
            }
        }
    }

    static void LogToHistory(object? sender, StatusChangedEventArgs<Parcel> e)
    {
        history.Add($"[{DateTime.Now:HH:mm:ss}] {e.Item.TrackingNumber}: {e.OldStatus} -> {e.NewStatus}");
    }
}