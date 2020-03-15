using System;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Data
{
    public class PostariusCdnContext : DbContext
    {
        public DbSet<Media> Media { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=127.0.0.1;Database=PostariusCdn;Username=asp;Password=asp");
        }
    }
}