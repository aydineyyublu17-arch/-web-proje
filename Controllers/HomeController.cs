using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrintMarket.Data;
using PrintMarket.Models;

namespace PrintMarket.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index(string? search, int[]? categoryIds, string? condition, string? sort, int page = 1)
    {
        int pageSize = 6;
        var query = _context.Products.Include(p => p.Category).Include(p => p.Images).AsQueryable();

        // Metin Araması
        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(search) || 
                                   p.Brand.ToLower().Contains(search));
        }

        // Kategori Filtreleme
        if (categoryIds != null && categoryIds.Length > 0)
        {
            query = query.Where(p => categoryIds.Contains(p.CategoryId));
        }

        // Durum Filtreleme (Sıfır / 2. El)
        if (!string.IsNullOrEmpty(condition))
        {
            if (condition == "new") query = query.Where(p => !p.IsSecondHand);
            else if (condition == "used") query = query.Where(p => p.IsSecondHand);
        }

        // Sıralama Mantığı
        query = sort switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "newest" => query.OrderByDescending(p => p.Id),
            _ => query.OrderByDescending(p => p.Id) // Varsayılan en yeniler
        };

        ViewBag.Categories = await _context.Categories.ToListAsync();
        ViewBag.CurrentSort = sort;

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        page = page < 1 ? 1 : (page > totalPages && totalPages > 0 ? totalPages : page);

        ViewBag.TotalPages = totalPages;
        ViewBag.CurrentPage = page;
        ViewBag.TotalItems = totalItems;

        var products = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_ProductList", products);
        }

        return View(products);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Business)
            .FirstOrDefaultAsync(x => x.Id == id);
        
        if (product == null) return NotFound();

        return View(product);
    }

    public async Task<IActionResult> Brands()
    {
        var brands = await _context.Products
            .Where(p => !string.IsNullOrEmpty(p.Brand))
            .Select(p => p.Brand)
            .Distinct()
            .OrderBy(b => b)
            .ToListAsync();
            
        return View(brands);
    }

    public IActionResult About()
    {
        return View();
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
