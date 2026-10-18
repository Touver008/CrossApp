using Core.Domain;

Console.WriteLine("=== Сценарій 1: Успішне виконання операцій ===");
Product product = Product.Create("P-001", "sku-001", "Цемент М400 25кг", "шт", 100);
Console.WriteLine($"Створено: {product}");

product.RegisterArrival(50);
Console.WriteLine($"Після приходу (+50): {product}");

product.Issue(30);
Console.WriteLine($"Після видачі (-30): {product}");

Console.WriteLine();
Console.WriteLine("=== Сценарій 2: Спроби порушення інваріантів (захист моделі) ===");

TryDo("Видача більша за залишок", () => product.Issue(1000));
TryDo("Прихід з від'ємною кількістю", () => product.RegisterArrival(-10));
TryDo("Створення з порожнім SKU", () => Product.Create("P-002", "", "Пісок", "т", 10));
TryDo("Створення з від'ємним залишком", () => Product.Create("P-003", "SKU-003", "Цегла", "шт", -5));

Console.WriteLine();
Console.WriteLine($"Стан об'єкта після всіх відмов: {product}");

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($" ! {title}: виняток НЕ спрацював (інваріант відсутній!)");
    }
    catch (Exception ex)
    {
        Console.WriteLine($" ✓ {title}: {ex.GetType().Name} — {ex.Message}");
    }
}