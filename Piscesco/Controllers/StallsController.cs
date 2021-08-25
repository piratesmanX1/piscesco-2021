using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Piscesco.Areas.Identity.Data;
using Piscesco.Controllers;
using Piscesco.Data;
using Piscesco.Models;

namespace Piscesco.Views.Stalls
{
    public class StallsController : Controller
    {
        private readonly PiscescoModelContext _context;
        private readonly UserManager<PiscescoUser> _userManager;



        public StallsController(PiscescoModelContext context, UserManager<PiscescoUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Stalls
        public async Task<IActionResult> Index()
        {
            var loginSession = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            var stalls = from s in _context.Stall select s;
            stalls = stalls.Where(item => item.OwnerID.Equals(loginSession.Id));
            
            return View(await stalls.ToListAsync());
        }

         // GET: Stalls/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stall = await _context.Stall
                .FirstOrDefaultAsync(m => m.StallID == id);
            if (stall == null)
            {
                return NotFound();
            }

            return View(stall);
        }

        // GET: Stalls/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Stalls/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StallID,OwnerID,StallName,Description,StallImage")] Stall stall, IFormFile files)
        {
            if (ModelState.IsValid)
            {
                //Get current user
                var loginSession = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
                stall.OwnerID = loginSession.Id;

                //Image upload
                Guid imageUUID = Guid.NewGuid(); //Unique UUID
                string imageUUIDString = imageUUID.ToString();
                stall.StallImage = imageUUIDString;
                BlobsController bc = new BlobsController();
                bc.UploadImage(files, imageUUIDString);


                _context.Add(stall);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(stall);
        }

        // GET: Stalls/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stall = await _context.Stall.FindAsync(id);
            if (stall == null)
            {
                return NotFound();
            }
            return View(stall);
        }

        // POST: Stalls/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StallID,OwnerID,StallName,Description,StallImage")] Stall stall, IFormFile files)
        {
            if (id != stall.StallID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Debug.WriteLine(files == null);
                    if(files != null)
                    {
                        Guid imageUUID = Guid.NewGuid();
                        string imageUUIDString = imageUUID.ToString();
                        stall.StallImage = imageUUIDString;
                        BlobsController bc = new BlobsController();
                        bc.UploadImage(files, imageUUIDString);
                        
                    }
                    _context.Update(stall);
                    await _context.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StallExists(stall.StallID))
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
            return View(stall);
        }

        // GET: Stalls/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stall = await _context.Stall
                .FirstOrDefaultAsync(m => m.StallID == id);
            if (stall == null)
            {
                return NotFound();
            }

            return View(stall);
        }

        // POST: Stalls/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var stall = await _context.Stall.FindAsync(id);
            _context.Stall.Remove(stall);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StallExists(int id)
        {
            return _context.Stall.Any(e => e.StallID == id);
        }
    }
}
