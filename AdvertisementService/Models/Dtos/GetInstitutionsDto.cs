using System;
using System.Collections.Generic;

namespace AdvertisementService.Models.Dtos
{
    public class GetInstitutionsDto
    {
        public string InstitutionId { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string PhoneNumber { get; set; }
        public string CountryIso { get; set; }
        public List<string> Services { get; set; }
    }
}
