using System.ComponentModel.DataAnnotations;

namespace PrintMarket.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Kategori adı zorunludur.")]
        [MaxLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Kategori Adı")]
        public string Name { get; set; } = string.Empty;

        // Navigation Property: Bir kategoride birden fazla ürün olabilir.
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
