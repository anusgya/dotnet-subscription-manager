using System.ComponentModel.DataAnnotations;

namespace dotnet_subscription_manager.Models;

public class Subscription
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }
    
    [Required]
    public string BillingCycle { get; set; } = "Monthly"; // monthly or yearly
    
    [Required]
    public DateTime NextPaymentDate { get; set; }
    
    public string? Category { get; set; }
    
    public string UserId { get; set; } = string.Empty;
}

