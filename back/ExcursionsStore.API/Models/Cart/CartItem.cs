// Models/Cart/CartItem.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcursionsStore.API.Models
{
    [Table("cart_items")]
    public class CartItem
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Required]
        [Column("client_id")]
        public int ClientId { get; set; }
        
        [Required]
        [Column("tour_id")]
        public int TourId { get; set; }
        
        [Required]
        [Column("guests_count")]
        [Range(1, 10)]
        public int Quantity { get; set; } = 1;
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Навигационные свойства
        [ForeignKey("ClientId")]
        public virtual Client? Client { get; set; }
        
        [ForeignKey("TourId")]
        public virtual Tour? Tour { get; set; }
    }
}