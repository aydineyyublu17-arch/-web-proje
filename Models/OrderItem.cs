using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrintMarket.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Order Order { get; set; } = null!;

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;

        [Display(Name = "Adet")]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Birim Fiyat")]
        public decimal UnitPrice { get; set; }
    }
}
