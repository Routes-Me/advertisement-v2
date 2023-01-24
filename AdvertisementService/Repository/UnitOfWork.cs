using AdvertisementService.Abstraction;
using AdvertisementService.Models.DBModels;
using System;

namespace AdvertisementService.Repository
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly AdvertisementContext _context;

        private bool disposed = false;
        public UnitOfWork(AdvertisementContext context)
        {
            _context = context;

            if (AdvertisementRepository is null)
            {
                AdvertisementRepository = new AdvertisementRepository(_context);

            }
            if (CampaignRepository is null)
            {
                CampaignRepository = new CampaignRepository(_context);

            }
            if (IntervalRepository is null)
            {
                IntervalRepository = new IntervalRepository(_context);

            }
            if (MediaRepository is null)
            {
                MediaRepository = new MediaRepository(_context);

            }
            if (MediaMetadataRepository is null)
            {
                MediaMetadataRepository = new MediaMetadataRepository(_context);

            }
            if (BroadcastRepository is null)
            {
                BroadcastRepository = new BroadcastRepository(_context);

            }
            if (AdvertisementsIntervalRepository is null)
            {
                AdvertisementsIntervalRepository = new AdvertisementsIntervalRepository(_context);
            }
        }

        public IAdvertisementRepository AdvertisementRepository { get; private set; }
        public ICampaignRepository CampaignRepository { get; private set; }
        public IIntervalRepository IntervalRepository { get; private set; }
        public IMediaMetadataRepository MediaMetadataRepository { get; private set; }
        public IMediaRepository MediaRepository { get; private set; }
        public IAdvertisementsIntervalRepository AdvertisementsIntervalRepository { get; private set; }
        public IBroadcastRepository BroadcastRepository { get; private set; }

        public void BeginTransaction()
        {
            _context.Database.BeginTransaction();
        }

        public void Commit()
        {
            _context.Database.CommitTransaction();
            _context.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Rollback()
        {
            _context.Database.RollbackTransaction();
            _context.Dispose();
        }

        public void Save()
        {
            _context.SaveChanges();
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            disposed = true;
        }
    }
}
