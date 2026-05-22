using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantApp.Models
{
    public class OrderDetails
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }
        [Required]
        [Column(TypeName = "decimal(3,2)")]
        public decimal Discount { get; set; }
        [Required]
        public int MenuID { get; set; }
        [Required]
        public int CustomerOrderID { get; set; }
        public virtual Menu? Menu { get; set; }
        public virtual CustomerOrder? CustomerOrder { get; set; }
    }
}
