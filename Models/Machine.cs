using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrintMarket.Models
{
    public class Machine
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Başlık zorunludur.")]
        [StringLength(200, MinimumLength = 10, ErrorMessage = "Başlık 10-200 karakter arasında olmalıdır.")]
        [Display(Name = "İlan Başlığı")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Fiyat zorunludur.")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Fiyat")]
        public decimal Price { get; set; }

        [Display(Name = "Üretim Yılı")]
        public int ManufactureYear { get; set; }

        [Display(Name = "Durum")]
        public MachineCondition Condition { get; set; }

        public MachineStatus Status { get; set; } = MachineStatus.Pending;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Kategori")]
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        [Display(Name = "Marka")]
        public int BrandId { get; set; }
        public virtual Brand? Brand { get; set; }

        // Owner (Seller)
        public string? SellerId { get; set; }
        [ForeignKey("SellerId")]
        public virtual AppUser? Seller { get; set; }

        public virtual ICollection<MachineImage> Images { get; set; } = new List<MachineImage>();
    }

    public enum MachineCondition
    {
        [Display(Name = "Yeni")]
        New,
        [Display(Name = "İkinci El")]
        Used,
        [Display(Name = "Yenilenmiş")]
        Refurbished
    }

    public enum MachineStatus
    {
        [Display(Name = "Onay Bekliyor")]
        Pending,
        [Display(Name = "Yayında")]
        Active,
        [Display(Name = "Satıldı")]
        Sold,
        [Display(Name = "Reddedildi")]
        Rejected
    }
}
