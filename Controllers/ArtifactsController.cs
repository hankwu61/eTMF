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
    public class ArtifactsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ArtifactsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Artifacts
        public async Task<IActionResult> Index()
        {
            var artifacts = await _context.Artifacts
                .Include(a => a.Section)
                    .ThenInclude(s => s != null ? s.Zone : null)
                .Include(a => a.Documents)
                .OrderBy(a => a.Section != null && a.Section.Zone != null ? a.Section.Zone.Number : string.Empty)
                .ThenBy(a => a.Section != null ? a.Section.Number : string.Empty)
                .ThenBy(a => a.Number)
                .ToListAsync();
                
            return View(artifacts);
        }

        // GET: Artifacts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var artifact = await _context.Artifacts
                .Include(a => a.Section)
                    .ThenInclude(s => s != null ? s.Zone : null)
                .Include(a => a.Documents)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (artifact == null)
            {
                return NotFound();
            }

            return View(artifact);
        }

        // GET: Artifacts/Create
        public IActionResult Create(int? sectionId)
        {
            if (sectionId.HasValue)
            {
                var section = _context.Sections
                    .Include(s => s.Zone)
                    .FirstOrDefault(s => s.Id == sectionId);
                    
                if (section != null)
                {
                    ViewData["SectionId"] = section.Id;
                    ViewData["SectionName"] = section.Zone != null
                        ? $"{section.Zone.Number}.{section.Number} - {section.Name}"
                        : $"{section.Number} - {section.Name}";
                }
            }
            else
            {
                var sections = _context.Sections
                    .Include(s => s.Zone)
                    .OrderBy(s => s.Zone != null ? s.Zone.Number : string.Empty)
                    .ThenBy(s => s.Number)
                    .ToList();
                    
                var selectList = sections
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.Zone != null
                            ? $"{s.Zone.Number}.{s.Number} - {s.Name}"
                            : $"{s.Number} - {s.Name}"
                    })
                    .ToList();
                    
                ViewData["SectionId"] = new SelectList(selectList, "Value", "Text");
            }
            
            return View();
        }

        // POST: Artifacts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Number,Name,Description,SectionId,IsRequired,IsEssential")] Artifact artifact)
        {
            if (ModelState.IsValid)
            {
                artifact.CreatedAt = DateTime.Now;
                _context.Add(artifact);
                await _context.SaveChangesAsync();
                
                if (Request.Form["returnToSection"].ToString() == "true" && artifact.SectionId > 0)
                {
                    return RedirectToAction("Details", "Sections", new { id = artifact.SectionId });
                }
                
                return RedirectToAction(nameof(Index));
            }
            
            if (artifact.SectionId > 0)
            {
                var section = await _context.Sections
                    .Include(s => s.Zone)
                    .FirstOrDefaultAsync(s => s.Id == artifact.SectionId);
                    
                if (section != null)
                {
                    ViewData["SectionId"] = section.Id;
                    ViewData["SectionName"] = section.Zone != null
                        ? $"{section.Zone.Number}.{section.Number} - {section.Name}"
                        : $"{section.Number} - {section.Name}";
                }
            }
            else
            {
                var sections = await _context.Sections
                    .Include(s => s.Zone)
                    .OrderBy(s => s.Zone != null ? s.Zone.Number : string.Empty)
                    .ThenBy(s => s.Number)
                    .ToListAsync();
                    
                var selectList = sections
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.Zone != null
                            ? $"{s.Zone.Number}.{s.Number} - {s.Name}"
                            : $"{s.Number} - {s.Name}"
                    })
                    .ToList();
                    
                ViewData["SectionId"] = new SelectList(selectList, "Value", "Text", artifact.SectionId);
            }
            
            return View(artifact);
        }

        // GET: Artifacts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var artifact = await _context.Artifacts.FindAsync(id);
            if (artifact == null)
            {
                return NotFound();
            }
            
            var sections = await _context.Sections
                .Include(s => s.Zone)
                .OrderBy(s => s.Zone != null ? s.Zone.Number : string.Empty)
                .ThenBy(s => s.Number)
                .ToListAsync();
                
            var selectList = sections
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Zone != null
                        ? $"{s.Zone.Number}.{s.Number} - {s.Name}"
                        : $"{s.Number} - {s.Name}"
                })
                .ToList();
                
            ViewData["SectionId"] = new SelectList(selectList, "Value", "Text", artifact.SectionId);
            return View(artifact);
        }

        // POST: Artifacts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Number,Name,Description,SectionId,IsRequired,IsEssential")] Artifact artifact)
        {
            if (id != artifact.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var originalArtifact = await _context.Artifacts
                        .AsNoTracking()
                        .FirstOrDefaultAsync(a => a.Id == id);
                        
                    if (originalArtifact != null)
                    {
                        artifact.CreatedAt = originalArtifact.CreatedAt;
                        artifact.UpdatedAt = DateTime.Now;
                        
                        _context.Update(artifact);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArtifactExists(artifact.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = artifact.Id });
            }
            
            var sections = await _context.Sections
                .Include(s => s.Zone)
                .OrderBy(s => s.Zone != null ? s.Zone.Number : string.Empty)
                .ThenBy(s => s.Number)
                .ToListAsync();
                
            var selectList = sections
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Zone != null
                        ? $"{s.Zone.Number}.{s.Number} - {s.Name}"
                        : $"{s.Number} - {s.Name}"
                })
                .ToList();
                
            ViewData["SectionId"] = new SelectList(selectList, "Value", "Text", artifact.SectionId);
            return View(artifact);
        }

        // GET: Artifacts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var artifact = await _context.Artifacts
                .Include(a => a.Section)
                    .ThenInclude(s => s != null ? s.Zone : null)
                .Include(a => a.Documents)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (artifact == null)
            {
                return NotFound();
            }

            return View(artifact);
        }

        // POST: Artifacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var artifact = await _context.Artifacts
                .Include(a => a.Documents)
                .FirstOrDefaultAsync(a => a.Id == id);
                
            if (artifact == null)
            {
                return NotFound();
            }
            
            if (artifact.Documents != null && artifact.Documents.Any())
            {
                ModelState.AddModelError(string.Empty, "无法删除该文档类型，因为它下面存在文档。请先删除相关文档。");
                
                // 重新加载 Section 以显示详细信息
                await _context.Entry(artifact)
                    .Reference(a => a.Section)
                    .LoadAsync();
                    
                if (artifact.Section != null)
                {
                    await _context.Entry(artifact.Section)
                        .Reference(s => s.Zone)
                        .LoadAsync();
                }
                
                return View(artifact);
            }
            
            int? sectionId = artifact.SectionId;
            
            _context.Artifacts.Remove(artifact);
            await _context.SaveChangesAsync();
            
            // 如果是从部分详情页过来的，返回部分详情
            if (Request.Headers["Referer"].ToString().Contains("/Sections/Details/"))
            {
                return RedirectToAction("Details", "Sections", new { id = sectionId });
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool ArtifactExists(int id)
        {
            return _context.Artifacts.Any(e => e.Id == id);
        }
    }
} 