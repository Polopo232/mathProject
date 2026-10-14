# 📈 mathProject — funktsiooni uurimise veebirakendus

Sisesta valem, näiteks `x^3 - 3x`, ja rakendus uurib funktsiooni sinu eest täielikult läbi: leiab määramispiirkonna, nullkohad, tuletise, ekstreemumid, asümptoodid, paarsuse ja märgipiirkonnad. Iga tulemuse juures on **samm-sammuline lahenduskäik** ning interaktiivne graafik.

Projekt on loodud matemaatika õppimise abivahendiks: see ei anna ainult vastust, vaid selgitab ka, **kuidas** vastuseni jõuti.

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET_Core-MVC-512BD4)
![SQLite](https://img.shields.io/badge/SQLite-EF_Core-003B57?logo=sqlite)
![Chart.js](https://img.shields.io/badge/Chart.js-4.4-FF6384?logo=chartdotjs)

---

## ✨ Võimalused

- **Automaatne analüüs** — valemi sisestamisel arvutatakse tuletis, määramispiirkond, kriitilised punktid ja ekstreemumid otse vormis (AJAX-päringuga, lehte uuesti laadimata).
- **Täielik funktsiooni uurimine** — iga omaduse kohta kuvatakse:
  - mis see on,
  - kasutatud valemid ja reeglid,
  - lahenduse sammud,
  - geomeetriline tähendus graafikul.
- **Interaktiivne graafik** — funktsiooni ja tuletise graafik koos märgitud eripunktidega (nullkohad, ekstreemumid, lõikepunktid). Graafikut saab suumida ja nihutada.
- **Ilus matemaatiline kuju** — valemid kuvatakse LaTeX-vormingus (KaTeX).
- **Valemi kontroll** — vigase süntaksi korral näitab vorm arusaadavat veateadet.
- **Ajalugu** — kõik uuritud funktsioonid salvestatakse andmebaasi; neid saab vaadata, muuta ja kustutada.

## 🧮 Toetatud funktsioonid

| Tüüp | Näide | Mida analüüsitakse |
|---|---|---|
| **Polünoom** | `x^2 - 4`, `x^3 - 3x`, `2x + 1` | nullkohad, tuletis, ekstreemumid, harude suund ja kumerus, paarsus, märgipiirkonnad, muutumispiirkond |
| **Murdfunktsioon** | `(x+1)/(x-2)`, `1/x` | lisaks katkevuskohad, vertikaalsed ja horisontaalsed asümptoodid |
| **Eksponentfunktsioon** | `e^x` | määramis- ja muutumispiirkond, tuletis, asümptoot |

**Sisestamise nipid:**
- astendamine: `x^2`
- korrutusmärgi võib ära jätta: `3x` → `3*x`, `2(x+1)` → `2*(x+1)`
- `e` tähendab Euleri arvu (≈ 2,71828)
- murru lugeja ja nimetaja pane sulgudesse: `(x^2-1)/(x+3)`

> ℹ️ Nullkohti leitakse analüütiliselt kuni teise astme polünoomideni (lineaar- ja ruutvõrrand). Seetõttu on ekstreemumid täpselt leitavad kuni kolmanda astme polünoomidel.

## 🛠 Tehnoloogiad

| Kiht | Tehnoloogia |
|---|---|
| Backend | ASP.NET Core MVC (.NET 10), C# |
| Andmebaas | SQLite + Entity Framework Core 10 |
| Avaldiste arvutamine | [NCalc](https://github.com/ncalc/ncalc) |
| Graafikud | Chart.js 4 + chartjs-plugin-zoom |
| Valemite kuvamine | KaTeX |
| Kujundus | Bootstrap 5, jQuery Validation |

## 🚀 Käivitamine

### Eeldused
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- (valikuline) EF Core tööriist migratsioonide jaoks:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

### Sammud

```bash
# 1. Klooni repositoorium
git clone https://github.com/Polopo232/mathProject.git
cd mathProject

# 2. Taasta sõltuvused
dotnet restore

# 3. Loo / uuenda andmebaas (vajalik, kui funktsioonid.db puudub)
dotnet ef database update

# 4. Käivita rakendus
dotnet run
```

Ava brauseris **http://localhost:5283** (või HTTPS-profiiliga https://localhost:7181).

Visual Studios piisab, kui avad faili `mathProject.slnx` ja vajutad **F5**.

## 🗺 Kasutamine

1. Avaleht avab kohe vormi **„Uus funktsioon"**.
2. Sisesta valem, näiteks `x^3 - 3x`. Tuletis, määramispiirkond, kriitilised punktid ja ekstreemumid täituvad automaatselt.
3. Vajuta **Salvesta** — avaneb detailvaade täieliku analüüsi ja graafikuga.
4. Kõiki varem uuritud funktsioone näed lehel **Funktsioonid → Index**.

### Marsruudid

| URL | Kirjeldus |
|---|---|
| `/Funktsioonid/Create` | uue funktsiooni sisestamine (avaleht) |
| `/Funktsioonid/Index` | kõigi salvestatud funktsioonide nimekiri |
| `/Funktsioonid/Details/{id}` | täielik analüüs ja graafik |
| `/Funktsioonid/Edit/{id}` | kirje muutmine |
| `/Funktsioonid/AutoCalculate?valem=...` | JSON-vastus kiiranalüüsiga |

**API näide:**
```http
GET /Funktsioonid/AutoCalculate?valem=x^2-4
```
```json
{
  "tuletis": "2x",
  "maaramispiirkond": "X: (-∞; ∞)",
  "kriitilisedPunktid": "x₁ = -2, x₂ = 2",
  "ekstreemumid": "min: (0; -4)"
}
```
*(Täpne tekstikuju sõltub funktsioonist.)*

## 📁 Projekti struktuur

```
mathProject/
├── Controllers/
│   ├── FunktsioonidController.cs   # CRUD + AutoCalculate API
│   └── HomeController.cs
├── Services/
│   └── FunktsiooniArvutaja.cs      # kogu matemaatiline analüüs
├── Models/
│   └── FunktsiooniUurimine.cs      # andmebaasi olem
├── Data/
│   └── AppDbContext.cs             # EF Core kontekst
├── Migrations/                     # andmebaasi migratsioonid
├── Views/
│   └── Funktsioonid/               # Create, Edit, Details, Index, _Vorm
├── wwwroot/                        # CSS, JS, teegid
├── funktsioonid.db                 # SQLite andmebaas
└── Program.cs
```

### Kuidas analüüs töötab

`FunktsiooniArvutaja.Analuusi()` tuvastab valemi tüübi ja suunab selle õigesse analüsaatorisse:

```
valem ──► normaliseerimine (3x → 3*x, e → 2.718…)
            │
            ├── sisaldab e^x  ──► AnaluusiEksponent()
            ├── sisaldab „/"  ──► AnaluusiMurdfunktsioon()
            └── muu           ──► AnaluusiPolunoom()
                                      │
                                      ▼
                          AnaluusiTulemus
                (omadused + sammud + LaTeX + graafiku punktid)
```

Polünoom parsitakse kujule `{aste → kordaja}`, millest arvutatakse tuletis, juured (lineaar- ja ruutvõrrandi valemiga) ning eripunktid. Valemi õigsust kontrollitakse NCalc-teegi abil.

## 💾 Andmemudel

| Väli | Tüüp | Näide |
|---|---|---|
| `Id` | int | `1` |
| `Valem` | string | `x^3 - 3*x` |
| `Maaramispiirkond` | string | `X: (-∞; ∞)` |
| `Tuletis` | string | `3x^2 - 3` |
| `KriitilisedPunktid` | string | `x₁ = -1, x₂ = 1` |
| `Ekstreemumid` | string | `max = 2, min = -2` |
| `LuodudAeg` | DateTime | loomise aeg (UTC) |

Ühendusstringi saab määrata failis `appsettings.json` (`ConnectionStrings:DefaultConnection`); vaikimisi kasutatakse faili `funktsioonid.db`.

## 🔮 Edasiarendused

- [ ] kõrgema astme polünoomide juurte numbriline leidmine (nt Newtoni meetod)
- [ ] trigonomeetrilised ja logaritmfunktsioonid
- [ ] teine tuletis ja käänupunktid
- [ ] analüüsi eksport PDF-i
- [ ] ühiktestid `FunktsiooniArvutaja` jaoks

## 👤 Autor

**Polopo232** — [github.com/Polopo232](https://github.com/Polopo232)
