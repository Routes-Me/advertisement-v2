using AdvertisementService.Abstraction;
using AdvertisementService.DAL;
using AdvertisementService.Models.Common;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AdvertisementService.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    public class ContentsController : ControllerBase
    {
        private readonly Dependencies _dependencies;
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;
        private readonly CampaignsDAL _campaignsDAL;


        public ContentsController(IUnitOfWork unitOfWork, IOptions<Dependencies> dependencies, IOptions<AppSettings> appSettings, IMapper mapper, CampaignsDAL campaignsDAL)
        {
            _unitOfWork = unitOfWork;
            _dependencies = dependencies.Value;
            _appSettings = appSettings.Value;
            _mapper = mapper;
            _campaignsDAL = campaignsDAL;
        }


        //[HttpGet]
        //public async Task<IActionResult> GetContent([FromQuery] Pagination pagination)
        //{
        //    try
        //    {
        //        var getContentDto = await new ContentsDAL(_unitOfWork, _dependencies, _appSettings, _mapper, _campaignsDAL).GetContent(pagination);
        //        return StatusCode(getContentDto.Code, getContentDto);
        //    }
        //    catch (Exception e)
        //    {
        //        return StatusCode(StatusCodes.Status400BadRequest, ReturnResponse.ExceptionResponse(e));
        //    }
        //}
    }
}
