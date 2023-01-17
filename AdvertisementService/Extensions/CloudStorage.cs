using AdvertisementService.Models.Common;
using AdvertisementService.Models.DBModels;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertisementService.Extensions
{
    public static class CloudStorage
    {
        static string blobUrl = string.Empty;
        internal static async Task<string> UploadImageToCloudReturnUrlAsync(Medias media, AzureStorageBlobConfig _config)
        {
            string mediaReferenceName = media.MediaFile.FileName.Split('.')[0] + "_" + DateTime.UtcNow.Ticks + "." + media.MediaFile.FileName.Split('.')[1];

            if (CloudStorageAccount.TryParse(_config.StorageConnection, out CloudStorageAccount storageAccount))
            {
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(_config.Container);
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(mediaReferenceName);
                await blockBlob.UploadFromStreamAsync(media.MediaFile.OpenReadStream());
                blobUrl = blockBlob.Uri.AbsoluteUri;
            }
            return blobUrl;
        }

        internal static async Task<string> UploadVideoToCloudReturnUrlAsync(Medias media, AzureStorageBlobConfig _config)
        {
            var videoPath = string.Empty; //await _videoConversionRepository.ConvertVideoAsync(model.media, model.Mute);
            string mediaReferenceName = videoPath.Split("\\").LastOrDefault();
            if (CloudStorageAccount.TryParse(_config.StorageConnection, out CloudStorageAccount storageAccount))
            {
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(_config.Container);
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(mediaReferenceName);
                await blockBlob.UploadFromStreamAsync(File.OpenRead(videoPath));
                blobUrl = blockBlob.Uri.AbsoluteUri;

            }
            return blobUrl;
        }
    }
}
