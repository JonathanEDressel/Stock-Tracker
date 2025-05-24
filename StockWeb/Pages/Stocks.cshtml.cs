using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Portfolio_Tracker.Controllers;
using Portfolio_Tracker.Models;
using System.Diagnostics;
using Portfolio_Tracker.Helpers;

namespace Portfolio_Tracker.Pages
{
    public class StocksModel : PageModel
    {
        private readonly StockController _stockController;
        private readonly DatabaseContext _context;
        private readonly ChartHelper _chartHelper;

        [BindProperty]
        public List<StockModel> Stocks { get; set; } = new List<StockModel>();

        [BindProperty]
        public StockModel NewStock { get; set; }

        [BindProperty]
        public int ChartSelection { get; set; }
        public ChartModel ChartData { get; set; }

        public StocksModel(DatabaseContext context, StockController stockController)
        {
            _context = context;
            _stockController = stockController;
            _chartHelper = new ChartHelper();
        }

        public async Task OnGetAsync()
        {
            try
            {
                Stocks = await _stockController.GetAllStocks(0);

                ChartData = await _chartHelper.CreateChart(Stocks, "pieChart");
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
                Stocks = await _stockController.GetAllStocks(0);
                var stock = Stocks.Find(x => x.Id == id); 

                if (stock != null)
                {
                    stock.SharesOwned = (decimal)shares;
                    stock.TotalValue = shares * stock.CurrentPrice;
                    await _stockController.UpdateStock(stock);
                    await _context.SaveChangesAsync();

                    ChartData = await _chartHelper.CreateChart(Stocks, "pieChart");
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
                Stocks = await _stockController.GetAllStocks(0);
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
