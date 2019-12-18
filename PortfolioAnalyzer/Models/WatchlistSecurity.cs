using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PortfolioAnalyzer.Models
{
    public class WatchlistSecurity
    {
        public int Id { get; set; }
        [Required]
        public int WatchlistId { get; set; }
        public Watchlist Watchlist { get; set; }
        [Required]
        public int SecurityId { get; set; }
        public Security Security { get; set; }
    }
}


