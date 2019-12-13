using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortfolioAnalyzer.Models.ViewModels
{
    public class PortfolioEditViewModel
    {
        public List<AssetClass> AssetClasses { get; set; }
        public List<SelectListItem> AssetClassOptions
        {
            get
            {
                if (AssetClasses == null) return null;
                return AssetClasses.Select(a => new SelectListItem(a.Name, a.Id.ToString())).ToList();
            }
        }
        public Portfolio Portfolio { get; set; }
        public List<PortfolioSecurity> PortfolioSecurities { get; set; }
    }
}
