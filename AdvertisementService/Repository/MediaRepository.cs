using AdvertisementService.Abstraction;
using AdvertisementService.Models.DBModels;

namespace AdvertisementService.Repository
{
    public class MediaRepository : GenericRepository<Medias>, IMediaRepository
    {
        public MediaRepository(AdvertisementContext context) : base(context)
        {

        }

    }
}
