public class OrderProfileDto
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int TourId { get; set; }

    public string? TourTitle { get; set; }

    public bool HasReview { get; set; }
}
