using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcursionsStore.API.Models
{
    [Table("review")]
    public class Review
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Required]
        [Column("rating")]
        [Range(1, 5)]
        public int Rating { get; set; }
        
        [Column("commenting")]
        public string? Commenting { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [ForeignKey("Client")]
        [Column("client_id")]
        public int ClientId { get; set; }
        
        [ForeignKey("Tour")]
        [Column("tour_id")]
        public int TourId { get; set; }

        [Column("order_id")]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }
        
        // Навигационные свойства
        public virtual Client? Client { get; set; }
        public virtual Tour? Tour { get; set; }
    }
}