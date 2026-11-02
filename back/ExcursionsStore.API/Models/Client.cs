// Models/Client.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcursionsStore.API.Models
{
    [Table("client")]
    public class Client
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Required]
        [Column("first_name")]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Column("last_name")]
        [StringLength(100)]
        public string? LastName { get; set; }
        
        [Required]
        [Column("email")]
        [StringLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [Column("password_hash")]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Column("phone_number")]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }
        
        [Column("registration_date")]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        
        [Column("role")]
        [StringLength(20)]
        public string Role { get; set; } = "User"; // По умолчанию "User"
        
        // Навигационные свойства
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<FavoriteTour> FavoriteTours { get; set; } = new List<FavoriteTour>();
    }
}