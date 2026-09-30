using System.Globalization;
using NCalc;

namespace mathProject.Services;

/// <summary>
/// Arvutab kasutaja sisestatud tekstilise valemi väärtusi (bonusülesanne, NCalc).
/// NCalc ei tunne tehet ^, seepärast muudame "x^2" -> "Pow(x,2)".
/// x antakse edasi PARAMEETRINA (mitte tekstiasendusega), nii et negatiivsed arvud töötavad õigesti.
/// </summary>
public static class FunktsiooniArvutaja
{
    // Kontrollib valemit. Tagastab null, kui kõik on korras, muidu veateate.
    public static string? Kontrolli(string valem)
    {
        try
        {
            var e = LooExpression(valem);
            if (e.HasErrors()) return "Valemi süntaks on vigane";
            e.Parameters["x"] = 1.0;
            e.Evaluate();
            return null;
        }
        catch (Exception ex)
        {
            return "Vigane valem: " + ex.Message + " (kasuta korrutamiseks *, nt 2*x)";
        }
    }

    // x väärtused: min, min+step, ... , max
    public static List<double> XVaartused(double min, double max, double step)
    {
        int n = (int)Math.Round((max - min) / step);
        var tulemus = new List<double>();
        for (int i = 0; i <= n; i++)
            tulemus.Add(Math.Round(min + i * step, 4));
        return tulemus;
    }

    // y väärtused antud x-ide jaoks. Kui punktis pole väärtust (nt 1/0), on tulemus null -> graafikul auk.
    public static List<double?> YVaartused(string valem, List<double> xs)
    {
        var tulemus = new List<double?>();
        if (string.IsNullOrWhiteSpace(valem)) return xs.Select(_ => (double?)null).ToList();

        Expression e;
        try { e = LooExpression(valem); }
        catch { return xs.Select(_ => (double?)null).ToList(); }

        foreach (var x in xs)
        {
            try
            {
                e.Parameters["x"] = x;
                double y = Convert.ToDouble(e.Evaluate(), CultureInfo.InvariantCulture);
                tulemus.Add(double.IsFinite(y) ? y : null);
            }
            catch
            {
                tulemus.Add(null);
            }
        }
        return tulemus;
    }

    private static Expression LooExpression(string valem)
    {
        return new Expression(MuudaAstmed(valem.Trim()));
    }

    // "x^2" -> "Pow(x,2)", "(x+1)^3" -> "Pow((x+1),3)", "2^3^2" -> "Pow(2,Pow(3,2))"
    private static string MuudaAstmed(string s)
    {
        int i;
        while ((i = s.LastIndexOf('^')) >= 0)   // paremalt vasakule => parem-assotsiatiivsus
        {
            // --- aste alus (tehtest ^ vasakul) ---
            int a = i - 1;
            while (a >= 0 && s[a] == ' ') a--;
            int algus;
            if (a >= 0 && s[a] == ')')
            {
                int sygavus = 0, j = a;
                for (; j >= 0; j--)
                {
                    if (s[j] == ')') sygavus++;
                    else if (s[j] == '(') { sygavus--; if (sygavus == 0) break; }
                }
                if (j < 0) throw new FormatException("sulgude arv ei klapi");
                algus = j;
                while (algus > 0 && (char.IsLetterOrDigit(s[algus - 1]) || s[algus - 1] == '_')) algus--; // nt sin(x)^2
            }
            else
            {
                int j = a;
                while (j >= 0 && (char.IsLetterOrDigit(s[j]) || s[j] == '.' || s[j] == '_')) j--;
                algus = j + 1;
                if (algus > a) throw new FormatException("märgi ^ ees puudub arv või x");
            }
            string alus = s.Substring(algus, a - algus + 1);

            // --- astendaja (tehtest ^ paremal) ---
            int b = i + 1;
            while (b < s.Length && s[b] == ' ') b++;
            int astendajaAlgus = b;
            if (b < s.Length && (s[b] == '-' || s[b] == '+')) b++;
            int sisuAlgus = b;
            if (b < s.Length && s[b] == '(')
            {
                b = LeiaSulguLopp(s, b);
            }
            else
            {
                while (b < s.Length && (char.IsLetterOrDigit(s[b]) || s[b] == '.' || s[b] == '_')) b++;
                if (b < s.Length && s[b] == '(') b = LeiaSulguLopp(s, b);   // funktsioon, nt Pow(...)
            }
            if (b == sisuAlgus) throw new FormatException("märgi ^ järel puudub arv või x");
            string astendaja = s.Substring(astendajaAlgus, b - astendajaAlgus);

            s = s.Substring(0, algus) + "Pow(" + alus + "," + astendaja + ")" + s.Substring(b);
        }
        return s;
    }

    // Tagastab indeksi PÄRAST vastavat sulgevat ")"
    private static int LeiaSulguLopp(string s, int avaIndeks)
    {
        int sygavus = 0;
        for (int j = avaIndeks; j < s.Length; j++)
        {
            if (s[j] == '(') sygavus++;
            else if (s[j] == ')') { sygavus--; if (sygavus == 0) return j + 1; }
        }
        throw new FormatException("sulgude arv ei klapi");
    }
}