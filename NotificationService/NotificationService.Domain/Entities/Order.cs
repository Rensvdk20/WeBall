namespace NotificationService.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; } 
    public string ClientEmail { get; set; }
}