using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PortfolioAnalyzer.Models.IEXModels
{
    public class IEXQuote
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }
        [JsonPropertyName("companyName")]
        public string CompanyName { get; set; }
        [JsonPropertyName("latestPrice")]
        public decimal LatestPrice { get; set; }
        [JsonPropertyName("change")]
        public decimal Change { get; set; }
        [JsonPropertyName("week52High")]
        public decimal Week52High { get; set; }
        [JsonPropertyName("week52Low")]
        public decimal Week52Low { get; set; }
        [JsonPropertyName("ytdChange")]
        public decimal YTDChange { get; set; }
        [JsonPropertyName("peRatio")]
        public decimal PERatio { get; set; }
    }
}


