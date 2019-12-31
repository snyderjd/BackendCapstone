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
using PortfolioAnalyzer.Models.ViewModels;
using PortfolioAnalyzer.Models.IEXModels;

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

        public async Task<IActionResult> Index(string ticker)
        {
            var viewModel = new HomeViewModel();

            if (ticker != null)
            {
                viewModel.Quote = await GetFullQuote(ticker);
                // Format Quote numbers as necessary

                return View(viewModel);
            }
            else
            {
                ViewData["Ticker"] = "";
                return View(viewModel);
            }

        }

        private async Task<IEXHomeQuote> GetFullQuote(string ticker)
        {
            var token = GetToken();
            var client = _clientFactory.CreateClient();
            IEXHomeQuote quote = new IEXHomeQuote();

            // Send a request to IEXCloud to retrieve a quote and return it
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://cloud.iexapis.com/stable/stock/{ticker}/quote?token={token}");
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                // Convert the quote into IEXHomeQuote and return it
                var json = await response.Content.ReadAsStreamAsync();
                quote = await System.Text.Json.JsonSerializer.DeserializeAsync<IEXHomeQuote>(json);
            }

            return quote;
        }

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
