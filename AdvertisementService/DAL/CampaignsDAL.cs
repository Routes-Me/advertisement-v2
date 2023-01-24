using AdvertisementService.Abstraction;
using AdvertisementService.Models;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.Dtos;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using RoutesSecurity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static AdvertisementService.Models.Response;

namespace AdvertisementService.DAL
{
    public class CampaignsDAL
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CampaignsDAL(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;

        }

        internal async Task<Campaigns> PostCampaign(Campaigns campaign)
        {
            try
            {
                if (campaign == null)
                {
                    throw new ArgumentNullException(CommonMessage.InvalidData);
                }
                await _unitOfWork.CampaignRepository.PostAsync(campaign);
                _unitOfWork.Save();
                return campaign;
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }

        }

        internal GetResponse<GetCampaignDto> GetAllCampaigns(Pagination pagination)
        {
            try
            {
                GetResponse<GetCampaignDto> getCampaignDto = new GetResponse<GetCampaignDto>();
                List<GetCampaignDto> getCampaignDtoList = new List<GetCampaignDto>();
                List<Campaigns> campaigns = new List<Campaigns>();
                if (MarkInactive())
                {
                    campaigns = _unitOfWork.CampaignRepository.Get(pagination, x => x.Status == "active").ToList();
                    getCampaignDtoList = _mapper.Map<List<GetCampaignDto>>(campaigns);
                    foreach (var campaign in getCampaignDtoList)
                    {
                        campaign.CampaignId = Obfuscation.Encode(Convert.ToInt32(campaign.CampaignId));
                    }

                }

                getCampaignDto.Status = true;
                getCampaignDto.Message = CommonMessage.CampaignRetrived;
                getCampaignDto.Pagination = pagination;
                getCampaignDto.Data = getCampaignDtoList;
                getCampaignDto.Code = StatusCodes.Status200OK;

                return getCampaignDto;
            }
            catch (Exception ex)
            {

                return ReturnResponse.ExceptionResponse(ex);
            }
        }
        internal GetCampaignDto GetCampaignById(string id)
        {
            try
            {
                var getCampaignDto = new GetCampaignDto();
                if (MarkInactive())
                {
                    var campaign = _unitOfWork.CampaignRepository.GetById(Obfuscation.Decode(id));

                    if (campaign == null)
                        throw new Exception(CommonMessage.CampaignNotFound);

                    getCampaignDto.CampaignId = Obfuscation.Encode(Convert.ToInt32(campaign.CampaignId));
                    getCampaignDto.Title = campaign.Title;
                    getCampaignDto.Status = campaign.Status;
                    getCampaignDto.StartAt = campaign.StartAt;
                    getCampaignDto.EndAt = campaign.EndAt;
                }

                return getCampaignDto;
            }
            catch (Exception ex)
            {

                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        internal Campaigns DeleteCampaign(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception(CommonMessage.InvalidData);

                _unitOfWork.BeginTransaction();

                var campaign = _unitOfWork.CampaignRepository.GetById(x => x.CampaignId == Obfuscation.Decode(id), null, x => x.Broadcasts);

                if (campaign == null)
                    throw new Exception(CommonMessage.CampaignNotFound);

                if (campaign.Broadcasts != null)
                {
                    foreach (var broadcast in campaign.Broadcasts)
                    {
                        _unitOfWork.BroadcastRepository.Delete(broadcast.BroadcastId);
                    }
                }

                _unitOfWork.CampaignRepository.Delete(campaign.CampaignId);
                _unitOfWork.Save();
                _unitOfWork.Commit();
                return campaign;
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                return ReturnResponse.ExceptionResponse(ex);
            }
        }
        public bool MarkInactive()
        {
            try
            {
                List<Campaigns> campaignsList = new List<Campaigns>();

                Pagination pagination = new Pagination();
                campaignsList = _unitOfWork.CampaignRepository.Get(pagination, x => x.Status == "active").ToList();

                foreach (var campaign in campaignsList)
                {
                    if (campaign.EndAt < DateTime.Now)
                    {
                        campaign.Status = "inactive";
                        _unitOfWork.CampaignRepository.Put(campaign);
                        _unitOfWork.Save();
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal Response UpdateCampaign(GetCampaignDto model)
        {
            try
            {
                var campaign = _unitOfWork.CampaignRepository.GetById(Obfuscation.Decode(model.CampaignId));
                if (campaign == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.CampaignNotFound, StatusCodes.Status404NotFound);
                else
                {
                    campaign.StartAt = model.StartAt;
                    campaign.EndAt = model.EndAt;
                    campaign.Title = model.Title;
                    campaign.Status = model.Status ?? "active";
                    campaign.UpdatedAt = DateTime.Now;
                    _unitOfWork.CampaignRepository.Put(campaign);
                    _unitOfWork.Save();
                    return ReturnResponse.SuccessResponse(CommonMessage.CampaignUpdate, false);
                }

            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        internal GetResponse<GetCampaignAdvertisementDto> GetAdvertisementsByCampaignId(string id, string advertisementsId)
        {
            try
            {
                var campaign = new Campaigns();
                if (string.IsNullOrEmpty(id) && string.IsNullOrEmpty(advertisementsId))
                    return ReturnResponse.ErrorResponse(CommonMessage.InvalidData, StatusCodes.Status400BadRequest);
                else if (string.IsNullOrEmpty(advertisementsId))
                    campaign = _unitOfWork.CampaignRepository.GetById(x => x.CampaignId == Obfuscation.Decode(id), null, x => x.Broadcasts);
                else
                    campaign = _unitOfWork.CampaignRepository.GetById(x => x.CampaignId == Obfuscation.Decode(id) && x.Broadcasts.FirstOrDefault().AdvertisementId == Obfuscation.Decode(advertisementsId), null, x => x.Broadcasts);
                if (campaign == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.CampaignNotFound, StatusCodes.Status404NotFound);
                else
                {
                    var campaignAdvertisementList = new List<GetCampaignAdvertisementDto>();
                    foreach (var broadcast in campaign.Broadcasts)
                    {
                        var advertisement = _unitOfWork.AdvertisementRepository.GetById(x => x.AdvertisementId == broadcast.AdvertisementId);

                        var campaignAdvertisement = new GetCampaignAdvertisementDto();
                        campaignAdvertisement.CampaignId = Obfuscation.Encode(Convert.ToInt32(campaign.CampaignId));
                        campaignAdvertisement.Title = campaign.Title;
                        campaignAdvertisement.Status = campaign.Status;
                        campaignAdvertisement.StartAt = campaign.StartAt;
                        campaignAdvertisement.EndAt = campaign.EndAt;
                        campaignAdvertisement.CreatedAt = campaign.CreatedAt;
                        campaignAdvertisement.UpdatedAt = campaign.UpdatedAt;

                        campaignAdvertisement.Advertisements = new GetAdvertisementsDto
                        {
                            AdvertisementId = Obfuscation.Encode(advertisement.AdvertisementId),
                            ResourceNumber = advertisement.ResourceNumber,
                            Name = advertisement.Name,
                            InstitutionId = Obfuscation.Encode(Convert.ToInt32(advertisement.InstitutionId)),
                            CreatedAt = advertisement.CreatedAt,
                            TintColor = advertisement.TintColor,
                            InvertedTintColor = advertisement.InvertedTintColor
                        };
                        campaignAdvertisementList.Add(campaignAdvertisement);
                    }


                    var response = new GetResponse<GetCampaignAdvertisementDto>
                    {
                        Code = 200,
                        Status = true,
                        Message = CommonMessage.CampaignRetrived,
                        Data = campaignAdvertisementList
                    };
                    return response;
                }
            }
            catch (Exception ex)
            {

                return ReturnResponse.ExceptionResponse(ex);
            }
        }

    }
}
