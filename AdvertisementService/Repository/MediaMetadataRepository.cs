using AdvertisementService.Abstraction;
using AdvertisementService.Models.DBModels;

namespace AdvertisementService.Repository
{
    public class MediaMetadataRepository : GenericRepository<MediaMetadata>, IMediaMetadataRepository
    {
        public MediaMetadataRepository(AdvertisementContext context) : base(context)
        {

        }
    }
}
