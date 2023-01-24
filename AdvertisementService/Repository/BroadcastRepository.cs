using AdvertisementService.Abstraction;
using AdvertisementService.Models.DBModels;

namespace AdvertisementService.Repository
{
    public class BroadcastRepository : GenericRepository<Broadcasts>, IBroadcastRepository
    {
        public BroadcastRepository(AdvertisementContext context) : base(context)
        {

        }
    }
}
