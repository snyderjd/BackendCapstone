using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
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
        public DbSet<Watchlist> Watchlists { get; set; }
        public DbSet<WatchlistSecurity> WatchlistSecurities {get; set;}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Ignore<Price>();

            // Customize the ASP.NET Identity model and override the defaults if needed
            // For example, you can rename the ASP.NET Identity table names and more
            // Add your customizations after calling base.OnModelCreating(builder)
            modelBuilder.Entity<Portfolio>().ToTable("Portfolio");
            modelBuilder.Entity<Security>().ToTable("Security");
            modelBuilder.Entity<PortfolioSecurity>().ToTable("PortfolioSecurity");
            modelBuilder.Entity<AssetClass>().ToTable("AssetClass");
            modelBuilder.Entity<Watchlist>().ToTable("Watchlist");
            modelBuilder.Entity<WatchlistSecurity>().ToTable("WatchlistSecurity");

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

            // Restrict deletion of related Security when WatchlistSecurity entry is removed
            modelBuilder.Entity<Security>()
                .HasMany(s => s.WatchlistSecurities)
                .WithOne(ws => ws.Security)
                .OnDelete(DeleteBehavior.Restrict);

            // Restrict delection of related Watchlist when WatchlistSecurity entry is removed
            modelBuilder.Entity<Watchlist>()
                .HasMany(w => w.WatchlistSecurities)
                .WithOne(ws => ws.Watchlist)
                .OnDelete(DeleteBehavior.Restrict);

            // Add seed data to the database
            ApplicationUser user = new ApplicationUser
            {
                FirstName = "Admin",
                LastName = "Admin",
                UserName = "admin@admin.com",
                NormalizedUserName = "ADMIN@ADMIN.COM",
                Email = "admin@admin.com",
                NormalizedEmail = "ADMIN@ADMIN.COM",
                EmailConfirmed = true,
                LockoutEnabled = false,
                SecurityStamp = "7f434309-a4d9-48e9-9ebb-8803db794577",
                Id = "00000000-ffff-ffff-ffff-ffffffffffff"
            };
            var passwordHash = new PasswordHasher<ApplicationUser>();
            user.PasswordHash = passwordHash.HashPassword(user, "Admin8*");
            modelBuilder.Entity<ApplicationUser>().HasData(user);

            ApplicationUser user2 = new ApplicationUser
            {
                FirstName = "Joe",
                LastName = "Snyder",
                UserName = "joe@gmail.com",
                NormalizedUserName = "JOE@GMAIL.COM",
                Email = "joe@gmail.com",
                NormalizedEmail = "JOE@GMAIL.COM",
                EmailConfirmed = true,
                LockoutEnabled = false,
                SecurityStamp = "7f434309-a4d9-48e9-9ebb-8803db794578",
                Id = "00000000-ffff-ffff-ffff-fffffffffff1"
            };
            user2.PasswordHash = passwordHash.HashPassword(user2, "Admin8*");
            modelBuilder.Entity<ApplicationUser>().HasData(user2);

            // Create seed portfolio
            modelBuilder.Entity<Portfolio>().HasData(
                new Portfolio()
                {
                    Id = 1,
                    Name = "Technology Stocks",
                    Description = "Portfolio of technology stocks",
                    UserId = user2.Id
                }
            );

            // Create seed securities
            modelBuilder.Entity<Security>().HasData(
                new Security()
                {
                    Id = 1,
                    Name = "Apple, Inc.",
                    Ticker = "AAPL",
                    Description = "Apple, Inc. engages in the design, manufacture, and sale of smartphones, personal computers, tablets, wearables and accessories, and other variety of related services. It operates through the following geographical segments: Americas, Europe, Greater China, Japan, and Rest of Asia Pacific. The Americas segment includes North and South America. The Europe segment consists of European countries, as well as India, the Middle East, and Africa. The Greater China segment comprises of China, Hong Kong, and Taiwan. The Rest of Asia Pacific segment includes Australia and Asian countries. Its products and services include iPhone, Mac, iPad, AirPods, Apple TV, Apple Watch, Beats products, Apple Care, iCloud, digital content stores, streaming, and licensing services. The company was founded by Steven Paul Jobs, Ronald Gerald Wayne, and Stephen G. Wozniak on April 1, 1976 and is headquartered in Cupertino, CA."
                },
                new Security()
                {
                    Id = 2,
                    Name = "Microsoft Corp.",
                    Ticker = "MSFT",
                    Description = "Microsoft Corp. engages in the development and support of software, services, devices, and solutions. It operates through the following business segments: Productivity and Business Processes; Intelligent Cloud; and More Personal Computing. The Productivity and Business Processes segment comprises products and services in the portfolio of productivity, communication, and information services of the company spanning a variety of devices and platform. The Intelligent Cloud segment refers to the public, private, and hybrid serve products and cloud services of the company which can power modern business. The More Personal Computing segment encompasses products and services geared towards the interests of end users, developers, and IT professionals across all devices. The firm also offers operating systems; cross-device productivity applications; server applications; business solution applications; desktop and server management tools; software development tools; video games; personal computers, tablets; gaming and entertainment consoles; other intelligent devices; and related accessories. The company was founded by Paul Gardner Allen and William Henry Gates III in 1975 and is headquartered in Redmond, WA."
                },
                new Security()
                {
                    Id = 3,
                    Name = "Netflix, Inc.",
                    Ticker = "NFLX",
                    Description = "Netflix, Inc. is an Internet subscription service company, which provides subscription service streaming movies and television episodes over the Internet and sending DVDs by mail. It operates through the following segments: Domestic Streaming, International Streaming and Domestic DVD. The Domestic Streaming segment derives revenues from monthly membership fees for services consisting solely of streaming content to its members in the United States. The International Streaming segment includes fees from members outside the United States. The Domestic DVD segment covers revenues from services consisting solely of DVD-by-mail. The company was founded by Marc Randolph and Wilmot Reed Hastings Jr., on August 29, 1997 and is headquartered in Los Gatos, CA."
                }
            );

            // Create seed data for AssetClass table
            modelBuilder.Entity<AssetClass>().HasData(
                new AssetClass()
                {
                    Id = 1,
                    Name = "Equities"
                },
                new AssetClass()
                {
                    Id = 2,
                    Name = "Fixed Income"
                },
                new AssetClass()
                {
                    Id = 3,
                    Name = "Commodities"
                },
                new AssetClass()
                {
                    Id = 4,
                    Name = "Precious Metals"
                },
                new AssetClass()
                {
                    Id = 5,
                    Name = "Real Estate"
                },
                new AssetClass()
                {
                    Id = 6,
                    Name = "Alternatives"
                },
                new AssetClass()
                {
                    Id = 7,
                    Name = "Cash"
                }
            );

            // Create seed data for PortfolioSecurity table
            modelBuilder.Entity<PortfolioSecurity>().HasData(
                new PortfolioSecurity()
                {
                    Id = 1,
                    PortfolioId = 1,
                    SecurityId = 1,
                    Weight = 30,
                    AssetClassId = 1
                },
                new PortfolioSecurity()
                {
                    Id = 2,
                    PortfolioId = 1,
                    SecurityId = 2,
                    Weight = 30,
                    AssetClassId = 1
                },
                new PortfolioSecurity()
                {
                    Id = 3,
                    PortfolioId = 1,
                    SecurityId = 3,
                    Weight = 40,
                    AssetClassId = 1
                }
            );

            // Create seed data for Watchlist table
            modelBuilder.Entity<Watchlist>().HasData(
                new Watchlist()
                {
                    Id = 1,
                    UserId = "00000000-ffff-ffff-ffff-fffffffffff1",
                    Name = "Joe's first watchlist",
                    Description = "Optional description goes here"
            });

            // Create seed data for WatchlistSecurity table
            modelBuilder.Entity<WatchlistSecurity>().HasData(
                new WatchlistSecurity()
                {
                    Id = 1,
                    WatchlistId = 1,
                    SecurityId = 1
                },
                new WatchlistSecurity()
                {
                    Id = 2,
                    WatchlistId = 1,
                    SecurityId = 2
                },
                new WatchlistSecurity()
                {
                    Id = 3,
                    WatchlistId = 1,
                    SecurityId = 3
                }
            );



        }
    }
}
