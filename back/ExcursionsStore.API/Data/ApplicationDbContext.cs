using Microsoft.EntityFrameworkCore;
using ExcursionsStore.API.Models;

namespace ExcursionsStore.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        // Просто объявляем свойства, EF Core их инициализирует сам
        public DbSet<Country> Countries => Set<Country>();
        public DbSet<City> Cities => Set<City>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<TourOperator> TourOperators => Set<TourOperator>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<Client> Clients => Set<Client>();
        public DbSet<Tour> Tours => Set<Tour>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<FavoriteTour> FavoriteTours => Set<FavoriteTour>();
        public DbSet<TourTag> TourTags => Set<TourTag>();
        public DbSet<CartItem> CartItems { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Настройка связи многие-ко-многим
            modelBuilder.Entity<TourTag>()
                .HasKey(tt => new { tt.TourId, tt.TagId });

            modelBuilder.Entity<TourTag>()
                .HasOne(tt => tt.Tour)
                .WithMany(t => t.TourTags)
                .HasForeignKey(tt => tt.TourId);

            modelBuilder.Entity<TourTag>()
                .HasOne(tt => tt.Tag)
                .WithMany(t => t.TourTags)
                .HasForeignKey(tt => tt.TagId);

                
            // Уникальные ограничения
            modelBuilder.Entity<Country>()
                .HasIndex(c => c.Name)
                .IsUnique();
                
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();
                
            modelBuilder.Entity<Tag>()
                .HasIndex(t => t.Name)
                .IsUnique();
                
            modelBuilder.Entity<Client>()
                .HasIndex(c => c.Email)
                .IsUnique();
                
            modelBuilder.Entity<FavoriteTour>()
                .HasIndex(ft => new { ft.ClientId, ft.TourId })
                .IsUnique();
                
            // Настройка типа decimal для цены
            modelBuilder.Entity<Tour>()
                .Property(t => t.Price)
                .HasColumnType("decimal(10,2)");
                
            modelBuilder.Entity<Order>()
                .Property(o => o.TotalPrice)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<CartItem>()
                .HasIndex(ci => new { ci.ClientId, ci.TourId })
                .IsUnique();
        }
    }
}