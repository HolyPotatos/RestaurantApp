using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantApp.Models
{
    public class Menu
    {
        [Key]
        public int ID { get; set; }
        [Required]
        [MaxLength(75)]
        public string Title { get; set; } = null!;
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }
        [Required]
        public bool isActive { get; set; }
        [Required]
        public int MenuCategoryID { get; set; }
        public virtual MenuCategory? MenuCategory { get; set; }
        public virtual ICollection<OrderDetails>? OrderDetails { get; set; }
    }
}
