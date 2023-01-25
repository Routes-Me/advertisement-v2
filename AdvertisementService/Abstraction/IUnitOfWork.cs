namespace AdvertisementService.Abstraction
{
    public interface IUnitOfWork
    {
        IAdvertisementRepository AdvertisementRepository { get; }
        ICampaignRepository CampaignRepository { get; }
        IIntervalRepository IntervalRepository { get; }
        IMediaMetadataRepository MediaMetadataRepository { get; }
        IMediaRepository MediaRepository { get; }
        IAdvertisementsIntervalRepository AdvertisementsIntervalRepository { get; }
        IBroadcastRepository BroadcastRepository { get; }
        void BeginTransaction();
        void Commit();
        void Rollback();

        void Save();
    }
}
