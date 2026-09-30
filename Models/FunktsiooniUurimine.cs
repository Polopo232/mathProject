using System.ComponentModel.DataAnnotations;

namespace mathProject.Models;

public class FunktsiooniUurimine
{
    public int Id { get; set; }
    public string Valem { get; set; } = string.Empty;              // nt "x^3 - 3*x"
    public string Maaramispiirkond { get; set; } = string.Empty;   // X: (-inf, inf)
    public string Tuletis { get; set; } = string.Empty;            // f'(x): 3*x^2 - 3
    public string KriitilisedPunktid { get; set; } = string.Empty; // x_e: x1 = -1, x2 = 1
    public string Ekstreemumid { get; set; } = string.Empty;       // y_e: max = 2, min = -2
    public DateTime LuodudAeg { get; set; } = DateTime.UtcNow;
}