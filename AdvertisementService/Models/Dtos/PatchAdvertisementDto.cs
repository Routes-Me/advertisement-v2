using System.Collections.Generic;

namespace AdvertisementService.Models.Dtos
{
    public class PatchAdvertisementDto
    {
        public int? Sort { get; set; }
    }

    public class PatchAdvertisementDtoList
    {
        public List<PatchAdvertisementDtoListItem> SortItem { get; set; }
    }

    public class PatchAdvertisementDtoListItem
    {
        public string AdvertisementId { get; set; }
        public int Sort { get; set; }
    }

}
