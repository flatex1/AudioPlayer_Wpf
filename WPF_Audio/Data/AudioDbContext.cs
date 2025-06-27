using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF_Audio.Models;

namespace WPF_Audio.Data
{
    public class AudioDbContext : DbContext
    {
        public DbSet<AudioTrack> AudioTracks { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<PlaylistTrack> PlaylistTracks { get; set; }
        public DbSet<QueueItem> QueueItems { get; set; }
        public DbSet<HotkeySetting> HotkeySettings { get; set; }
        public DbSet<TrackFeature> TrackFeatures { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=AudioLibrary.db;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PlaylistTrack>()
                .HasOne(pt => pt.Playlist)
                .WithMany(p => p.PlaylistTracks)
                .HasForeignKey(pt => pt.PlaylistId);

            modelBuilder.Entity<PlaylistTrack>()
                .HasOne(pt => pt.AudioTrack)
                .WithMany() 
                .HasForeignKey(pt => pt.AudioTrackId);

            modelBuilder.Entity<QueueItem>()
                .HasOne(q => q.AudioTrack)
                .WithMany()
                .HasForeignKey(q => q.AudioTrackId);

            base.OnModelCreating(modelBuilder);
        }
    }
}