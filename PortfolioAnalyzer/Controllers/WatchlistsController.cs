using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PortfolioAnalyzer.Data;
using PortfolioAnalyzer.Models;
using PortfolioAnalyzer.Models.IEXModels;
using PortfolioAnalyzer.Models.ViewModels;

namespace PortfolioAnalyzer.Controllers
{
    public class WatchlistsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _clientFactory;

        public WatchlistsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IConfiguration config, IHttpClientFactory clientFactory)
        {
            _context = context;
            _userManager = userManager;
            _config = config;
            _clientFactory = clientFactory;
        }

        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);
        private string GetToken() => _config.GetValue<string>("Tokens:IEXCloudSK");

        // GET: Watchlists
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();

            var watchlists = await _context.Watchlists
                .Where(w => w.UserId == user.Id)
                .Include(w => w.User).ToListAsync();

            return View(watchlists);
        }

        // GET: Watchlists/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var watchlist = await _context.Watchlists
                .Include(w => w.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (watchlist == null) return NotFound();

            return View(watchlist);
        }

        // GET: Watchlists/Create
        public IActionResult Create()
        {
            // Create the viewModel
            var viewModel = new WatchlistCreateViewModel
            {
                Watchlist = new Watchlist(),
                WatchlistSecurities = new List<WatchlistSecurityInput>()
            };

            // Add 10 WatchlistSecurity objects to the viewModel's list
            for (int i = 0; i < 10; i++)
            {
                viewModel.WatchlistSecurities.Add(new WatchlistSecurityInput());
            }

            return View(viewModel);
        }

        // POST: Watchlists/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Watchlist, WatchlistSecurities")] WatchlistCreateViewModel viewModel)
        {
            var user = await GetCurrentUserAsync();
            string token = GetToken();
            var client = _clientFactory.CreateClient();

            // Get all the securities from the database
            var securities = _context.Securities;

            // Iterate over the list of WatchlistSecurities entered by the user
            foreach (WatchlistSecurityInput ws in viewModel.WatchlistSecurities)
            {
                if (ws.Security.Ticker != null) // Only check for security if the row was not blank
                {
                    string ticker = ws.Security.Ticker;
                    if (!securities.Any(s => s.Ticker == ticker))
                    {
                        // Security is not in the DB & needs to be retrieved from IEXCloud and saved to the DB
                        var request = new HttpRequestMessage(HttpMethod.Get,
                            $"https://cloud.iexapis.com/stable/stock/{ticker}/company?token={token}");
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
                        }
                    }
                }
            }

            // Now all the securities should be in the DB. Get a new reference to them and iterate over the list of WatchlistSecurities again
            var updatedSecurities = _context.Securities;

            // Save the new watchlist to the database and get a reference to its Id
            viewModel.Watchlist.UserId = user.Id;
            _context.Add(viewModel.Watchlist);
            await _context.SaveChangesAsync();
            int watchlistId = viewModel.Watchlist.Id;

            // Iterate over WatchlistSecurities again and enter them into the database with their properties
            foreach (WatchlistSecurityInput ws in viewModel.WatchlistSecurities)
            {
                if (ws.Security.Ticker != null) // only create new WS if the row was not blank
                {
                    Security matchingSecurity = updatedSecurities.First(s => s.Ticker == ws.Security.Ticker);

                    WatchlistSecurity newWS = new WatchlistSecurity
                    {
                        WatchlistId = watchlistId,
                        SecurityId = matchingSecurity.Id
                    };

                    _context.Add(newWS);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Watchlists/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // Get the watchlist and include navigation properties
            var watchlist = await _context.Watchlists
                .Include(w => w.WatchlistSecurities)
                    .ThenInclude(w => w.Security)
                .FirstOrDefaultAsync(w => w.Id == id);

            var viewModel = new WatchlistEditViewModel()
            {
                Watchlist = watchlist,
                WatchlistSecurities = new List<WatchlistSecurityInput>()
            };

            // Assign the current WatchlistSecurities to the viewModel
            foreach (WatchlistSecurity ws in viewModel.Watchlist.WatchlistSecurities)
            {
                WatchlistSecurityInput currentWS = new WatchlistSecurityInput()
                {
                    Id = ws.Id,
                    WatchlistId = ws.WatchlistId,
                    SecurityId = ws.SecurityId,
                    Security = new SecurityInput
                    {
                        Ticker = ws.Security.Ticker
                    },
                    HasSecurity = true
                };

                viewModel.WatchlistSecurities.Add(currentWS);
            }

            // Add 10 more WatchlistSecurity objects to the viewModel's list
            for (int i = 0; i < 10; i++)
            {
                viewModel.WatchlistSecurities.Add(new WatchlistSecurityInput());
            }

            if (watchlist == null) return NotFound();

            return View(viewModel);
        }

        // POST: Watchlists/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Watchlist", "WatchlistSecurities")] WatchlistEditViewModel viewModel)
        {
            var user = await GetCurrentUserAsync();
            string token = GetToken();
            var client = _clientFactory.CreateClient();

            // Remove all of the current WatchlistSecurities for the watchlist
            var watchlistSecuritiesToDelete = await _context.WatchlistSecurities
                .Where(ws => ws.WatchlistId == viewModel.Watchlist.Id).ToListAsync();

            foreach (WatchlistSecurity ws in watchlistSecuritiesToDelete)
            {
                _context.Remove(ws);
            }

            var securities = _context.Securities; // Get all the securities from the DB

            // Iterate over the list of WatchlistSecurities entered by the user
            foreach (WatchlistSecurityInput ws in viewModel.WatchlistSecurities)
            {
                if (ws.Security.Ticker != null) // Only check for security if the input was not blank
                {
                    string ticker = ws.Security.Ticker;
                    if (!securities.Any(s => s.Ticker == ticker))
                    {
                        // Security is not in the DB, needs to be retrieved from IEXCloud and saved to the DB
                        var request = new HttpRequestMessage(HttpMethod.Get,
                            $"http://cloud.iexapis.com/stable/stock/{ticker}/company?token={token}");
                        var response = await client.SendAsync(request);
                        if (response.IsSuccessStatusCode)
                        {
                            // Convert the response to an object and save the new security to the DB
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
                        }
                    }
                }
            }

            // Now all the securities should be in the DB. Get a new reference to them and iterate over the list of portfolio securities again
            var updatedSecurities = _context.Securities;

            if (id != viewModel.Watchlist.Id) return NotFound();

            // Save the new watchlist to the database and get a reference to its Id
            viewModel.Watchlist.UserId = user.Id;
            _context.Update(viewModel.Watchlist);
            await _context.SaveChangesAsync();
            int watchlistId = viewModel.Watchlist.Id;

            // Iterate over WatchlistSecurities again and enter them into the database with their properties
            foreach(WatchlistSecurityInput ws in viewModel.WatchlistSecurities)
            {
                if (ws.Security.Ticker != null) // only create new WS if the row was not blank
                {
                    Security matchingSecurity = updatedSecurities.First(s => s.Ticker == ws.Security.Ticker);

                    WatchlistSecurity newWS = new WatchlistSecurity
                    {
                        WatchlistId = watchlistId,
                        SecurityId = matchingSecurity.Id
                    };

                    _context.Add(newWS);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Watchlists/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var watchlist = await _context.Watchlists
                .Include(w => w.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (watchlist == null) return NotFound();

            return View(watchlist);
        }

        // POST: Watchlists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var watchlist = await _context.Watchlists.FindAsync(id);
            _context.Watchlists.Remove(watchlist);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WatchlistExists(int id)
        {
            return _context.Watchlists.Any(e => e.Id == id);
        }
    }
}
