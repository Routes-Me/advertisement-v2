using AdvertisementService.Models.Dtos;
using System.Threading.Tasks;

namespace AdvertisementService.Abstraction
{
    public interface IMediaTypeConversionRepository
    {
        Task<VideoMetadata> ConvertVideoAsync(string filepath);
        Task<float> ConvertImageAsync(string mediaUrl);
    }
}
