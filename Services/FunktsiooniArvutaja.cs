using System.Globalization;
using System.Text.RegularExpressions;
using NCalc;

namespace mathProject.Services;

public class OmaduseInfo
{
    public string Nimetus { get; set; } = string.Empty;
    public string Vaartus { get; set; } = string.Empty;
    public string VaartusLatex { get; set; } = string.Empty;
    public List<string> Sammud { get; set; } = new();
    public string MisSeeOn { get; set; } = string.Empty;
    public string ValemidJaReeglid { get; set; } = string.Empty;
    public string GeomeetrilineTahendus { get; set; } = string.Empty;
}

public class AnaluusiTulemus
{
    public string Valem { get; set; } = string.Empty;
    public string ValemLatex { get; set; } = string.Empty;
    public string Tuletis1 { get; set; } = string.Empty;
    public string Tuletis1Latex { get; set; } = string.Empty;

    public List<double> VertikaalsedAsumptoodid { get; set; } = new();
    public double? HorisontaalneAsumptoot { get; set; }

    public OmaduseInfo MaaramispiirkondX { get; set; } = new();
    public OmaduseInfo MuutumispiirkondY { get; set; } = new();
    public OmaduseInfo Nullkohad { get; set; } = new();
    public OmaduseInfo YLoikepunkt { get; set; } = new();
    public OmaduseInfo Margipiirkonnad { get; set; } = new();
    public OmaduseInfo HarudJaKumerus { get; set; } = new();
    public OmaduseInfo Paarsus { get; set; } = new();
    public OmaduseInfo Ekstreemumid { get; set; } = new();
    public OmaduseInfo TuletisInfo { get; set; } = new();

    public List<(double X, double Y, string Silt)> Markepunktid { get; set; } = new();
}

public static class FunktsiooniArvutaja
{
    public static string Normaliseeri(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
        s = s.Trim().Replace(" ", "");
        s = Regex.Replace(s, @"(?<![a-zA-Z0-9_])e(?![a-zA-Z0-9_])", "2.718281828459");
        s = Regex.Replace(s, @"(?<![a-zA-Z0-9_])E(?![a-zA-Z0-9_])", "2.718281828459");
        s = Regex.Replace(s, @"(\d)([a-zA-Z(])", "$1*$2");
        s = Regex.Replace(s, @"([a-zA-Z)])(\()", "$1*$2");
        s = Regex.Replace(s, @"(\))([a-zA-Z0-9])", "$1*$2");
        return s;
    }

    public static string TeisendaLatexiks(string valem)
    {
        if (string.IsNullOrWhiteSpace(valem)) return string.Empty;
        string s = valem.Trim();

        var match = Regex.Match(s, @"^\(?(.*?)\)?\s*/\s*\(?(.*?)\)?$");
        if (match.Success && s.Contains('/'))
        {
            string lugeja = match.Groups[1].Value.Trim('(', ')').Replace("*", @" \cdot ");
            string nimetaja = match.Groups[2].Value.Trim('(', ')').Replace("*", @" \cdot ");
            return $@"\frac{{{lugeja}}}{{{nimetaja}}}";
        }

        return s.Replace("*", @" \cdot ");
    }

    public static string? Kontrolli(string valem)
    {
        try
        {
            var e = LooExpression(Normaliseeri(valem));
            if (e.HasErrors()) return "Valemi süntaks on vigane";
            e.Parameters["x"] = 2.5;
            e.Evaluate();
            return null;
        }
        catch (Exception ex)
        {
            return "Vigane valem: " + ex.Message;
        }
    }

    public static AnaluusiTulemus Analuusi(string valemRaw)
    {
        string norm = valemRaw.Trim().Replace(" ", "");
        var res = new AnaluusiTulemus
        {
            Valem = valemRaw,
            ValemLatex = TeisendaLatexiks(valemRaw)
        };

        if (Regex.IsMatch(norm, @"(?<![a-zA-Z0-9_])e\^x", RegexOptions.IgnoreCase) || norm == "e^x")
        {
            AnaluusiEksponent(norm, res);
        }
        else if (norm.Contains('/'))
        {
            AnaluusiMurdfunktsioon(Normaliseeri(norm), res);
        }
        else
        {
            AnaluusiPolunoom(Normaliseeri(norm), res);
        }

        return res;
    }

    private static void AnaluusiEksponent(string norm, AnaluusiTulemus res)
    {
        res.Tuletis1 = "e^x";
        res.Tuletis1Latex = "e^x";

        res.MaaramispiirkondX.Nimetus = "Määramispiirkond (X)";
        res.MaaramispiirkondX.MisSeeOn = "Määramispiirkond näitab kõiki x väärtusi, mida on lubatud valemisse sisestada ja mille korral funktsioon on arvutatav.";
        res.MaaramispiirkondX.ValemidJaReeglid = @"X = \mathbb{R} \quad\text{ehk}\quad x \in (-\infty; \infty)";
        res.MaaramispiirkondX.GeomeetrilineTahendus = "Graafik on katkematu joon, mis jookseb takistusteta kogu horisontaaltelje X ulatuses.";
        res.MaaramispiirkondX.Sammud.Add("Samm 1: Funktsioonis f(x) = e^x on astme aluseks konstantne Euleri arv e ≈ 2.71828.");
        res.MaaramispiirkondX.Sammud.Add("Samm 2: Positiivset arvu e saab astendada mistahes reaalarvulise astendajaga (positiivse, negatiivse ja nulliga).");
        res.MaaramispiirkondX.Sammud.Add("Samm 3: Jagamistehteid ega ruutjuuri valemis ei ole, seega kitsendusi pole: X: (-∞; ∞).");
        res.MaaramispiirkondX.Vaartus = "X: (-∞; ∞)";
        res.MaaramispiirkondX.VaartusLatex = @"X \in (-\infty; \infty)";

        res.YLoikepunkt.Nimetus = "Y-telje lõikepunkt";
        res.YLoikepunkt.MisSeeOn = "Punkt, kus graafik läbib vertikaalset Y-telge. Selles punktis on x-koordinaat alati 0.";
        res.YLoikepunkt.ValemidJaReeglid = @"y = f(0) = e^0 = 1";
        res.YLoikepunkt.GeomeetrilineTahendus = "Graafik läbib Y-telge täpselt kõrgusel 1, koordinaatidega P(0; 1).";
        res.YLoikepunkt.Sammud.Add("Samm 1: Y-telje lõikepunkti leidmiseks asendame x-i asemele 0: f(0).");
        res.YLoikepunkt.Sammud.Add("Samm 2: Matemaatika põhireegli järgi on iga nullist erineva arvu nullis aste võrdne ühega: e^0 = 1.");
        res.YLoikepunkt.Sammud.Add("Samm 3: Lõikepunkti koordinaadid on P(0; 1).");
        res.YLoikepunkt.Vaartus = "P(0; 1)";
        res.YLoikepunkt.VaartusLatex = @"P(0;\; 1)";
        res.Markepunktid.Add((0, 1, "Y-lõikepunkt (0; 1)"));

        res.Nullkohad.Nimetus = "Nullkohad (X₀, f(x) = 0)";
        res.Nullkohad.MisSeeOn = "X-telje punktid, kus funktsiooni kõrgus y võrdub nulliga (kohad, kus graafik lõikab horisontaaltelge).";
        res.Nullkohad.ValemidJaReeglid = @"e^x = 0 \implies \text{lahend puudub, sest } e^x > 0 \text{ alati}";
        res.Nullkohad.GeomeetrilineTahendus = "Graafik ei puuduta ega lõika X-telge kuskil, vaid hõljub alati sellest kõrgemal.";
        res.Nullkohad.Sammud.Add("Samm 1: Nullkohtade leidmiseks seame võrrandi: e^x = 0.");
        res.Nullkohad.Sammud.Add("Samm 2: Kuna e ≈ 2.71828 on positiivne arv, on selle mistahes aste alati rangelt suurem kui null (e^x > 0).");
        res.Nullkohad.Sammud.Add("Samm 3: Võrrandil puudub lahend, seega graafikul nullkohti ei eksisteeri.");
        res.Nullkohad.Vaartus = "Puuduvad";
        res.Nullkohad.VaartusLatex = @"\emptyset";

        res.TuletisInfo.Nimetus = "Esimene tuletis f'(x)";
        res.TuletisInfo.MisSeeOn = "Tuletis näitab funktsiooni muutumise kiirust ja graafiku puutuja tõusu igas punktis.";
        res.TuletisInfo.ValemidJaReeglid = @"(e^x)' = e^x";
        res.TuletisInfo.GeomeetrilineTahendus = "Kuna tuletis on alati positiivne (e^x > 0), siis funktsiooni graafik tõuseb kogu aeg vasakult paremale.";
        res.TuletisInfo.Sammud.Add("Samm 1: Rakendame eksponentfunktsiooni diferentseerimisreeglit: (e^x)' = e^x · ln(e).");
        res.TuletisInfo.Sammud.Add("Samm 2: Naturaallogaritm naturaalarvust ln(e) = 1, mistõttu tuletis on identne algse funktsiooniga: f'(x) = e^x.");
        res.TuletisInfo.Sammud.Add("Samm 3: Kuna f'(x) > 0 kõigi x väärtuste korral, on funktsioon pidevalt rangelt kasvav.");
        res.TuletisInfo.Vaartus = "f'(x) = e^x";
        res.TuletisInfo.VaartusLatex = "f'(x) = e^x";

        res.Ekstreemumid.Nimetus = "Ekstreemumid (haripunktid)";
        res.Ekstreemumid.MisSeeOn = "Funktsiooni lokaalsed tipud (maksimumid) või orud (miinimumid), kus kasvamine vahetub kahanemisega.";
        res.Ekstreemumid.ValemidJaReeglid = @"f'(x) = 0 \iff e^x = 0 \implies \text{lahend puudub}";
        res.Ekstreemumid.GeomeetrilineTahendus = "Graafikul puuduvad pöördepunktid ja horisontaalsed puutujad.";
        res.Ekstreemumid.Sammud.Add("Samm 1: Ekstreemumi leidmiseks võrdsustame tuletise nulliga: f'(x) = 0.");
        res.Ekstreemumid.Sammud.Add("Samm 2: Saame võrrandi e^x = 0, millel pole reaalarvulisi lahendeid.");
        res.Ekstreemumid.Sammud.Add("Samm 3: Kuna tuletis ei muutu nulliks ega vaheta märki, puuduvad funktsioonil haripunktid.");
        res.Ekstreemumid.Vaartus = "Puuduvad";
        res.Ekstreemumid.VaartusLatex = @"\emptyset";

        res.HarudJaKumerus.Nimetus = "Asümptoodid ja harude käitumine";
        res.HarudJaKumerus.MisSeeOn = "Asümptoot on sirgjoon, millele graafik järjest lähemale liigub, aga mida ta kunagi päriselt ei ületa.";
        res.HarudJaKumerus.ValemidJaReeglid = @"\lim_{x \to -\infty} e^x = 0 \implies \text{horisontaalne sirge } y = 0";
        res.HarudJaKumerus.GeomeetrilineTahendus = "Vasakpoolne haru lamendub piki X-telge, parempoolne haru sööstab järsult üles.";
        res.HarudJaKumerus.Sammud.Add("Samm 1: Uurime käitumist miinuslõpmatuses (x → -∞): väärtus e^x muutub lõpmatult väikeseks positiivseks arvuks ja läheneb nullile.");
        res.HarudJaKumerus.Sammud.Add("Samm 2: Sellest tuleneb, et horisontaalne sirge y = 0 (X-telg) on funktsiooni horisontaalne asümptoot vasakul poolel.");
        res.HarudJaKumerus.Sammud.Add("Samm 3: Plusslõpmatusse liikudes (x → +∞) kasvab e^x piiramatult lõpmatusse (+∞).");
        res.HarudJaKumerus.Vaartus = "Horisontaalne asümptoot: y = 0 (vasakul); Vertikaalsed puuduvad";
        res.HarudJaKumerus.VaartusLatex = @"\text{Horisontaalne: } y = 0\; (x \to -\infty)";
        res.HorisontaalneAsumptoot = 0;

        res.Paarsus.Nimetus = "Funktsiooni paarsus (sümmeetria)";
        res.Paarsus.MisSeeOn = "Näitab, kas graafik on peegelsümmeetriline Y-telje suhtes (paaris) või pöördsümmeetriline alguspunkti suhtes (paaritu).";
        res.Paarsus.ValemidJaReeglid = @"f(-x) = e^{-x} = \frac{1}{e^x} \neq f(x) \quad\text{ja}\quad f(-x) \neq -f(x)";
        res.Paarsus.GeomeetrilineTahendus = "Graafikul puudub sümmeetriatelg ja sümmeetriakeskpunt.";
        res.Paarsus.Sammud.Add("Samm 1: Asendame funktsioonis x vastandarvuga (-x): f(-x) = e^(-x) = 1 / e^x.");
        res.Paarsus.Sammud.Add("Samm 2: Tulemus 1 / e^x ei võrdu algse funktsiooniga e^x (seega funktsioon ei ole paaris).");
        res.Paarsus.Sammud.Add("Samm 3: Tulemus ei võrdu ka vastandfunktsiooniga -e^x (seega pole paaritu). Tegemist on üldkujulise funktsiooniga.");
        res.Paarsus.Vaartus = "EI OLE PAARIS EGA PAARITU (üldkuju)";
        res.Paarsus.VaartusLatex = @"\text{Ei ole paaris ega paaritu}";

        res.Margipiirkonnad.Nimetus = "Märgitüsivuse piirkonnad (X⁺ ja X⁻)";
        res.Margipiirkonnad.MisSeeOn = "X-telje vahemikud, kus funktsioon on positiivne (graafik ülalpool telge) või negatiivne (allpool telge).";
        res.Margipiirkonnad.ValemidJaReeglid = @"e^x > 0 \quad\text{kogu määramispiirkonnas}";
        res.Margipiirkonnad.GeomeetrilineTahendus = "Kogu graafik asub täielikult X-teljest kõrgemal.";
        res.Margipiirkonnad.Sammud.Add("Samm 1: Funktsioonil pole nullkohti ega katkestuspunkte, kus märk saaks vahetuda.");
        res.Margipiirkonnad.Sammud.Add("Samm 2: Kuna e^x on alati positiivne suurus, kehtib f(x) > 0 kõigi reaalarvude korral.");
        res.Margipiirkonnad.Sammud.Add("Samm 3: Positiivsuspiirkond on X⁺: (-∞; ∞), negatiivsuspiirkond X⁻ puudub.");
        res.Margipiirkonnad.Vaartus = "X⁺: (-∞; ∞), X⁻ puudub";
        res.Margipiirkonnad.VaartusLatex = @"X^+ \in (-\infty; \infty)";

        res.MuutumispiirkondY.Nimetus = "Muutumispiirkond (Y)";
        res.MuutumispiirkondY.MisSeeOn = "Kõik võimalikud tulemused ja y väärtused, mida funktsioon saab omandada.";
        res.MuutumispiirkondY.ValemidJaReeglid = @"Y = (0; \infty)";
        res.MuutumispiirkondY.GeomeetrilineTahendus = "Graafiku vertikaalne ulatus katab vahemiku nullist lõpmatuseni, aga arvu 0 ennast ei saavutata.";
        res.MuutumispiirkondY.Sammud.Add("Samm 1: Astendades positiivset arvu, on tulemus alati suurem kui 0.");
        res.MuutumispiirkondY.Sammud.Add("Samm 2: Kui x läheneb miinuslõpmatusele, läheneb väärtus nullile; kui x kasvab, kasvab väärtus lõpmatusse.");
        res.MuutumispiirkondY.Sammud.Add("Samm 3: Järelikult on muutumispiirkond Y: (0; ∞).");
        res.MuutumispiirkondY.Vaartus = "Y: (0; ∞)";
        res.MuutumispiirkondY.VaartusLatex = @"Y \in (0; \infty)";
    }

    private static void AnaluusiMurdfunktsioon(string norm, AnaluusiTulemus res)
    {
        var (lugejaStr, nimetajaStr) = EraldaLugejaJaNimetaja(norm);
        var uTerms = PuhastaPolunoom(ParsiPolunoom(lugejaStr));
        var vTerms = PuhastaPolunoom(ParsiPolunoom(nimetajaStr));

        var uJuured = LeiaPolunoomiJuured(uTerms);
        var vJuured = LeiaPolunoomiJuured(vTerms);
        res.VertikaalsedAsumptoodid = vJuured;

        string uStr = PolunoomToString(uTerms);
        string vStr = PolunoomToString(vTerms);

        res.MaaramispiirkondX.Nimetus = "Määramispiirkond (X)";
        res.MaaramispiirkondX.MisSeeOn = "Argumendi x lubatud väärtuste hulk. Murru puhul ei tohi jagada nulliga, mis tähendab, et murru alumine pool ehk nimetaja ei tohi võrduda nulliga.";
        res.MaaramispiirkondX.ValemidJaReeglid = @"\text{Reegel: } \frac{\text{Lugeja}}{\text{Nimetaja}} \implies \text{Nimetaja} \neq 0";
        res.MaaramispiirkondX.GeomeetrilineTahendus = "Nendes x väärtustes, kus nimetaja saab nulliks, katkeb graafiku joon ja tekivad vertikaalsed asümptoodid.";
        res.MaaramispiirkondX.Sammud.Add($"Samm 1: Vaatleme murru nimetajat (alumist osa): {vStr}.");
        res.MaaramispiirkondX.Sammud.Add($"Samm 2: Kuna nulliga jagamine on võimatu, paneme kirja tingimuse: {vStr} ≠ 0.");
        res.MaaramispiirkondX.Sammud.Add($"Samm 3: Leiame keelatud punktid, lahendades võrrandi {vStr} = 0.");

        if (vJuured.Any())
        {
            res.MaaramispiirkondX.Sammud.Add($"Samm 4: Nimetaja saab nulliks punktides: {string.Join(", ", vJuured.Select(x => $"x = {x}"))}.");
            res.MaaramispiirkondX.Sammud.Add($"Samm 5: Jätame need punktid reaalarvude hulgast välja: X = R \\ {{{string.Join("; ", vJuured)}}}.");
            res.MaaramispiirkondX.Vaartus = $"X: R \\ {{{string.Join("; ", vJuured)}}}";
            res.MaaramispiirkondX.VaartusLatex = $@"X = \mathbb{{R}} \setminus \{{{string.Join("; ", vJuured)}\}}";
        }
        else
        {
            res.MaaramispiirkondX.Sammud.Add("Samm 4: Nimetajal reaalarvulised nullkohad puuduvad (nimetaja pole ühegi x korral 0).");
            res.MaaramispiirkondX.Sammud.Add("Samm 5: Jagamist nulliga ei teki, seega on funktsioon määratud kogu reaalarvude teljel: X: (-∞; ∞).");
            res.MaaramispiirkondX.Vaartus = "X: (-∞; ∞)";
            res.MaaramispiirkondX.VaartusLatex = @"X \in (-\infty; \infty)";
        }

        res.YLoikepunkt.Nimetus = "Y-telje lõikepunkt";
        res.YLoikepunkt.MisSeeOn = "Koht, kus funktsiooni graafik lõikab vertikaalset Y-telge. Selles punktis on x alati 0.";
        res.YLoikepunkt.ValemidJaReeglid = @"y = f(0) = \frac{\text{Lugeja}(0)}{\text{Nimetaja}(0)}";
        res.YLoikepunkt.GeomeetrilineTahendus = "Graafiku alguspunkt vertikaalteljel x = 0.";
        res.YLoikepunkt.Sammud.Add("Samm 1: Asendame funktsiooni valemisse x = 0.");

        if (vJuured.Contains(0))
        {
            res.YLoikepunkt.Vaartus = "Puudub (asümptoot)";
            res.YLoikepunkt.VaartusLatex = @"\text{Puudub}";
            res.YLoikepunkt.Sammud.Add("Samm 2: Kohal x = 0 on nimetaja väärtus 0, mis tähendab jagamist nulliga.");
            res.YLoikepunkt.Sammud.Add("Samm 3: Kuna x = 0 ei kuulu määramispiirkonda, siis graafik Y-telge ei lõika.");
        }
        else
        {
            double cU = uTerms.GetValueOrDefault(0, 0);
            double cV = vTerms.GetValueOrDefault(0, 0);
            double y0 = Math.Round(cU / cV, 3);
            res.YLoikepunkt.Vaartus = $"P(0; {y0})";
            res.YLoikepunkt.VaartusLatex = $@"P(0;\; {y0})";
            res.YLoikepunkt.Sammud.Add($"Samm 2: Arvutame lugeja väärtuse: {cU}.");
            res.YLoikepunkt.Sammud.Add($"Samm 3: Arvutame nimetaja väärtuse: {cV}.");
            res.YLoikepunkt.Sammud.Add($"Samm 4: Jagame tulemused: f(0) = {cU} / {cV} = {y0}. Lõikepunkt on P(0; {y0}).");
            res.Markepunktid.Add((0, y0, $"Y-lõikepunkt (0; {y0})"));
        }

        res.Nullkohad.Nimetus = "Nullkohad (X₀, f(x) = 0)";
        res.Nullkohad.MisSeeOn = "X-telje punktid, kus funktsiooni kõrgus y on 0. Murru väärtus saab olla 0 vaid siis, kui selle ülemine pool (lugeja) on 0 ja alumine pool (nimetaja) ei ole samal ajal 0.";
        res.Nullkohad.ValemidJaReeglid = @"\frac{\text{Lugeja}}{\text{Nimetaja}} = 0 \iff \text{Lugeja} = 0 \quad\text{ja}\quad \text{Nimetaja} \neq 0";
        res.Nullkohad.GeomeetrilineTahendus = "Kohad, kus graafik läbib või puudutab horisontaaltelge X.";
        res.Nullkohad.Sammud.Add($"Samm 1: Murru väärtus võrdub nulliga parajasti siis, kui selle lugeja on null: {uStr} = 0.");

        var tegelikudJuured = uJuured.Where(x => !vJuured.Contains(x)).ToList();
        if (tegelikudJuured.Any())
        {
            res.Nullkohad.Sammud.Add($"Samm 2: Lahendame lugeja võrrandi, saame: {string.Join(", ", uJuured.Select(x => $"x = {x}"))}.");
            res.Nullkohad.Sammud.Add("Samm 3: Kontrollime, et leitud punktid ei muudaks nimetajat nulliks (kõik sobivad).");
            res.Nullkohad.Sammud.Add($"Samm 4: Nullkohad on: {string.Join(", ", tegelikudJuured.Select(x => $"x = {x}"))}.");
            res.Nullkohad.Vaartus = string.Join(", ", tegelikudJuured.Select((x, i) => $"x{i + 1} = {x}"));
            res.Nullkohad.VaartusLatex = string.Join(@",\; ", tegelikudJuured.Select((x, i) => $@"x_{i + 1} = {x}"));
            foreach (var r in tegelikudJuured)
                res.Markepunktid.Add((r, 0, $"Nullkoht ({r}; 0)"));
        }
        else
        {
            res.Nullkohad.Vaartus = "Puuduvad";
            res.Nullkohad.VaartusLatex = @"\emptyset";
            res.Nullkohad.Sammud.Add("Samm 2: Lugejal reaalarvulised lahendid puuduvad või need kattuvad nimetaja nullkohtadega.");
            res.Nullkohad.Sammud.Add("Samm 3: Graafik ei lõika X-telge kuskil.");
        }

        var uD = PuhastaPolunoom(VotaTuletis(uTerms));
        var vD = PuhastaPolunoom(VotaTuletis(vTerms));
        var uDv = PuhastaPolunoom(KorrutaPolunoomid(uD, vTerms));
        var uvD = PuhastaPolunoom(KorrutaPolunoomid(uTerms, vD));
        var lugejaTuletis = PuhastaPolunoom(LahutaPolunoomid(uDv, uvD));

        string uDStr = PolunoomToString(uD);
        string vDStr = PolunoomToString(vD);
        string numDerStr = PolunoomToString(lugejaTuletis);

        res.Tuletis1 = $"({numDerStr}) / ({vStr})^2";
        res.Tuletis1Latex = $@"\frac{{{numDerStr}}}{{({vStr})^2}}";

        res.TuletisInfo.Nimetus = "Esimene tuletis f'(x)";
        res.TuletisInfo.MisSeeOn = "Funktsiooni muutumiskiirus. Murru tuletist arvutatakse jagatise tuletise reegli järgi.";
        res.TuletisInfo.ValemidJaReeglid = @"\left(\frac{u}{v}\right)' = \frac{u' \cdot v - u \cdot v'}{v^2} \quad (\text{kus } u = \text{lugeja},\; v = \text{nimetaja})";
        res.TuletisInfo.GeomeetrilineTahendus = "Tuletise märk määrab funktsiooni kasvamis- ja kahanemisvahemikud.";
        res.TuletisInfo.Sammud.Add($"Samm 1: Tähistame lugeja u = {uStr} ja nimetaja v = {vStr}.");
        res.TuletisInfo.Sammud.Add($"Samm 2: Leiame lugeja tuletise u' = {uDStr} ja nimetaja tuletise v' = {vDStr}.");
        res.TuletisInfo.Sammud.Add($"Samm 3: Arvutame uute reeglite järgi lugeja: u'·v - u·v' = ({uDStr})({vStr}) - ({uStr})({vDStr}) = {numDerStr}.");
        res.TuletisInfo.Sammud.Add($"Samm 4: Paneme tulemuse murruna kokku: f'(x) = ({numDerStr}) / ({vStr})².");
        res.TuletisInfo.Vaartus = res.Tuletis1;
        res.TuletisInfo.VaartusLatex = res.Tuletis1Latex;

        res.Ekstreemumid.Nimetus = "Ekstreemumid (haripunktid)";
        res.Ekstreemumid.MisSeeOn = "Funktsiooni lokaalsed miinimum- või maksimumkohad. Murru tuletis saab olla 0 ainult siis, kui selle lugeja on 0.";
        res.Ekstreemumid.ValemidJaReeglid = @"f'(x) = 0 \iff \text{Tuletise lugeja} = 0";
        res.Ekstreemumid.GeomeetrilineTahendus = "Punktid, kus graafiku puutuja on horisontaalne.";
        res.Ekstreemumid.Sammud.Add($"Samm 1: Ekstreemumite leidmiseks võrdsustame tuletise nulliga: {numDerStr} = 0.");

        var dJuured = LeiaPolunoomiJuured(lugejaTuletis).Where(x => !vJuured.Contains(x)).ToList();
        if (dJuured.Any())
        {
            var eList = new List<string>();
            var eLatex = new List<string>();
            foreach (var xk in dJuured)
            {
                double yk = ArvutaMurdVaartus(uTerms, vTerms, xk);
                string tyyp = yk >= 0 ? "min" : "max";
                res.Ekstreemumid.Sammud.Add($"Samm 2: Leiame kriitilise punkti x = {xk}. Arvutame kõrguse f({xk}) = {yk}.");
                res.Ekstreemumid.Sammud.Add($"Samm 3: Tegemist on lokaalse {tyyp}-punktiga ({xk}; {yk}).");
                eList.Add($"{tyyp} = {yk} (kui x = {xk})");
                eLatex.Add($@"\text{{{tyyp}}} = {yk}\;(x={xk})");
                res.Markepunktid.Add((xk, yk, $"{tyyp.ToUpper()} ({xk}; {yk})"));
            }
            res.Ekstreemumid.Vaartus = string.Join("; ", eList);
            res.Ekstreemumid.VaartusLatex = string.Join(@",\; ", eLatex);
        }
        else
        {
            res.Ekstreemumid.Vaartus = "Puuduvad";
            res.Ekstreemumid.VaartusLatex = @"\emptyset";
            res.Ekstreemumid.Sammud.Add("Samm 2: Tuletise lugejal lahendid puuduvad.");
            res.Ekstreemumid.Sammud.Add("Samm 3: Tuletis ei muutu nulliks, seega ekstreemumeid pole.");
        }

        int degU = uTerms.Keys.DefaultIfEmpty(0).Max();
        int degV = vTerms.Keys.DefaultIfEmpty(0).Max();
        res.HarudJaKumerus.Nimetus = "Asümptoodid ja harude käitumine";
        res.HarudJaKumerus.MisSeeOn = "Sirged, millele graafik lõpmatusse kaugenedes järjest lähemale jõuab.";
        res.HarudJaKumerus.ValemidJaReeglid = @"\text{Vertikaalne: } \text{Nimetaja} = 0, \quad \text{Horisontaalne: } y = \lim_{x \to \pm\infty} f(x)";
        res.HarudJaKumerus.GeomeetrilineTahendus = "Määrab funktsiooni kuju ja piirid koordinaatteljestikul.";
        res.HarudJaKumerus.Sammud.Add("Samm 1: Vertikaalsed asümptoodid tekivad seal, kus murru nimetaja on 0.");

        if (vJuured.Any())
        {
            res.HarudJaKumerus.Sammud.Add($"Samm 2: Vertikaalsed asümptoodid on sirged: {string.Join(", ", vJuured.Select(x => $"x = {x}"))}.");
        }
        else
        {
            res.HarudJaKumerus.Sammud.Add("Samm 2: Vertikaalsed asümptoodid puuduvad.");
        }

        string horizStr = "puudub";
        if (degU == degV)
        {
            double aVal = uTerms.GetValueOrDefault(degU, 1);
            double bVal = vTerms.GetValueOrDefault(degV, 1);
            double yH = Math.Round(aVal / bVal, 3);
            res.HorisontaalneAsumptoot = yH;
            horizStr = $"y = {yH}";
            res.HarudJaKumerus.Sammud.Add($"Samm 3: Lugeja ja nimetaja astmed on võrdsed, jagame kordajad: y = {aVal}/{bVal} = {yH}. Horisontaalne asümptoot on sirge y = {yH}.");
        }
        else if (degU < degV)
        {
            res.HorisontaalneAsumptoot = 0;
            horizStr = "y = 0";
            res.HarudJaKumerus.Sammud.Add("Samm 3: Nimetaja aste on suurem, mistõttu läheneb murd lõpmatuses nullile: horisontaalne asümptoot on y = 0.");
        }
        else
        {
            res.HarudJaKumerus.Sammud.Add("Samm 3: Lugeja aste on nimetajast suurem, mistõttu horisontaalset asümptooti ei teki.");
        }

        res.HarudJaKumerus.Vaartus = $"Vertikaalsed: {(vJuured.Any() ? string.Join(", ", vJuured.Select(x => $"x={x}")) : "puuduvad")}; Horisontaalne: {horizStr}";
        res.HarudJaKumerus.VaartusLatex = $@"\text{{Vert: }} {(vJuured.Any() ? string.Join(@",\; ", vJuured.Select(x => $@"x={x}")) : @"\text{puuduvad}")} \quad|\quad \text{{Horiz: }} {horizStr}";

        bool uEven = uTerms.Keys.All(k => k % 2 == 0);
        bool vEven = vTerms.Keys.All(k => k % 2 == 0);
        res.Paarsus.Nimetus = "Funktsiooni paarsus";
        res.Paarsus.MisSeeOn = "Sümmeetria Y-telje (paaris) või koordinaatide alguspunkti suhtes (paaritu).";
        res.Paarsus.ValemidJaReeglid = @"\text{Paaris: } f(-x) = f(x), \quad \text{Paaritu: } f(-x) = -f(x)";
        res.Paarsus.GeomeetrilineTahendus = "Peegelsümmeetria.";
        res.Paarsus.Sammud.Add("Samm 1: Asendame valemis x vastandarvuga (-x) ja uurime astmeid.");

        if (uEven && vEven)
        {
            res.Paarsus.Vaartus = "PAARISFUNKTSIOON (sümmeetriline Y-telje suhtes)";
            res.Paarsus.VaartusLatex = @"\text{Paarisfunktsioon } (f(-x) = f(x))";
            res.Paarsus.Sammud.Add("Samm 2: Nii lugejas kui nimetajas on ainult paarisastmed, seega (-x)² = x².");
            res.Paarsus.Sammud.Add("Samm 3: f(-x) = f(x), graafik on peegelsümmeetriline Y-telje suhtes.");
        }
        else
        {
            res.Paarsus.Vaartus = "EI OLE PAARIS EGA PAARITU (üldkuju)";
            res.Paarsus.VaartusLatex = @"\text{Ei ole paaris ega paaritu}";
            res.Paarsus.Sammud.Add("Samm 2: Valemis esineb paarituid astmeid või nihkeid: f(-x) ≠ f(x) ja f(-x) ≠ -f(x).");
            res.Paarsus.Sammud.Add("Samm 3: Graafikul puudub sümmeetriatelg.");
        }

        var piirid = tegelikudJuured.Concat(vJuured).Distinct().OrderBy(x => x).ToList();
        res.Margipiirkonnad.Nimetus = "Märgitüsivuse piirkonnad (X⁺ ja X⁻)";
        res.Margipiirkonnad.MisSeeOn = "Vahemikud, kus graafik on teljest kõrgemal (y > 0) või madalamal (y < 0).";
        res.Margipiirkonnad.ValemidJaReeglid = @"X^+: f(x) > 0, \quad X^-: f(x) < 0";
        res.Margipiirkonnad.GeomeetrilineTahendus = "Märgi vahetus saab toimuda ainult nullkohtades ja asümptootides.";
        res.Margipiirkonnad.Sammud.Add("Samm 1: Kanname arvteljele kõik punktid, kus murru märk saab vahetuda (nullkohad ja nimetaja nullkohad).");
        res.Margipiirkonnad.Sammud.Add($"Samm 2: Piirpunktid on: {string.Join(", ", piirid.Select(x => $"x = {x}"))}.");
        res.Margipiirkonnad.Vaartus = piirid.Any() ? $"Piirpunktid: {string.Join("; ", piirid)}" : "Konstantne märk";
        res.Margipiirkonnad.VaartusLatex = piirid.Any() ? $@"x \in \{{{string.Join(@";\; ", piirid)}\}}" : @"\text{Püsiv märk}";

        res.MuutumispiirkondY.Nimetus = "Muutumispiirkond (Y)";
        res.MuutumispiirkondY.MisSeeOn = "Kõigi võimalike y väärtuste hulk.";
        res.MuutumispiirkondY.ValemidJaReeglid = @"Y = \{y \in \mathbb{R} \mid y = f(x)\}";
        res.MuutumispiirkondY.GeomeetrilineTahendus = "Graafiku vertikaalne ulatus.";
        res.MuutumispiirkondY.Sammud.Add($"Samm 1: Funktsiooni väärtused koonduvad horisontaalse asümptoodi ({horizStr}) poole.");
        res.MuutumispiirkondY.Sammud.Add("Samm 2: Arvestatakse leitud ekstreemumväärtusi ja katkestuskohti.");
        res.MuutumispiirkondY.Vaartus = "Vaata väärtuste vahemikke graafikult";
        res.MuutumispiirkondY.VaartusLatex = @"Y \subset \mathbb{R}";
    }

    private static void AnaluusiPolunoom(string norm, AnaluusiTulemus res)
    {
        var terms = PuhastaPolunoom(ParsiPolunoom(norm));
        int maxAste = terms.Keys.DefaultIfEmpty(0).Max();
        double aVal = terms.GetValueOrDefault(maxAste, 0);

        string pStr = PolunoomToString(terms);

        res.MaaramispiirkondX.Nimetus = "Määramispiirkond (X)";
        res.MaaramispiirkondX.MisSeeOn = "Argumendi x lubatud väärtuste hulk.";
        res.MaaramispiirkondX.ValemidJaReeglid = @"X = \mathbb{R} \quad\text{ehk}\quad x \in (-\infty; \infty)";
        res.MaaramispiirkondX.GeomeetrilineTahendus = "Graafik on katkematu joon kogu telje pikkuses.";
        res.MaaramispiirkondX.Sammud.Add("Samm 1: Funktsioonis puuduvad murrud (jagamised nulliga) ja ruutjuured.");
        res.MaaramispiirkondX.Sammud.Add("Samm 2: Polünoom on määratud kõigi reaalarvude korral: X: (-∞; ∞).");
        res.MaaramispiirkondX.Vaartus = "X: (-∞; ∞)";
        res.MaaramispiirkondX.VaartusLatex = @"X \in (-\infty; \infty)";

        double c = terms.GetValueOrDefault(0, 0);
        res.YLoikepunkt.Nimetus = "Y-telje lõikepunkt";
        res.YLoikepunkt.MisSeeOn = "Punkt, kus graafik läbib vertikaalset Y-telge kohal x = 0.";
        res.YLoikepunkt.ValemidJaReeglid = @"y = f(0)";
        res.YLoikepunkt.GeomeetrilineTahendus = "Lõikepunkt teljega x = 0.";
        res.YLoikepunkt.Sammud.Add("Samm 1: Asendame funktsiooni valemisse x = 0.");
        res.YLoikepunkt.Sammud.Add($"Samm 2: Kõik x-i sisaldavad liikmed muutuvad nulliks: f(0) = {c}.");
        res.YLoikepunkt.Sammud.Add($"Samm 3: Lõikepunkti koordinaadid on P(0; {c}).");
        res.YLoikepunkt.Vaartus = $"P(0; {c})";
        res.YLoikepunkt.VaartusLatex = $@"P(0;\; {c})";
        res.Markepunktid.Add((0, c, $"Y-lõikepunkt (0; {c})"));

        var d1 = PuhastaPolunoom(VotaTuletis(terms));
        res.Tuletis1 = PolunoomToString(d1);
        res.Tuletis1Latex = TeisendaLatexiks(res.Tuletis1);

        res.TuletisInfo.Nimetus = "Esimene tuletis f'(x)";
        res.TuletisInfo.MisSeeOn = "Funktsiooni hetkekiirus ja puutuja tõus.";
        res.TuletisInfo.ValemidJaReeglid = @"(c \cdot x^n)' = c \cdot n \cdot x^{n-1}";
        res.TuletisInfo.GeomeetrilineTahendus = "Puutuja tõus igas punktis.";
        res.TuletisInfo.Sammud.Add("Samm 1: Rakendame astmereeglit (a·xⁿ)' = a·n·xⁿ⁻¹ igale liikmele eraldi:");
        foreach (var (exp, coef) in terms.OrderByDescending(k => k.Key))
        {
            if (exp > 0)
                res.TuletisInfo.Sammud.Add($"• ({coef}·x^{exp})' = {coef * exp}·x^{exp - 1}");
            else
                res.TuletisInfo.Sammud.Add($"• Vabaliikme ({coef})' = 0");
        }
        res.TuletisInfo.Sammud.Add($"Samm 2: Tulemus on f'(x) = {res.Tuletis1}.");
        res.TuletisInfo.Vaartus = res.Tuletis1;
        res.TuletisInfo.VaartusLatex = res.Tuletis1Latex;

        var juured = LeiaPolunoomiJuured(terms);
        res.Nullkohad.Nimetus = "Nullkohad (X₀, f(x) = 0)";
        res.Nullkohad.MisSeeOn = "Punktid, kus funktsiooni kõrgus y = 0.";
        res.Nullkohad.ValemidJaReeglid = @"f(x) = 0";
        res.Nullkohad.GeomeetrilineTahendus = "Lõikepunktid X-teljega.";
        res.Nullkohad.Sammud.Add($"Samm 1: Seame funktsiooni võrduma nulliga: {pStr} = 0.");

        if (maxAste == 1)
        {
            double k = terms.GetValueOrDefault(1, 0);
            double bVal = terms.GetValueOrDefault(0, 0);
            double x0 = Math.Round(-bVal / k, 3);
            res.Nullkohad.Sammud.Add($"Samm 2: Lineaarvõrrand: {k}x + ({bVal}) = 0.");
            res.Nullkohad.Sammud.Add($"Samm 3: Viime vabaliikme teisele poole ja jagame kordajaga: x = {x0}.");
        }
        else if (maxAste == 2)
        {
            double a2 = terms.GetValueOrDefault(2, 0);
            double b2 = terms.GetValueOrDefault(1, 0);
            double c2 = terms.GetValueOrDefault(0, 0);
            double D = b2 * b2 - 4 * a2 * c2;
            res.Nullkohad.Sammud.Add($"Samm 2: Ruutvõrrand: {a2}x² + ({b2})x + ({c2}) = 0.");
            res.Nullkohad.Sammud.Add($"Samm 3: Diskriminant: D = b² - 4ac = ({b2})² - 4·({a2})·({c2}) = {D}.");
            if (D > 0)
            {
                double x1 = Math.Round((-b2 - Math.Sqrt(D)) / (2 * a2), 3);
                double x2 = Math.Round((-b2 + Math.Sqrt(D)) / (2 * a2), 3);
                res.Nullkohad.Sammud.Add($"Samm 4: D > 0, kaks lahendit: x₁ = {x1}, x₂ = {x2}.");
            }
            else if (Math.Abs(D) < 0.0001)
            {
                double x0 = Math.Round(-b2 / (2 * a2), 3);
                res.Nullkohad.Sammud.Add($"Samm 4: D = 0, üks kahekordne lahend: x = {x0}.");
            }
            else
            {
                res.Nullkohad.Sammud.Add("Samm 4: D < 0, reaalarvulised lahendid puuduvad.");
            }
        }
        res.Nullkohad.Vaartus = juured.Any() ? string.Join(", ", juured.Select((x, i) => $"x{i + 1} = {x}")) : "Puuduvad";
        res.Nullkohad.VaartusLatex = juured.Any() ? string.Join(@",\; ", juured.Select((x, i) => $@"x_{i + 1} = {x}")) : @"\emptyset";
        foreach (var r in juured)
            res.Markepunktid.Add((r, 0, $"Nullkoht ({r}; 0)"));

        var dJuured = LeiaPolunoomiJuured(d1);
        res.Ekstreemumid.Nimetus = "Ekstreemumid (haripunktid)";
        res.Ekstreemumid.MisSeeOn = "Funktsiooni haripunktid (maksimum või miinimum).";
        res.Ekstreemumid.ValemidJaReeglid = @"f'(x) = 0";
        res.Ekstreemumid.GeomeetrilineTahendus = "Puutuja tõus on 0.";
        res.Ekstreemumid.Sammud.Add($"Samm 1: Ekstreemumi leidmiseks seame tuletise nulliks: {res.Tuletis1} = 0.");

        if (dJuured.Any())
        {
            var eList = new List<string>();
            var eLatex = new List<string>();
            foreach (var xk in dJuured)
            {
                double yk = Math.Round(ArvutaVaartus(terms, xk), 3);
                string tyyp = aVal > 0 ? "min" : "max";
                res.Ekstreemumid.Sammud.Add($"Samm 2: Kriitiline punkt: x = {xk}. Väärtus: f({xk}) = {yk}.");
                res.Ekstreemumid.Sammud.Add($"Samm 3: Tegemist on lokaalse {tyyp}-punktiga ({xk}; {yk}).");
                eList.Add($"{tyyp} = {yk} (kui x = {xk})");
                eLatex.Add($@"\text{{{tyyp}}} = {yk}\;(x={xk})");
                res.Markepunktid.Add((xk, yk, $"{tyyp.ToUpper()} ({xk}; {yk})"));
            }
            res.Ekstreemumid.Vaartus = string.Join("; ", eList);
            res.Ekstreemumid.VaartusLatex = string.Join(@",\; ", eLatex);

            if (maxAste == 2)
            {
                double extrY = Math.Round(ArvutaVaartus(terms, dJuured[0]), 3);
                res.MuutumispiirkondY.Vaartus = aVal > 0 ? $"Y: [{extrY}; ∞)" : $"Y: (-∞; {extrY}]";
                res.MuutumispiirkondY.VaartusLatex = aVal > 0 ? $@"Y \in [{extrY}; \infty)" : $@"Y \in (-\infty; {extrY}]";
                res.MuutumispiirkondY.Sammud.Add($"Samm 1: Parabooli tipp asub kõrgusel y = {extrY}.");
                res.MuutumispiirkondY.Sammud.Add($"Samm 2: Muutumispiirkond on {res.MuutumispiirkondY.Vaartus}.");
            }
            else
            {
                res.MuutumispiirkondY.Vaartus = "Y: (-∞; ∞)";
                res.MuutumispiirkondY.VaartusLatex = @"Y \in (-\infty; \infty)";
                res.MuutumispiirkondY.Sammud.Add("Samm 1: Paaritu astme polünoomi muutumispiirkond on Y: (-∞; ∞).");
            }
        }
        else
        {
            res.Ekstreemumid.Vaartus = "Puuduvad";
            res.Ekstreemumid.VaartusLatex = @"\emptyset";
            res.Ekstreemumid.Sammud.Add("Samm 2: Tuletisel lahendid puuduvad, funktsioon on monotoonne.");

            res.MuutumispiirkondY.Vaartus = "Y: (-∞; ∞)";
            res.MuutumispiirkondY.VaartusLatex = @"Y \in (-\infty; \infty)";
            res.MuutumispiirkondY.Sammud.Add("Samm 1: Muutumispiirkond on kogu reaalarvude telg: Y: (-∞; ∞).");
        }

        res.MuutumispiirkondY.Nimetus = "Muutumispiirkond (Y)";
        res.MuutumispiirkondY.MisSeeOn = "Kõigi võimalike y väärtuste hulk.";
        res.MuutumispiirkondY.ValemidJaReeglid = @"Y = [y_{\min}; \infty) \text{ või } (-\infty; y_{\max}]";
        res.MuutumispiirkondY.GeomeetrilineTahendus = "Graafiku ulatus vertikaalsuunas.";

        res.HarudJaKumerus.Nimetus = "Harude suund ja kumerus";
        res.HarudJaKumerus.MisSeeOn = "Graafiku lõpmatute otste suund.";
        res.HarudJaKumerus.ValemidJaReeglid = @"a > 0 \implies \text{üles}, \quad a < 0 \implies \text{alla}";
        res.HarudJaKumerus.GeomeetrilineTahendus = "Parabooli avanemise suund.";
        if (maxAste % 2 == 0)
        {
            res.HarudJaKumerus.Vaartus = aVal > 0 ? "Harud suunatud ÜLES (+∞)" : "Harud suunatud ALLA (-∞)";
            res.HarudJaKumerus.VaartusLatex = aVal > 0 ? @"\text{Harud üles } (+\infty)" : @"\text{Harud alla } (-\infty)";
            res.HarudJaKumerus.Sammud.Add($"Samm 1: Polünoomi aste on paarisarv {maxAste}.");
            res.HarudJaKumerus.Sammud.Add($"Samm 2: Pealiikme kordaja on a = {aVal} {(aVal > 0 ? "> 0 (harud avanevad üles)" : "< 0 (harud avanevad alla)")}.");
        }
        else
        {
            res.HarudJaKumerus.Vaartus = aVal > 0 ? "Vasak haru ALLA (-∞), parem ÜLES (+∞)" : "Vasak haru ÜLES (+∞), parem ALLA (-∞)";
            res.HarudJaKumerus.VaartusLatex = aVal > 0 ? @"\text{Vasak alla, parem üles}" : @"\text{Vasak üles, parem alla}";
            res.HarudJaKumerus.Sammud.Add($"Samm 1: Aste on paaritu arv {maxAste}.");
            res.HarudJaKumerus.Sammud.Add("Samm 2: Harud kulgevad vastassuundadesse.");
        }

        bool isEven = terms.Keys.All(k => k % 2 == 0);
        bool isOdd = terms.Keys.All(k => k % 2 == 1);
        res.Paarsus.Nimetus = "Funktsiooni paarsus";
        res.Paarsus.MisSeeOn = "Sümmeetria Y-telje või alguspunkti suhtes.";
        res.Paarsus.ValemidJaReeglid = @"\text{Paaris: } f(-x) = f(x), \quad \text{Paaritu: } f(-x) = -f(x)";
        res.Paarsus.GeomeetrilineTahendus = "Peegelsümmeetria.";
        res.Paarsus.Sammud.Add("Samm 1: Uurime liikmete astmeid valemis.");
        if (isEven)
        {
            res.Paarsus.Vaartus = "PAARISFUNKTSIOON (sümmeetriline Y-telje suhtes)";
            res.Paarsus.VaartusLatex = @"\text{Paaris } (f(-x) = f(x))";
            res.Paarsus.Sammud.Add("Samm 2: Kõik astmed on paarisarvud: f(-x) = f(x).");
        }
        else if (isOdd)
        {
            res.Paarsus.Vaartus = "PAARITU FUNKTSIOON (sümmeetriline alguspunkti suhtes)";
            res.Paarsus.VaartusLatex = @"\text{Paaritu } (f(-x) = -f(x))";
            res.Paarsus.Sammud.Add("Samm 2: Kõik astmed on paaritud arvud: f(-x) = -f(x).");
        }
        else
        {
            res.Paarsus.Vaartus = "EI OLE PAARIS EGA PAARITU (üldkuju)";
            res.Paarsus.VaartusLatex = @"\text{Ei ole paaris ega paaritu}";
            res.Paarsus.Sammud.Add("Samm 2: Valemis on segatüüpi astmed, sümmeetria puudub.");
        }

        res.Margipiirkonnad.Nimetus = "Märgitüsivuse piirkonnad (X⁺ ja X⁻)";
        res.Margipiirkonnad.MisSeeOn = "Asend teljest kõrgemal (X⁺) või madalamal (X⁻).";
        res.Margipiirkonnad.ValemidJaReeglid = @"f(x) > 0 \text{ vs } f(x) < 0";
        res.Margipiirkonnad.GeomeetrilineTahendus = "Telje ületuskohad.";
        res.Margipiirkonnad.Sammud.Add("Samm 1: Piirpunktideks on leitud nullkohad.");
        if (juured.Any())
        {
            res.Margipiirkonnad.Sammud.Add($"Samm 2: Piirpunktid on: {string.Join(", ", juured)}.");
        }
        else
        {
            res.Margipiirkonnad.Sammud.Add($"Samm 2: Nullkohti pole, märk on ühtlane {(aVal > 0 ? "(X⁺)" : "(X⁻)")}.");
        }
        res.Margipiirkonnad.Vaartus = juured.Any() ? $"Eralduspunktid: {string.Join("; ", juured)}" : (aVal > 0 ? "Kogu ulatuses X⁺" : "Kogu ulatuses X⁻");
        res.Margipiirkonnad.VaartusLatex = juured.Any() ? $@"x \in \{{{string.Join(@";\; ", juured)}\}}" : @"\text{Püsiv märk}";
    }

    private static Dictionary<int, double> PuhastaPolunoom(Dictionary<int, double> terms)
    {
        return terms.Where(kv => Math.Abs(kv.Value) > 0.00001).ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    private static (string Lugeja, string Nimetaja) EraldaLugejaJaNimetaja(string valem)
    {
        int slash = valem.IndexOf('/');
        string l = valem.Substring(0, slash).Trim('(', ')');
        string n = valem.Substring(slash + 1).Trim('(', ')');
        return (l, n);
    }

    private static List<double> LeiaPolunoomiJuured(Dictionary<int, double> rawTerms)
    {
        var terms = PuhastaPolunoom(rawTerms);
        var juured = new List<double>();
        int maxAste = terms.Keys.DefaultIfEmpty(0).Max();

        if (maxAste == 1)
        {
            double k = terms.GetValueOrDefault(1, 0);
            double bVal = terms.GetValueOrDefault(0, 0);
            if (Math.Abs(k) > 0.00001) juured.Add(Math.Round(-bVal / k, 3));
        }
        else if (maxAste == 2)
        {
            double aVal = terms.GetValueOrDefault(2, 0);
            double bVal = terms.GetValueOrDefault(1, 0);
            double cVal = terms.GetValueOrDefault(0, 0);
            double D = bVal * bVal - 4 * aVal * cVal;

            if (D > 0)
            {
                double x1 = Math.Round((-bVal - Math.Sqrt(D)) / (2 * aVal), 3);
                double x2 = Math.Round((-bVal + Math.Sqrt(D)) / (2 * aVal), 3);
                juured.Add(Math.Min(x1, x2));
                juured.Add(Math.Max(x1, x2));
            }
            else if (Math.Abs(D) < 0.0001)
            {
                juured.Add(Math.Round(-bVal / (2 * aVal), 3));
            }
        }
        return juured;
    }

    private static Dictionary<int, double> KorrutaPolunoomid(Dictionary<int, double> p1, Dictionary<int, double> p2)
    {
        var res = new Dictionary<int, double>();
        foreach (var (e1, c1) in p1)
            foreach (var (e2, c2) in p2)
            {
                int exp = e1 + e2;
                res[exp] = (res.ContainsKey(exp) ? res[exp] : 0) + (c1 * c2);
            }
        return res;
    }

    private static Dictionary<int, double> LahutaPolunoomid(Dictionary<int, double> p1, Dictionary<int, double> p2)
    {
        var res = new Dictionary<int, double>(p1);
        foreach (var (e2, c2) in p2)
            res[e2] = (res.ContainsKey(e2) ? res[e2] : 0) - c2;
        return res;
    }

    private static double ArvutaMurdVaartus(Dictionary<int, double> l, Dictionary<int, double> n, double x)
    {
        double yL = ArvutaVaartus(l, x);
        double yN = ArvutaVaartus(n, x);
        return Math.Abs(yN) > 0.0001 ? Math.Round(yL / yN, 3) : 0;
    }

    private static double ArvutaVaartus(Dictionary<int, double> terms, double x)
    {
        double sum = 0;
        foreach (var (exp, coef) in terms)
            sum += coef * Math.Pow(x, exp);
        return sum;
    }

    private static Dictionary<int, double> VotaTuletis(Dictionary<int, double> terms)
    {
        var res = new Dictionary<int, double>();
        foreach (var (exp, coef) in terms)
        {
            if (exp > 0)
            {
                int uuse = exp - 1;
                double uusk = coef * exp;
                res[uuse] = (res.ContainsKey(uuse) ? res[uuse] : 0) + uusk;
            }
        }
        return res;
    }

    private static Dictionary<int, double> ParsiPolunoom(string valem)
    {
        var dict = new Dictionary<int, double>();
        string s = Normaliseeri(valem).Replace("-", "+-");
        var parts = s.Split('+', StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            var match = Regex.Match(part, @"^([+-]?\d*(?:\.\d+)?)?\*?x(?:\^(\d+))?$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                string cStr = match.Groups[1].Value;
                double cVal = 1.0;
                if (cStr == "-") cVal = -1.0;
                else if (!string.IsNullOrEmpty(cStr)) cVal = double.Parse(cStr, CultureInfo.InvariantCulture);

                int exp = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 1;
                dict[exp] = (dict.ContainsKey(exp) ? dict[exp] : 0) + cVal;
            }
            else if (double.TryParse(part, NumberStyles.Any, CultureInfo.InvariantCulture, out double num))
            {
                dict[0] = (dict.ContainsKey(0) ? dict[0] : 0) + num;
            }
        }
        return dict;
    }

    private static string PolunoomToString(Dictionary<int, double> terms)
    {
        var list = new List<string>();
        foreach (var exp in terms.Keys.OrderByDescending(k => k))
        {
            double cVal = terms[exp];
            if (Math.Abs(cVal) < 0.00001) continue;

            string s = exp switch
            {
                0 => $"{cVal}",
                1 => cVal == 1 ? "x" : cVal == -1 ? "-x" : $"{cVal}*x",
                _ => cVal == 1 ? $"x^{exp}" : cVal == -1 ? $"-x^{exp}" : $"{cVal}*x^{exp}"
            };
            list.Add(s);
        }
        if (!list.Any()) return "0";
        return string.Join(" + ", list).Replace("+ -", "- ");
    }

    private static Expression LooExpression(string valem)
    {
        string prepared = MuudaAstmed(valem.Trim());
        prepared = Regex.Replace(prepared, @"(?<![a-zA-Z0-9_\.])(\d+)(?!\.\d)", "$1.0");
        return new Expression(prepared);
    }

    private static string MuudaAstmed(string s)
    {
        int i;
        while ((i = s.LastIndexOf('^')) >= 0)
        {
            int aIdx = i - 1;
            while (aIdx >= 0 && s[aIdx] == ' ') aIdx--;
            int algus;
            if (aIdx >= 0 && s[aIdx] == ')')
            {
                int sygavus = 0, j = aIdx;
                for (; j >= 0; j--)
                {
                    if (s[j] == ')') sygavus++;
                    else if (s[j] == '(') { sygavus--; if (sygavus == 0) break; }
                }
                algus = j;
                while (algus > 0 && (char.IsLetterOrDigit(s[algus - 1]) || s[algus - 1] == '_')) algus--;
            }
            else
            {
                int j = aIdx;
                while (j >= 0 && (char.IsLetterOrDigit(s[j]) || s[j] == '.' || s[j] == '_')) j--;
                algus = j + 1;
            }
            string alus = s.Substring(algus, aIdx - algus + 1);

            int bIdx = i + 1;
            while (bIdx < s.Length && s[bIdx] == ' ') bIdx++;
            int astendajaAlgus = bIdx;
            if (bIdx < s.Length && (s[bIdx] == '-' || s[bIdx] == '+')) bIdx++;
            while (bIdx < s.Length && (char.IsLetterOrDigit(s[bIdx]) || s[bIdx] == '.' || s[bIdx] == '_')) bIdx++;
            string astendaja = s.Substring(astendajaAlgus, bIdx - astendajaAlgus);

            s = s.Substring(0, algus) + "Pow(" + alus + "," + astendaja + ")" + s.Substring(bIdx);
        }
        return s;
    }
}