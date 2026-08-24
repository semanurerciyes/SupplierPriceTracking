using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupplierPriceTracking.Data;
using SupplierPriceTracking.Models;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalMaterials = await _context.Materials.CountAsync();
        ViewBag.TotalSuppliers = await _context.Suppliers.CountAsync();
        ViewBag.TotalPriceQuotes = await _context.PriceQuotes.CountAsync();

        // Son eklenen 5 fiyat teklifini getir
        var recentQuotes = await _context.PriceQuotes
            .Include(pq => pq.Material)
            .Include(pq => pq.Supplier)
            .OrderByDescending(pq => pq.CreatedDate)
            .Take(5)
            .ToListAsync();

        return View(recentQuotes);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}