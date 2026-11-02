using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcursionsStore.API.Models
{
    [Table("country")]
    public class Country
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Required]
        [Column("name")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        // Навигационное свойство для городов
        public virtual ICollection<City> Cities { get; set; } = new List<City>();
    }
}