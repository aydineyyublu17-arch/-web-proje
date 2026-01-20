using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrintMarket.Data;
using PrintMarket.Models;
using PrintMarket.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace PrintMarket.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<object> _passwordHasher;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<object>();
        }

        // --- GİRİŞ (LOGIN) ---
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            // 1. Admin Kontrolü
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == model.Email);
            if (admin != null)
            {
                var result = _passwordHasher.VerifyHashedPassword(new object(), admin.PasswordHash, model.Password);
                if (result == PasswordVerificationResult.Success)
                {
                    HttpContext.Session.SetString("AdminUser", admin.Email);
                    HttpContext.Session.SetString("DisplayName", "Admin");
                    HttpContext.Session.SetString("UserRole", "Admin");
                    return RedirectToAction("Index", "Admin");
                }
            }

            // 2. İşletme Kontrolü
            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.Email == model.Email);
            if (business != null)
            {
                var result = _passwordHasher.VerifyHashedPassword(new object(), business.PasswordHash, model.Password);
                if (result == PasswordVerificationResult.Success)
                {
                    HttpContext.Session.SetString("UserEmail", business.Email);
                    HttpContext.Session.SetString("DisplayName", business.BusinessName);
                    HttpContext.Session.SetString("UserRole", "Business");
                    HttpContext.Session.SetString("BusinessId", business.Id.ToString());

                    return RedirectToAction("Index", "Admin"); // İşletmeler de admin paneline erişir
                }
            }

            // 3. Kullanıcı Kontrolü
            var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user != null)
            {
                var result = _passwordHasher.VerifyHashedPassword(new object(), user.PasswordHash, model.Password);
                if (result == PasswordVerificationResult.Success)
                {
                    if (!user.IsActive)
                    {
                        ModelState.AddModelError("", "Hesabınız pasif durumdadır.");
                        return View(model);
                    }

                    HttpContext.Session.SetString("UserEmail", user.Email);
                    HttpContext.Session.SetString("DisplayName", user.FullName);
                    HttpContext.Session.SetString("UserRole", "User");

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);
                        
                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError("", "Geçersiz e-posta veya şifre.");
            return View(model);
        }

        // --- KAYIT (REGISTER) ---
        [HttpGet]
        public IActionResult RegisterUser() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterUser(UserRegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (await _context.AppUsers.AnyAsync(u => u.Email == model.Email) || 
                await _context.Businesses.AnyAsync(b => b.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanımda.");
                return View(model);
            }

            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = _passwordHasher.HashPassword(new object(), model.Password),
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            _context.AppUsers.Add(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Kayıt başarıyla tamamlandı. Giriş yapabilirsiniz.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult RegisterBusiness() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterBusiness(BusinessRegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (await _context.AppUsers.AnyAsync(u => u.Email == model.Email) || 
                await _context.Businesses.AnyAsync(b => b.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanımda.");
                return View(model);
            }

            var business = new Business
            {
                BusinessName = model.BusinessName,
                AuthorizedPerson = model.AuthorizedPerson,
                Email = model.Email,
                Phone = model.Phone,
                PasswordHash = _passwordHasher.HashPassword(new object(), model.Password),
                CreatedDate = DateTime.UtcNow,
                IsApproved = true,
                IsAdminAccess = false // Başlangıçta admin yetkisi yok
            };

            _context.Businesses.Add(business);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "İşletme kaydınız başarıyla tamamlandı. Giriş yapabilirsiniz.";
            return RedirectToAction(nameof(Login));
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
