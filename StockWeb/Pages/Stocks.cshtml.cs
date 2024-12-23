using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;
using Portfolio_Tracker.Controllers;
using Portfolio_Tracker.Models;
using System.Windows.Forms;
using System.Runtime.InteropServices.JavaScript;
using Xceed.Wpf.Toolkit;

namespace Portfolio_Tracker.Pages
{
    public class StocksModel : PageModel
    {
        private readonly DatabaseContext _context;
        private readonly StockController _stockController;

        public List <Stock> Stocks { get; set; } = new List<Stock>();
        [BindProperty]
        public Stock NewStock { get; set; }

        public StocksModel(DatabaseContext context)
        {
            _context = context;
            _stockController = new StockController();
        }

        public async Task OnGetAsync()
        {
            Stocks = await StockController.GetAllStocks(0);
        }

        public async Task<IActionResult> OnPostSyncStocks()
        {
            Stocks = await StockController.GetAllStocks(0);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            string sym = NewStock?.Symbol ?? "";
            NewStock = await StockController.GetStockDetails(NewStock?.Symbol ?? "");
            if (NewStock == null)
                return RedirectToPage();

            var res = await StockController.AddStock(NewStock);
            if(res > 0)
            {
                _context.Stocks.Add(NewStock);
                _context.SaveChanges();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDelete(int id)
        {
            var res = await StockController.RemoveStock(id);
            if (res)
                return RedirectToPage();
            return RedirectToPage();
        }

    }
}
