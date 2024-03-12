using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Blogg.Data;
using Blogg.Models;
using Microsoft.AspNetCore.Authorization;

namespace Blogg.Controllers
{
    [Authorize]
    public class BloggController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BloggController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Blogg
        public async Task<IActionResult> Index()
        {
            // Kontrollera if_context.Bloggs.is.null
            if(_context.Bloggs == null) {
                return NotFound();
            }

            return View(await _context.Bloggs.ToListAsync());
        }

        // GET: Blogg/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Kontrollera if_context.Bloggs.is.null
            if(_context.Bloggs == null) {
                return NotFound();
            }

            var bloggModel = await _context.Bloggs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bloggModel == null)
            {
                return NotFound();
            }

            return View(bloggModel);
        }

        // GET: Blogg/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Blogg/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Content,ImageName,PublishDate")] BloggModel bloggModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bloggModel);

                // Lägga till skapre automatiskt
                bloggModel.CreateBy = User.Identity?.Name ?? "Unknow";

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(bloggModel);
        }

        // GET: Blogg/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Kontrollera if_context.Bloggs.is.null
            if(_context.Bloggs == null) {
                return NotFound();
            }

            var bloggModel = await _context.Bloggs.FindAsync(id);
            if (bloggModel == null)
            {
                return NotFound();
            }
            return View(bloggModel);
        }

        // POST: Blogg/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,ImageName,PublishDate")] BloggModel bloggModel)
        {
            if (id != bloggModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bloggModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BloggModelExists(bloggModel.Id))
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
            return View(bloggModel);
        }

        // GET: Blogg/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Kontrollera if_context.Bloggs.is.null
            if(_context.Bloggs == null) {
                return NotFound();
            }

            var bloggModel = await _context.Bloggs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bloggModel == null)
            {
                return NotFound();
            }

            return View(bloggModel);
        }

        // POST: Blogg/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Kontrollera if_context.Bloggs.is.null
            if(_context.Bloggs == null) {
                return NotFound();
            }

            var bloggModel = await _context.Bloggs.FindAsync(id);
            if (bloggModel != null)
            {
                _context.Bloggs.Remove(bloggModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BloggModelExists(int id)
        {
            // Kontrollera if_context.Bloggs.is.null
            if(_context.Bloggs == null) {
                return false;
            }

            return _context.Bloggs.Any(e => e.Id == id);
        }
    }
}
