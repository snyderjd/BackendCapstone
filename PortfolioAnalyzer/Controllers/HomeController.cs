using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PortfolioAnalyzer.Models;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

namespace PortfolioAnalyzer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _clientFactory;

        public HomeController(ILogger<HomeController> logger, IConfiguration config, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _config = config;
            _clientFactory = clientFactory;
        }
        private string GetToken() => _config.GetValue<string>("Tokens:IEXCloudSK");

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetFullQuote(string ticker)
        {
            var token = GetToken();
            var client = _clientFactory.CreateClient();
            

        }

        //public async Task<IActionResult> AddSecurity(string newTicker)
        //{
        //    var token = GetToken();
        //    var client = _clientFactory.CreateClient();
        //    var securities = await _context.Securities.ToListAsync();

        //    // Check to ensure no securities are found in the database with the same ticker, then retrieve from IEXCloud and save to the security table
        //    if (!securities.Any(s => s.Ticker == newTicker))
        //    {
        //        var request = new HttpRequestMessage(HttpMethod.Get,
        //            $"https://cloud.iexapis.com/stable/stock/{newTicker}/company?token={token}");
        //        var response = await client.SendAsync(request);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            // Convert the response to an object and save the new security to the database
        //            var json = await response.Content.ReadAsStreamAsync();
        //            var stockResponse = await System.Text.Json.JsonSerializer.DeserializeAsync<IEXSecurity>(json);
        //            Security newSecurity = new Security
        //            {
        //                Name = stockResponse.CompanyName,
        //                Ticker = stockResponse.Ticker,
        //                Description = stockResponse.Description
        //            };

        //            _context.Securities.Add(newSecurity);
        //            await _context.SaveChangesAsync();

        //            TempData["SavedMessage"] = "The new security has been added.";
        //        }

        //        return RedirectToAction(nameof(Index));
        //    }

        //    TempData["SavedMessage"] = "";
        //    return RedirectToAction(nameof(Index));
        //}

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
