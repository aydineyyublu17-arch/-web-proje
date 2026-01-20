using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PrintMarket.Data;
using PrintMarket.Filters;
using PrintMarket.Models;

namespace PrintMarket.Controllers
{
    [AdminAuthFilter] // Tüm controller yetki gerektirir
    public class AdminProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminProductController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var businessIdStr = HttpContext.Session.GetString("BusinessId");

            IQueryable<Product> query = _context.Products.Include(p => p.Category).Include(p => p.Images);

            // Eğer "Admin" değilse, sadece kendi işletmesinin ürünlerini görsün
            if (userRole != "Admin" && !string.IsNullOrEmpty(businessIdStr))
            {
                int businessId = int.Parse(businessIdStr);
                query = query.Where(p => p.BusinessId == businessId);
            }

            var products = await query.ToListAsync();
            return View(products);
        }

        // CREATE: GET
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // CREATE: POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, List<IFormFile> Images)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Giriş yapan işletmenin ID'sini ata
                    var businessIdStr = HttpContext.Session.GetString("BusinessId");
                    if (!string.IsNullOrEmpty(businessIdStr))
                    {
                        product.BusinessId = int.Parse(businessIdStr);
                    }

                    _context.Add(product);
                    await _context.SaveChangesAsync(); // Ürün ID'sini almak için önce kaydediyoruz

                    // Çoklu Resim Yükleme İşlemi
                    if (Images != null && Images.Count > 0)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/products");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                        foreach (var file in Images)
                        {
                            if (file.Length > 0)
                            {
                                string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                                using (var fileStream = new FileStream(filePath, FileMode.Create))
                                {
                                    await file.CopyToAsync(fileStream);
                                }

                                var productImage = new ProductImage
                                {
                                    ImagePath = "/images/products/" + uniqueFileName,
                                    ProductId = product.Id
                                };
                                _context.ProductImages.Add(productImage);

                                // İlk yüklenen resmi kapak resmi (ImageUrl) olarak ata
                                if (string.IsNullOrEmpty(product.ImageUrl))
                                {
                                    product.ImageUrl = productImage.ImagePath;
                                }
                            }
                        }
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        ModelState.AddModelError("Images", "Lütfen en az bir ürün resmi seçiniz.");
                        ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
                        return View(product);
                    }
                    
                    TempData["SuccessMessage"] = "Ürün ve görseller başarıyla eklendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Veritabanına kayıt sırasında bir hata oluştu: " + ex.Message;
                }
            }
            else
            {
                // Validation hatalarını toplama (Debug veya kullanıcıya göstermek için)
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                ViewBag.Error = "Lütfen alanları kontrol ediniz. Hatalar: " + string.Join(", ", errors);
            }

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var userRole = HttpContext.Session.GetString("UserRole");
            var businessIdStr = HttpContext.Session.GetString("BusinessId");
            
            var query = _context.Products.Include(p => p.Images).AsQueryable();

            if (userRole != "Admin" && !string.IsNullOrEmpty(businessIdStr))
            {
                int businessId = int.Parse(businessIdStr);
                query = query.Where(p => p.BusinessId == businessId);
            }

            var product = await query.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return Unauthorized(); // Yetkisiz erişim veya ürün bulunamadı

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, List<IFormFile> Images)
        {
            if (id != product.Id) return NotFound();

            var userRole = HttpContext.Session.GetString("UserRole");
            var businessIdStr = HttpContext.Session.GetString("BusinessId");

            // Güvenlik Kontrolü: İşletme kendi ürününe mi bakıyor?
            if (userRole != "Admin" && !string.IsNullOrEmpty(businessIdStr))
            {
                int businessId = int.Parse(businessIdStr);
                var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                if (existingProduct == null || existingProduct.BusinessId != businessId)
                {
                    return Unauthorized();
                }
                product.BusinessId = businessId; // BusinessId'yi koru
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Yeni Resimleri Yükle
                    if (Images != null && Images.Count > 0)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/products");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                        foreach (var file in Images)
                        {
                            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }

                            var productImage = new ProductImage
                            {
                                ImagePath = "/images/products/" + uniqueFileName,
                                ProductId = product.Id
                            };
                            _context.ProductImages.Add(productImage);

                            // Eğer ürünün hiç resmi yoksa veya ImageUrl boşsa kapak resmi yap
                            if (string.IsNullOrEmpty(product.ImageUrl))
                            {
                                product.ImageUrl = productImage.ImagePath;
                            }
                        }
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Ürün ve yeni görseller güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.Id == product.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                ViewBag.Error = "Lütfen alanları kontrol ediniz. Hatalar: " + string.Join(", ", errors);
            }
            
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var image = await _context.ProductImages.FindAsync(imageId);
            if (image == null) return NotFound();

            int productId = image.ProductId;

            // Dosyayı sunucudan sil
            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, image.ImagePath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();

            // Eğer silinen resim kapak resmiyse, varsa başka bir resmi kapak resmi yap
            var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == productId);
            if (product != null && product.ImageUrl == image.ImagePath)
            {
                var nextImage = product.Images.FirstOrDefault();
                product.ImageUrl = nextImage?.ImagePath;
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Görsel silindi.";
            return RedirectToAction(nameof(Edit), new { id = productId });
        }

        // DELETE: POST (Direkt silme linki veya butonu ile)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var businessIdStr = HttpContext.Session.GetString("BusinessId");
            
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // Yetki Kontrolü
                if (userRole != "Admin" && !string.IsNullOrEmpty(businessIdStr))
                {
                    int businessId = int.Parse(businessIdStr);
                    if (product.BusinessId != businessId) return Unauthorized();
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Ürün silindi.";
            }
            return RedirectToAction(nameof(Index));
        }
        // TOGGLE APPROVAL: POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleApproval(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var businessIdStr = HttpContext.Session.GetString("BusinessId");

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            // Yetki Kontrolü
            if (userRole != "Admin" && !string.IsNullOrEmpty(businessIdStr))
            {
                int businessId = int.Parse(businessIdStr);
                if (product.BusinessId != businessId) return Unauthorized();
            }

            product.IsApproved = !product.IsApproved;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = product.IsApproved 
                ? "Ürün yayına alındı." 
                : "Ürün yayından kaldırıldı.";
                
            return RedirectToAction(nameof(Index));
        }
    }
}
