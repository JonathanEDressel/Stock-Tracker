using Azure;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Portfolio_Tracker.Models;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Xceed.Wpf.Toolkit;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Portfolio_Tracker.Controllers
{
    public class StockController : Controller
    {
        
        private static readonly string URL = "http://192.168.4.74:5000/";
        public IActionResult Index()
        {
            return View();
        }

        public static async Task<Stock> GetStockDetails(string symbol)
        {
            using HttpClient client = new HttpClient();
            string url = $"{URL}api/stock/{symbol}";

            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(content);
                var name = json["name"].ToString();
                Stock stk;
                stk = new Stock();
                stk.Id = (int)(json["id"] ?? 0);
                stk.Symbol = json["symbol"]?.ToString().ToUpper();
                stk.Company = json["name"]?.ToString();
                stk.Sector = json["sector"]?.ToString();
                stk.SharesOwned = (decimal)(json["sharesOwned"] ?? 0.00);
                stk.CurrentPrice = (double)(json["current_price"] ?? 0);
                stk.HighPrice = (double)(json["high_price"] ?? 0);
                stk.LowPrice = (double)(json["low_price"] ?? 0);
                stk.OpenPrice = (double)(json["open_price"] ?? 0);
                stk.ExpenseRatio = (double)(json["expense_ratio"] ?? 0);
                stk.DividendYield = (double)(json["dividend_yeild"] ?? 0);

                if (json != null && name != "")
                    return stk;
            }

            return null;
        }

        //Stock_Data table
        public static async Task<List<Stock>> GetAllStocks(int clientid)
        {
            try
            {
                List<Stock> stocks = new List<Stock>();

                using HttpClient client = new HttpClient();
                string url = $"{URL}stocks";

                var response = await client.GetAsync(url);
                if(response.IsSuccessStatusCode)
                {
                    dynamic json = await response.Content.ReadAsStringAsync();
                    dynamic dynJson = JsonConvert.DeserializeObject(json);
                    if (dynJson != null)
                    {
                        foreach(var item in dynJson)
                        {
                            var sym = item["symbol"].ToString();
                            Stock stk = new Stock();
                            stk = await GetStockDetails(sym);
                            stk.Id = (int)item["id"];
                            stk.Company = item["company"].ToString();
                            stk.Symbol = sym;
                            stk.Sector = item["sector"].ToString();
                            stk.SharesOwned = (decimal) (item["sharesOwned"] ?? 0.00);

                            stocks.Add(stk);
                        }
                    }
                }
                
                return stocks;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public static async Task<bool> RemoveStock(int id)
        {
            try
            {
                using HttpClient client = new HttpClient();
                string url = $"{URL}stocks/{id}";

                var jscontent = JsonConvert.SerializeObject(id);
                var content = new StringContent(jscontent, Encoding.UTF8, "application/json");

                var response = await client.DeleteAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    //string res = await response.Content.ReadAsStringAsync();
                    //JObject json = JObject.Parse(res);
                    //var id = (int)json["id"];
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public static async Task<int> AddStock(Stock stk)
        {
            try
            {
                using HttpClient client = new HttpClient();
                string url = $"{URL}stocks/stock";

                var jscontent = JsonConvert.SerializeObject(stk);
                var content = new StringContent(jscontent, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    string res = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(res);
                    var id = (int) json["id"];
                    return id;
                }

                return -1;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return -1;
            }
        }
    }
}
