using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PortfolioAnalyzer.Models.ViewModels
{
    public class PortfolioDetailsViewModel
    {
        public Portfolio Portfolio { get; set; }
        public Dictionary<DateTime, decimal> PortfolioValues { get; set; } = new Dictionary<DateTime, decimal>();
        public Dictionary<DateTime, decimal> MonthlyReturns { get; set; } = new Dictionary<DateTime, decimal>();
        public SortedDictionary<DateTime, decimal> CumulativeReturns { get; set; } = new SortedDictionary<DateTime, decimal>();
        public decimal Return { get; set; }
        public decimal CAGR { get; set; }
        public decimal StdDeviation { get; set; }
        public List<SelectListItem> TimePeriodOptions { get; }
        public string TimePeriod { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public string ChartData { get; set; }

        public string AssetAllocationData { get; set; }

        public PortfolioDetailsViewModel()
        {
            TimePeriodOptions = new List<SelectListItem>()
            {
                new SelectListItem("1 Year", "1y"),
                new SelectListItem("2 Years", "2y"),
                new SelectListItem("5 Years", "5y"),
                new SelectListItem("Max", "max")
            };
        }
    }
}
