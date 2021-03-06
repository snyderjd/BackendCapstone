﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PortfolioAnalyzer.Models.IEXModels
{
    public class IEXNewsItem
    {
        [JsonPropertyName("datetime")]
        public double DateNumber { get; set; }
        public DateTime Date
        {
            get
            {
                DateTime beginDate = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                return beginDate.AddMilliseconds(DateNumber).ToLocalTime();
            }
        }

        [JsonPropertyName("headline")]
        public string Headline { get; set; }
        [JsonPropertyName("source")]
        public string Source { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("summary")]
        public string Summary { get; set; }
        [JsonPropertyName("related")]
        public string Related { get; set; }
        [JsonPropertyName("image")]
        public string Image { get; set; }
        [JsonPropertyName("lang")]
        public string Language { get; set; }
        [JsonPropertyName("hasPaywall")]
        public bool HasPaywall { get; set; }
         
    }
}
