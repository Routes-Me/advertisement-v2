using AdvertisementService.Abstraction;
using AdvertisementService.Extensions;
using AdvertisementService.Models;
using AdvertisementService.Models.Common;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.Dtos;
using AutoMapper;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RoutesSecurity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static AdvertisementService.Models.Response;
using static System.Net.Mime.MediaTypeNames;

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

        internal async Task<Advertisements> DeleteAdvertisementAsync(string id)
        {
            try
            {
                var advertisement = _unitOfWork.AdvertisementRepository.GetById(x => x.AdvertisementId == Obfuscation.Decode(id), null, x => x.AdvertisementsIntervals, x => x.Broadcasts, x => x.Media, x => x.Media.MediaMetadata);
                var mediaReferenceName = advertisement.Media.Url.Split('/');



                if (advertisement == null)
                    throw new KeyNotFoundException(CommonMessage.AdvertisementNotFound);

                _unitOfWork.BeginTransaction();


                _unitOfWork.MediaMetadataRepository.Remove(advertisement.Media.MediaMetadata);
                _unitOfWork.Save();
                _unitOfWork.AdvertisementRepository.Remove(advertisement);
                _unitOfWork.Save();

                //APIExtensions.DeleteApi(_appSettings.Host + _dependencies.PromotionsByAdvertisementUrl + id);

                if (CloudStorageAccount.TryParse(_config.StorageConnection, out CloudStorageAccount storageAccount))
                {
                    CloudBlobClient BlobClient = storageAccount.CreateCloudBlobClient();
                    CloudBlobContainer container = BlobClient.GetContainerReference(_config.Container);
                    if (await container.ExistsAsync())
                    {
                        CloudBlob file = container.GetBlobReference(mediaReferenceName.LastOrDefault());
                        if (await file.ExistsAsync())
                            await file.DeleteAsync();
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

        internal GetResponse<GetAdvertisementsDto> GetAdvertisementById(string id, string include)
        {
            var response = new GetResponse<GetAdvertisementsDto>();
            var getAdvertisementsDtoList = new List<GetAdvertisementsDto>();
            dynamic includeData = new JObject();
            var getMediaDtoList = new List<GetMediaDto>();
            var getCampaignDtoList = new List<GetCampaignDto>();

            var advertisement = _unitOfWork.AdvertisementRepository.GetById(x => x.AdvertisementId == Obfuscation.Decode(id), null, x => x.AdvertisementsIntervals, x => x.Broadcasts, x => x.Media, x => x.Media.MediaMetadata);
            if (advertisement == null)
                return ReturnResponse.ErrorResponse(CommonMessage.AdvertisementNotFound, 404);


            var getAdvertisementsDto = new GetAdvertisementsDto
            {
                AdvertisementId = Obfuscation.Encode(advertisement.AdvertisementId),
                ResourceNumber = advertisement.ResourceNumber,
                Name = advertisement.Name,
                InstitutionId = Obfuscation.Encode(Convert.ToInt32(advertisement.InstitutionId)),
                CreatedAt = advertisement.CreatedAt,
                TintColor = advertisement.TintColor,
                InvertedTintColor = advertisement.InvertedTintColor,
                CampaignId = advertisement.Broadcasts.Select(x => Obfuscation.Encode(x.CampaignId)).ToList(),
                IntervalId = advertisement.AdvertisementsIntervals.Select(x => Obfuscation.Encode(x.IntervalId)).FirstOrDefault()
            };
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
                            var getMedia = _mapper.Map<GetMediaDto>(advertisement.Media);
                            getMedia.MediaId = Obfuscation.Encode(Convert.ToInt32(getMedia.MediaId));
                            getMediaDtoList.Add(getMedia);
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
            else
            {
                getMediaDtoList.Add(_mapper.Map<GetMediaDto>(advertisement.Media));
                getMediaDtoList[0].MediaId = Obfuscation.Encode(Convert.ToInt32(getMediaDtoList[0].MediaId));
                includeData.media = JArray.Parse(JsonConvert.SerializeObject(getMediaDtoList.GroupBy(x => x.MediaId).Select(a => a.First()).ToList().Cast<dynamic>().ToList()));
                foreach (var broadcast in advertisement.Broadcasts)
                {
                    var campaign = _unitOfWork.CampaignRepository.GetById(x => x.CampaignId == broadcast.CampaignId);
                    var getCampaignDto = _mapper.Map<GetCampaignDto>(campaign);
                    getCampaignDto.CampaignId = Obfuscation.Encode(Convert.ToInt32(getCampaignDto.CampaignId));
                    getCampaignDtoList.Add(getCampaignDto);
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
            var response = new GetResponse<GetAdvertisementsDto>();
            var getAdvertisementsDtoList = new List<GetAdvertisementsDto>();
            dynamic includeData = new JObject();
            var getMediaDtoList = new List<GetMediaDto>();
            var getCampaignDtoList = new List<GetCampaignDto>();

            var advertisements = _unitOfWork.AdvertisementRepository.Get(pagination, null, x => x.OrderBy(x => x.Broadcasts.FirstOrDefault().Sort), x => x.AdvertisementsIntervals, x => x.Broadcasts, x => x.Media, x => x.Media.MediaMetadata).ToList();
            foreach (var advertisement in advertisements)
            {
                GetAdvertisementsDto getAdvertisementsDto = new GetAdvertisementsDto
                {
                    AdvertisementId = Obfuscation.Encode(advertisement.AdvertisementId),
                    ResourceNumber = advertisement.ResourceNumber,
                    Name = advertisement.Name,
                    InstitutionId = Obfuscation.Encode(Convert.ToInt32(advertisement.InstitutionId)),
                    CreatedAt = advertisement.CreatedAt,
                    TintColor = advertisement.TintColor,
                    InvertedTintColor = advertisement.InvertedTintColor,
                    CampaignId = advertisement.Broadcasts.Select(x => Obfuscation.Encode(x.CampaignId)).ToList(),
                    IntervalId = advertisement.AdvertisementsIntervals.Select(x => Obfuscation.Encode(x.IntervalId)).FirstOrDefault()
                };
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
                                var getMedia = _mapper.Map<GetMediaDto>(advertisement.Media);
                                getMedia.MediaId = Obfuscation.Encode(Convert.ToInt32(getMedia.MediaId));
                                getMediaDtoList.Add(getMedia);
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
            else
            {
                foreach (var advertisement in advertisements)
                {
                    var getMedia = _mapper.Map<GetMediaDto>(advertisement.Media);
                    getMedia.MediaId = Obfuscation.Encode(Convert.ToInt32(getMedia.MediaId));
                    getMediaDtoList.Add(getMedia);

                    foreach (var broadcast in advertisement.Broadcasts)
                    {
                        var campaign = _unitOfWork.CampaignRepository.GetById(x => x.CampaignId == broadcast.CampaignId);
                        GetCampaignDto getCampaignDto = _mapper.Map<GetCampaignDto>(campaign);
                        getCampaignDto.CampaignId = Obfuscation.Encode(Convert.ToInt32(getCampaignDto.CampaignId));
                        getCampaignDtoList.Add(getCampaignDto);
                    }
                }
                includeData.media = JArray.Parse(JsonConvert.SerializeObject(getMediaDtoList.GroupBy(x => x.MediaId).Select(x => x.First()).ToList()));
                includeData.campaign = JArray.Parse(JsonConvert.SerializeObject(getCampaignDtoList.GroupBy(x => x.CampaignId).Select(x => x.First()).ToList()));

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
            int? mediaId = 0, MediaMetadataId = 0;
            try
            {
                var interval = _unitOfWork.IntervalRepository.GetById(Obfuscation.Decode(postAdvertisements.IntervalId));
                if (interval == null)
                    throw new KeyNotFoundException(CommonMessage.IntervalNotFound);

                if (postAdvertisements.CampaignId.Count > 0)
                {
                    foreach (var id in postAdvertisements.CampaignId)
                    {
                        var campaign = _unitOfWork.CampaignRepository.GetById(Obfuscation.Decode(id));
                        if (campaign == null)
                            throw new KeyNotFoundException(CommonMessage.CampaignNotFound);
                        listCampaign.Add(campaign);
                    }
                }

                if (!string.IsNullOrEmpty(postAdvertisements.MediaUrl))
                {
                    var existingMediaReferenceName = postAdvertisements.MediaUrl.Split('/');
                    fileExtension = existingMediaReferenceName.Last().Split('.').Last();
                    VideoMetadata videoMetadata = new VideoMetadata();
                    MediaMetadata mediaMetadata = new MediaMetadata();

                    if (fileExtension == "mp4")
                    {
                        videoMetadata = await _mediaTypeConversionRepository.ConvertVideoAsync(postAdvertisements.MediaUrl);
                        mediaMetadata.Duration = videoMetadata.Duration;
                        mediaMetadata.Size = videoMetadata.VideoSize;
                    }
                    else if (fileExtension == "jpg" || fileExtension == "png" || fileExtension == "jpeg")
                    {
                        var imagesize = await _mediaTypeConversionRepository.ConvertImageAsync(postAdvertisements.MediaUrl);
                        mediaMetadata.Duration = 0;
                        mediaMetadata.Size = imagesize;
                    }
                    _unitOfWork.BeginTransaction();

                    await _unitOfWork.MediaMetadataRepository.PostAsync(mediaMetadata);
                    _unitOfWork.Save();
                    MediaMetadataId = mediaMetadata.MediaMetadataId;

                    Medias media = new Medias
                    {
                        Url = postAdvertisements.MediaUrl,
                        CreatedAt = DateTime.Now
                    };

                    if (fileExtension == "mp4")
                        media.MediaType = MediaType.video;
                    else if (fileExtension == "jpg" || fileExtension == "png" || fileExtension == "jpeg")
                        media.MediaType = MediaType.image;
                    media.MediaMetadataId = MediaMetadataId;

                    await _unitOfWork.MediaRepository.PostAsync(media);
                    _unitOfWork.Save();
                    mediaId = media.MediaId;
                }


                var advertisement = new Advertisements()
                {
                    Name = postAdvertisements.Name,
                    TintColor = postAdvertisements.TintColor,
                    InvertedTintColor = postAdvertisements.InvertedTintColor,
                    MediaId = mediaId,
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
                foreach (var _campaign in listCampaign)
                {
                    Broadcasts broadcast = new Broadcasts()
                    {
                        AdvertisementId = advertisement.AdvertisementId,
                        CampaignId = _campaign.CampaignId,
                        CreatedAt = DateTime.Now
                    };
                    await _unitOfWork.BroadcastRepository.PostAsync(broadcast);
                    _unitOfWork.Save();
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

        internal async Task<Response> UpdateAdvertisementAsync(PostAdvertisementsDto advertisementsDto)
        {
            try
            {
                int? mediaMetadataId = 0;
                var advertisement = _unitOfWork.AdvertisementRepository.GetById(x => x.AdvertisementId == Obfuscation.Decode(advertisementsDto.AdvertisementId));
                _unitOfWork.BeginTransaction();

                if (advertisement == null)
                    throw new KeyNotFoundException(CommonMessage.AdvertisementNotFound);
                if (!string.IsNullOrEmpty(advertisementsDto.IntervalId))
                {
                    var interval = _unitOfWork.IntervalRepository.GetById(x => x.IntervalId == Obfuscation.Decode(advertisementsDto.IntervalId));
                    if (interval != null)
                    {
                        var advertisementInterval = _unitOfWork.AdvertisementsIntervalRepository.GetById(x => x.AdvertisementId == Obfuscation.Decode(advertisementsDto.AdvertisementId) && x.IntervalId == Obfuscation.Decode(advertisementsDto.IntervalId));
                        if (advertisementInterval == null)
                        {
                            var model = new AdvertisementsIntervals
                            {
                                AdvertisementId = Obfuscation.Decode(advertisementsDto.AdvertisementId),
                                IntervalId = Obfuscation.Decode(advertisementsDto.IntervalId)
                            };

                            _unitOfWork.AdvertisementsIntervalRepository.Post(model);
                            _unitOfWork.Save();
                        }
                        else
                        {
                            advertisementInterval.AdvertisementId = advertisement.AdvertisementId;
                            advertisementInterval.IntervalId = interval.IntervalId;
                            _unitOfWork.AdvertisementsIntervalRepository.Put(advertisementInterval);
                            _unitOfWork.Save();
                        }


                    }
                }
                var campaignList = new List<Campaigns>();

                foreach (var campaign in advertisementsDto.CampaignId)
                {
                    var _campaign = _unitOfWork.CampaignRepository.GetById(x => x.CampaignId == Obfuscation.Decode(campaign));

                    if (_campaign != null)
                    {
                        campaignList.Add(_campaign);
                    }
                }

                var advertisementsCampaign = _unitOfWork.BroadcastRepository.Get(null, x => x.AdvertisementId == Obfuscation.Decode(advertisementsDto.AdvertisementId)).ToList();
                _unitOfWork.BroadcastRepository.RemoveRange(advertisementsCampaign);
                _unitOfWork.Save();

                foreach (var campaign in campaignList)
                {
                    Broadcasts _broadcast = new Broadcasts()
                    {
                        AdvertisementId = advertisement.AdvertisementId,
                        CampaignId = campaign.CampaignId,
                        CreatedAt = DateTime.Now
                    };
                    _unitOfWork.BroadcastRepository.Post(_broadcast);
                    _unitOfWork.Save();
                }
                if (!string.IsNullOrEmpty(advertisementsDto.MediaUrl))
                {
                    var mediaData = new Medias();
                    mediaData = _unitOfWork.MediaRepository.GetById(x => x.Url == advertisementsDto.MediaUrl, null, x => x.MediaMetadata);
                    if (mediaData != null)
                    {

                        var fileName = advertisementsDto.MediaUrl.Split('/');
                        var fileExtension = fileName.Last().Split('.').Last();
                        var videoMetadata = new VideoMetadata();


                        if (mediaData.MediaMetadata == null)
                        {
                            var mediaMetadata = new MediaMetadata();

                            if (fileExtension == "mp4")
                            {
                                videoMetadata = await _mediaTypeConversionRepository.ConvertVideoAsync(advertisementsDto.MediaUrl);
                                mediaMetadata.Duration = videoMetadata.Duration;
                                mediaMetadata.Size = videoMetadata.VideoSize;
                            }
                            else if (fileExtension == "jpg" || fileExtension == "png" || fileExtension == "jpeg")
                            {
                                var imagesize = await _mediaTypeConversionRepository.ConvertImageAsync(advertisementsDto.MediaUrl);
                                mediaMetadata.Duration = 0;
                                mediaMetadata.Size = imagesize;
                            }
                            _unitOfWork.MediaMetadataRepository.Post(mediaMetadata);
                            _unitOfWork.Save();
                            mediaMetadataId = mediaMetadata.MediaMetadataId;
                        }
                        else
                        {
                            if (fileExtension == "mp4")
                            {
                                videoMetadata = await _mediaTypeConversionRepository.ConvertVideoAsync(advertisementsDto.MediaUrl);
                                mediaData.MediaMetadata.Duration = videoMetadata.Duration;
                                mediaData.MediaMetadata.Size = videoMetadata.VideoSize;
                            }
                            else if (fileExtension == "jpg" || fileExtension == "png" || fileExtension == "jpeg")
                            {
                                var imagesize = await _mediaTypeConversionRepository.ConvertImageAsync(advertisementsDto.MediaUrl);
                                mediaData.MediaMetadata.Duration = 0;
                                mediaData.MediaMetadata.Size = imagesize;
                            }
                            _unitOfWork.MediaMetadataRepository.Put(mediaData.MediaMetadata);
                            _unitOfWork.Save();
                            mediaMetadataId = mediaData.MediaMetadataId;
                        }
                        if (fileExtension == "mp4")
                            mediaData.MediaType = MediaType.video;
                        else
                            mediaData.MediaType = MediaType.image;
                        mediaData.MediaMetadataId = mediaMetadataId;
                        mediaData.Url = advertisementsDto.MediaUrl;
                        advertisement.MediaId = mediaData.MediaId;
                        _unitOfWork.MediaRepository.Put(mediaData);
                        _unitOfWork.Save();
                    }

                }
                else
                {
                    advertisement.MediaId = null;

                }
                advertisement.InstitutionId = Obfuscation.Decode(advertisementsDto.InstitutionId);
                advertisement.ResourceNumber = advertisementsDto.ResourceNumber;
                advertisement.Name = advertisementsDto.Name;
                advertisement.TintColor = advertisementsDto.TintColor;
                advertisement.InvertedTintColor = advertisementsDto.InvertedTintColor;

                _unitOfWork.AdvertisementRepository.Put(advertisement);
                _unitOfWork.Save();
                _unitOfWork.Commit();

                //var response = new GetResponse<Advertisements>{
                //    Code= 200,
                //    Status = true,
                //    Message = CommonMessage.AdvertisementUpdate
                //};
                var response = new Response
                {
                    Code = 200,
                    Status = true,
                    Message = CommonMessage.AdvertisementUpdate
                };
                return response;
            }
            catch (Exception ex)
            {

                _unitOfWork.Rollback();
                throw ex;
            }

        }
    }
}
