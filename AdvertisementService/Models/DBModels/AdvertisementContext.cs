﻿using Microsoft.EntityFrameworkCore;

namespace AdvertisementService.Models.DBModels
{
    public partial class AdvertisementContext : DbContext
    {
        public AdvertisementContext()
        {
        }

        public AdvertisementContext(DbContextOptions<AdvertisementContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Advertisements> Advertisements { get; set; }
        public virtual DbSet<Broadcasts> Broadcasts { get; set; }
        public virtual DbSet<AdvertisementsIntervals> AdvertisementsIntervals { get; set; }
        public virtual DbSet<Campaigns> Campaigns { get; set; }
        public virtual DbSet<Intervals> Intervals { get; set; }
        public virtual DbSet<MediaMetadata> MediaMetadata { get; set; }
        public virtual DbSet<Medias> Medias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Advertisements>(entity =>
            {
                entity.HasKey(e => e.AdvertisementId)
                    .HasName("PRIMARY");

                entity.ToTable("advertisements");

                entity.HasIndex(e => e.MediaId)
                    .HasName("media_id");

                entity.Property(e => e.AdvertisementId).HasColumnName("advertisement_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("timestamp");

                entity.Property(e => e.InstitutionId).HasColumnName("institution_id");

                entity.Property(e => e.InvertedTintColor).HasColumnName("inverted_tint_color");

                entity.Property(e => e.MediaId).HasColumnName("media_id");

                entity.Property(e => e.ResourceNumber)
                    .HasColumnName("resource_number")
                    .HasColumnType("varchar(21)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.TintColor).HasColumnName("tint_color");

                entity.HasOne(d => d.Media)
                    .WithMany(p => p.Advertisements)
                    .HasForeignKey(d => d.MediaId)
                    .HasConstraintName("advertisements_ibfk_1");
            });

            modelBuilder.Entity<Broadcasts>(entity =>
            {
                entity.HasKey(e => e.BroadcastId)
                    .HasName("PRIMARY");

                entity.ToTable("broadcasts");

                entity.HasIndex(e => e.CampaignId)
                    .HasName("campaign_id");

                entity.Property(e => e.BroadcastId).HasColumnName("broadcast_id");

                entity.Property(e => e.AdvertisementId).HasColumnName("advertisement_id");

                entity.Property(e => e.CampaignId).HasColumnName("campaign_id");

                entity.Property(e => e.Sort).HasColumnName("sort");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("timestamp");

                entity.HasOne(d => d.Advertisement)
                    .WithMany(p => p.Broadcasts)
                    .HasForeignKey(d => d.AdvertisementId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("advertisements_campaigns_ibfk_2");

                entity.HasOne(d => d.Campaign)
                    .WithMany(p => p.Broadcasts)
                    .HasForeignKey(d => d.CampaignId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("advertisements_campaigns_ibfk_1");
            });

            modelBuilder.Entity<AdvertisementsIntervals>(entity =>
            {
                entity.HasKey(e => new { e.IntervalId, e.AdvertisementId })
                    .HasName("PRIMARY");

                entity.ToTable("advertisements_intervals");

                entity.HasIndex(e => e.AdvertisementId)
                    .HasName("advertisement_id");

                entity.Property(e => e.IntervalId).HasColumnName("interval_id");

                entity.Property(e => e.AdvertisementId).HasColumnName("advertisement_id");

                entity.HasOne(d => d.Advertisement)
                    .WithMany(p => p.AdvertisementsIntervals)
                    .HasForeignKey(d => d.AdvertisementId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("advertisements_intervals_ibfk_2");

                entity.HasOne(d => d.Interval)
                    .WithMany(p => p.AdvertisementsIntervals)
                    .HasForeignKey(d => d.IntervalId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("advertisements_intervals_ibfk_1");
            });

            modelBuilder.Entity<Campaigns>(entity =>
            {
                entity.HasKey(e => e.CampaignId)
                    .HasName("PRIMARY");

                entity.ToTable("campaigns");

                entity.Property(e => e.CampaignId).HasColumnName("campaign_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("timestamp");

                entity.Property(e => e.EndAt)
                    .HasColumnName("end_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.StartAt)
                    .HasColumnName("start_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasColumnType("enum('active','inactive')")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Title)
                    .HasColumnName("title")
                    .HasColumnType("varchar(50)")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("timestamp");
            });

            modelBuilder.Entity<Intervals>(entity =>
            {
                entity.HasKey(e => e.IntervalId)
                    .HasName("PRIMARY");

                entity.ToTable("intervals");

                entity.Property(e => e.IntervalId).HasColumnName("interval_id");

                entity.Property(e => e.Title)
                    .HasColumnName("title")
                    .HasColumnType("varchar(30)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");
            });

            modelBuilder.Entity<MediaMetadata>(entity =>
            {
                entity.ToTable("media_metadata");

                entity.Property(e => e.MediaMetadataId).HasColumnName("media_metadata_id");

                entity.Property(e => e.Duration).HasColumnName("duration");

                entity.Property(e => e.Size).HasColumnName("size");
            });

            modelBuilder.Entity<Medias>(entity =>
            {
                entity.HasKey(e => e.MediaId)
                    .HasName("PRIMARY");

                entity.ToTable("medias");

                entity.HasIndex(e => e.MediaMetadataId)
                    .HasName("media_metadata_id");

                entity.Property(e => e.MediaId).HasColumnName("media_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("timestamp");

                entity.Property(e => e.MediaMetadataId).HasColumnName("media_metadata_id");

                entity.Property(e => e.MediaType)
                    .HasColumnName("media_type")
                    .HasColumnType("enum('video','image')")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Url)
                    .HasColumnName("url")
                    .HasColumnType("varchar(255)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.HasOne(d => d.MediaMetadata)
                    .WithMany(p => p.Medias)
                    .HasForeignKey(d => d.MediaMetadataId)
                    .HasConstraintName("medias_ibfk_1");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
