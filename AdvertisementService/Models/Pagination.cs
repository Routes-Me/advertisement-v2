namespace AdvertisementService.Models
{
    public class Pagination
    {
        public int Offset { get; set; } = 1;
        public int Limit { get; set; } = 10;
        public int Total { get; set; }
    }
}
