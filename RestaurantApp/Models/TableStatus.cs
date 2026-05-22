using System.ComponentModel.DataAnnotations;

namespace RestaurantApp.Models
{
    public class TableStatus
    {
        [Key]
        public int ID { get; set; }
        [Required]
        [MaxLength(20)]
        public string Title { get; set; } = null!;
        public virtual ICollection<SeatTable>? SeatTables { get; set; }
    }
}
