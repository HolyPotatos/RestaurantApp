using System.ComponentModel.DataAnnotations;

namespace RestaurantApp.Models
{
    public class PaymentType
    {
        [Key]
        public int ID { get; set; }
        [Required]
        [MaxLength(20)]
        public string Title { get; set; } = null!;
        public virtual ICollection<CustomerOrder>? CustomerOrders { get; set; }
    }
}
