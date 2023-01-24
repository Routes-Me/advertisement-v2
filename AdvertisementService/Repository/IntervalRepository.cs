using AdvertisementService.Abstraction;
using AdvertisementService.Models.DBModels;

namespace AdvertisementService.Repository
{
    public class IntervalRepository : GenericRepository<Intervals>, IIntervalRepository
    {
        public IntervalRepository(AdvertisementContext context) : base(context)
        {
        }

    }
}
