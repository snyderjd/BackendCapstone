using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PortfolioAnalyzer.Models.IEXModels
{
    public class IEXPrice
    {
        [JsonPropertyName("date")]
        public string StringDate { get; set; }
        [JsonPropertyName("close")]
        public decimal Close { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date
        {
            get
            {
                return Convert.ToDateTime(StringDate);
            }
        }
        
    }
}
