using AdvertisementService.Abstraction;
using AdvertisementService.Models.Common;
using AdvertisementService.Models.Dtos;
using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static AdvertisementService.Models.Response;

namespace AdvertisementService.Repository
{
    public class MediaTypeConversionRepository : IMediaTypeConversionRepository
    {
        private readonly IWebHostEnvironment _env;
        private readonly AzureStorageBlobConfig _config;
        public MediaTypeConversionRepository(IWebHostEnvironment webHostEnvironment, IOptions<AzureStorageBlobConfig> config)
        {
            _env = webHostEnvironment;
            _config = config.Value;
        }
        public async Task<float> ConvertImageAsync(string file)
        {
            try
            {
                CloudBlockBlob blockBlob = new CloudBlockBlob(new Uri(file));
                var fileName = blockBlob.Name.Split('/');
                string originalFilePath = Path.Combine(_env.ContentRootPath, fileName.Last());

                using (var fs = new FileStream(originalFilePath, FileMode.Create))
                {
                    await blockBlob.DownloadToStreamAsync(fs);
                }
                FileInfo fInfo = new FileInfo(originalFilePath);
                var size = Convert.ToDecimal(Convert.ToDecimal(fInfo.Length / 1024) / 1024).ToString("0.####");   //display size in mb
                fInfo.Delete();
                return (float)Convert.ToDecimal(size);
            }
            catch (Exception ex)
            {
                throw ReturnResponse.ExceptionResponse(ex);
            }
        }

        public async Task<VideoMetadata> ConvertVideoAsync(string file)
        {
            try
            {
                var existingMediaReferenceName = file.Split('/');
                VideoMetadata videoMetadata = new VideoMetadata();
                CloudBlockBlob blockBlob = new CloudBlockBlob(new Uri(file));
                var fileName = blockBlob.Name.Split('/');
                var updatedFileName = fileName.Last().Split('.');
                string inputFilePath = Path.Combine(_env.ContentRootPath, updatedFileName.First() + "_input" + ".mp4");
                string outputFilePath = Path.Combine(_env.ContentRootPath, fileName.Last());

                using (var fs = new FileStream(inputFilePath, FileMode.Create))
                {
                    await blockBlob.DownloadToStreamAsync(fs);
                }
                var inputFile = new MediaFile { Filename = inputFilePath };
                var outputFile = new MediaFile { Filename = outputFilePath };

                var conversionOptions = new ConversionOptions();
                conversionOptions.VideoSize = VideoSize.Hd1080;
                conversionOptions.VideoBitRate = 1600;
                conversionOptions.AudioSampleRate = AudioSampleRate.Hz48000;

                try
                {
                    using (var engine = new Engine())
                    {
                        engine.Convert(inputFile, outputFile, conversionOptions);
                        engine.GetMetadata(outputFile);
                        engine.Dispose();
                    }
                }
                catch (Exception ex) { throw ex; }

                if (outputFile != null)
                {
                    if (outputFile.Metadata != null)
                    {
                        var duration = outputFile.Metadata.Duration;
                        videoMetadata.Duration = (float)duration.TotalSeconds;
                    }
                }
                FileInfo outputFileInfo = new FileInfo(outputFile.Filename);

                var videoSize = Convert.ToDecimal(Convert.ToDecimal(outputFileInfo.Length / 1024) / 1024).ToString("0.##");
                videoMetadata.CompressedFile = outputFileInfo.FullName;
                videoMetadata.VideoSize = (float)Convert.ToDecimal(videoSize);

                if (CloudStorageAccount.TryParse(_config.StorageConnection, out CloudStorageAccount storageAccount))
                {
                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                    CloudBlobContainer container = blobClient.GetContainerReference(_config.Container);
                    if (await container.ExistsAsync())
                    {
                        CloudBlob unComperresedfile = container.GetBlobReference(existingMediaReferenceName.LastOrDefault());
                        if (await unComperresedfile.ExistsAsync())
                            await unComperresedfile.DeleteAsync();
                    }

                    CloudBlockBlob uploadBlockBlob = container.GetBlockBlobReference(videoMetadata.CompressedFile.Split('\\').LastOrDefault());
                    uploadBlockBlob.Properties.ContentType = "video/mp4";
                    using (var stream = File.OpenRead(videoMetadata.CompressedFile))
                    {
                        await uploadBlockBlob.UploadFromStreamAsync(stream);
                    }

                }

                FileInfo inputFileInfo = new FileInfo(inputFilePath);
                inputFileInfo.Delete();
                outputFileInfo.Delete();
                return videoMetadata;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
