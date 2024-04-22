using System.ComponentModel.DataAnnotations;

namespace zadanie_zajecia_7.Models;

public class Order
{
    [Required]
    public int IdOrder { get; set; }
    [Required]
    public int IdProduct { get; set; }
    [Required]
    public int Amount { get; set; }
    [Required]
    public DateTime CreatedAt { get; set; }

    public Order(int idOrder, int idProduct, int amount, DateTime createdAt)
    {
        IdOrder = idOrder;
        IdProduct = idProduct;
        Amount = amount;
        CreatedAt = createdAt;
    }

    public void Deconstruct(out int idOrder, out int idProduct, out int amount, out DateTime createdAt)
    {
        idOrder = IdOrder;
        idProduct = IdProduct;
        amount = Amount;
        createdAt = CreatedAt;
    }
}