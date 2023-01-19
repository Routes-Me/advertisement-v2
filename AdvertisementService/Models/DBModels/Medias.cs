using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdvertisementService.Models.DBModels
{
    public partial class Medias
    {
        public Medias()
        {
            Advertisements = new HashSet<Advertisements>();
        }

        public int MediaId { get; set; }
        public string Url { get; set; }
        public DateTime? CreatedAt { get; set; }
        public MediaType? MediaType { get; set; }
        public int? MediaMetadataId { get; set; }
        [NotMappedAttribute]
        public IFormFile MediaFile { get; set; }
        

        public virtual MediaMetadata MediaMetadata { get; set; }
        public virtual ICollection<Advertisements> Advertisements { get; set; }
    }
    public enum MediaType
    {
        video,
        image
    }

}
