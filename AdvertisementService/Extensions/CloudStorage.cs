using AdvertisementService.Models.Common;
using AdvertisementService.Models.DBModels;
using AdvertisementService.Models.Dtos;
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
        internal static async Task<string> UploadImageToCloudReturnUrlAsync(PostMediaDto media, AzureStorageBlobConfig _config)
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

        internal static async Task<string> UploadVideoToCloudReturnUrlAsync(PostMediaDto media, AzureStorageBlobConfig _config)
        {
            
            string mediaReferenceName = media.MediaFile.FileName.Split("\\").LastOrDefault();
            if (CloudStorageAccount.TryParse(_config.StorageConnection, out CloudStorageAccount storageAccount))
            {
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(_config.Container);
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(mediaReferenceName);
                await blockBlob.UploadFromStreamAsync(File.OpenRead(mediaReferenceName));
                blobUrl = blockBlob.Uri.AbsoluteUri;

            }
            return blobUrl;
        }
    }
}
