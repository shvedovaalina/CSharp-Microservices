namespace Core.Models;

public class Product
{
    public long ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public decimal Price { get; set; }
}