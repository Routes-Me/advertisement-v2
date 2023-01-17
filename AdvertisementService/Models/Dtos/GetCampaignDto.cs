using System;

namespace AdvertisementService.Models.Dtos
{
    public class GetCampaignDto
    {
        public string CampaignId { get; set; }
        public string Title { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public string Status { get; set; }
        public int? Sort { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
