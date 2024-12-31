using Newtonsoft.Json;

namespace Portfolio_Tracker.Models
{
    public class StockModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("sharesOwned")]
        public decimal SharesOwned { get; set; }
        [JsonProperty("symbol")]
        public string? Symbol { get; set; }
        [JsonProperty("name")]
        public string? Company { get; set; }
        [JsonProperty("sector")]
        public string? Sector { get; set; }
        [JsonProperty("current_price")]
        public double CurrentPrice { get; set; }
        [JsonProperty("high_price")]
        public double HighPrice { get; set; }
        [JsonProperty("low_price")]
        public double LowPrice { get; set; }
        [JsonProperty("open_price")]
        public double OpenPrice { get; set; }
        [JsonProperty("expense_ratio")]
        public double ExpenseRatio { get; set; }
        [JsonProperty("div_yield")]
        public double DividendYield { get; set; }
    }
    public class StockUpdateModel
    {
        public int StockId { get; set; }
        public double SharesOwned { get; set; }
    }
}
