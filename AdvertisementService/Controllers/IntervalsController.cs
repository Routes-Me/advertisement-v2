using AdvertisementService.Abstraction;
using AdvertisementService.DAL;
using AdvertisementService.Models;
using AdvertisementService.Models.DBModels;
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
    public class IntervalsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public IntervalsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Intervals interval)
        {
            try
            {
                var intervals = await new IntervalsDAL(_unitOfWork, _mapper).PostInterval(interval);
                return StatusCode(StatusCodes.Status201Created, ReturnResponse.SuccessResponse(CommonMessage.IntervalInsert, true, interval.IntervalId));
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ReturnResponse.ErrorResponse(e.Message, 400));
            }
        }

        [HttpGet]
        public IActionResult Get([FromQuery] Pagination pagination)
        {
            try
            {
                var GetIntervalsDtoGetResponse = new IntervalsDAL(_unitOfWork, _mapper).GetInterval(pagination);
                return StatusCode(GetIntervalsDtoGetResponse.Code, GetIntervalsDtoGetResponse);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ReturnResponse.ErrorResponse(e.Message, 400));
            }
        }

        [HttpGet]
        [Route("{id?}")]
        public IActionResult GetById(string id)
        {

            try
            {
                var getIntervalsDto = new IntervalsDAL(_unitOfWork, _mapper).GetIntervalById(id);
                return StatusCode(StatusCodes.Status200OK, getIntervalsDto);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ReturnResponse.ErrorResponse(e.Message, 400));
            }

        }

        [HttpDelete]
        [Route("{id}")]
        public IActionResult Delete(string id)
        {
            try
            {
                var interval = new IntervalsDAL(_unitOfWork, _mapper).DeleteInterval(id);
                return StatusCode(StatusCodes.Status200OK, ReturnResponse.SuccessResponse(CommonMessage.IntervalDelete, false, interval.IntervalId));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ReturnResponse.ExceptionResponse(ex));
            }
        }

        [HttpPut]
        public IActionResult Put([FromBody] Intervals interval)
        {
            try
            {
                var _interval = new IntervalsDAL(_unitOfWork, _mapper).UpdateIntervals(interval);
                return StatusCode(StatusCodes.Status200OK, ReturnResponse.SuccessResponse(CommonMessage.IntervalUpdate, false));

            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                return StatusCode(StatusCodes.Status400BadRequest, ReturnResponse.ErrorResponse(ex.Message, 400));
            }
        }
    }
}
