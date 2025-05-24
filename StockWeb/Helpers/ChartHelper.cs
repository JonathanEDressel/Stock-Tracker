using Portfolio_Tracker.Models;

namespace Portfolio_Tracker.Helpers
{
    public class ChartHelper
    {
        public Task<ChartModel> CreateChart(List<StockModel> data, string chartId, string chartType = "pie")
        {
            ChartModel chart = new ChartModel();
            try
            {
                List<string> labels = new List<string>();
                List<double> values = new List<double>();

                for (int i = 0; i < data.Count; i++)
                {
                    labels.Add(data[i].Symbol ?? "");
                    values.Add((double)data[i].SharesOwned * data[i].CurrentPrice);
                }

                chart = new ChartModel
                {
                    Labels = labels,
                    Data = values,
                    ChartType = chartType,
                    ChartId = chartId
                };
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error creating chart: {e.Message}");
            }
            return Task.FromResult(chart);
        }
    }
}
