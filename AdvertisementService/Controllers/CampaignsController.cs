using AdvertisementService.Abstraction;
using AdvertisementService.DAL;
using AdvertisementService.Models;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.Dtos;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using static AdvertisementService.Models.Response;

namespace AdvertisementService.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    public class CampaignsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public CampaignsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        [HttpPost]
        public async Task<ActionResult> Post(Campaigns campaign)
        {
            try
            {
                var campaigns = await new CampaignsDAL(_unitOfWork, _mapper).PostCampaign(campaign);
                return StatusCode(StatusCodes.Status200OK, ReturnResponse.SuccessResponse(CommonMessage.CampaignInsert, true, campaigns.CampaignId));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ReturnResponse.ExceptionResponse(ex));
            }
        }

        [HttpGet]
        public IActionResult Get([FromQuery] Pagination pagination)
        {
            try
            {
                var getCampaignReadDtoResponse = new CampaignsDAL(_unitOfWork, _mapper).GetAllCampaigns(pagination);
                return StatusCode(getCampaignReadDtoResponse.Code, getCampaignReadDtoResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ReturnResponse.ExceptionResponse(ex));
            }
        }


        [HttpGet]
        [Route("{id}")]
        public IActionResult GetById(string id)
        {
            try
            {
                var campaignReadDto = new CampaignsDAL(_unitOfWork, _mapper).GetCampaignById(id);
                return StatusCode(StatusCodes.Status200OK, campaignReadDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ReturnResponse.ExceptionResponse(ex));
            }
        }

        [HttpDelete]
        [Route("{id}")]
        public IActionResult Delete(string id)
        {
            try
            {
                var campaign = new CampaignsDAL(_unitOfWork, _mapper).DeleteCampaign(id);
                return StatusCode(StatusCodes.Status200OK, ReturnResponse.SuccessResponse(CommonMessage.CampaignDelete, false, campaign.CampaignId));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ReturnResponse.ExceptionResponse(ex));
            }
        }


        [HttpPut]
        public IActionResult Put(GetCampaignDto model)
        {
            try
            {
                var campaign = new CampaignsDAL(_unitOfWork, _mapper).UpdateCampaign(model);
                return StatusCode(StatusCodes.Status200OK, ReturnResponse.SuccessResponse(CommonMessage.CampaignUpdate, false));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ReturnResponse.ExceptionResponse(ex));
            }
        }





        [HttpGet]
        [Route("{id}/advertisements/{advertisementsId?}")]
        public IActionResult GetAdvertisementsByCampaignId(string id, string advertisementsId)
        {
            try
            {
                var advertisementCampaign = new CampaignsDAL(_unitOfWork, _mapper).GetAdvertisementsByCampaignId(id, advertisementsId);
                return StatusCode(StatusCodes.Status200OK, advertisementCampaign);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ReturnResponse.ExceptionResponse(ex));
            }
        }




        #region Update Sort Values

        [HttpPatch]
        [Route("{campaignsId}/advertisements/{advertisementsId}")]
        public IActionResult Patch(string campaignsId, string advertisementsId, PatchAdvertisementDto model)
        {
            try
            {
                var response = new BroadcastsDAL(_unitOfWork).UpdateCampaignAdvertisement(campaignsId, advertisementsId, model);
                return StatusCode(StatusCodes.Status200OK, response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ReturnResponse.ExceptionResponse(ex));
            }
        }

        [HttpPatch]
        [Route("{campaignsId}/advertisements")]
        public IActionResult UpdateCampaignAdvertisementList(string campaignsId, PatchAdvertisementDtoList model)
        {
            try
            {
                var response = new BroadcastsDAL(_unitOfWork).UpdateCampaignAdvertisementList(campaignsId, model);
                return StatusCode(StatusCodes.Status200OK, response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ReturnResponse.ExceptionResponse(ex));
            }

        }
        #endregion
    }
}
