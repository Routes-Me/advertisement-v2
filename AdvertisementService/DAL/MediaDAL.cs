using AdvertisementService.Abstraction;
using AdvertisementService.Extensions;
using AdvertisementService.Models;
using AdvertisementService.Models.Common;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.Dtos;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using RoutesSecurity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static AdvertisementService.Models.Response;

namespace AdvertisementService.DAL
{
    public class MediaDAL
    {
        private readonly AzureStorageBlobConfig _config;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MediaDAL(IUnitOfWork unitOfWork, AzureStorageBlobConfig config, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _config = config;
            _mapper = mapper;
        }

        internal async Task DeleteMediaAsync(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception(CommonMessage.InvalidData);

                _unitOfWork.BeginTransaction();

                Medias media = _unitOfWork.MediaRepository.GetById(x => x.MediaId == Obfuscation.Decode(id), null, x => x.Advertisements, x => x.MediaMetadata);

                if (media == null)
                    throw new Exception(CommonMessage.MediaNotFound);

                var mediaReferenceName = media.Url.Split('/');
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

                if (media.MediaMetadata != null)
                {
                    await _unitOfWork.MediaMetadataRepository.DeleteAsync(media.MediaMetadata.MediaMetadataId);
                }

                await _unitOfWork.MediaRepository.DeleteAsync(media.MediaId);
                _unitOfWork.Save();
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {

                _unitOfWork.Rollback();
                throw ex;
            }
        }

        internal GetResponse<GetMediaDto> GetMedia(Pagination pagination)
        {
            GetResponse<GetMediaDto> getMediaDto = new GetResponse<GetMediaDto>();
            List<GetMediaDto> getMediaDtoList = new List<GetMediaDto>();
            List<Medias> mediasList = _unitOfWork.MediaRepository.Get(pagination, null, null, x => x.MediaMetadata).ToList();
            getMediaDtoList = _mapper.Map<List<GetMediaDto>>(mediasList);
            foreach (var media in getMediaDtoList)
            {
                media.MediaId = Obfuscation.Encode(Convert.ToInt32(media.MediaId));
            }

            getMediaDto.Status = true;
            getMediaDto.Message = CommonMessage.MediaRetrived;
            getMediaDto.Pagination = pagination;
            getMediaDto.Data = getMediaDtoList;
            getMediaDto.Code = StatusCodes.Status200OK;

            return getMediaDto;
        }

        internal GetMediaDto GetMediaById(string id)
        {
            Medias media = _unitOfWork.MediaRepository.GetById(x => x.MediaId == Obfuscation.Decode(id), null, x => x.MediaMetadata);

            if (media == null)
                throw new Exception(CommonMessage.MediaNotFound);

            var getMediaDto = _mapper.Map<GetMediaDto>(media);
            getMediaDto.MediaId = Obfuscation.Encode(Convert.ToInt32(getMediaDto.MediaId));

            return getMediaDto;
        }

        internal async Task<Medias> PostMediaAsync(PostMediaDto media)
        {
            if (media == null)
                throw new Exception(CommonMessage.InvalidData);
            if (media.MediaType == MediaType.video)
            {
                media.Url = await CloudStorage.UploadVideoToCloudReturnUrlAsync(media, _config);
            }
            if (media.MediaType == MediaType.image)
            {
                media.Url = await CloudStorage.UploadImageToCloudReturnUrlAsync(media, _config);
            }
            MediaMetadata mediaMetadata = new MediaMetadata()
            {
                Duration = media.Duration,
                Size = media.Size
            };
            await _unitOfWork.MediaMetadataRepository.PostAsync(mediaMetadata);
            _unitOfWork.Save();

            Medias _media = new Medias()
            {
                Url = media.Url,
                CreatedAt = DateTime.Now,
                MediaType = media.MediaType,
                MediaMetadataId = mediaMetadata.MediaMetadataId
            };
            return _media;
        }

        internal async Task<Medias> UpdateMediaAsync(PostMediaDto media)
        {
            try
            {
                if (media == null)
                    throw new Exception(CommonMessage.InvalidData);

                _unitOfWork.BeginTransaction();

                var _media = _unitOfWork.MediaRepository.GetById(x => x.MediaId == media.MediaId, null, x => x.Advertisements, x => x.MediaMetadata);
                if (_media == null)
                    throw new Exception(CommonMessage.MediaNotFound);

                if (_media.Url != null)
                {
                    var _mediaReferenceName = _media.Url.Split('/');
                    if (CloudStorageAccount.TryParse(_config.StorageConnection, out CloudStorageAccount storageAccount))
                    {
                        CloudBlobClient BlobClient = storageAccount.CreateCloudBlobClient();
                        CloudBlobContainer container = BlobClient.GetContainerReference(_config.Container);
                        if (await container.ExistsAsync())
                        {
                            CloudBlob file = container.GetBlobReference(_mediaReferenceName.LastOrDefault());
                            if (await file.ExistsAsync())
                                await file.DeleteAsync();
                        }
                    }
                }
                if (media.MediaType == MediaType.video)
                {
                    media.Url = await CloudStorage.UploadVideoToCloudReturnUrlAsync(media, _config);
                }
                if (media.MediaType == MediaType.image)
                {
                    media.Url = await CloudStorage.UploadImageToCloudReturnUrlAsync(media, _config);
                }
                _media.MediaMetadata.Duration = media.Duration;
                _media.MediaMetadata.Size = media.Size;
                _unitOfWork.MediaMetadataRepository.Put(_media.MediaMetadata);
                _unitOfWork.Save();
                _media.MediaFile = media.MediaFile;
                _media.Url = media.Url;
                _media.MediaType = media.MediaType;
                _media.CreatedAt = DateTime.Now;
                _unitOfWork.MediaRepository.Put(_media);
                _unitOfWork.Save();
                _unitOfWork.Commit();
                return _media;
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                throw ex;
            }
        }
    }
}
