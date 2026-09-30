using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mathProject.Data;
using mathProject.Models;

namespace mathProject.Controllers;

public class FunktsioonidController : Controller
{
    private readonly AppDbContext _context;

    public FunktsioonidController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _context.FunktsiooniUurimised
            .OrderByDescending(f => f.LuodudAeg)
            .ToListAsync();
        return View(list);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FunktsiooniUurimine olem)
    {
        if (ModelState.IsValid)
        {
            olem.LuodudAeg = DateTime.UtcNow;
            _context.FunktsiooniUurimised.Add(olem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(olem);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var olem = await _context.FunktsiooniUurimised
            .FirstOrDefaultAsync(m => m.Id == id);

        if (olem == null) return NotFound();

        return View(olem);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var olem = await _context.FunktsiooniUurimised.FindAsync(id);
        if (olem != null)
        {
            _context.FunktsiooniUurimised.Remove(olem);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}