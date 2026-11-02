using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcursionsStore.API.Models
{
    [Table("orders")]
    public class Order
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Column("order_date")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        [Column("total_price")]
        public decimal TotalPrice { get; set; }
        
        [Column("status")]
        [StringLength(50)]
        public string Status { get; set; } = "В обработке";
        
        [Column("guests_count")]
        public int GuestsCount { get; set; } = 1;
        
        [ForeignKey("Client")]
        [Column("client_id")]
        public int ClientId { get; set; }
        
        [ForeignKey("Tour")]
        [Column("tour_id")]
        public int TourId { get; set; }
        
        // Навигационные свойства
        public virtual Client? Client { get; set; }
        public virtual Tour? Tour { get; set; }
    }
}