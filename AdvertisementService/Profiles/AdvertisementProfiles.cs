using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.Dtos;
using AutoMapper;

namespace AdvertisementService.Profiles
{
    public class AdvertisementProfiles : Profile
    {
        public AdvertisementProfiles()
        {
            //Read Campaigns
            CreateMap<Campaigns, GetCampaignDto>();

            //Read Media
            CreateMap<Medias, GetMediaDto>().IncludeMembers(s => s.MediaMetadata);
            CreateMap<MediaMetadata, GetMediaDto>(MemberList.None);

            //Read Interval
            CreateMap<Intervals, GetIntervalsDto>();

        }
    }
}
