using PortfolioAnalyzer.Models.IEXModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortfolioAnalyzer.Models.ViewModels
{
    public class HomeViewModel
    {
        public IEXHomeQuote Quote { get; set; } = new IEXHomeQuote();

        public NewsAPIResult NewsAPIResult { get; set; }
    }
}
