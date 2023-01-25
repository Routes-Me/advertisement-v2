using AdvertisementService.Abstraction;
using AdvertisementService.Models.DBModels;

namespace AdvertisementService.Repository
{
    public class CampaignRepository : GenericRepository<Campaigns>, ICampaignRepository
    {
        public CampaignRepository(AdvertisementContext context) : base(context)
        {
        }
    }
}
