namespace AdvertisementService.Models.Dtos
{
    public class GetContentDto
    {
        public string ContentId { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public string ResourceNumber { get; set; }
        public string Name { get; set; }
        public int? TintColor { get; set; }
        public int? InvertedTintColor { get; set; }
        public GetPromotionDto Promotion { get; set; }
    }
}
