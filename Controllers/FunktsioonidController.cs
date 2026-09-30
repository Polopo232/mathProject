using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mathProject.Data;
using mathProject.Models;
using NCalc;

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

        ViewBag.XValues = CalculatePoints(olem.Valem, -5, 5, 0.2);
        ViewBag.YFunction = CalculatePoints(olem.Valem, -5, 5, 0.2, evaluateY: true);
        ViewBag.YDerivative = CalculatePoints(olem.Tuletis, -5, 5, 0.2, evaluateY: true);

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
    private List<double> CalculatePoints(string expressionText, double minX, double maxX, double step, bool evaluateY = false)
    {
        var result = new List<double>();
        if (string.IsNullOrWhiteSpace(expressionText)) return result;

        string normalizedExpr = expressionText.Replace("^", "**");

        for (double x = minX; x <= maxX; x += step)
        {
            x = Math.Round(x, 1);
            if (!evaluateY)
            {
                result.Add(x);
                continue;
            }

            try
            {
                var expr = new Expression(expressionText.Replace("x", x.ToString(System.Globalization.CultureInfo.InvariantCulture)));

                expr.Parameters["x"] = x;

                object eval = expr.Evaluate();
                result.Add(Convert.ToDouble(eval));
            }
            catch
            {
                result.Add(0);
            }
        }
        return result;
    }
}