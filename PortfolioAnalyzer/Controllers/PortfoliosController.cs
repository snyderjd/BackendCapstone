﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
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

        [Authorize]
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

            // Asset Allocation data for pie chart
            Dictionary<string, int> assetAllocation = new Dictionary<string, int>();
            foreach(PortfolioSecurity ps in viewModel.Portfolio.PortfolioSecurities)
            {
                if (!assetAllocation.ContainsKey(ps.AssetClass.Name))
                {
                    assetAllocation[ps.AssetClass.Name] = 0;
                }

                assetAllocation[ps.AssetClass.Name] += ps.Weight;
            }
            // Convert dictionary to json and assign to viewModel
            viewModel.AssetAllocationKeys = JsonConvert.SerializeObject(assetAllocation.Keys);
            viewModel.AssetAllocationValues = JsonConvert.SerializeObject(assetAllocation.Values);

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

                decimal begValue = viewModel.PortfolioValues.First().Value;
                decimal endValue = viewModel.PortfolioValues.Last().Value;
                viewModel.Return = ((endValue / begValue) - 1) * 100;

                decimal numYears = dates.Count() / 12;

                viewModel.CAGR = (decimal)(Math.Pow((double)(endValue / begValue), (double)(1 / numYears))) - 1;
                viewModel.CAGR = viewModel.CAGR * 100;
                // Calculate the StdDeviation
                List<decimal> monthlyReturns = viewModel.MonthlyReturns.Values.ToList();
                // Remove the first value since the 1st period has no return
                monthlyReturns.Remove(monthlyReturns[0]); 
                decimal monthlyStdDev = StdDev(monthlyReturns);
                // Get estimated annualized StdDev
                viewModel.StdDeviation = monthlyStdDev * (decimal)Math.Sqrt(12);
                viewModel.StdDeviation = viewModel.StdDeviation * 100;

                // Set dates
                viewModel.StartDate = viewModel.PortfolioValues.First().Key;
                viewModel.EndDate = viewModel.PortfolioValues.Last().Key;

                // Convert portfolio values to JSON and set in the viewModel
                viewModel.ChartData = JsonConvert.SerializeObject(viewModel.PortfolioValues);


                return View(viewModel);
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
                PortfolioSecurities = new List<PortfolioSecurityInput>()
            };

            // Add 10 PortfolioSecurity objects to the viewModel's list
            for (int i = 0; i < 10; i++)
            {
                viewModel.PortfolioSecurities.Add(new PortfolioSecurityInput());
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
            // Make sure weights sum to 100
            if (viewModel.PortfolioSecurities.Select(ps => ps.Weight).Sum() != 100)
            {
                viewModel.AssetClasses = _context.AssetClasses.ToList();
                ViewData["WeightError"] = "The sum of all weights must equal 100";
                return View(viewModel);
            }

            var user = await GetCurrentUserAsync();
            string token = GetToken();
            var client = _clientFactory.CreateClient();

            // Get all the securities from the database
            var securities = _context.Securities;

            // Iterate over the list of PortfolioSecurities entered by the user
            foreach(PortfolioSecurityInput ps in viewModel.PortfolioSecurities)
            {
                if (ps.Security.Ticker != null) // Only check for security if the row was not blank
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
                            var stockResponse = await System.Text.Json.JsonSerializer.DeserializeAsync<IEXSecurity>(json);
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
            }
            // Now all the securities should be in the DB. Get a new reference to them and iterate over the list of portfolio securities again

            var updatedSecurities = _context.Securities;

            // Save the new portfolio to the database and get a reference to its Id
            viewModel.Portfolio.UserId = user.Id;
            _context.Add(viewModel.Portfolio);
            await _context.SaveChangesAsync();
            int portfolioId = viewModel.Portfolio.Id;

            // iterate over PortfolioSecurities again and enter them into the database with their properties
            foreach(PortfolioSecurityInput ps in viewModel.PortfolioSecurities)
            {
                if (ps.Security.Ticker != null) // only create new PS if the row was not blank
                {
                    Security matchingSecurity = updatedSecurities.First(s => s.Ticker == ps.Security.Ticker);

                    PortfolioSecurity newPS = new PortfolioSecurity
                    {
                        PortfolioId = portfolioId,
                        SecurityId = matchingSecurity.Id,
                        Weight = (int)ps.Weight,
                        AssetClassId = (int)ps.AssetClassId
                    };

                    _context.Add(newPS);
                }
                
            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Portfolios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // Get the portfolio and include navigation properties
            var portfolio = await _context.Portfolios
                .Include(p => p.PortfolioSecurities)
                    .ThenInclude(p => p.Security)
                .Include(p => p.PortfolioSecurities)
                    .ThenInclude(p => p.AssetClass)
                .FirstOrDefaultAsync(p => p.Id == id);

            var viewModel = new PortfolioEditViewModel()
            {
                Portfolio = portfolio,
                AssetClasses = _context.AssetClasses.ToList(),
                PortfolioSecurities = new List<PortfolioSecurityInput>()
            };

            // Assign the current PortfolioSecurities to the viewModel's list of PortfolioSecurities
            foreach(PortfolioSecurity ps in viewModel.Portfolio.PortfolioSecurities)
            {
                PortfolioSecurityInput currentPS = new PortfolioSecurityInput()
                {
                    Id = ps.Id,
                    PortfolioId = ps.PortfolioId,
                    SecurityId = ps.SecurityId,
                    Security = new SecurityInput()
                    {
                        Ticker = ps.Security.Ticker
                    },
                    Weight = ps.Weight,
                    AssetClassId = ps.AssetClassId
                };
                viewModel.PortfolioSecurities.Add(currentPS);
            }

            // Add 10 more PortfolioSecurity objects to the viewModel's list
            for (int i = 0; i < 10; i++)
            {
                viewModel.PortfolioSecurities.Add(new PortfolioSecurityInput());
            }

            if (portfolio == null) return NotFound();

            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", portfolio.UserId);
            return View(viewModel);
        }

        // POST: Portfolios/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Portfolio", "PortfolioSecurities")] PortfolioEditViewModel viewModel)
        {
            // Make sure all weights add up to 100
            if (viewModel.PortfolioSecurities.Select(ps => ps.Weight).Sum() != 100)
            {
                viewModel.AssetClasses = _context.AssetClasses.ToList();
                ViewData["WeightError"] = "The sum of all weights must equal 100";
                return View(viewModel);
            }

            var user = await GetCurrentUserAsync();
            string token = GetToken();
            var client = _clientFactory.CreateClient();

            // Remove all of the current PortfolioSecurities for the portfolio
            var portfolioSecuritiesToDelete = await _context.PortfolioSecurities
                .Where(ps => ps.PortfolioId == viewModel.Portfolio.Id).ToListAsync();

            foreach(PortfolioSecurity ps in portfolioSecuritiesToDelete)
            {
                _context.Remove(ps);
            }


            // Get all the securities from the database
            var securities = _context.Securities;

            // Iterate over the list of PortfolioSecurities entered by the user
            foreach (PortfolioSecurityInput ps in viewModel.PortfolioSecurities)
            {
                if (ps.Security.Ticker != null) // Only check for security if the row was not blank
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
                            var stockResponse = await System.Text.Json.JsonSerializer.DeserializeAsync<IEXSecurity>(json);
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
                
            }

            // Now all the securities should be in the DB. Get a new reference to them and iterate over the list of portfolio securities again
            var updatedSecurities = _context.Securities;

            if (id != viewModel.Portfolio.Id) return NotFound();

            // Save the new portfolio to the database and get a reference to its Id
            viewModel.Portfolio.UserId = user.Id;
            _context.Update(viewModel.Portfolio);
            await _context.SaveChangesAsync();
            int portfolioId = viewModel.Portfolio.Id;

            // iterate over PortfolioSecurities again and enter them into the database with their properties
            foreach (PortfolioSecurityInput ps in viewModel.PortfolioSecurities)
            {
                if (ps.Security.Ticker != null) // only create new PS if the row was not blank
                {
                    Security matchingSecurity = updatedSecurities.First(s => s.Ticker == ps.Security.Ticker);

                    PortfolioSecurity newPS = new PortfolioSecurity
                    {
                        PortfolioId = portfolioId,
                        SecurityId = matchingSecurity.Id,
                        Weight = (int)ps.Weight,
                        AssetClassId = (int)ps.AssetClassId
                    };

                    _context.Add(newPS);
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Portfolios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var portfolio = await _context.Portfolios
                .Include(p => p.User)
                .Include(p => p.PortfolioSecurities)
                    .ThenInclude(ps => ps.Security)
                .Include(p => p.PortfolioSecurities)
                    .ThenInclude(ps => ps.AssetClass)
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

            // Get all the portfolio's PortfolioSecurities
            var portfolioSecurities = await _context.PortfolioSecurities
                .Where(ps => ps.PortfolioId == id).ToListAsync();

            portfolioSecurities.ForEach(ps => _context.PortfolioSecurities.Remove(ps));

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
                    List<IEXPrice> IEXPrices = await System.Text.Json.JsonSerializer.DeserializeAsync<List<IEXPrice>>(json);
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


        // Take in a list of numbers and return the standard deviation
        private decimal StdDev(List<decimal> nums)
        {
            double stdDeviation = 0;
            // Get the sum of the squared differences
            double average = (double)nums.Average();

            List<double> squaredDiffs = nums.Select(n => Math.Pow(((double)n - average), 2)).ToList();
            double sumSquaredDiffs = squaredDiffs.Sum();
            stdDeviation = Math.Sqrt(sumSquaredDiffs / (nums.Count() - 1));

            return (decimal)stdDeviation;
        }
        //private double CalculateStdDev(IEnumerable<double> values)
        //{
        //    double ret = 0;
        //    if (values.Count() > 0)
        //    {
        //        //Compute the Average      
        //        double avg = values.Average();
        //        //Perform the Sum of (value-avg)_2_2      
        //        double sum = values.Sum(d => Math.Pow(d - avg, 2));
        //        //Put it all together      
        //        ret _= Math.Sqrt((sum) / (values.Count() - 1));
        //    }
        //    return ret;
        //}

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
