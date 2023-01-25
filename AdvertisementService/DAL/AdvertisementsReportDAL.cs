using AdvertisementService.Abstraction;
using AdvertisementService.Models.Dtos;
using AutoMapper;
using System.Collections.Generic;

namespace AdvertisementService.DAL
{
    public class AdvertisementsReportDAL
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdvertisementsReportDAL(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public List<GetAdvertisementReportDto> ReportAdvertisements(List<int> advertisementIds, List<string> attributes)
        {
            var getAdvertisementReportDtoList = new List<GetAdvertisementReportDto>();
            foreach (var advertisementId in advertisementIds)
            {
                var advertisement = _unitOfWork.AdvertisementRepository.GetById(x => x.AdvertisementId == advertisementId);
                var getAdvertisementReportDto = new GetAdvertisementReportDto();
                getAdvertisementReportDto.AdvertisementId = advertisement.AdvertisementId;
                getAdvertisementReportDto.ResourceNumber = attributes.Contains(nameof(advertisement.ResourceNumber)) ? advertisement.ResourceNumber : null;
                getAdvertisementReportDto.Name = attributes.Contains(nameof(advertisement.Name)) ? advertisement.Name : null;
                getAdvertisementReportDto.InvertedTintColor = attributes.Contains(nameof(advertisement.InvertedTintColor)) ? advertisement.InvertedTintColor : null;
                getAdvertisementReportDto.TintColor = attributes.Contains(nameof(advertisement.TintColor)) ? advertisement.TintColor : null;
                getAdvertisementReportDtoList.Add(getAdvertisementReportDto);
            }
            return getAdvertisementReportDtoList;
        }
    }
}
