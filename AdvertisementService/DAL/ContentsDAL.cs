using AdvertisementService.Abstraction;
using AdvertisementService.Models.Common;
using AutoMapper;

namespace AdvertisementService.DAL
{
    public class ContentsDAL
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Dependencies _dependencies;
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;
        private readonly CampaignsDAL _campaignsDAL;
        public ContentsDAL(IUnitOfWork unitOfWork, Dependencies dependencies, AppSettings appSettings, IMapper mapper, CampaignsDAL campaignsDAL)
        {
            _unitOfWork = unitOfWork;
            _dependencies = dependencies;
            _appSettings = appSettings;
            _mapper = mapper;
            _campaignsDAL = campaignsDAL;
        }

        //public async Task<GetResponse<GetContentDto>> GetContent(Pagination pagination)
        //{
        //    try
        //    {
        //        var response = new GetResponse<GetContentDto>();
        //        var getContentDtoList = new List<GetContentDto>();

        //        if (_campaignsDAL.MarkInactive())
        //        {
        //            var broadcastsList = await _unitOfWork.BroadcastRepository.GetAsync(pagination, x => x.Campaign.Status == "active" && x.Campaign.StartAt <= DateTime.Now && x.Campaign.EndAt >= DateTime.Now, null, x => x.Campaign, x => x.Advertisement, x => x.Advertisement.Medias);

        //            foreach (var broadcast in broadcastsList)
        //            {

        //                var getContentDto = new GetContentDto
        //                {
        //                    ContentId = Obfuscation.Encode(broadcast.AdvertisementId),
        //                    ResourceNumber = broadcast.Advertisement.ResourceNumber,
        //                    Name = broadcast.Advertisement.Name,
        //                    TintColor = broadcast.Advertisement.TintColor
        //                };
        //                for (int i = 0; i < broadcast.Advertisement.Medias.Count; i++)
        //                {
        //                    getContentDto.Type = broadcast.Advertisement.Medias[i].MediaType.ToString();
        //                    getContentDto.Url = broadcast.Advertisement.Medias[i].Url;
        //                }
        //                getContentDtoList.Add(getContentDto);
        //            }
        //            if (getContentDtoList.Count > 0)
        //            {
        //                var promotionsGetModelList = APIExtensions.GetPromotionsContents(getContentDtoList, _appSettings.Host + _dependencies.CouponsUrl);
        //                if (promotionsGetModelList != null && promotionsGetModelList.Count > 0)
        //                {
        //                    foreach (var content in getContentDtoList)
        //                    {
        //                        foreach (var promotion in promotionsGetModelList)
        //                        {
        //                            if (content.ContentId == promotion.AdvertisementId)
        //                            {
        //                                var getPromotionDto = new GetPromotionDto
        //                                {
        //                                    PromotionId = promotion.PromotionId,
        //                                    Title = promotion.Title,
        //                                    Subtitle = promotion.Subtitle
        //                                };
        //                                if (promotion.Type.ToLower() == "links")
        //                                {
        //                                    getPromotionDto.Link = _appSettings.LinkUrlForContent + promotion.PromotionId;
        //                                }
        //                                if (promotion.Type.ToLower() == "coupons")
        //                                {
        //                                    getPromotionDto.Link = _appSettings.CouponUrlForContent + promotion.PromotionId;
        //                                }
        //                                if (promotion.Type.ToLower() == "places")
        //                                {
        //                                    getPromotionDto.Link = null;
        //                                }
        //                                content.Promotion = getPromotionDto;

        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        response.Status = true;
        //        response.Message = CommonMessage.ContentsRetrive;
        //        response.Pagination = pagination;
        //        response.Data = getContentDtoList;
        //        response.Code = StatusCodes.Status200OK;

        //        return response;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }

        //}
    }
}