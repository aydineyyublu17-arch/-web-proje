using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrintMarket.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Ürün adı zorunludur.")]
        [MaxLength(200, ErrorMessage = "Ürün adı en fazla 200 karakter olabilir.")]
        [Display(Name = "Ürün Adı")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Fiyat zorunludur.")]
        [Column(TypeName = "decimal(18, 2)")] // Veritabanı için hassasiyet ayarı
        [Display(Name = "Fiyat")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Para birimi zorunludur.")]
        [MaxLength(5)]
        [Display(Name = "Para Birimi")]
        public string Currency { get; set; } = "₺";

        [Display(Name = "Açıklama")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Resim URL")]
        public string? ImageUrl { get; set; }

        [MaxLength(100)]
        [Display(Name = "Marka")]
        public string Brand { get; set; } = string.Empty;

        [Display(Name = "Yıl")]
        public int? Year { get; set; }

        [Display(Name = "İkinci El mi?")]
        public bool IsSecondHand { get; set; }

        [Display(Name = "Onay Durumu")]
        public bool IsApproved { get; set; } = true;

        [Display(Name = "Baskı Hızı")]
        public string? PrintSpeed { get; set; }

        [Display(Name = "Renk Sayısı")]
        public int? ColorCount { get; set; }

        [MaxLength(100)]
        [Display(Name = "Model")]
        public string? ModelName { get; set; }

        [MaxLength(100)]
        [Display(Name = "Menşei")]
        public string? Origin { get; set; }

        [Display(Name = "Stok Adedi")]
        public int Stock { get; set; }

        // Foreign Key
        [Display(Name = "Kategori")]
        public int CategoryId { get; set; }

        // Navigation Property
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        // Multi-tenant desteği için BusinessId
        public int? BusinessId { get; set; }
        [ForeignKey("BusinessId")]
        public virtual Business? Business { get; set; }

        // Navigation Property for Multiple Images
        public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    }
}
