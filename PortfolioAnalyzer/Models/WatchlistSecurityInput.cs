using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortfolioAnalyzer.Models
{
    public class WatchlistSecurityInput
    {
        public int? Id { get; set; }
        public int? WatchlistId { get; set; }
        public Watchlist Watchlist { get; set; }
        public int? SecurityId { get; set; }
        public SecurityInput Security { get; set; }
        public bool HasSecurity { get; set; }
    }
}

