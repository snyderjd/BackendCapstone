using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PortfolioAnalyzer.Data;
using PortfolioAnalyzer.Models;
using PortfolioAnalyzer.Models.ViewModels;

namespace PortfolioAnalyzer.Controllers
{
    public class PortfoliosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PortfoliosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        // GET: Portfolios
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();
            var portfolios = await _context.Portfolios.Where(p => p.UserId == user.Id)
                .Include(p => p.User).ToListAsync();
            return View(portfolios);
        }

        // GET: Portfolios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var portfolio = await _context.Portfolios
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (portfolio == null) return NotFound();

            return View(portfolio);
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

            ModelState.Remove("Portfolio.UserId");

            if (ModelState.IsValid)
            {
                viewModel.Portfolio.UserId = user.Id;
                _context.Add(viewModel.Portfolio);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
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
    }
}
