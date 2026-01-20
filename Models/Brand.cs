using System.ComponentModel.DataAnnotations;

namespace PrintMarket.Models
{
    public class Brand
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Marka adı zorunludur.")]
        [Display(Name = "Marka Adı")]
        public string Name { get; set; } = string.Empty;

        public virtual ICollection<Machine> Machines { get; set; } = new List<Machine>();
    }
}
