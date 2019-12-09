using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PortfolioAnalyzer.Models
{
    public class Price
    {
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        public decimal AdjClose { get; set; }
    }
}
