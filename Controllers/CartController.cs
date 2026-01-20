using Microsoft.AspNetCore.Mvc;
using PrintMarket.Data;
using PrintMarket.Extensions;
using PrintMarket.Models;

namespace PrintMarket.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "Cart";

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        [HttpPost]
        public IActionResult AddToCart(int id, int quantity)
        {
            // Giriş kontrolü
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserEmail")) && 
                string.IsNullOrEmpty(HttpContext.Session.GetString("AdminUser")))
            {
                TempData["ErrorMessage"] = "Lütfen önce giriş yapınız.";
                string returnUrl = Request.Headers["Referer"].ToString();
                return RedirectToAction("Login", "Account", new { returnUrl = returnUrl });
            }

            var product = _context.Products.Find(id);
            if (product == null) return NotFound();

            var cart = GetCart();
            var cartItem = cart.FirstOrDefault(c => c.Product.Id == id);

            if (cartItem == null)
            {
                cart.Add(new CartItem { Product = product, Quantity = quantity });
            }
            else
            {
                cartItem.Quantity += quantity;
            }

            SaveCart(cart);
            TempData["SuccessMessage"] = "Ürün sepete eklendi.";
            
            // Geldiği sayfaya geri dön (Referer yoksa Anasayfa)
            string referer = Request.Headers["Referer"].ToString();
            return !string.IsNullOrEmpty(referer) ? Redirect(referer) : RedirectToAction("Index", "Home");
        }

        public IActionResult RemoveFromCart(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.Product.Id == id);
            
            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
            }

            return RedirectToAction("Index");
        }

        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove(CartSessionKey);
            return RedirectToAction("Index");
        }

        private List<CartItem> GetCart()
        {
            return HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
        }
    }
}
