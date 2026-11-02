namespace ExcursionsStore.API.DTOs
{
    public class CreateReviewDto
    {
        public int TourId { get; set; }
        public int OrderId { get; set; }
        public int Rating { get; set; }
        public string Commenting { get; set; } = string.Empty;
    }
}
