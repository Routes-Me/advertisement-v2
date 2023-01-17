using AdvertisementService.Abstraction;
using AdvertisementService.DAL;
using AdvertisementService.Models;
using AdvertisementService.Models.Common;
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
    public class AdvertisementsController : ControllerBase
    {
        private readonly AzureStorageBlobConfig _config;
        private readonly IMediaTypeConversionRepository _mediaTypeConversionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly Dependencies _dependencies;
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;


        public AdvertisementsController(IUnitOfWork unitOfWork, IOptions<AzureStorageBlobConfig> config, IMediaTypeConversionRepository mediaTypeConversionRepository, IOptions<Dependencies> dependencies, IOptions<AppSettings> appSettings, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mediaTypeConversionRepository = mediaTypeConversionRepository;
            _dependencies = dependencies.Value;
            _appSettings = appSettings.Value;
            _mapper = mapper;
            _config = config.Value;
        }

        [HttpPost]
        public async Task<ActionResult> Post(PostAdvertisementsDto postAdvertisements)
        {
            try
            {
                var advertisement = await new AdvertisementsDAL(_unitOfWork, _config, _mediaTypeConversionRepository, _dependencies, _appSettings, _mapper).PostAdvertisementAsync(postAdvertisements);
                return StatusCode(StatusCodes.Status201Created, ReturnResponse.SuccessResponse(CommonMessage.AdvertisementInsert, true, advertisement.AdvertisementId));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ReturnResponse.ExceptionResponse(ex));
            }
        }

        [HttpGet]
        public IActionResult Get(string include, [FromQuery] Pagination pagination)
        {
            try
            {
                var getAdvertisementDto = new AdvertisementsDAL(_unitOfWork, _config, _mediaTypeConversionRepository, _dependencies, _appSettings, _mapper).GetAdvertisements(include, pagination);
                return StatusCode(getAdvertisementDto.Code, getAdvertisementDto);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ReturnResponse.ExceptionResponse(e));
            }
        }


        [HttpGet]
        [Route("{id}")]
        public IActionResult GetById(string id, string include)
        {
            try
            {
                GetResponse<GetAdvertisementsDto> getAdvertisementDto = new AdvertisementsDAL(_unitOfWork, _config, _mediaTypeConversionRepository, _dependencies, _appSettings, _mapper).GetAdvertisementById(id, include);
                return StatusCode(getAdvertisementDto.Code, getAdvertisementDto);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ReturnResponse.ExceptionResponse(e));
            }
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await new AdvertisementsDAL(_unitOfWork, _config, _mediaTypeConversionRepository, _dependencies, _appSettings, _mapper).DeleteAdvertisementAsync(id);
                return StatusCode(StatusCodes.Status200OK, ReturnResponse.SuccessResponse(CommonMessage.AdvertisementDelete, false));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ReturnResponse.ExceptionResponse(ex));
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put(PostAdvertisementsDto advertisementsDto)
        {
            try
            {
                var advertisement = await new AdvertisementsDAL(_unitOfWork, _config, _mediaTypeConversionRepository, _dependencies, _appSettings, _mapper).UpdateAdvertisementAsync(advertisementsDto);
                return ReturnResponse.SuccessResponse(CommonMessage.AdvertisementUpdate, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }
    }
}
