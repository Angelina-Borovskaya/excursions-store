namespace ExcursionsStore.API.DTOs
{
    public class AddToCartDto
    {
        public int TourId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
