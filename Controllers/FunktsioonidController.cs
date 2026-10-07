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

    public async Task<IActionResult> Index()
    {
        var list = await _context.FunktsiooniUurimised
            .OrderByDescending(f => f.LuodudAeg)
            .ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var olem = await _context.FunktsiooniUurimised.FirstOrDefaultAsync(m => m.Id == id);
        if (olem == null) return NotFound();

        var analuus = FunktsiooniArvutaja.Analuusi(olem.Valem);
        ViewBag.Analuus = analuus;
        ViewBag.ValemF = olem.Valem;
        ViewBag.ValemD1 = analuus.Tuletis1;
        ViewBag.Markepunktid = analuus.Markepunktid.Select(p => new { x = p.X, y = p.Y, label = p.Silt }).ToList();

        return View(olem);
    }

    [HttpGet]
    public IActionResult AutoCalculate(string valem)
    {
        if (string.IsNullOrWhiteSpace(valem)) return BadRequest();
        var analuus = FunktsiooniArvutaja.Analuusi(valem);
        return Json(new
        {
            tuletis = analuus.Tuletis1,
            maaramispiirkond = analuus.MaaramispiirkondX.Vaartus,
            kriitilisedPunktid = analuus.Nullkohad.Vaartus,
            ekstreemumid = analuus.Ekstreemumid.Vaartus
        });
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Valem,Maaramispiirkond,Tuletis,KriitilisedPunktid,Ekstreemumid")] FunktsiooniUurimine olem)
    {
        if (string.IsNullOrWhiteSpace(olem.Tuletis))
        {
            var auto = FunktsiooniArvutaja.Analuusi(olem.Valem);
            olem.Tuletis = auto.Tuletis1;
            olem.Maaramispiirkond = auto.MaaramispiirkondX.Vaartus;
            olem.KriitilisedPunktid = auto.Nullkohad.Vaartus;
            olem.Ekstreemumid = auto.Ekstreemumid.Vaartus;
        }

        KontrolliValemeid(olem);
        if (!ModelState.IsValid) return View(olem);

        olem.LuodudAeg = DateTime.UtcNow;
        _context.FunktsiooniUurimised.Add(olem);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = olem.Id });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var olem = await _context.FunktsiooniUurimised.FindAsync(id);
        return olem == null ? NotFound() : View(olem);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Valem,Maaramispiirkond,Tuletis,KriitilisedPunktid,Ekstreemumid")] FunktsiooniUurimine vorm)
    {
        var olem = await _context.FunktsiooniUurimised.FindAsync(id);
        if (olem == null) return NotFound();

        KontrolliValemeid(vorm);
        if (!ModelState.IsValid)
        {
            vorm.Id = id;
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

    private void KontrolliValemeid(FunktsiooniUurimine olem)
    {
        if (!string.IsNullOrWhiteSpace(olem.Valem))
        {
            var viga = FunktsiooniArvutaja.Kontrolli(olem.Valem);
            if (viga != null) ModelState.AddModelError(nameof(olem.Valem), viga);
        }
    }
}