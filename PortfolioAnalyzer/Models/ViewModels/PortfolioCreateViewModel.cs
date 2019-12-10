using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortfolioAnalyzer.Models.ViewModels
{
    public class PortfolioCreateViewModel
    {
        public List<string> Tickers { get; set; }
        public List<int> Weights { get; set; }
        public List<SelectListItem> AssetClasses { get; set; }
        public List<int> AssetClassIds { get; set; }
        public Portfolio Portfolio { get; set; }

    }
}
