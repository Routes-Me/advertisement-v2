using System.Collections.Generic;

namespace AdvertisementService.Models.Dtos
{
    public class PostAdvertisementsDto
    {
        public string AdvertisementId { get; set; }
        public string InstitutionId { get; set; }
        public string IntervalId { get; set; }
        public List<string> CampaignId { get; set; }
        public string MediaUrl { get; set; }
        public string Name { get; set; }
        public int? TintColor { get; set; }
        public int? InvertedTintColor { get; set; }
        public string? ResourceNumber { get; set; }

    }
}
