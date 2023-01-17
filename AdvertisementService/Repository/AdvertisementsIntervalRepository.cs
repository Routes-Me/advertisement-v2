using AdvertisementService.Abstraction;
using AdvertisementService.Models.DBModels;

namespace AdvertisementService.Repository
{
    public class AdvertisementsIntervalRepository : GenericRepository<AdvertisementsIntervals>, IAdvertisementsIntervalRepository
    {
        public AdvertisementsIntervalRepository(AdvertisementContext context) : base(context)
        {

        }
    }
}
