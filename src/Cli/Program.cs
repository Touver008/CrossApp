using Core.Dto;
using Core.Import;

string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Помилка: файл не знайдено за шляхом: {Path.GetFullPath(path)}");
    return 1;
}

ImportResult<ProductDto> result = ProductCsvImporter.Load(path);

Console.WriteLine($"Завантажено записів: {result.Items.Count}");
Console.WriteLine(new string('-', 60));
foreach (ProductDto p in result.Items.Take(5))
{
    Console.WriteLine($" {p.Id,-6} {p.Sku,-10} {p.Name,-30} {p.Quantity,5} {p.Unit}");
}
Console.WriteLine(new string('-', 60));

if (result.Errors.Count > 0)
{
    Console.WriteLine($"Пропущено пошкоджених рядків: {result.Errors.Count}");
    foreach (string err in result.Errors)
    {
        Console.WriteLine($" ! {err}");
    }
}

return 0;