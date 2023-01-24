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
    public class IntervalsDAL
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public IntervalsDAL(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;

        }

        internal async Task<Intervals> PostInterval(Intervals interval)
        {
            try
            {
                if (string.IsNullOrEmpty(interval.Title))
                {
                    throw new Exception(CommonMessage.InvalidData);
                }
                await _unitOfWork.IntervalRepository.PostAsync(interval);
                _unitOfWork.Save();
                return interval;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal GetResponse<GetIntervalsDto> GetInterval(Pagination pagination)
        {
            GetResponse<GetIntervalsDto> getIntervalsDtoResponse = new GetResponse<GetIntervalsDto>();
            List<GetIntervalsDto> getIntervalsDtoList = new List<GetIntervalsDto>();

            List<Intervals> intervalsList = _unitOfWork.IntervalRepository.Get(pagination).ToList();
            getIntervalsDtoList = _mapper.Map<List<GetIntervalsDto>>(intervalsList);
            foreach (var interval in getIntervalsDtoList)
            {
                interval.IntervalId = Obfuscation.Encode(Convert.ToInt32(interval.IntervalId));
            }

            getIntervalsDtoResponse.Status = true;
            getIntervalsDtoResponse.Message = CommonMessage.IntervalRetrived;
            getIntervalsDtoResponse.Pagination = pagination;
            getIntervalsDtoResponse.Data = getIntervalsDtoList;
            getIntervalsDtoResponse.Code = StatusCodes.Status200OK;

            return getIntervalsDtoResponse;
        }

        internal GetIntervalsDto GetIntervalById(string id)
        {
            var interval = _unitOfWork.IntervalRepository.GetById(Obfuscation.Decode(id));

            if (interval == null)
                throw new Exception(CommonMessage.IntervalNotFound);

            var getIntervalsDto = new GetIntervalsDto
            {
                Title = interval.Title
            };
            return getIntervalsDto;
        }

        internal Intervals DeleteInterval(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception(CommonMessage.InvalidData);

                _unitOfWork.BeginTransaction();
                var interval = _unitOfWork.IntervalRepository.GetById(x => x.IntervalId == Obfuscation.Decode(id), null, x => x.AdvertisementsIntervals);
                if (interval == null)
                    throw new Exception(CommonMessage.IntervalNotFound);
                if (interval.AdvertisementsIntervals != null)
                {
                    _unitOfWork.AdvertisementsIntervalRepository.RemoveRange(interval.AdvertisementsIntervals);

                }
                _unitOfWork.IntervalRepository.Remove(interval);
                _unitOfWork.Save();
                _unitOfWork.Commit();
                return interval;
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                throw ex;
            }
        }

        internal Response UpdateIntervals(Intervals interval)
        {
            try
            {
                var intervalData = _unitOfWork.IntervalRepository.GetById(x => x.IntervalId == interval.IntervalId);
                if (intervalData == null)
                    return ReturnResponse.ErrorResponse(CommonMessage.IntervalNotFound, StatusCodes.Status404NotFound);

                intervalData.Title = interval.Title;
                _unitOfWork.IntervalRepository.Put(intervalData);
                _unitOfWork.Save();
                return ReturnResponse.SuccessResponse(CommonMessage.IntervalUpdate, false);
            }
            catch (Exception ex)
            {
                return ReturnResponse.ExceptionResponse(ex);
            }
        }
    }
}
