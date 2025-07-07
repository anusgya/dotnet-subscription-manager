using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using dotnet_subscription_manager.Data;
using dotnet_subscription_manager.Models;
using System.Security.Claims;

namespace dotnet_subscription_manager.Controllers;

[Authorize]
public class SubscriptionsController : Controller
{
    private readonly ApplicationDbContext _context;
    
    public SubscriptionsController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // dashboard
    public async Task<IActionResult> Index(string? category, string? billingCycle, string? sortBy)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var query = _context.Subscriptions.Where(s => s.UserId == userId);
        
        // filtering
        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(s => s.Category == category);
        }
        
        if (!string.IsNullOrEmpty(billingCycle))
        {
            query = query.Where(s => s.BillingCycle == billingCycle);
        }
        
        // sorting
        query = sortBy switch
        {
            "price" => query.OrderBy(s => s.Price),
            "date" => query.OrderBy(s => s.NextPaymentDate),
            _ => query.OrderBy(s => s.Name)
        };
        
        var subscriptions = await query.ToListAsync();
        
        // calculate total monthly cost
        decimal totalMonthly = 0;
        foreach (var sub in subscriptions)
        {
            if (sub.BillingCycle == "Monthly")
                totalMonthly += sub.Price;
            else if (sub.BillingCycle == "Yearly")
                totalMonthly += sub.Price / 12; // divide yearly by 12
        }
        
        ViewBag.TotalMonthly = totalMonthly;
        ViewBag.Categories = await _context.Subscriptions
            .Where(s => s.UserId == userId && s.Category != null)
            .Select(s => s.Category)
            .Distinct()
            .ToListAsync();
        
        return View(subscriptions);
    }
    
    // show form to add new
    public IActionResult Create()
    {
        return View();
    }
    
    // save new subscription
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Subscription subscription)
    {
        if (ModelState.IsValid)
        {
            subscription.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            _context.Add(subscription);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(subscription);
    }
    
    // show edit form
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
        
        if (subscription == null) return NotFound();
        
        return View(subscription);
    }
    
    // save edited subscription
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Subscription subscription)
    {
        if (id != subscription.Id) return NotFound();
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var existing = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
        
        if (existing == null) return NotFound();
        
        if (ModelState.IsValid)
        {
            existing.Name = subscription.Name;
            existing.Price = subscription.Price;
            existing.BillingCycle = subscription.BillingCycle;
            existing.NextPaymentDate = subscription.NextPaymentDate;
            existing.Category = subscription.Category;
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(subscription);
    }
    
    // show delete confirmation
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
        
        if (subscription == null) return NotFound();
        
        return View(subscription);
    }
    
    // actually delete
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
        
        if (subscription != null)
        {
            _context.Subscriptions.Remove(subscription);
            await _context.SaveChangesAsync();
        }
        
        return RedirectToAction(nameof(Index));
    }
}

