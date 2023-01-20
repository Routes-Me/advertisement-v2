using AdvertisementService.Abstraction;
using AdvertisementService.Controllers;
using AdvertisementService.Extensions;
using AdvertisementService.Models;
using AdvertisementService.Models.Common;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.Dtos;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoutesSecurity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static AdvertisementService.Models.Response;

namespace AdvertisementService.DAL
{
    public class ContentsDAL
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Dependencies _dependencies;
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;
        public ContentsDAL(IUnitOfWork unitOfWork, Dependencies dependencies, AppSettings appSettings, IMapper mapper )
        {
            _unitOfWork = unitOfWork;
            _dependencies = dependencies;
            _appSettings = appSettings;
            _mapper = mapper;
        }

       
        public async Task<GetResponse<GetContentDto>> GetContent(Pagination pagination)
        {
            try
            {
                var response = new GetResponse<GetContentDto>();
                var getContentDtoList = new List<GetContentDto>();

                if (new CampaignsDAL(_unitOfWork, _mapper).MarkInactive())
                {
                    var broadcastsList = await _unitOfWork.BroadcastRepository.GetAsync(pagination, x => x.Campaign.Status == "active" && x.Campaign.StartAt <= DateTime.Now && x.Campaign.EndAt >= DateTime.Now, null, x => x.Campaign, x => x.Advertisement, x => x.Advertisement.Media);

                    foreach (var broadcast in broadcastsList)
                    {

                        var getContentDto = new GetContentDto
                        {
                            ContentId = Obfuscation.Encode(broadcast.AdvertisementId),
                            ResourceNumber = broadcast.Advertisement.ResourceNumber,
                            Name = broadcast.Advertisement.Name,
                            TintColor = broadcast.Advertisement.TintColor,
                            Type = broadcast.Advertisement.Media.MediaType.ToString(),
                            Url = broadcast.Advertisement.Media.Url,
                        };
                        
                        getContentDtoList.Add(getContentDto);
                    }
                    if (getContentDtoList.Count > 0)
                    {
                        var promotionsGetModelList = APIExtensions.GetPromotionsContents(getContentDtoList, _appSettings.Host + _dependencies.CouponsUrl);
                        if (promotionsGetModelList != null && promotionsGetModelList.Count > 0)
                        {
                            foreach (var content in getContentDtoList)
                            {
                                foreach (var promotion in promotionsGetModelList)
                                {
                                    if (content.ContentId == promotion.AdvertisementId)
                                    {
                                        var getPromotionDto = new GetPromotionDto
                                        {
                                            PromotionId = promotion.PromotionId,
                                            Title = promotion.Title,
                                            Subtitle = promotion.Subtitle
                                        };
                                        if (promotion.Type.ToLower() == "links")
                                        {
                                            getPromotionDto.Link = _appSettings.LinkUrlForContent + promotion.PromotionId;
                                        }
                                        if (promotion.Type.ToLower() == "coupons")
                                        {
                                            getPromotionDto.Link = _appSettings.CouponUrlForContent + promotion.PromotionId;
                                        }
                                        if (promotion.Type.ToLower() == "places")
                                        {
                                            getPromotionDto.Link = null;
                                        }
                                        content.Promotion = getPromotionDto;

                                    }
                                }
                            }
                        }
                    }
                }

                response.Status = true;
                response.Message = CommonMessage.ContentsRetrive;
                response.Pagination = pagination;
                response.Data = getContentDtoList;
                response.Code = StatusCodes.Status200OK;

                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

       
        public GetResponseById<GetContentDto> GetByIdContent(string id)
        {
            var response = new GetResponseById<GetContentDto>();
            GetContentDto contentReadDto = new GetContentDto();

            if (new CampaignsDAL(_unitOfWork,_mapper).MarkInactive())
            {
                var broadcast = _unitOfWork.BroadcastRepository.GetById(x => x.Campaign.Status == "active" && x.Campaign.StartAt <= DateTime.Now && x.Campaign.EndAt >= DateTime.Now && x.Advertisement.AdvertisementId == Obfuscation.Decode(id), null, x => x.Advertisement.Media);

                contentReadDto.ContentId = Obfuscation.Encode(broadcast.AdvertisementId);
                contentReadDto.Type = broadcast.Advertisement.Media.MediaType.ToString();
                contentReadDto.Url = broadcast.Advertisement.Media.Url;
                contentReadDto.ResourceNumber = broadcast.Advertisement.ResourceNumber;
                contentReadDto.Name = broadcast.Advertisement.Name;
                contentReadDto.TintColor = broadcast.Advertisement.TintColor;


                if (contentReadDto != null)
                {
                    GetPromotionDto promotionGetModel = APIExtensions.GetPromotionsContentById(contentReadDto, _appSettings.Host + _dependencies.CouponsUrl);

                    if (contentReadDto.ContentId == promotionGetModel.AdvertisementId)
                    {
                        GetPromotionDto promotionReadDto = new GetPromotionDto();
                        promotionReadDto.Title = promotionGetModel.Title;
                        promotionReadDto.Subtitle = promotionGetModel.Subtitle;
                        promotionReadDto.PromotionId = promotionGetModel.PromotionId;
                        if (!string.IsNullOrEmpty(promotionGetModel.Type))
                        {
                            if (promotionGetModel.Type.ToLower() == "links")
                            {
                                promotionReadDto.Link = _appSettings.LinkUrlForContent + promotionGetModel.PromotionId;
                            }
                            if (promotionGetModel.Type.ToLower() == "coupons")
                            {
                                promotionReadDto.Link = _appSettings.CouponUrlForContent + promotionGetModel.PromotionId;
                            }
                            if (promotionGetModel.Type.ToLower() == "places")
                            {
                                promotionReadDto.Link = null;
                            }
                        }
                        contentReadDto.Promotion = promotionReadDto;
                    }
                }
            }

            response.Status = true;
            response.Message = CommonMessage.ContentsRetrive;
            response.Data = contentReadDto;
            response.Code = StatusCodes.Status200OK;

            return response;
        }
    }
}