using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
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
    public class PortfoliosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _clientFactory;

        public PortfoliosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IConfiguration config, IHttpClientFactory clientFactory)
        {
            _context = context;
            _userManager = userManager;
            _config = config;
            _clientFactory = clientFactory;
        }

        
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);
        private string GetToken() => _config.GetValue<string>("Tokens:IEXCloudSK");

        // GET: Portfolios
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();
            var portfolios = await _context.Portfolios.Where(p => p.UserId == user.Id)
                .Include(p => p.User).ToListAsync();
            return View(portfolios);
        }

        // GET: Portfolios/Details/5
        public async Task<IActionResult> Details(int? id, string timePeriod)
        {
            var viewModel = new PortfolioDetailsViewModel();

            viewModel.TimePeriod = timePeriod;

            if (id == null) return NotFound();

            viewModel.Portfolio = await _context.Portfolios
                .Include(p => p.User)
                .Include(p => p.PortfolioSecurities)
                    .ThenInclude(p => p.Security)
                .Include(p => p.PortfolioSecurities)
                    .ThenInclude(p => p.AssetClass)
                .FirstOrDefaultAsync(p => p.Id == id);

            // Get the prices for all the securities in the portfolio
            if (timePeriod != null)
            {
                viewModel.Portfolio.PortfolioSecurities = await GetPrices(viewModel);

                List<DateTime> dates = viewModel.Portfolio.PortfolioSecurities.FirstOrDefault().Prices
                    .Select(p => p.Date).ToList();
                
                // Iterate over all the dates returned for prices and make entries in Dictionaries
                foreach(DateTime date in dates)
                {
                    viewModel.PortfolioValues[date] = 100_000;
                    viewModel.MonthlyReturns[date] = 0;
                    viewModel.CumulativeReturns[date] = 0;
                }

                // Iterate over all the PortfolioSecurities, calculate the weighted monthly return and cumulative return for each security and add it to the dictionary
                decimal monthlyReturn = 0;
                decimal cumulativeReturn = 0;
                foreach(PortfolioSecurity ps in viewModel.Portfolio.PortfolioSecurities)
                {
                    for (int i = 0; i < ps.Prices.Count(); i++)
                    {
                        if (i != 0)
                        {
                            monthlyReturn = (ps.Prices[i].AdjClose / ps.Prices[i - 1].AdjClose) - 1;
                            viewModel.MonthlyReturns[ps.Prices[i].Date] += monthlyReturn * ps.Weight / 100;

                            cumulativeReturn = (ps.Prices[i].AdjClose / ps.Prices[0].AdjClose) - 1;
                            viewModel.CumulativeReturns[ps.Prices[i].Date] += cumulativeReturn * ps.Weight / 100;
                        }
                    }


                }

                // Iterate over the viewModel's CumulativeReturns dictionary and update the PortfolioValues dictionary with the appropriate value
                //DateTime firstDate = viewModel.PortfolioValues.Keys.First();
                //viewModel.PortfolioValues[firstDate] = 100_000;
                
                foreach(KeyValuePair<DateTime, decimal> kvp in viewModel.CumulativeReturns)
                {
                    viewModel.PortfolioValues[kvp.Key] = 100_000 * (1 + kvp.Value);
                }

            }

            if (viewModel.Portfolio == null) return NotFound();

            return View(viewModel);
        }

        // GET: Portfolios/Create
        public IActionResult Create()
        {
            // Create the viewModel
            var viewModel = new PortfolioCreateViewModel()
            {
                Portfolio = new Portfolio(),
                AssetClasses = _context.AssetClasses.ToList(),
                PortfolioSecurities = new List<PortfolioSecurity>()
            };

            // Add 10 PortfolioSecurity objects to the viewModel's list
            for (int i = 0; i < 10; i++)
            {
                viewModel.PortfolioSecurities.Add(new PortfolioSecurity());
            }

            return View(viewModel);
        }

        // POST: Portfolios/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        // Original Bind: [Bind("Id,Name,Description,UserId,DateCreated,Notes")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Portfolio", "PortfolioSecurities")] PortfolioCreateViewModel viewModel)
        {
            var user = await GetCurrentUserAsync();
            string token = GetToken();
            var client = _clientFactory.CreateClient();

            // Get all the securities from the database
            var securities = _context.Securities;

            // Iterate over the list of PortfolioSecurities entered by the user
            foreach(PortfolioSecurity ps in viewModel.PortfolioSecurities)
            {
                string ticker = ps.Security.Ticker;
                if (!securities.Any(s => s.Ticker == ticker))
                {
                    // Security is not in the DB and needs to be retrieved from IEX Cloud and saved to the DB
                    var request = new HttpRequestMessage(HttpMethod.Get,
                        $"https://cloud.iexapis.com/stable/stock/{ticker}/company?token={token}");
                    var response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        // Convert the response to an object and save the new security to the database
                        var json = await response.Content.ReadAsStreamAsync();
                        var stockResponse = await JsonSerializer.DeserializeAsync<IEXSecurity>(json);
                        //SaveSecurity(stockResponse);
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
            // Now all the securities should be in the DB. Get a new reference to them and iterate over the list of portfolio securities again

            var updatedSecurities = _context.Securities;

            // Save the new portfolio to the database and get a reference to its Id
            viewModel.Portfolio.UserId = user.Id;
            _context.Add(viewModel.Portfolio);
            await _context.SaveChangesAsync();
            int portfolioId = viewModel.Portfolio.Id;

            // iterate over PortfolioSecurities again and enter them into the database with their properties
            foreach(PortfolioSecurity ps in viewModel.PortfolioSecurities)
            {
                Security matchingSecurity = updatedSecurities.First(s => s.Ticker == ps.Security.Ticker);

                PortfolioSecurity newPS = new PortfolioSecurity
                {
                    PortfolioId = portfolioId,
                    SecurityId = matchingSecurity.Id,
                    Weight = ps.Weight,
                    AssetClassId = ps.AssetClassId
                };

                _context.Add(newPS);
            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Portfolios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var portfolio = await _context.Portfolios.FindAsync(id);

            if (portfolio == null) return NotFound();

            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", portfolio.UserId);
            return View(portfolio);
        }

        // POST: Portfolios/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,UserId,DateCreated,Notes")] Portfolio portfolio)
        {
            if (id != portfolio.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(portfolio);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PortfolioExists(portfolio.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", portfolio.UserId);
            return View(portfolio);
        }

        // GET: Portfolios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var portfolio = await _context.Portfolios
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (portfolio == null) return NotFound();

            return View(portfolio);
        }

        // POST: Portfolios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var portfolio = await _context.Portfolios.FindAsync(id);
            _context.Portfolios.Remove(portfolio);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PortfolioExists(int id)
        {
            return _context.Portfolios.Any(e => e.Id == id);
        }

        // Gets the prices for all of the securities in a portfolio from IEX Cloud
        private async Task<ICollection<PortfolioSecurity>> GetPrices(PortfolioDetailsViewModel viewModel)
        {
            string token = GetToken();
            var client = _clientFactory.CreateClient();
            var timePeriod = viewModel.TimePeriod;

            foreach(PortfolioSecurity ps in viewModel.Portfolio.PortfolioSecurities)
            {
                var request = new HttpRequestMessage(HttpMethod.Get,
                    $"https://cloud.iexapis.com/stable/stock/{ps.Security.Ticker}/chart/{timePeriod}/?chartCloseOnly=true&chartInterval=21&token={token}");
                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    // Convert the response into price objects and save as a list to the PS's List<Price>
                    var json = await response.Content.ReadAsStreamAsync();
                    List<IEXPrice> IEXPrices = await JsonSerializer.DeserializeAsync<List<IEXPrice>>(json);
                    foreach(IEXPrice p in IEXPrices)
                    {
                        ps.Prices.Add(new Price
                        {
                            Date = p.Date,
                            AdjClose = p.Close
                        });
                    }
                }
            }

            return viewModel.Portfolio.PortfolioSecurities;
        }

        //foreach(PortfolioSecurity ps in viewModel.PortfolioSecurities)
        //    {
        //        string ticker = ps.Security.Ticker;
        //        if (!securities.Any(s => s.Ticker == ticker))
        //        {
        //            // Security is not in the DB and needs to be retrieved from IEX Cloud and saved to the DB
        //            var request = new HttpRequestMessage(HttpMethod.Get,
        //                $"https://cloud.iexapis.com/stable/stock/{ticker}/company?token={token}");
        //            var response = await client.SendAsync(request);

        //            if (response.IsSuccessStatusCode)
        //            {
        //                // Convert the response to an object and save the new security to the database
        //                var json = await response.Content.ReadAsStreamAsync();
        //                var stockResponse = await JsonSerializer.DeserializeAsync<IEXSecurity>(json);
        //                //SaveSecurity(stockResponse);
        //                Security newSecurity = new Security
        //                {
        //                    Name = stockResponse.CompanyName,
        //                    Ticker = stockResponse.Ticker,
        //                    Description = stockResponse.Description
        //                };

        //                _context.Securities.Add(newSecurity);
        //                await _context.SaveChangesAsync();
        //            }
        //        }
        //    }

        //private async void SaveSecurity(IEXSecurity security)
        //{
        //    Security newSecurity = new Security
        //    {
        //        Name = security.CompanyName,
        //        Ticker = security.Ticker,
        //        Description = security.Description
        //    };

        //    _context.Securities.Add(newSecurity);
        //    await _context.SaveChangesAsync();
        //}
    }
}
