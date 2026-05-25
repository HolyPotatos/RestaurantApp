using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantApp.Models
{
    public class CustomerOrder
    {
        [Key]
        public int ID { get; set; }
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }
        [Required]
        public DateTimeOffset OrderDate { get; set; }
        [Required]
        public int EmployeeID { get; set; }
        [Required]
        public int SeatTableID { get; set; }
        [Required]
        public int PaymentTypeID { get; set; }
        [Required]
        public virtual SeatTable? SeatTable { get; set; }
        public virtual Employee? Employee { get; set; }
        public virtual PaymentType? PaymentType { get; set; }
        public virtual ICollection<OrderDetails>? OrderDetails { get; set; }
    }
}
