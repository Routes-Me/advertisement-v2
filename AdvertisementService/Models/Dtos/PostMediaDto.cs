using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using AdvertisementService.Models.DBModels;

namespace AdvertisementService.Models.Dtos
{
    public class PostMediaDto
    {
        public int MediaId { get; set; }
        public string Url { get; set; }
        public DateTime? CreatedAt { get; set; }
        public MediaType? MediaType { get; set; }
        public int? MediaMetadataId { get; set; }
        [NotMappedAttribute]
        public IFormFile MediaFile { get; set; }
        public float? Size { get; set; }
        public float? Duration { get; set; }
    }
   
}
