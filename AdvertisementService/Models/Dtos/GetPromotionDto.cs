namespace AdvertisementService.Models.Dtos
{
    public class GetPromotionDto
    {
        public string PromotionId { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string CreatedAt { get; set; }
        public string AdvertisementId { get; set; }
        public string InstitutionId { get; set; }
        public string Type { get; set; }
        public string LogoUrl { get; set; }
    }
}
