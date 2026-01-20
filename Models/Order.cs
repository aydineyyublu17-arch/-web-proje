using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrintMarket.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Sipariş Tarihi")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Müşteri adı zorunludur.")]
        [MaxLength(100)]
        [Display(Name = "Müşteri Adı")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Adres zorunludur.")]
        [MaxLength(500)]
        [Display(Name = "Adres")]
        public string CustomerAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress]
        [Display(Name = "E-posta")]
        public string CustomerEmail { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Toplam Tutar")]
        public decimal TotalPrice { get; set; }

        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        [Phone]
        [Display(Name = "Telefon")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Display(Name = "Sipariş Durumu")]
        public string Status { get; set; } = "Yeni Sipariş";

        [Display(Name = "Silindi mi?")]
        public bool IsDeleted { get; set; } = false;

        // Navigation Property: Siparişin kalemleri
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
