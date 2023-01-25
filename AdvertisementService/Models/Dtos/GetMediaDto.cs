using System;

namespace AdvertisementService.Models.Dtos
{
    public class GetMediaDto
    {
        public string MediaId { get; set; }
        public string Url { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string MediaType { get; set; }
        public float? Size { get; set; }
        public float? Duration { get; set; } = 0;
    }
}
