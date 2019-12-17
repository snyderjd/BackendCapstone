using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PortfolioAnalyzer.Data;
using PortfolioAnalyzer.Models;

namespace PortfolioAnalyzer.Controllers
{
    public class SecuritiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SecuritiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Securities
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
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


            
            //return View(await PaginatedList<Student>.CreateAsync(students.AsNoTracking(), pageNumber ?? 1, pageSize));
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
