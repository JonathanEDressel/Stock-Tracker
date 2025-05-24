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
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;

namespace Portfolio_Tracker.Pages
{
    public class StocksModel : PageModel
    {
        private readonly StockController _stockController;
        private readonly DatabaseContext _context;

        public List<StockModel> Stocks { get; set; } = new List<StockModel>();

        [BindProperty]
        public StockModel NewStock { get; set; }

        [BindProperty]
        public int ChartSelection { get; set; }
        public ChartModel ChartData { get; set; }

        public StocksModel(IMemoryCache cache, DatabaseContext context, StockController stockController)
        {
            _context = context;
            _stockController = stockController;
        }

        public async Task OnGetAsync()
        {
            try
            {
                Stocks = await _stockController.GetAllStocks(0);

                List<string> labels = new List<string>();
                List<double> values = new List<double>();

                foreach (var s in Stocks)
                {
                    labels.Add(s.Symbol ?? "");
                    values.Add((double)s.SharesOwned * s.CurrentPrice);
                }

                ChartData = new ChartModel
                {
                    Labels = labels,
                    Data = values,
                    ChartType = "pie",
                    ChartId = "pieChart"
                };
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception: ", e);
                ModelState.AddModelError(string.Empty, "Error loading stocks");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var selectedValue = ChartSelection;
            if (NewStock == null || string.IsNullOrEmpty(NewStock.Symbol))
            {
                ModelState.AddModelError(string.Empty, "Invalid stock symbol.");
                return RedirectToPage();
            }

            try
            {
                NewStock = await _stockController.GetStockDetails(NewStock.Symbol);
                if (NewStock == null)
                    return RedirectToPage();

                var res = await _stockController.AddStock(NewStock);
                if (res > 0)
                {
                    _context.Stocks.Add(NewStock);
                    //_cache.Set("StockData", data, cacheData);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception: ", e);
                ModelState.AddModelError(string.Empty, "Error adding stock.");
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSyncStocks()
        {
            try
            {
                Stocks = await _stockController.GetAllStocks(0);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception: ", e);
                ModelState.AddModelError(string.Empty, "Error syncing stocks.");
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSaveStocks(int id, double shares)
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var stock = Stocks.Find(x => x.Id == NewStock.Id); //stocks.Find(x => x.Id == NewStock.Id);

                if (stock != null)
                {
                    stock.SharesOwned = (decimal)shares;
                    await _stockController.UpdateStock(stock);
                    await _context.SaveChangesAsync();
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine("Exception: ", e);
                ModelState.AddModelError(string.Empty, "Error saving stocks.");
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDelete(int id)
        {
            try
            {
                Stocks.RemoveAll(x => x.Id == id);
                var res = await _stockController.RemoveStock(id);
                if (!res)
                {
                    ModelState.AddModelError(string.Empty, "Error deleting stock.");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception: ", e);
                ModelState.AddModelError(string.Empty, "Error deleting stock.");
            }

            return RedirectToPage();
        }
    }
}
