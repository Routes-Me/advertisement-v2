using AdvertisementService.Abstraction;
using AdvertisementService.Models;
using AdvertisementService.Models.Dtos;
using Microsoft.AspNetCore.Http;
using RoutesSecurity;
using System;
using static AdvertisementService.Models.Response;

namespace AdvertisementService.DAL
{
    public class BroadcastsDAL
    {
        private readonly IUnitOfWork _unitOfWork;

        public BroadcastsDAL(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        internal object UpdateCampaignAdvertisement(string campaignsId, string advertisementsId, PatchAdvertisementDto model)
        {
            try
            {
                if (string.IsNullOrEmpty(campaignsId))
                {
                    throw new ArgumentException($"'{nameof(campaignsId)}' cannot be null or empty.", nameof(campaignsId));
                }

                if (string.IsNullOrEmpty(advertisementsId))
                {
                    throw new ArgumentException($"'{nameof(advertisementsId)}' cannot be null or empty.", nameof(advertisementsId));
                }

                if (model is null)
                {
                    throw new ArgumentNullException(nameof(model));
                }

                var broadcast = _unitOfWork.BroadcastRepository.GetById(x => x.AdvertisementId == Obfuscation.Decode(advertisementsId) && x.CampaignId == Obfuscation.Decode(campaignsId), null);
                if (broadcast == null)
                {
                    return ReturnResponse.ErrorResponse(CommonMessage.BroadcastNotFound, StatusCodes.Status404NotFound);
                }
                else
                {
                    broadcast.Sort = model.Sort;
                    _unitOfWork.BroadcastRepository.Put(broadcast);
                    _unitOfWork.Save();
                    return ReturnResponse.SuccessResponse(CommonMessage.SortUpdate, false);
                }
            }
            catch (Exception ex)
            {

                return ReturnResponse.ExceptionResponse(ex);
            }
        }

        public object UpdateCampaignAdvertisementList(string campaignsId, PatchAdvertisementDtoList model)
        {
            try
            {
                if (string.IsNullOrEmpty(campaignsId))
                    return ReturnResponse.ErrorResponse(CommonMessage.CampaignRequired, StatusCodes.Status400BadRequest);

                if (model == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.InvalidData, StatusCodes.Status400BadRequest);



                foreach (var item in model.SortItem)
                {
                    if (string.IsNullOrEmpty(item.AdvertisementId))
                        return ReturnResponse.ErrorResponse(CommonMessage.CampaignRequired, StatusCodes.Status400BadRequest);


                    var broadcast = _unitOfWork.BroadcastRepository.GetById(x => x.AdvertisementId == Obfuscation.Decode(item.AdvertisementId) && x.CampaignId == Obfuscation.Decode(campaignsId), null);
                    if (broadcast == null)
                    {
                        return ReturnResponse.ErrorResponse(CommonMessage.BroadcastNotFound, StatusCodes.Status404NotFound);
                    }
                    else
                    {
                        broadcast.Sort = item.Sort;
                        _unitOfWork.BroadcastRepository.Put(broadcast);
                        _unitOfWork.Save();
                    }
                }
                return ReturnResponse.SuccessResponse(CommonMessage.SortUpdate, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }
    }
}
