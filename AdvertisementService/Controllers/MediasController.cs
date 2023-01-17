using AdvertisementService.Abstraction;
using AdvertisementService.DAL;
using AdvertisementService.Models;
using AdvertisementService.Models.Common;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.Dtos;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using static AdvertisementService.Models.Response;

namespace AdvertisementService.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    public class MediasController : ControllerBase
    {
        private readonly AzureStorageBlobConfig _config;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public MediasController(IUnitOfWork unitOfWork, IOptions<AzureStorageBlobConfig> config, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _config = config.Value;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Get([FromQuery] Pagination pagination)
        {
            try
            {
                GetResponse<GetMediaDto> getMediaDto = new MediaDAL(_unitOfWork, _config, _mapper).GetMedia(pagination);
                return StatusCode(getMediaDto.Code, getMediaDto);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ReturnResponse.ExceptionResponse(e));
            }
        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult GetById(string id)
        {
            try
            {
                var getMediaDto = new MediaDAL(_unitOfWork, _config, _mapper).GetMediaById(id);
                return StatusCode(StatusCodes.Status200OK, getMediaDto);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ReturnResponse.ErrorResponse(e.Message, 400));
            }
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Post([FromBody] Medias media)
        {
            try
            {
                var _media = await new MediaDAL(_unitOfWork, _config, _mapper).PostMediaAsync(media);
                return StatusCode(StatusCodes.Status201Created, ReturnResponse.SuccessResponse(CommonMessage.MediaInsert, true, _media.MediaId));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ReturnResponse.ExceptionResponse(ex));
            }
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await new MediaDAL(_unitOfWork, _config, _mapper).DeleteMediaAsync(id);
                return StatusCode(StatusCodes.Status200OK, ReturnResponse.SuccessResponse(CommonMessage.MediaDelete, false));
            }
            catch (Exception e)
            {
                _unitOfWork.Rollback();
                return StatusCode(StatusCodes.Status400BadRequest, ReturnResponse.ErrorResponse(e.Message, 400));
            }

        }

    }
}
