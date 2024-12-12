namespace Portfolio_Tracker.Models
{
    public class StockDetails
    {
        
    }

    public class Stock : StockDetails
    {
        public int Id { get; set; }
        public decimal SharesOwned { get; set; }
        public string? Symbol { get; set; }
        public string? Company { get; set; }
        public string? Sector { get; set; }
        public double CurrentPrice { get; set; }
        public double HighPrice { get; set; }
        public double LowPrice { get; set; }
        public double OpenPrice { get; set; }
        public double ExpenseRatio { get; set; }
        public double DividendYield { get; set; }
    }
}
