using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcursionsStore.API.Models
{
    [Table("tour")]
    public class Tour
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("title")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Required]
        [Column("price")]
        public decimal Price { get; set; }

        [Required]
        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column("end_date")]
        public DateTime EndDate { get; set; }

        [Column("image_file_name")]
        public string? ImageFileName { get; set; }

        [Column("city_id")]
        public int CityId { get; set; }

        [Column("category_id")]
        public int CategoryId { get; set; }

        [Column("tour_operator_id")]
        public int TourOperatorId { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("average_rating")]
        public decimal AverageRating { get; set; }

        // Навигация
        public virtual City? City { get; set; }
        public virtual Category? Category { get; set; }
        public virtual TourOperator? TourOperator { get; set; }

        // через join-entity
        public virtual ICollection<TourTag> TourTags { get; set; } = new List<TourTag>();

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<FavoriteTour> FavoriteTours { get; set; } = new List<FavoriteTour>();
    }
}
