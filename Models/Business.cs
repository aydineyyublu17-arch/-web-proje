using System.ComponentModel.DataAnnotations;

namespace PrintMarket.Models
{
    public class Business
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "İşletme adı zorunludur.")]
        [Display(Name = "İşletme Adı")]
        public string BusinessName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yetkili kişi zorunludur.")]
        [Display(Name = "Yetkili Kişi")]
        public string AuthorizedPerson { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçersiz e-posta adresi.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon zorunludur.")]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Onay Durumu")]
        public bool IsApproved { get; set; } = false;

        [Display(Name = "Admin Erişimi")]
        public bool IsAdminAccess { get; set; } = false;
    }
}
