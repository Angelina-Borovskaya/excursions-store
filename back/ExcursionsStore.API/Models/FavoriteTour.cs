// Models/FavoriteTour.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcursionsStore.API.Models
{
    [Table("favorite_tour")]
    public class FavoriteTour
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
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } // ← используем CreatedAt вместо AddedAt
        
        // Навигационные свойства
        [ForeignKey("ClientId")]
        public virtual Client? Client { get; set; }
        
        [ForeignKey("TourId")]
        public virtual Tour? Tour { get; set; }
    }
}