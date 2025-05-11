using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ETMF.Data;
using ETMF.Models;

namespace ETMF.Controllers
{
    public class SectionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SectionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Sections
        public async Task<IActionResult> Index()
        {
            var sections = await _context.Sections
                .Include(s => s.Zone)
                .Include(s => s.Artifacts)
                .OrderBy(s => s.Zone != null ? s.Zone.Number : "")
                .ThenBy(s => s.Number)
                .ToListAsync();
                
            return View(sections);
        }

        // GET: Sections/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var section = await _context.Sections
                .Include(s => s.Zone)
                .Include(s => s.Artifacts)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (section == null)
            {
                return NotFound();
            }

            return View(section);
        }

        // GET: Sections/Create
        public IActionResult Create(int? zoneId)
        {
            if (zoneId.HasValue)
            {
                var zone = _context.Zones.Find(zoneId);
                if (zone != null)
                {
                    ViewData["ZoneId"] = zone.Id;
                    ViewData["ZoneName"] = $"{zone.Number} - {zone.Name}";
                }
            }
            else
            {
                ViewData["ZoneId"] = new SelectList(_context.Zones.OrderBy(z => z.Number), "Id", "Name");
            }
            
            return View();
        }

        // POST: Sections/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Number,Name,Description,ZoneId")] Section section)
        {
            if (ModelState.IsValid)
            {
                section.CreatedAt = DateTime.Now;
                _context.Add(section);
                await _context.SaveChangesAsync();
                
                if (Request.Form["returnToZone"].ToString() == "true" && section.ZoneId > 0)
                {
                    return RedirectToAction("Details", "Zones", new { id = section.ZoneId });
                }
                
                return RedirectToAction(nameof(Index));
            }
            
            if (section.ZoneId > 0)
            {
                var zone = await _context.Zones.FindAsync(section.ZoneId);
                if (zone != null)
                {
                    ViewData["ZoneId"] = zone.Id;
                    ViewData["ZoneName"] = $"{zone.Number} - {zone.Name}";
                }
            }
            else
            {
                ViewData["ZoneId"] = new SelectList(_context.Zones.OrderBy(z => z.Number), "Id", "Name", section.ZoneId);
            }
            
            return View(section);
        }

        // GET: Sections/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var section = await _context.Sections.FindAsync(id);
            if (section == null)
            {
                return NotFound();
            }
            
            ViewData["ZoneId"] = new SelectList(_context.Zones.OrderBy(z => z.Number), "Id", "Name", section.ZoneId);
            return View(section);
        }

        // POST: Sections/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Number,Name,Description,ZoneId")] Section section)
        {
            if (id != section.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var originalSection = await _context.Sections.AsNoTracking()
                        .FirstOrDefaultAsync(s => s.Id == id);
                        
                    if (originalSection != null)
                    {
                        section.CreatedAt = originalSection.CreatedAt;
                        section.UpdatedAt = DateTime.Now;
                        
                        _context.Update(section);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SectionExists(section.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = section.Id });
            }
            
            ViewData["ZoneId"] = new SelectList(_context.Zones.OrderBy(z => z.Number), "Id", "Name", section.ZoneId);
            return View(section);
        }

        // GET: Sections/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var section = await _context.Sections
                .Include(s => s.Zone)
                .Include(s => s.Artifacts)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (section == null)
            {
                return NotFound();
            }

            return View(section);
        }

        // POST: Sections/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var section = await _context.Sections
                .Include(s => s.Artifacts)
                .ThenInclude(a => a != null ? a.Documents : null)
                .FirstOrDefaultAsync(s => s.Id == id);
                
            if (section == null)
            {
                return NotFound();
            }
            
            if (section.Artifacts != null && section.Artifacts.Any(a => a.Documents != null && a.Documents.Any()))
            {
                ModelState.AddModelError(string.Empty, "无法删除该部分，因为它包含的文档类型下存在文档。请先删除相关文档。");
                
                // 重新加载 Zone 以显示详细信息
                await _context.Entry(section)
                    .Reference(s => s.Zone)
                    .LoadAsync();
                    
                return View(section);
            }
            
            // 删除关联的文档类型
            if (section.Artifacts != null && section.Artifacts.Any())
            {
                _context.Artifacts.RemoveRange(section.Artifacts);
            }
            
            _context.Sections.Remove(section);
            await _context.SaveChangesAsync();
            
            // 如果是从区域详情页过来的，返回区域详情
            if (Request.Headers["Referer"].ToString().Contains("/Zones/Details/"))
            {
                return RedirectToAction("Details", "Zones", new { id = section.ZoneId });
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool SectionExists(int id)
        {
            return _context.Sections.Any(e => e.Id == id);
        }
    }
} 