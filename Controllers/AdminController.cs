using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrintMarket.Data;
using PrintMarket.Filters;
using PrintMarket.Models;

namespace PrintMarket.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Dashboard Sayfası - Yetki kontrolü var
        [AdminAuthFilter]
        public async Task<IActionResult> Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var businessIdStr = HttpContext.Session.GetString("BusinessId");

            IQueryable<Product> productQuery = _context.Products;
            IQueryable<Order> orderQuery = _context.Orders;

            if (userRole != "Admin" && !string.IsNullOrEmpty(businessIdStr))
            {
                int businessId = int.Parse(businessIdStr);
                
                // Ürünleri filtrele
                productQuery = productQuery.Where(p => p.BusinessId == businessId);
                
                // Siparişleri filtrele (O işletmenin ürünlerini içeren siparişler)
                orderQuery = orderQuery.Where(o => o.OrderItems.Any(oi => oi.Product.BusinessId == businessId));
            }

            var viewModel = new PrintMarket.ViewModels.AdminDashboardViewModel
            {
                TotalProducts = await productQuery.CountAsync(),
                TotalOrders = await orderQuery.CountAsync(o => !o.IsDeleted),
                PendingOrders = await orderQuery.CountAsync(o => !o.IsDeleted && o.Status == "Yeni Sipariş"),
                TotalCategories = await _context.Categories.CountAsync(),
                RecentProducts = await productQuery.OrderByDescending(p => p.Id).Take(5).ToListAsync(),
                RecentOrders = await orderQuery
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .Where(o => !o.IsDeleted)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // Login Sayfası (GET)
        [HttpGet]
        public IActionResult Login()
        {
            return RedirectToAction("Login", "Account");
        }

        // Login İşlemi (POST)
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var admin = _context.Admins.FirstOrDefault(x => x.Email == email && x.PasswordHash == password);

            if (admin != null)
            {
                // Başarılı giriş: Session ata
                HttpContext.Session.SetString("AdminUser", admin.Email);
                return RedirectToAction("Index");
            }
            else
            {
                // Başarısız giriş
                ViewBag.Error = "Kullanıcı adı veya şifre hatalı!";
                return View();
            }
        }

        // Çıkış İşlemi
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Session'ı temizle
            return RedirectToAction("Login");
        }

        // SİPARİŞLER (Orders)
        [AdminAuthFilter]
        public async Task<IActionResult> Orders()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var businessIdStr = HttpContext.Session.GetString("BusinessId");

            IQueryable<Order> query = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => !o.IsDeleted)
                .OrderByDescending(o => o.OrderDate);

            if (userRole != "Admin" && !string.IsNullOrEmpty(businessIdStr))
            {
                int businessId = int.Parse(businessIdStr);
                query = query.Where(o => o.OrderItems.Any(oi => oi.Product.BusinessId == businessId));
            }

            var orders = await query.ToListAsync();
            return View(orders);
        }

        // SİPARİŞ DETAY (Order Details Page)
        [AdminAuthFilter]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var businessIdStr = HttpContext.Session.GetString("BusinessId");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            // İşletme filtreleme: Eğer işletmeyse, sadece kendi ürünlerini görsün
            if (userRole != "Admin" && !string.IsNullOrEmpty(businessIdStr))
            {
                int businessId = int.Parse(businessIdStr);
                // Siparişin içindeki diğer işletmelere ait ürünleri temizleyerek göster (veya hiç gösterme)
                order.OrderItems = order.OrderItems.Where(oi => oi.Product.BusinessId == businessId).ToList();
                
                if (!order.OrderItems.Any()) return Unauthorized();
            }

            return View(order);
        }

        // SİPARİŞ DETAY MODAL (AJAX)
        [AdminAuthFilter]
        public async Task<IActionResult> GetOrderDetails(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var businessIdStr = HttpContext.Session.GetString("BusinessId");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            if (userRole != "Admin" && !string.IsNullOrEmpty(businessIdStr))
            {
                int businessId = int.Parse(businessIdStr);
                order.OrderItems = order.OrderItems.Where(oi => oi.Product.BusinessId == businessId).ToList();
                
                if (!order.OrderItems.Any()) return Unauthorized();
            }

            return PartialView("_OrderDetailsModal", order);
        }

        // KATEGORİLER (Categories)
        [AdminAuthFilter]
        public async Task<IActionResult> Categories()
        {
            return View(await _context.Categories.ToListAsync());
        }

        // KATEGORİ EKLE (GET)
        [AdminAuthFilter]
        public IActionResult CreateCategory()
        {
            return View();
        }

        // KATEGORİ EKLE (POST)
        [HttpPost]
        [AdminAuthFilter]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"'{category.Name}' kategorisi başarıyla eklendi.";
                return RedirectToAction(nameof(Categories));
            }
            return View(category);
        }

        // KATEGORİ DÜZENLE (GET)
        [AdminAuthFilter]
        public async Task<IActionResult> EditCategory(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        // KATEGORİ DÜZENLE (POST)
        [HttpPost]
        [AdminAuthFilter]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, Category category)
        {
            if (id != category.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Kategori başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Categories.Any(e => e.Id == category.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Categories));
            }
            return View(category);
        }

        // KATEGORİ SİL (POST)
        [HttpPost]
        [AdminAuthFilter]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            // Eğer bu kategoriye ait ürünler varsa silmeyi engelle veya uyar
            var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
            {
                TempData["ErrorMessage"] = "Bu kategoriye ait ürünler olduğu için silemezsiniz. Önce ürünleri başka bir kategoriye taşıyın.";
                return RedirectToAction(nameof(Categories));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Kategori başarıyla silindi.";
            return RedirectToAction(nameof(Categories));
        }
        // SİPARİŞ DURUM GÜNCELLE (Update Order Status)
        [HttpPost]
        [AdminAuthFilter]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
                
            if (order == null) return NotFound();

            // Sipariş reddediliyorsa ve daha önce reddedilmemişse stokları geri ekle
            if (status == "Reddedildi" && order.Status != "Reddedildi")
            {
                foreach (var item in order.OrderItems)
                {
                    if (item.Product != null)
                    {
                        item.Product.Stock += item.Quantity;
                    }
                }
            }

            order.Status = status;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"#{id} nolu sipariş durumu '{status}' olarak güncellendi.";
            return RedirectToAction("Orders");
        }

        // SİPARİŞ SİL (Soft Delete)
        [HttpPost]
        [AdminAuthFilter]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                order.IsDeleted = true;
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Sipariş başarıyla silindi." });
            }
            return Json(new { success = false, message = "Sipariş bulunamadı." });
        }
    }
}
