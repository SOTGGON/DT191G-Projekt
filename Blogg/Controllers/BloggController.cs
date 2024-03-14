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
    public class BloggController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly string wwwRootPath;

        public BloggController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
            wwwRootPath = hostEnvironment.WebRootPath;
        }

        // GET: Blogg
        public async Task<IActionResult> Index()
        {
            // Kontrollera if_context.Bloggs.is.null
            if (_context.Bloggs == null)
            {
                return NotFound();
            }

            return View(await _context.Bloggs.ToListAsync());
        }

        [Route("Blogg/Search")]
        public async Task<IActionResult> Index(string? searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var bloggs = from b in _context.Bloggs
                        select b;

            if (!String.IsNullOrEmpty(searchString))
            {
                string searchStringLower = searchString.ToLower(); // Konvertera söksträng till gemener

                bloggs = bloggs.Where(b =>
                    b.Title.ToLower().Contains(searchStringLower) ||  // Konvertera boknamn till gemener för jämförelse
                    b.CreateBy.ToLower().Contains(searchStringLower)   // Konvertera boktyp till gemener för jämförelse
                );
            }

            return View(await bloggs.ToListAsync());
        }

        // GET: Blogg/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Kontrollera if_context.Bloggs.is.null
            if (_context.Bloggs == null)
            {
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
        public async Task<IActionResult> Create([Bind("Id,Title,Content,ImageFile")] BloggModel bloggModel)
        {
            if (ModelState.IsValid)
            {
                // Kontrollera bilder
                if (bloggModel.ImageFile != null)
                {
                    // Generera unipue filnamn
                    string fileName = Path.GetFileNameWithoutExtension(bloggModel.ImageFile.FileName);
                    string extension = Path.GetExtension(bloggModel.ImageFile.FileName);

                    bloggModel.ImageName = fileName = fileName.Replace(" ", String.Empty) + DateTime.Now.ToString("yymmssfff") + extension;

                    string path = Path.Combine(wwwRootPath + "/images", fileName);

                    // Store in file system
                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await bloggModel.ImageFile.CopyToAsync(fileStream);
                    }
                }
                else
                {
                    bloggModel.ImageName = "";
                }

                _context.Add(bloggModel);

                // Lägga till skapre automatiskt
                bloggModel.CreateBy = User.Identity?.Name ?? "Unknow";
                bloggModel.PublishDate = DateTime.Now;

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
            if (_context.Bloggs == null)
            {
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,ImageFile,ImageName,PublishDate")] BloggModel bloggModel)
        {
            if (id != bloggModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 找到要编辑的博客对象
                    var existingBloggModel = await _context.Bloggs.FindAsync(id);
                    if (existingBloggModel == null)
                    {
                        return NotFound();
                    }

                    // 如果存在新的图片文件，更新图片
                    if (bloggModel.ImageFile != null)
                    {
                        // 删除原始图片文件
                        if (!string.IsNullOrEmpty(existingBloggModel.ImageName) && System.IO.File.Exists(Path.Combine(wwwRootPath + "/images", existingBloggModel.ImageName)))
                        {
                            System.IO.File.Delete(Path.Combine(wwwRootPath + "/images", existingBloggModel.ImageName));
                        }

                        // 生成新的文件名并存储图片文件到文件系统
                        string fileName = Path.GetFileNameWithoutExtension(bloggModel.ImageFile.FileName);
                        string extension = Path.GetExtension(bloggModel.ImageFile.FileName);

                        bloggModel.ImageName = fileName.Replace(" ", String.Empty) + DateTime.Now.ToString("yymmssfff") + extension;

                        string path = Path.Combine(wwwRootPath + "/images", bloggModel.ImageName);

                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await bloggModel.ImageFile.CopyToAsync(fileStream);
                        }

                        // 更新博客对象的图片名称
                        existingBloggModel.ImageName = bloggModel.ImageName;
                    }

                    // 更新其他属性
                    existingBloggModel.Title = bloggModel.Title;
                    existingBloggModel.Content = bloggModel.Content;
                    existingBloggModel.PublishDate = bloggModel.PublishDate;

                    // 更新数据库中的记录
                    _context.Update(existingBloggModel);
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
            if (_context.Bloggs == null)
            {
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
            if (_context.Bloggs == null)
            {
                return NotFound();
            }

            var bloggModel = await _context.Bloggs.FindAsync(id);
            if (bloggModel != null)
            {
                if (!string.IsNullOrEmpty(bloggModel.ImageName) && System.IO.File.Exists(Path.Combine(wwwRootPath + "/images", bloggModel.ImageName)))
                {
                    System.IO.File.Delete(Path.Combine(wwwRootPath + "/images", bloggModel.ImageName));
                }
                
                _context.Bloggs.Remove(bloggModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BloggModelExists(int id)
        {
            // Kontrollera if_context.Bloggs.is.null
            if (_context.Bloggs == null)
            {
                return false;
            }

            return _context.Bloggs.Any(e => e.Id == id);
        }
    }
}
