using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Piscesco.Data;
using Piscesco.Models;

namespace Piscesco.Views
{
    public class FeaturedProductsController : Controller
    {
        private readonly PiscescoModelContext _context;

        public FeaturedProductsController(PiscescoModelContext context)
        {
            _context = context;
        }

        // GET: FeaturedProducts
        public async Task<IActionResult> Index()
        {
            return View(await _context.FeaturedProduct.ToListAsync());
        }

        // GET: FeaturedProducts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var featuredProduct = await _context.FeaturedProduct
                .FirstOrDefaultAsync(m => m.FeaturedProductID == id);
            if (featuredProduct == null)
            {
                return NotFound();
            }

            return View(featuredProduct);
        }

        // GET: FeaturedProducts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: FeaturedProducts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FeaturedProductID,StallID,ProductID")] FeaturedProduct featuredProduct)
        {
            if (ModelState.IsValid)
            {
                _context.Add(featuredProduct);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(featuredProduct);
        }

        // GET: FeaturedProducts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var featuredProduct = await _context.FeaturedProduct.FindAsync(id);
            if (featuredProduct == null)
            {
                return NotFound();
            }
            return View(featuredProduct);
        }

        // POST: FeaturedProducts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FeaturedProductID,StallID,ProductID")] FeaturedProduct featuredProduct)
        {
            if (id != featuredProduct.FeaturedProductID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(featuredProduct);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FeaturedProductExists(featuredProduct.FeaturedProductID))
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
            return View(featuredProduct);
        }

        // GET: FeaturedProducts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var featuredProduct = await _context.FeaturedProduct
                .FirstOrDefaultAsync(m => m.FeaturedProductID == id);
            if (featuredProduct == null)
            {
                return NotFound();
            }

            return View(featuredProduct);
        }

        // POST: FeaturedProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var featuredProduct = await _context.FeaturedProduct.FindAsync(id);
            _context.FeaturedProduct.Remove(featuredProduct);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FeaturedProductExists(int id)
        {
            return _context.FeaturedProduct.Any(e => e.FeaturedProductID == id);
        }
    }
}
