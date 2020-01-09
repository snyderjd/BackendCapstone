using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PortfolioAnalyzer.Models
{
    public class NewsAPIResult
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("totalResults")]
        public int TotalResults { get; set; }
        [JsonPropertyName("articles")]
        public List<NewsAPIArticle> Articles { get; set; } = new List<NewsAPIArticle>();

    }
}

