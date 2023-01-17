using AdvertisementService.Abstraction;
using AdvertisementService.Extensions;
using AdvertisementService.Models;
using AdvertisementService.Models.Common;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.Dtos;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RoutesSecurity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static AdvertisementService.Models.Response;

namespace AdvertisementService.DAL
{
    public class AdvertisementsDAL
    {
        private readonly AzureStorageBlobConfig _config;
        private readonly IMediaTypeConversionRepository _mediaTypeConversionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly Dependencies _dependencies;
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;

        public AdvertisementsDAL(IUnitOfWork unitOfWork, AzureStorageBlobConfig config, IMediaTypeConversionRepository mediaTypeConversionRepository, Dependencies dependencies, AppSettings appSettings, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mediaTypeConversionRepository = mediaTypeConversionRepository;
            _dependencies = dependencies;
            _appSettings = appSettings;
            _mapper = mapper;
            _config = config;
        }

        internal async Task DeleteAdvertisementAsync(string id)
        {
            try
            {
                var advertisement = _unitOfWork.AdvertisementRepository.GetById(x => x.AdvertisementId == Obfuscation.Decode(id), null, x => x.AdvertisementsIntervals, x => x.Broadcasts);



                if (advertisement == null)
                    throw new Exception(CommonMessage.AdvertisementNotFound);

                var medias = _unitOfWork.MediaRepository.Get(null, x => x.AdvertisementId == advertisement.AdvertisementId, null, x => x.MediaMetadata).ToList();


                _unitOfWork.BeginTransaction();

                if (CloudStorageAccount.TryParse(_config.StorageConnection, out CloudStorageAccount storageAccount))
                {
                    CloudBlobClient BlobClient = storageAccount.CreateCloudBlobClient();
                    CloudBlobContainer container = BlobClient.GetContainerReference(_config.Container);
                    if (await container.ExistsAsync())
                    {
                        foreach (var media in medias)
                        {
                            var mediaReferenceName = media.Url.Split('/');
                            CloudBlob file = container.GetBlobReference(mediaReferenceName.LastOrDefault());
                            if (await file.ExistsAsync())
                                await file.DeleteAsync();

                            await _unitOfWork.MediaMetadataRepository.DeleteAsync(Convert.ToInt32(media.MediaMetadataId));
                            _unitOfWork.Save();
                        }

                    }
                }
                await _unitOfWork.AdvertisementRepository.DeleteAsync(advertisement.AdvertisementId);
                _unitOfWork.Save();

                _unitOfWork.Commit();

            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                throw ex;
            }


            //APIExtensions.DeleteApi(_appSettings.Host + _dependencies.PromotionsByAdvertisementUrl + id);





        }

        internal GetResponse<GetAdvertisementsDto> GetAdvertisementById(string id, string include)
        {
            GetResponse<GetAdvertisementsDto> response = new GetResponse<GetAdvertisementsDto>();
            List<GetAdvertisementsDto> getAdvertisementsDtoList = new List<GetAdvertisementsDto>();
            dynamic includeData = new JObject();
            List<GetMediaDto> getMediaDtoList = new List<GetMediaDto>();
            List<GetCampaignDto> getCampaignDtoList = new List<GetCampaignDto>();

            var advertisement = _unitOfWork.AdvertisementRepository.GetById(x => x.AdvertisementId == Obfuscation.Decode(id), null, x => x.AdvertisementsIntervals, x => x.Broadcasts);
            List<GetCampaignDto> lstItems = new List<GetCampaignDto>();

            GetAdvertisementsDto getAdvertisementsDto = new GetAdvertisementsDto
            {
                AdvertisementId = Obfuscation.Encode(advertisement.AdvertisementId),
                ResourceNumber = advertisement.ResourceNumber,
                Name = advertisement.Name,
                InstitutionId = Obfuscation.Encode(Convert.ToInt32(advertisement.InstitutionId)),
                CreatedAt = advertisement.CreatedAt,
                TintColor = advertisement.TintColor,
                InvertedTintColor = advertisement.InvertedTintColor
            };

            getAdvertisementsDto.Campaign = new List<GetCampaignDto>();

            foreach (var broadcast in advertisement.Broadcasts)
            {
                var campaign = _unitOfWork.CampaignRepository.GetById(x => x.CampaignId == broadcast.CampaignId, null, x => x.Broadcasts);
                GetCampaignDto getCampaignDto = _mapper.Map<GetCampaignDto>(campaign);
                getCampaignDto.Sort = campaign.Broadcasts.FirstOrDefault(x => x.AdvertisementId == advertisement.AdvertisementId).Sort;
                getCampaignDto.CampaignId = Obfuscation.Encode(Convert.ToInt32(getCampaignDto.CampaignId));

                getAdvertisementsDto.Campaign.Add(getCampaignDto);
            }
            getAdvertisementsDto.IntervalId = Obfuscation.Encode(advertisement.AdvertisementsIntervals.Select(x => x.IntervalId).FirstOrDefault());
            getAdvertisementsDto.TintColor = advertisement.TintColor;
            getAdvertisementsDto.InvertedTintColor = advertisement.InvertedTintColor;
            getAdvertisementsDtoList.Add(getAdvertisementsDto);


            if (!string.IsNullOrEmpty(include) && getAdvertisementsDtoList.Count > 0)
            {
                string[] includeArr = include.Split(',');
                if (includeArr.Length > 0)
                {
                    foreach (var included in includeArr)
                    {
                        if (included.ToLower() == "institution" || included.ToLower() == "institutions")
                        {
                            includeData.institution = APIExtensions.GetInstitutionsIncludedData(getAdvertisementsDtoList, _appSettings.Host + _dependencies.InstitutionUrl);
                        }
                        else if (included.ToLower() == "media" || included.ToLower() == "media")
                        {

                            var media = _unitOfWork.MediaRepository.GetById(x => x.AdvertisementId == advertisement.AdvertisementId, null, x => x.MediaMetadata);

                            if (media != null)
                            {
                                GetMediaDto getMedia = _mapper.Map<GetMediaDto>(media);
                                getMedia.MediaId = Obfuscation.Encode(Convert.ToInt32(media.MediaId));
                                getMediaDtoList.Add(getMedia);

                            }
                            includeData.media = JArray.Parse(JsonConvert.SerializeObject(getMediaDtoList.GroupBy(x => x.MediaId).Select(x => x.First()).ToList()));
                        }
                        else if (included.ToLower() == "campaign" || included.ToLower() == "campaigns")
                        {

                            foreach (var broadcast in advertisement.Broadcasts)
                            {
                                var campaign = _unitOfWork.CampaignRepository.GetById(x => x.CampaignId == broadcast.CampaignId);
                                GetCampaignDto getCampaignDto = _mapper.Map<GetCampaignDto>(campaign);
                                getCampaignDto.CampaignId = Obfuscation.Encode(Convert.ToInt32(getCampaignDto.CampaignId));
                                getCampaignDtoList.Add(getCampaignDto);
                            }
                            includeData.campaign = JArray.Parse(JsonConvert.SerializeObject(getCampaignDtoList.GroupBy(x => x.CampaignId).Select(x => x.First()).ToList()));
                        }
                        else if (included.ToLower() == "promotion" || included.ToLower() == "promotions")
                        {
                            includeData.promotion = APIExtensions.GetPromotionsAdvertisement(getAdvertisementsDtoList, _appSettings.Host + _dependencies.PromotionReportUrl);
                        }
                    }
                }
            }

            response.Status = true;
            response.Message = CommonMessage.AdvertisementRetrived;
            response.Data = getAdvertisementsDtoList;
            response.Included = includeData;
            response.Code = StatusCodes.Status200OK;

            return response;
        }

        internal GetResponse<GetAdvertisementsDto> GetAdvertisements(string include, Pagination pagination)
        {
            GetResponse<GetAdvertisementsDto> response = new GetResponse<GetAdvertisementsDto>();
            List<GetAdvertisementsDto> getAdvertisementsDtoList = new List<GetAdvertisementsDto>();
            dynamic includeData = new JObject();
            List<GetMediaDto> getMediaDtoList = new List<GetMediaDto>();
            List<GetCampaignDto> getCampaignDtoList = new List<GetCampaignDto>();

            var advertisements = _unitOfWork.AdvertisementRepository.Get(pagination, null, x => x.OrderBy(x => x.Broadcasts.FirstOrDefault().Sort), x => x.AdvertisementsIntervals, x => x.Broadcasts).ToList();
            foreach (var advertisement in advertisements)
            {
                List<GetCampaignDto> lstItems = new List<GetCampaignDto>();

                GetAdvertisementsDto getAdvertisementsDto = new GetAdvertisementsDto
                {
                    AdvertisementId = Obfuscation.Encode(advertisement.AdvertisementId),
                    ResourceNumber = advertisement.ResourceNumber,
                    Name = advertisement.Name,
                    InstitutionId = Obfuscation.Encode(Convert.ToInt32(advertisement.InstitutionId)),
                    CreatedAt = advertisement.CreatedAt,
                    TintColor = advertisement.TintColor,
                    InvertedTintColor = advertisement.InvertedTintColor,
                };
                getAdvertisementsDto.Campaign = new List<GetCampaignDto>();

                foreach (var broadcast in advertisement.Broadcasts)
                {
                    var campaign = _unitOfWork.CampaignRepository.GetById(x => x.CampaignId == broadcast.CampaignId, null, x => x.Broadcasts);
                    GetCampaignDto getCampaignDto = _mapper.Map<GetCampaignDto>(campaign);
                    getCampaignDto.CampaignId = Obfuscation.Encode(Convert.ToInt32(getCampaignDto.CampaignId));
                    getCampaignDto.Sort = campaign.Broadcasts.FirstOrDefault(x => x.AdvertisementId == advertisement.AdvertisementId).Sort;
                    getAdvertisementsDto.Campaign.Add(getCampaignDto);
                }

                getAdvertisementsDto.IntervalId = Obfuscation.Encode(advertisement.AdvertisementsIntervals.Select(x => x.IntervalId).FirstOrDefault());
                getAdvertisementsDto.TintColor = advertisement.TintColor;
                getAdvertisementsDto.InvertedTintColor = advertisement.InvertedTintColor;
                getAdvertisementsDtoList.Add(getAdvertisementsDto);
            }

            if (!string.IsNullOrEmpty(include) && getAdvertisementsDtoList.Count > 0)
            {
                string[] includeArr = include.Split(',');
                if (includeArr.Length > 0)
                {
                    foreach (var included in includeArr)
                    {
                        if (included.ToLower() == "institution" || included.ToLower() == "institutions")
                        {
                            includeData.institution = APIExtensions.GetInstitutionsIncludedData(getAdvertisementsDtoList, _appSettings.Host + _dependencies.InstitutionUrl);
                        }
                        else if (included.ToLower() == "media" || included.ToLower() == "media")
                        {
                            foreach (var advertisement in advertisements)
                            {
                                var media = _unitOfWork.MediaRepository.GetById(x => x.AdvertisementId == advertisement.AdvertisementId, null, x => x.MediaMetadata);

                                if (media != null)
                                {
                                    GetMediaDto getMedia = _mapper.Map<GetMediaDto>(media);
                                    getMedia.MediaId = Obfuscation.Encode(Convert.ToInt32(media.MediaId));
                                    getMediaDtoList.Add(getMedia);

                                }
                            }
                            includeData.media = JArray.Parse(JsonConvert.SerializeObject(getMediaDtoList.GroupBy(x => x.MediaId).Select(x => x.First()).ToList()));
                        }
                        else if (included.ToLower() == "campaign" || included.ToLower() == "campaigns")
                        {
                            foreach (var advertisement in advertisements)
                            {
                                foreach (var broadcast in advertisement.Broadcasts)
                                {
                                    var campaign = _unitOfWork.CampaignRepository.GetById(x => x.CampaignId == broadcast.CampaignId);
                                    GetCampaignDto getCampaignDto = _mapper.Map<GetCampaignDto>(campaign);
                                    getCampaignDto.CampaignId = Obfuscation.Encode(Convert.ToInt32(getCampaignDto.CampaignId));
                                    getCampaignDtoList.Add(getCampaignDto);
                                }
                            }
                            includeData.campaign = JArray.Parse(JsonConvert.SerializeObject(getCampaignDtoList.GroupBy(x => x.CampaignId).Select(x => x.First()).ToList()));
                        }
                        else if (included.ToLower() == "promotion" || included.ToLower() == "promotions")
                        {
                            includeData.promotion = APIExtensions.GetPromotionsAdvertisement(getAdvertisementsDtoList, _appSettings.Host + _dependencies.PromotionReportUrl);
                        }
                    }
                }
            }

            response.Status = true;
            response.Message = CommonMessage.AdvertisementRetrived;
            response.Pagination = pagination;
            response.Data = getAdvertisementsDtoList;
            response.Included = includeData;
            response.Code = StatusCodes.Status200OK;

            return response;
        }

        internal async Task<Advertisements> PostAdvertisementAsync(PostAdvertisementsDto postAdvertisements)
        {
            var listCampaign = new List<Campaigns>();
            string fileExtension;
            int? mediaId, MediaMetadataId;
            try
            {
                _unitOfWork.BeginTransaction();
                var interval = _unitOfWork.IntervalRepository.GetById(Obfuscation.Decode(postAdvertisements.IntervalId));
                if (interval == null)
                    throw new KeyNotFoundException();

                if (postAdvertisements.CampaignId.Count > 0)
                {
                    foreach (var id in postAdvertisements.CampaignId)
                    {
                        var campaign = _unitOfWork.CampaignRepository.GetById(Obfuscation.Decode(id));
                        if (campaign == null)
                            throw new KeyNotFoundException();
                        listCampaign.Add(campaign);
                    }
                }
                var advertisement = new Advertisements()
                {
                    Name = postAdvertisements.Name,
                    TintColor = postAdvertisements.TintColor,
                    InvertedTintColor = postAdvertisements.InvertedTintColor,
                    InstitutionId = Obfuscation.Decode(postAdvertisements.InstitutionId),
                    CreatedAt = DateTime.Now,
                    ResourceNumber = JsonConvert.DeserializeObject<ResourceNamesResponse>(APIExtensions.GetAPI(_appSettings.Host + _dependencies.IdentifiersUrl, "key=advertisements").Content).ResourceName.ToString()
                };


                await _unitOfWork.AdvertisementRepository.PostAsync(advertisement);
                _unitOfWork.Save();

                if (!string.IsNullOrEmpty(postAdvertisements.IntervalId))
                {
                    AdvertisementsIntervals advertisementsinterval = new AdvertisementsIntervals()
                    {
                        AdvertisementId = advertisement.AdvertisementId,
                        IntervalId = interval.IntervalId
                    };
                    await _unitOfWork.AdvertisementsIntervalRepository.PostAsync(advertisementsinterval);
                    _unitOfWork.Save();
                }
                int counter = 1;
                foreach (var _campaign in listCampaign)
                {
                    Broadcasts broadcast = new Broadcasts()
                    {
                        AdvertisementId = advertisement.AdvertisementId,
                        CampaignId = _campaign.CampaignId,
                        CreatedAt = DateTime.Now,
                        Sort = counter
                    };
                    await _unitOfWork.BroadcastRepository.PostAsync(broadcast);
                    _unitOfWork.Save();
                    counter++;
                }

                if (postAdvertisements.MediaUrl.Count > 0)
                {
                    for (int i = 0; i < postAdvertisements.MediaUrl.Count(); i++)
                    {
                        var existingMediaReferenceName = postAdvertisements.MediaUrl[i].Split('/');
                        fileExtension = existingMediaReferenceName.Last().Split('.').Last();
                        VideoMetadata videoMetadata = new VideoMetadata();
                        MediaMetadata mediaMetadata = new MediaMetadata();

                        if (fileExtension == "mp4")
                        {
                            videoMetadata = await _mediaTypeConversionRepository.ConvertVideoAsync(postAdvertisements.MediaUrl[i]);
                            mediaMetadata.Duration = videoMetadata.Duration;
                            mediaMetadata.Size = videoMetadata.VideoSize;
                        }
                        else if (fileExtension == "jpg" || fileExtension == "png" || fileExtension == "jpeg")
                        {
                            var imagesize = await _mediaTypeConversionRepository.ConvertImageAsync(postAdvertisements.MediaUrl[i]);
                            mediaMetadata.Duration = 0;
                            mediaMetadata.Size = imagesize;
                        }
                        await _unitOfWork.MediaMetadataRepository.PostAsync(mediaMetadata);
                        _unitOfWork.Save();
                        MediaMetadataId = mediaMetadata.MediaMetadataId;

                        Medias media = new Medias
                        {
                            Url = postAdvertisements.MediaUrl[i],
                            CreatedAt = DateTime.Now
                        };

                        if (fileExtension == "mp4")
                            media.MediaType = MediaType.video;
                        else if (fileExtension == "jpg" || fileExtension == "png" || fileExtension == "jpeg")
                            media.MediaType = MediaType.image;
                        media.MediaMetadataId = MediaMetadataId;
                        media.AdvertisementId = advertisement.AdvertisementId;

                        await _unitOfWork.MediaRepository.PostAsync(media);
                        _unitOfWork.Save();
                        mediaId = media.MediaId;
                    }
                }
                _unitOfWork.Commit();
                return advertisement;
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                throw ex;
            }
        }

        internal async Task<GetResponse<Advertisements>> UpdateAdvertisementAsync(PostAdvertisementsDto advertisementsDto)
        {
            var advertisements = await _unitOfWork.AdvertisementRepository.GetAsync(null, x => x.AdvertisementId == Obfuscation.Decode(advertisementsDto.AdvertisementId), null, x => x.Broadcasts, x => x.AdvertisementsIntervals);
            if (advertisements.Count > 0)
            {
                if (advertisements.FirstOrDefault().AdvertisementsIntervals.FirstOrDefault().IntervalId != Obfuscation.Decode(advertisementsDto.IntervalId))
                {
                    var interval = _unitOfWork.IntervalRepository.GetById(x => x.IntervalId == Obfuscation.Decode(advertisementsDto.IntervalId));
                    if (interval != null)
                    {
                        var advertisementInterval = new AdvertisementsIntervals
                        {
                            AdvertisementId = Obfuscation.Decode(advertisementsDto.AdvertisementId),
                            IntervalId = Obfuscation.Decode(advertisementsDto.IntervalId)
                        };
                        _unitOfWork.AdvertisementsIntervalRepository.Put(advertisementInterval);
                        _unitOfWork.Save();
                    }
                }
                var campaignList = new List<Campaigns>();
                foreach (var campaign in advertisementsDto.CampaignId)
                {
                    var _campaign = _unitOfWork.CampaignRepository.GetById(x => x.CampaignId == Obfuscation.Decode(campaign));
                    if (_campaign == null)
                        return ReturnResponse.ErrorResponse(CommonMessage.CampaignNotFound, StatusCodes.Status404NotFound);

                    campaignList.Add(_campaign);
                }

                var advertisementsCampaign = _unitOfWork.BroadcastRepository.Get(null, x => x.AdvertisementId == Obfuscation.Decode(advertisementsDto.AdvertisementId)).ToList();
                foreach (var broadcast in advertisementsCampaign)
                {
                    _unitOfWork.BroadcastRepository.Remove(broadcast);
                    _unitOfWork.Save();
                }
                //var campaignList = new List<Campaign>();
                //var b = _unitOfWork.CampaignRepository.Get(null, x => x.CampaignId == advertisements.FirstOrDefault().Broadcasts.FirstOrDefault().CampaignId, null, x => x.Broadcasts).ToList();
                //foreach (var campaign in b)
                //{
                //    //Campaign _campaign = _mapper.Map<Campaign>(campaign);
                //    campaignList.Add(campaign);
                //}


                //foreach (var broadcast in campaignList)
                //{
                //    _unitOfWork.BroadcastRepository.Delete(broadcast.Broadcasts.FirstOrDefault().BroadcastId);
                //    _unitOfWork.Save();
                //}
                int counter = 1;
                foreach (var broadcasts in campaignList)
                {
                    var _broadcast = new Broadcasts
                    {
                        AdvertisementId = Obfuscation.Decode(advertisementsDto.AdvertisementId),
                        CampaignId = broadcasts.CampaignId,
                        CreatedAt = DateTime.Now,
                        Sort = counter
                    };
                    counter++;
                    _unitOfWork.BroadcastRepository.Post(_broadcast);
                    _unitOfWork.Save();
                }
                var medias = _unitOfWork.MediaRepository.Get(null, x => x.AdvertisementId == Obfuscation.Decode(advertisementsDto.AdvertisementId), null, x => x.MediaMetadata).ToList();

                var videoMetadata = new VideoMetadata();
                MediaMetadata mediaMetadata = medias.FirstOrDefault().MediaMetadata;

                for (int i = 0; i < advertisementsDto.MediaUrl.Count; i++)
                {
                    if (medias.FirstOrDefault().Url != advertisementsDto.MediaUrl[i])
                    {
                        var fileName = advertisementsDto.MediaUrl[i].Split('/');
                        var fileExtension = fileName.Last().Split('.').Last();
                        mediaMetadata = new MediaMetadata();

                        if (fileExtension == "mp4")
                        {
                            videoMetadata = await _mediaTypeConversionRepository.ConvertVideoAsync(advertisementsDto.MediaUrl[i]);
                            mediaMetadata.Duration = videoMetadata.Duration;
                            mediaMetadata.Size = videoMetadata.VideoSize;
                            medias.FirstOrDefault().MediaType = MediaType.video;
                        }
                        else if (fileExtension == "jpg" || fileExtension == "png" || fileExtension == "jpeg")
                        {
                            var imagesize = await _mediaTypeConversionRepository.ConvertImageAsync(advertisementsDto.MediaUrl[i]);
                            mediaMetadata.Duration = 0;
                            mediaMetadata.Size = imagesize;
                            medias.FirstOrDefault().MediaType = MediaType.image;
                        }
                        medias.FirstOrDefault().Url = advertisementsDto.MediaUrl[i];

                    }
                    _unitOfWork.MediaMetadataRepository.Put(mediaMetadata);
                    _unitOfWork.Save();
                    medias.FirstOrDefault().MediaMetadataId = mediaMetadata.MediaMetadataId;
                    _unitOfWork.MediaRepository.Put(medias.FirstOrDefault());
                    _unitOfWork.Save();
                }

            }
            var response = new GetResponse<Advertisements>();
            return await Task.FromResult(response);
        }
    }
}
