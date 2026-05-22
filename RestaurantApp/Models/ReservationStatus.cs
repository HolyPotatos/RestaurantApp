using System.ComponentModel.DataAnnotations;

namespace RestaurantApp.Models
{
    public class ReservationStatus
    {
        [Key]
        public int ID { get; set; }
        [Required]
        [MaxLength(20)]
        public string Title { get; set; } = null!;
        public virtual ICollection<Reservation>? Reservations { get; set; }
    }
}
