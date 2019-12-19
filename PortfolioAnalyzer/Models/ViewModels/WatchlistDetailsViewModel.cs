using PortfolioAnalyzer.Models.IEXModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortfolioAnalyzer.Models.ViewModels
{
    public class WatchlistDetailsViewModel
    {
        public Watchlist Watchlist { get; set; }
        public ICollection<IEXQuote> Quotes { get; set; } = new List<IEXQuote>();

    }
}
