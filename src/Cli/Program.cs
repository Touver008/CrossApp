using Core;
using Core.Abstractions;
using Core.Events;
using Core.Services;
using Core.Storage;

bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

ICatalogStore store = useFile
    ? new FileCatalogStore(dataPath)
    : new InMemoryCatalogStore(SampleData.Products());

var service = new CatalogService(store);

// Налаштування лог-файлу в каталозі виконання
var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

// --- Підписник 1: Локальний метод (для можливості подальшої відписки) ---
void OnCatalogChanged(object? sender, CatalogChangedEventArgs e)
{
    string line = $"{e.At:HH:mm:ss} {e.Kind,-8} {e.Id} {e.Delta,4:+0;-0} залишок {e.QuantityAfter}";
    Console.WriteLine(line);
    File.AppendAllText(logPath, line + Environment.NewLine);
}

service.Changed += OnCatalogChanged;

// --- Підписник 2: Лямбда-вираз із замиканням зовнішньої змінної (лічильника) ---
int changesCount = 0;
service.Changed += (sender, e) => changesCount++;

Console.WriteLine($"=== Сховище: {store.GetType().Name} ===");
Console.WriteLine("Поточних подій до операцій: " + changesCount);
Console.WriteLine();

// --- Сценарій виконання та перевірка подій ---
Console.WriteLine("--- Сценарій успішних операцій ---");
var created = service.Add("SKU-101", "Кабель UTP cat6", "м", 50);
service.Receive(created.Id, 25);

Console.WriteLine($"\n[Лічильник] Отримано подій (очікується 2 виклики для кожної події): {changesCount}");

// --- Перевірка відписки ---
Console.WriteLine("\n--- Перевірка відписки локального методу ---");
service.Changed -= OnCatalogChanged; // Відписуємо вивід у консоль і файл

service.Receive(created.Id, 5); // Ця операція буде «тихою» для консолі/файлу, але піймається лічильником
Console.WriteLine($"[Лічильник] Після відписки і ще однієї зміни: {changesCount} (+1 лише від лямбди)");

// --- Сценарій відмови (інваріанти) ---
Console.WriteLine("\n=== Сценарій відмови (помилки не повинні генерувати події) ===");
TryDo("Видача більша за залишок", () => service.Issue(created.Id, 9999));
Console.WriteLine($"[Лічильник] Після невдалої операції (має залишитись незмінним): {changesCount}");

// --- Використання Func замість події ---
Console.WriteLine("\n--- Використання делегата Func (фільтрація товарів із залишком < 20) ---");
var lowStockProducts = service.FindAll(p => p.Quantity < 20);
foreach (var p in lowStockProducts)
{
    Console.WriteLine($" [Низький залишок] {p.Id} {p.Sku} — {p.Quantity} {p.Unit}");
}

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($" ! {title}: виняток НЕ спрацював");
    }
    catch (Exception ex)
    {
        Console.WriteLine($" ✓ {title}: {ex.GetType().Name} — {ex.Message}");
    }
}