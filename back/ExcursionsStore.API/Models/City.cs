using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcursionsStore.API.Models
{
    [Table("city")]
    public class City
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Required]
        [Column("name")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [ForeignKey("Country")]
        [Column("country_id")]
        public int CountryId { get; set; }
        
        // Навигационное свойство
        public virtual Country? Country { get; set; }
        
        // Навигационное свойство для туров
        public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
    }
}