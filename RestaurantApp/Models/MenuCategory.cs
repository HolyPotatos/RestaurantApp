using System.ComponentModel.DataAnnotations;

namespace RestaurantApp.Models
{
    public class MenuCategory
    {
        [Key]
        public int ID { get; set; }
        [Required]
        [MaxLength(20)]
        public string Title { get; set; } = null!;
        public virtual ICollection<Menu>? Menus { get; set; }
    }
}
