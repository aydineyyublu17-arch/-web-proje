using Microsoft.AspNetCore.Mvc;
using PrintMarket.Data;
using PrintMarket.Extensions;
using PrintMarket.Models;

namespace PrintMarket.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "Cart";

        public CheckoutController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey);
            
            if (cart == null || cart.Count == 0)
            {
                return RedirectToAction("Index", "Cart");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(Order order)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey);
            
            if (cart == null || cart.Count == 0)
            {
                ModelState.AddModelError("", "Sepetiniz boş!");
                return RedirectToAction("Index", "Cart");
            }

            // Order modelinde CustomerName, Address zorunlu olduğu için Model validasyonu işler
            if (ModelState.IsValid)
            {
                order.OrderDate = DateTime.Now;
                order.TotalPrice = cart.Sum(c => c.TotalPrice);

                // OrderItems oluştur
                foreach (var item in cart)
                {
                    var orderItem = new OrderItem
                    {
                         ProductId = item.Product.Id,
                         Quantity = item.Quantity,
                         UnitPrice = item.Product.Price
                    };
                    order.OrderItems.Add(orderItem);
                    
                    // Opsiyonel: Stok düşme (Product objesi üzerinden değil, ID ile contextten çekip yapmak daha safe olur ama basitlik adına)
                    var productInDb = await _context.Products.FindAsync(item.Product.Id);
                    if(productInDb != null)
                    {
                        productInDb.Stock -= item.Quantity; // Stok eksiye düşebilir kontrolü eklenmeli aslında
                    }
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Sepeti temizle
                HttpContext.Session.Remove(CartSessionKey);

                TempData["SuccessMessage"] = "Siparişiniz Başarıyla Alındı! En kısa sürede sizinle iletişime geçeceğiz.";
                return RedirectToAction("Index", "Home");
            }

            return View(order);
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}
