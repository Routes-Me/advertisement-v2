using AdvertisementService.Abstraction;
using AdvertisementService.Models.DBModels;

namespace AdvertisementService.Repository
{
    public class AdvertisementRepository : GenericRepository<Advertisements>, IAdvertisementRepository
    {
        public AdvertisementRepository(AdvertisementContext context) : base(context)
        {
        }
    }

}
