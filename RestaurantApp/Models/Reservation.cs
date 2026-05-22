using System.ComponentModel.DataAnnotations;

namespace RestaurantApp.Models
{
    public class Reservation
    {
        [Key]
        public int ID { get; set; }
        [Required]
        [MaxLength(150)]
        public string CustomerName { get; set; } = null!;
        [Required]
        [Phone]
        [MaxLength(16)]
        public string CustomerPhone { get; set; } = null!;
        [Required]
        public DateTimeOffset ReservationDateStart { get; set; }
        [Required]
        public DateTimeOffset ReservationDateEnd { get; set; }
        [Required]
        public int SeatTableID { get; set; }
        [Required]
        public int ReservationStatusID { get; set; }
        public virtual SeatTable? SeatTable { get; set; }
        public virtual ReservationStatus? ReservationStatus { get; set; }
    }
}