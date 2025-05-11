using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ETMF.Data;
using ETMF.Models;

namespace ETMF.Controllers
{
    public class ZonesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ZonesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Zones
        public async Task<IActionResult> Index()
        {
            return View(await _context.Zones.ToListAsync());
        }

        // GET: Zones/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var zone = await _context.Zones
                .Include(z => z.Sections)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (zone == null)
            {
                return NotFound();
            }

            return View(zone);
        }

        // GET: Zones/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Zones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Number,Name,Description")] Zone zone)
        {
            if (ModelState.IsValid)
            {
                zone.CreatedAt = DateTime.Now;
                _context.Add(zone);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(zone);
        }

        // GET: Zones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var zone = await _context.Zones.FindAsync(id);
            if (zone == null)
            {
                return NotFound();
            }
            return View(zone);
        }

        // POST: Zones/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Number,Name,Description")] Zone zone)
        {
            if (id != zone.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 获取原始数据，保留创建时间等信息
                    var originalZone = await _context.Zones.AsNoTracking()
                        .FirstOrDefaultAsync(z => z.Id == id);
                        
                    if (originalZone != null)
                    {
                        zone.CreatedAt = originalZone.CreatedAt;
                        zone.UpdatedAt = DateTime.Now;
                        
                        _context.Update(zone);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ZoneExists(zone.Id))
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
            return View(zone);
        }

        // GET: Zones/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var zone = await _context.Zones
                .Include(z => z.Sections)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (zone == null)
            {
                return NotFound();
            }

            return View(zone);
        }

        // POST: Zones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var zone = await _context.Zones
                .Include(z => z.Sections)
                .ThenInclude(s => s.Artifacts)
                .ThenInclude(a => a.Documents)
                .FirstOrDefaultAsync(z => z.Id == id);
                
            if (zone == null)
            {
                return NotFound();
            }
            
            // 级联删除
            _context.Zones.Remove(zone);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        private bool ZoneExists(int id)
        {
            return _context.Zones.Any(e => e.Id == id);
        }
    }
} 