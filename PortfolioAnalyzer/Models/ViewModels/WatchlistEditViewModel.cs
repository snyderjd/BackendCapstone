using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortfolioAnalyzer.Models.ViewModels
{
    public class WatchlistEditViewModel
    {
        public Watchlist Watchlist { get; set; }
        public List<WatchlistSecurityInput> WatchlistSecurities { get; set; }
    }
}

