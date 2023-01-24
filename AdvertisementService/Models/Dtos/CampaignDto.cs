using System;

namespace AdvertisementService.Models.Dtos
{
    public class CampaignDto
    {
        public string CampaignId { get; set; }
        public string Title { get; set; }
        public string StartAt { get; set; }
        public string EndAt { get; set; }
        public string Status { get; set; }
        public int? Sort { get; set; }
    }
}
