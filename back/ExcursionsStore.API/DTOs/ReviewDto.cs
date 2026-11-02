namespace ExcursionsStore.API.DTOs
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string? Commenting { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ClientId { get; set; }
        public int TourId { get; set; }
        public string? ClientName { get; set; }
        public string? TourTitle { get; set; }
        public int OrderId { get; set; }

    }
}