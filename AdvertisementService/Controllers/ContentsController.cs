using AdvertisementService.Abstraction;
using AdvertisementService.DAL;
using AdvertisementService.Models;
using AdvertisementService.Models.Common;
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
    public class ContentsController : ControllerBase
    {
        private readonly Dependencies _dependencies;
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;


        public ContentsController(IUnitOfWork unitOfWork, IOptions<Dependencies> dependencies, IOptions<AppSettings> appSettings, IMapper mapper )
        {
            _unitOfWork = unitOfWork;
            _dependencies = dependencies.Value;
            _appSettings = appSettings.Value;
            _mapper = mapper;
        }


        [HttpGet]
        public async Task<IActionResult> GetContent([FromQuery] Pagination pagination)
        {
            try
            {
                var getContentDto = await new ContentsDAL(_unitOfWork, _dependencies, _appSettings, _mapper).GetContent(pagination);
                return StatusCode(getContentDto.Code, getContentDto);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ReturnResponse.ExceptionResponse(e));
            }
        }

        [HttpGet]
        [Route("{id}")]
        public  IActionResult GetById(string id)
        {
            try
            {
                var response =  new ContentsDAL(_unitOfWork, _dependencies, _appSettings, _mapper).GetByIdContent(id);
                return StatusCode(response.Code, response);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ReturnResponse.ExceptionResponse(ex));
            }
        }
    }
}
