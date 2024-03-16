using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Blogg.Models;
using Microsoft.AspNetCore.Authorization;
using Blogg.Data;
using Microsoft.EntityFrameworkCore;


namespace Blogg.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        // Kontrollera if_context.Bloggs.is.null
        if (_context.Bloggs == null)
        {
            return NotFound();
        }

        var latestBlogg = _context.Bloggs.OrderByDescending(b => b.PublishDate).FirstOrDefault();
        return View(latestBlogg);
    }
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

    public IActionResult BloggList()
    {
        // Kontrollera if_context.Bloggs.is.null
        if (_context.Bloggs == null)
        {
            return NotFound();
        }

        return View(_context.Bloggs.ToList());
    }

    [Route("BloggList/Search")]
    public async Task<IActionResult> BloggList(string? searchString)
    {
        // Kontrollera if_context.Bloggs.is.null
        if (_context.Bloggs == null)
        {
            return NotFound();
        }
        
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

    [HttpGet("BloggList/FilterByAuthor")]
    public async Task<IActionResult> FilterByAuthor(string author)
    {
        var filteredBloggs = await _context.Bloggs
                                    .Where(b => b.CreateBy == author)
                                    .ToListAsync();

        return View("BloggList", filteredBloggs);
    }

    public IActionResult Exhibits()
    {
        return View();
    }

    [Authorize]
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
