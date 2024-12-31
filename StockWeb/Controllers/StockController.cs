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

        private readonly HttpClient _client;
        private readonly string _baseUrl;

        public StockController(HttpClient client, IConfiguration configuration)
        {
            _client = client;
            _baseUrl = configuration.GetSection("ApiSettings:BaseUrl").Value;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<StockModel> GetStockDetails(string symbol)
        {
            try
            {
                string url = $"{_baseUrl}api/stock/{symbol}";
                var response = await _client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var res = JsonConvert.DeserializeObject<StockModel>(content);
                    return JsonConvert.DeserializeObject<StockModel>(content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching stock details: {ex.Message}");
            }

            return null;
        }

        //Stock_Data table
        public async Task<List<StockModel>> GetAllStocks(int clientid)
        {
            try
            {
                string url = $"{_baseUrl}stocks";
                var response = await _client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var stocks = JsonConvert.DeserializeObject<List<StockModel>>(content);

                    for (var i = 0; i < stocks?.Count; i++)
                    {
                        stocks[i] = await GetStockDetails(stocks[i].Symbol ?? "");
                    }

                    return stocks;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching stocks: {ex.Message}");
            }

            return new List<StockModel>();
        }

        public async Task<bool> RemoveStock(int id)
        {
            try
            {
                string url = $"{_baseUrl}stocks/{id}";
                var response = await _client.DeleteAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing stock: {ex.Message}");
                return false;
            }
        }

        public async Task<int> AddStock(StockModel stk)
        {
            try
            {
                string url = $"{_baseUrl}stocks/stock";
                var content = new StringContent(JsonConvert.SerializeObject(stk), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var resContent = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonConvert.DeserializeObject<StockModel>(resContent);
                    return jsonResponse.Id;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding stock: {ex.Message}");
            }

            return -1;
        }

        public async Task<int> UpdateStock(StockModel stk)
        {
            try
            {
                string url = $"{_baseUrl}stocks/stock/{stk.Id}";
                var content = new StringContent(JsonConvert.SerializeObject(stk), Encoding.UTF8, "application/json");
                var response = await _client.PatchAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var resContent = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonConvert.DeserializeObject<StockModel>(resContent);
                    return jsonResponse.Id;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding stock: {ex.Message}");
            }

            return -1;
        }
    }
}
