using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PortfolioAnalyzer.Models.IEXModels
{
    public class IEXHomeQuote
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }
        [JsonPropertyName("companyName")]
        public string CompanyName { get; set; }
        [JsonPropertyName("calculationPrice")]
        public string CalculationPrice { get; set; }
        [JsonPropertyName("open")]
        public decimal? Open { get; set; }
        [JsonPropertyName("openTime")]
        public long? OpenTime { get; set; }
        [JsonPropertyName("close")]
        public decimal? Close { get; set; }
        [JsonPropertyName("closeTime")]
        public long? CloseTime { get; set; }
        [JsonPropertyName("high")]
        public decimal? High { get; set; }
        [JsonPropertyName("low")]
        public decimal? Low { get; set; }
        [JsonPropertyName("latestPrice")]
        public decimal LatestPrice { get; set; }
        [JsonPropertyName("latestSource")]
        public string LatestSource { get; set; }
        [JsonPropertyName("latestTime")]
        public string LatestTime { get; set; }
        [JsonPropertyName("latestUpdate")]
        public long LatestUpdate { get; set; }
        [JsonPropertyName("latestVolume")]
        public int LatestVolume { get; set; }
        [JsonPropertyName("volume")]
        public int Volume { get; set; }
        [JsonPropertyName("iexRealtimePrice")]
        public decimal IEXRealtimePrice { get; set; }
        [JsonPropertyName("iexRealtimeSize")]
        public int IEXRealtimeSize { get; set; }
        [JsonPropertyName("iexLastUpdate")]
        public long IEXLastUpdated { get; set; }
        [JsonPropertyName("delayedPrice")]
        public decimal? DelayedPrice { get; set; }
        [JsonPropertyName("delayedPriceTime")]
        public long? DelayedPriceTime { get; set; }
        [JsonPropertyName("extendedPrice")]
        public decimal? ExtendedPrice { get; set; }
        [JsonPropertyName("extendedChange")]
        public decimal? ExtendedChange { get; set; }
        [JsonPropertyName("extendedChangePercent")]
        public decimal? ExtendedChangePercent { get; set; }
        [JsonPropertyName("extendedPriceTime")]
        public long? ExtendedPriceTime { get; set; }
        [JsonPropertyName("previousClose")]
        public decimal PreviousClose { get; set; }
        [JsonPropertyName("previousVolume")]
        public int PreviousVolume { get; set; }
        [JsonPropertyName("change")]
        public decimal Change { get; set; }
        [JsonPropertyName("changePercent")]
        public decimal ChangePercent { get; set; }
        [JsonPropertyName("iexMarketPercent")]
        public decimal IEXMarketPercent { get; set; }
        [JsonPropertyName("iexVolume")]
        public int IEXVolume { get; set; }
        [JsonPropertyName("avgTotalVolume")]
        public int AvgTotalVolume { get; set; }
        [JsonPropertyName("iexBidPrice")]
        public decimal IEXBidPrice { get; set; }
        [JsonPropertyName("iexBidSize")]
        public int IEXBidSize { get; set; }
        [JsonPropertyName("iexAskPrice")]
        public decimal IEXAskPrice { get; set; }
        [JsonPropertyName("iexAskSize")]
        public int IEXAskSize { get; set; }
        [JsonPropertyName("marketCap")]
        public long MarketCap { get; set; }
        [JsonPropertyName("week52High")]
        public decimal Week52High { get; set; }
        [JsonPropertyName("week52Low")]
        public decimal Week52Low { get; set; }
        [JsonPropertyName("ytdChange")]
        public decimal YTDChange { get; set; }
        [JsonPropertyName("peRatio")]
        public decimal? PERatio { get; set; }
        [JsonPropertyName("lastTradeTime")]
        public long LastTradeTime { get; set; }
        [JsonPropertyName("isUSMarketOpen")]
        public bool IsUSMarketOpen { get; set; }
    }
}
