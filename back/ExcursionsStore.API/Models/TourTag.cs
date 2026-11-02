using System.ComponentModel.DataAnnotations.Schema;

namespace ExcursionsStore.API.Models
{
    [Table("tour_tag")]
    public class TourTag
    {
        [Column("tour_id")]
        public int TourId { get; set; }
        
        [Column("tag_id")]
        public int TagId { get; set; }
        
        // Навигационные свойства
        [ForeignKey("TourId")]
        public virtual Tour? Tour { get; set; }
        
        [ForeignKey("TagId")]
        public virtual Tag? Tag { get; set; }
    }
}