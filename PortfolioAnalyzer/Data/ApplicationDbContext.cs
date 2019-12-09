using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PortfolioAnalyzer.Models;

namespace PortfolioAnalyzer.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Portfolio> Portfolios { get; set; }
        public DbSet<Security> Securities { get; set; }
        public DbSet<PortfolioSecurity> PortfolioSecurities { get; set; }
        public DbSet<AssetClass> AssetClasses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Customize the ASP.NET Identity model and override the defaults if needed
            // For example, you can rename the ASP.NET Identity table names and more
            // Add your customizations after calling base.OnModelCreating(builder)
            modelBuilder.Entity<Portfolio>().ToTable("Portfolio");
            modelBuilder.Entity<Security>().ToTable("Security");
            modelBuilder.Entity<PortfolioSecurity>().ToTable("PortfolioSecurity");
            modelBuilder.Entity<AssetClass>().ToTable("AssetClass");

            modelBuilder.Entity<Portfolio>()
                .Property(p => p.DateCreated)
                .HasDefaultValueSql("GETDATE()");

            // Restrict deletion of related Security when PortfolioSecurity entry is removed
            modelBuilder.Entity<Security>()
                .HasMany(s => s.PortfolioSecurities)
                .WithOne(ps => ps.Security)
                .OnDelete(DeleteBehavior.Restrict);

            // Restrict deletion of related Portfolio when PortfolioSecurity entry is removed
            modelBuilder.Entity<Portfolio>()
                .HasMany(p => p.PortfolioSecurities)
                .WithOne(ps => ps.Portfolio)
                .OnDelete(DeleteBehavior.Restrict);

            // Restrict deletion of related AssetClass when PortfolioSecurity entry is removed
            modelBuilder.Entity<AssetClass>()
                .HasMany(ac => ac.PortfolioSecurities)
                .WithOne(ps => ps.AssetClass)
                .OnDelete(DeleteBehavior.Restrict);

            // Add seed data to the database

        }
    }
}
