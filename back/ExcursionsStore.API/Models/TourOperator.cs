using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcursionsStore.API.Models
{
    [Table("tour_operator")]
    public class TourOperator
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Required]
        [Column("name")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        // Навигационное свойство для туров
        public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
    }
}