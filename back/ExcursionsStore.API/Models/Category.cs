using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcursionsStore.API.Models
{
    [Table("category")]
    public class Category
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Required]
        [Column("name")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        // Навигационное свойство для туров
        public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
    }
}