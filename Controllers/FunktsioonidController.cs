using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mathProject.Data;
using mathProject.Models;
using mathProject.Services;

namespace mathProject.Controllers;

public class FunktsioonidController : Controller
{
    private readonly AppDbContext _context;

    public FunktsioonidController(AppDbContext context)
    {
        _context = context;
    }

    // READ: nimekiri
    public async Task<IActionResult> Index()
    {
        var list = await _context.FunktsiooniUurimised
            .OrderByDescending(f => f.LuodudAeg)
            .ToListAsync();
        return View(list);
    }

    // READ: detailvaade + graafik
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var olem = await _context.FunktsiooniUurimised.FirstOrDefaultAsync(m => m.Id == id);
        if (olem == null) return NotFound();

        // Punktid arvutatakse serveris NCalc-iga: x vahemikus [-10; 10], samm 0.1
        var xs = FunktsiooniArvutaja.XVaartused(-10, 10, 0.1);
        ViewBag.XValues = xs;
        ViewBag.YFunction = FunktsiooniArvutaja.YVaartused(olem.Valem, xs);
        ViewBag.YDerivative = FunktsiooniArvutaja.YVaartused(olem.Tuletis, xs);

        return View(olem);
    }

    // CREATE
    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Valem,Maaramispiirkond,Tuletis,KriitilisedPunktid,Ekstreemumid")] FunktsiooniUurimine olem)
    {
        KontrolliValemeid(olem);

        if (!ModelState.IsValid) return View(olem);

        olem.LuodudAeg = DateTime.UtcNow;
        _context.FunktsiooniUurimised.Add(olem);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // UPDATE
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var olem = await _context.FunktsiooniUurimised.FindAsync(id);
        if (olem == null) return NotFound();

        return View(olem);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("Valem,Maaramispiirkond,Tuletis,KriitilisedPunktid,Ekstreemumid")] FunktsiooniUurimine vorm)
    {
        var olem = await _context.FunktsiooniUurimised.FindAsync(id);
        if (olem == null) return NotFound();

        KontrolliValemeid(vorm);

        if (!ModelState.IsValid)
        {
            vorm.Id = id;   // et vorm teaks, millist kirjet muudetakse
            return View(vorm);
        }

        olem.Valem = vorm.Valem;
        olem.Maaramispiirkond = vorm.Maaramispiirkond;
        olem.Tuletis = vorm.Tuletis;
        olem.KriitilisedPunktid = vorm.KriitilisedPunktid;
        olem.Ekstreemumid = vorm.Ekstreemumid;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    // DELETE
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

    // Kontrollib, et valem ja tuletis on NCalc-iga arvutatavad
    private void KontrolliValemeid(FunktsiooniUurimine olem)
    {
        if (!string.IsNullOrWhiteSpace(olem.Valem))
        {
            var viga = FunktsiooniArvutaja.Kontrolli(olem.Valem);
            if (viga != null) ModelState.AddModelError(nameof(olem.Valem), viga);
        }
        if (!string.IsNullOrWhiteSpace(olem.Tuletis))
        {
            var viga = FunktsiooniArvutaja.Kontrolli(olem.Tuletis);
            if (viga != null) ModelState.AddModelError(nameof(olem.Tuletis), viga);
        }
    }
}