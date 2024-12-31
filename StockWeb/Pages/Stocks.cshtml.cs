using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;
using Portfolio_Tracker.Controllers;
using Portfolio_Tracker.Models;
using System.Windows.Forms;
using System.Runtime.InteropServices.JavaScript;
using Xceed.Wpf.Toolkit;
using Portfolio_Tracker.Pages.Shared;
using System.Data.Entity.Core.Common.EntitySql;

namespace Portfolio_Tracker.Pages
{
    public class StocksModel : PageModel
    {
        private readonly StockController _stockController;
        public List<StockModel> Stocks { get; set; } = new List<StockModel>();
        private readonly DatabaseContext _context;

        [BindProperty]
        public StockModel NewStock { get; set; }
        public ChartModel PieChartData { get; set; }

        public StocksModel(DatabaseContext context, StockController stockController)
        {
            _context = context;
            _stockController = stockController;
        }

        public async Task OnGetAsync()
        {
            var stocksTask = _stockController.GetAllStocks(0);
            Stocks = await stocksTask;

            List<string> labels = new List<string>();
            List<double> data = new List<double>();

            Stocks.ForEach(x =>
                { 
                    labels.Add(x.Symbol ?? "");

                    double price = (double) x.CurrentPrice;
                    double shares = (double) x.SharesOwned;
                    var currentValue = shares * price;
                    data.Add(currentValue);
                });

            PieChartData = new ChartModel
            {
                ChartId = "myPieChart",
                Labels = labels,
                Data = data
            };
        }

        public async Task<IActionResult> OnPostSyncStocks()
        {
            await _stockController.GetAllStocks(0);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (NewStock == null || string.IsNullOrEmpty(NewStock.Symbol))
            {
                ModelState.AddModelError(string.Empty, "Invalid stock symbol.");
                return RedirectToPage();
            }

            NewStock = await _stockController.GetStockDetails(NewStock.Symbol);
            if (NewStock == null)
                return RedirectToPage();

            var res = await _stockController.AddStock(NewStock);
            if (res > 0)
            {
                _context.Stocks.Add(NewStock);
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSaveStocks(int id, double shares)
        {
            if (!ModelState.IsValid)
                return Page();

            var stocks = await _stockController.GetAllStocks(0);
            var stock = stocks.Find(x => x.Id == NewStock.Id);
            
            if(stock != null)
            {
                stock.SharesOwned = (decimal) shares;
                await _stockController.UpdateStock(stock);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDelete(int id)
        {
            var res = await _stockController.RemoveStock(id);
            if (res)
                return RedirectToPage();
            return RedirectToPage();
        }

    }
}
