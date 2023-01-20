using AdvertisementService.Abstraction;
using AdvertisementService.DAL;
using AdvertisementService.Models.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using static AdvertisementService.Models.Response;

namespace AdvertisementService.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/")]
    public class AdvertisementsReportController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public AdvertisementsReportController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        [Route("advertisements/reports")]
        public IActionResult ReportAdvertisements(List<int> advertisementIds, [FromQuery] List<string> attr)
        {
            var getAdvertisementReportsDto = new GetResponse<GetAdvertisementReportDto>();
            try
            {
                getAdvertisementReportsDto.Data = new AdvertisementsReportDAL(_unitOfWork).ReportAdvertisements(advertisementIds, attr);

                return StatusCode(StatusCodes.Status201Created, getAdvertisementReportsDto.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ReturnResponse.ExceptionResponse(ex));
            }
        }
    }
}
