using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PortfolioAnalyzer.Data;
using PortfolioAnalyzer.Models;
using PortfolioAnalyzer.Models.IEXModels;

namespace PortfolioAnalyzer.Controllers
{
    public class SecuritiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _clientFactory;

        public SecuritiesController(ApplicationDbContext context, IConfiguration config, IHttpClientFactory clientFactory)
        {
            _context = context;
            _config = config;
            _clientFactory = clientFactory;
        }

        private string GetToken() => _config.GetValue<string>("Tokens:IEXCloudSK");

        // GET: Securities
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            ViewData["SavedMessage"] = TempData["SavedMessage"];
            ViewData["CurrentSort"] = sortOrder;

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            
            ViewData["CurrentFilter"] = searchString;

            var securities = await _context.Securities.OrderBy(s => s.Ticker).ToListAsync();

            if (!String.IsNullOrEmpty(searchString))
            {
                securities = securities.Where(s => s.Ticker.Contains(searchString)
                    || s.Name.Contains(searchString)).ToList();
            }

            int pageSize = 5;
            return View(PaginatedList<Security>.Create(securities.AsQueryable(), pageNumber ?? 1, pageSize));
        }

        public async Task<IActionResult> AddSecurity(string newTicker)
        {
            var token = GetToken();
            var client = _clientFactory.CreateClient();
            var securities = await _context.Securities.ToListAsync();

            // Check to ensure no securities are found in the database with the same ticker, then retrieve from IEXCloud and save to the security table
            if (!securities.Any(s => s.Ticker == newTicker))
            {
                var request = new HttpRequestMessage(HttpMethod.Get,
                    $"https://cloud.iexapis.com/stable/stock/{newTicker}/company?token={token}");
                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    // Convert the response to an object and save the new security to the database
                    var json = await response.Content.ReadAsStreamAsync();
                    var stockResponse = await System.Text.Json.JsonSerializer.DeserializeAsync<IEXSecurity>(json);
                    Security newSecurity = new Security
                    {
                        Name = stockResponse.CompanyName,
                        Ticker = stockResponse.Ticker,
                        Description = stockResponse.Description
                    };

                    _context.Securities.Add(newSecurity);
                    await _context.SaveChangesAsync();

                    TempData["SavedMessage"] = "The new security has been added.";
                }

                return RedirectToAction(nameof(Index));
            }

            TempData["SavedMessage"] = "";
            return RedirectToAction(nameof(Index));
        }

        // GET: Securities/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var security = await _context.Securities
                .FirstOrDefaultAsync(m => m.Id == id);

            if (security == null) return NotFound();

            return View(security);
        }

        // GET: Securities/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Securities/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Ticker,Description")] Security security)
        {
            if (ModelState.IsValid)
            {
                _context.Add(security);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(security);
        }

        private bool SecurityExists(int id)
        {
            return _context.Securities.Any(e => e.Id == id);
        }
    }
}
