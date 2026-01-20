using Microsoft.AspNetCore.Identity;

namespace PrintMarket.Models
{
    public class AppUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? CompanyName { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
