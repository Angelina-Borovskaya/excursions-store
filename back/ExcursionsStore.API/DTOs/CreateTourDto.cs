using Microsoft.AspNetCore.Http;

namespace ExcursionsStore.API.DTOs.Tours
{
    public class CreateTourDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int CityId { get; set; }
        public int CategoryId { get; set; }
        public int TourOperatorId { get; set; }

        // Фото
        public IFormFile? Image { get; set; }

        // Теги (список ID)
        public List<int> TagIds { get; set; } = new();
    }

    public class UpdateTourDto : CreateTourDto
    {
        public bool? IsActive { get; set; } // опционально
    }
}
