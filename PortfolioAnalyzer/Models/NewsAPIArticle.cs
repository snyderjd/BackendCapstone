using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PortfolioAnalyzer.Models
{
    public class NewsAPIArticle
    {
        [JsonPropertyName("author")]
        public string Author { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("urlToImage")]
        public string ImageUrl { get; set; }
        [JsonPropertyName("publishedAt")]
        public string PublishedAt { get; set; }
        [JsonPropertyName("content")]
        public string Content { get; set; }

        public DateTime PublishDate
        {
            get
            {
                return DateTime.Parse(PublishedAt);
            }
        }
    }
}
