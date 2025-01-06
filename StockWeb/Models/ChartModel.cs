namespace Portfolio_Tracker.Models
{
    public class ChartModel
    {
        public string ChartId { get; set; } = "pieChart";
        public string ChartType { get; set; } = "pie";
        public List<string> Labels { get; set; } = [];
        public List<double> Data { get; set; } = [];
    }
}
