using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AdvertisementService.Models.Dtos
{
    public class GetAdvertisementsDto
    {
        public string AdvertisementId { get; set; }
        public string ResourceNumber { get; set; }
        public string Name { get; set; }
        public string InstitutionId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string MediaId { get; set; }
        public List<string> CampaignId { get; set; }
        public string IntervalId { get; set; }
        public int? TintColor { get; set; }
        public int? InvertedTintColor { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Sort { get; set; }
    }
}
