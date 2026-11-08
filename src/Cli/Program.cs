using Core;
using Core.Abstractions;
using Core.Services;
using Core.Storage;

bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

// Composition Root: єдине місце вибору реалізації сховища (Manual DI)
ICatalogStore store = useFile
    ? new FileCatalogStore(dataPath)
    : new InMemoryCatalogStore(SampleData.Products());

var service = new CatalogService(store);

Console.WriteLine($"=== Сховище: {store.GetType().Name} ===");

// 1. Показ початкового стану
Console.WriteLine($"\nУсього товарів у каталозі: {service.All().Count}");

// 2. Виконання бізнес-операції добавлення
var newProduct = service.Add("SKU-999", "Кабель силовий 3х2.5", "м", 100);
Console.WriteLine($"\n[+] Додано новий товар: {newProduct}");

// 3. Зміна стану через прихід та видачу
service.Receive(newProduct.Id, 50);
Console.WriteLine($"[+] Прихід (+50): {service.Find(newProduct.Id)}");

service.Issue(newProduct.Id, 30);
Console.WriteLine($"[-] Видача (-30): {service.Find(newProduct.Id)}");

// 4. Показ перших 5 товарів
Console.WriteLine("\n--- Перелік товарів (перші 5) ---");
foreach (var p in service.All().Take(5))
{
    Console.WriteLine($" {p.Id,-8} {p.Sku,-10} {p.Name,-32} {p.Quantity,6} {p.Unit}");
}

// 5. Демонстрація обробки винятків (Сценарії відмови)
Console.WriteLine("\n=== Сценарії відмови ===");

TryDo("Прихід для неіснуючого ID", () => service.Receive("NON-EXISTENT-ID", 10));
TryDo("Видача більша за залишок", () => service.Issue(newProduct.Id, 9999));

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