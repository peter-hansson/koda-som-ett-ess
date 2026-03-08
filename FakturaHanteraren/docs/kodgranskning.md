# FakturaHanteraren – Kodgranskning och Dokumentation

> **Syfte:** Fullständig genomgång av koden med fokus på säkerhet, testbarhet, best practice och arkitektur. Används som underlag för refaktorering och förbättringsarbete.

---

## 1. Systembeskrivning

FakturaHanteraren är ett konsolbaserat fakturahanteringssystem skrivet i C# (.NET 8) med SQLite som databas. Systemet hanterar kunder, produkter, fakturor, betalningar och påminnelser via en textbaserad meny.

**Ingångspunkter:**
- `Main()` startar applikationen, öppnar databas, autentiserar användaren och kör huvudmenyn

**Inloggning:**
| Användare | Lösenord  | Roll          |
|-----------|-----------|---------------|
| admin     | admin123  | Admin         |
| anna      | anna123   | Handläggare   |
| erik      | erik123   | Läsare        |

---

## 2. Filstruktur

```
FakturaHanteraren/
├── Program.cs                  # All kod – 918 rader i en enda klass
├── FakturaHanteraren.csproj    # .NET 8, Microsoft.Data.Sqlite 8.0.10
├── FakturaHanteraren.sln
├── README.md                   # Workshop-instruktioner
├── docs/
│   └── kodgranskning.md        # Detta dokument
└── .vscode/
    ├── launch.json
    └── tasks.json
```

---

## 3. Databasschema

Databasen skapas automatiskt vid första körning (`FixaDB()`). Testdata laddas in om databasen är tom (`SeedaOmTom()`).

### Tabeller

#### `Användare`
| Kolumn   | Typ     | Notering                        |
|----------|---------|---------------------------------|
| Id       | INTEGER | PK                              |
| Namn     | TEXT    | Användarnamn                    |
| Lösenord | TEXT    | **Klartext – kritisk säkerhetsrisk** |
| Roll     | TEXT    | Admin / Handläggare / Läsare    |
| Email    | TEXT    |                                 |
| Skapad   | TEXT    | ISO-datumstring                 |

#### `Kunder`
| Kolumn    | Typ     | Notering                             |
|-----------|---------|--------------------------------------|
| Id        | INTEGER | PK                                   |
| Namn      | TEXT    |                                      |
| OrgNr     | TEXT    | Org.nr eller personnummer            |
| Adress    | TEXT    |                                      |
| Postnr    | TEXT    |                                      |
| Ort       | TEXT    |                                      |
| Email     | TEXT    |                                      |
| Tele      | TEXT    |                                      |
| KundTyp   | INTEGER | 0=Privat, 1=Företag, 2=Kommun        |
| Rabatt    | REAL    | Procent, t.ex. 5.0 = 5%             |
| Aktiv     | INTEGER | 1=Aktiv, 0=Inaktiverad              |
| Skapad    | TEXT    |                                      |
| Uppdaterad| TEXT    |                                      |

#### `Produkter`
| Kolumn      | Typ     | Notering                      |
|-------------|---------|-------------------------------|
| Id          | INTEGER | PK                            |
| Namn        | TEXT    |                               |
| Beskrivning | TEXT    |                               |
| Pris        | REAL    | Pris exkl. moms               |
| Enhet       | TEXT    | t.ex. "tim", "st", "mån"     |
| MomsKod     | INTEGER | Alltid 1 i nuläget            |
| Kategori    | TEXT    |                               |
| Aktiv       | INTEGER | 1=Aktiv                       |
| LagerSaldo  | INTEGER | 0 = ingen lagerhantering      |

#### `Fakturor`
| Kolumn       | Typ     | Notering                                     |
|--------------|---------|----------------------------------------------|
| Id           | INTEGER | PK                                           |
| KundId       | INTEGER | FK → Kunder                                  |
| FakturaNr    | TEXT    | Format: FAK-YYYY-NNNN / KRED-YYYY-NNNN      |
| Datum        | TEXT    | Fakturadatum (ISO-format)                    |
| Förfallodatum| TEXT    | Beräknas vid skapande                        |
| Status       | INTEGER | 0=Ny, 1=Skickad, 2=Betald, 3=Krediterad     |
| Totalt       | REAL    | Inkl. moms och eventuella påminnelseavgifter |
| Moms         | REAL    | Momsbelopp                                   |
| Rabatt       | REAL    | Kundrabatt vid fakturatillfället             |
| Notering     | TEXT    |                                              |
| SkapadAv     | INTEGER | FK → Användare                               |
| Betald       | TEXT    | Betalningsdatum                              |

#### `FakturaRader`
| Kolumn     | Typ     | Notering       |
|------------|---------|----------------|
| Id         | INTEGER | PK             |
| FakturaId  | INTEGER | FK → Fakturor  |
| ProduktId  | INTEGER | FK → Produkter |
| Antal      | REAL    |                |
| ÅPris      | REAL    | À-pris         |
| Rabatt     | REAL    | Radrabatt %    |
| Summa      | REAL    | Antal × Pris × (1 - Rabatt%) |

#### `Betalningar`
| Kolumn    | Typ     | Notering                        |
|-----------|---------|---------------------------------|
| Id        | INTEGER | PK                              |
| FakturaId | INTEGER | FK → Fakturor                   |
| Belopp    | REAL    |                                 |
| Datum     | TEXT    |                                 |
| Metod     | TEXT    | "Bank" / "Swish" / "Kort"      |
| Referens  | TEXT    |                                 |

#### `Påminnelser`
| Kolumn    | Typ     | Notering                                    |
|-----------|---------|---------------------------------------------|
| Id        | INTEGER | PK                                          |
| FakturaId | INTEGER | FK → Fakturor                               |
| Datum     | TEXT    |                                             |
| Typ       | INTEGER | 1=Första, 2=Andra, 3=Inkassovarning         |
| Avgift    | REAL    | 60 / 180 / 450 kr                           |

---

## 4. Affärslogik och flöden

### Fakturaflöde

```
Ny faktura (Status=0)
    → Skickad (Status=1)   [manuell statusändring saknas i koden!]
    → Betald (Status=2)    [automatiskt när betalningar täcker totalt]
    → Krediterad (Status=3) [admin skapar kreditfaktura med negativt belopp]
```

### Beräkningsregler

- **Radbelopp:** `Antal × ÅPris × (1 - Rabatt% / 100)`
- **Moms:** `Summa exkl. moms × 0.25` (konfigurerbar men hårdkodad som default)
- **Förfallodatum:** Fakturadatum + 30 dagar (60 dagar för kommuner, `KundTyp=2`)
- **Delfakturering:** Stöds – faktura markeras betald när `ΣBetalningar ≥ Totalt - 0.01`

### Påminnelsetrappan

| Nivå | Typ             | Avgift | Bestäms av antal tidigare påminnelser |
|------|-----------------|--------|---------------------------------------|
| 1    | Första påminnelse | 60 kr  | AntPåm = 0                           |
| 2    | Andra påminnelse  | 180 kr | AntPåm = 1                           |
| 3+   | Inkassovarning    | 450 kr | AntPåm ≥ 2                           |

Avgiften läggs på fakturans `Totalt` direkt i databasen.

### Lagerhantering

- Produkter med `LagerSaldo > 0` har lagerhantering
- Vid fakturering kontrolleras att `Antal ≤ LagerSaldo`
- Lagret minskas direkt när fakturaraden skapas (ingen reservation)

### Kundstatistik

Hämtar per kund: antal fakturor, totalt fakturerat, varav betalt, och utestående belopp.

---

## 5. Behörighetsmodell

| Funktion                    | Admin | Handläggare | Läsare |
|-----------------------------|:-----:|:-----------:|:------:|
| Lista/visa kunder           | ✓     | ✓           | ✓      |
| Söka kunder                 | ✓     | ✓           | ✓      |
| Skapa kund                  | ✓     | ✓           | ✗      |
| Redigera kund               | ✓     | ✓           | ✗      |
| Inaktivera kund             | ✓     | ✗           | ✗      |
| Lista/visa produkter        | ✓     | ✓           | ✓      |
| Skapa produkt               | ✓     | ✓           | ✗      |
| Uppdatera pris              | ✓     | ✓           | ✗      |
| Lista/visa fakturor         | ✓     | ✓           | ✓      |
| Skapa faktura               | ✓     | ✓           | ✗      |
| Kreditera faktura           | ✓     | ✗           | ✗      |
| Registrera betalning        | ✓     | ✓           | ✗      |
| Skicka påminnelser          | ✓     | ✓           | ✗      |
| Se rapporter                | ✓     | ✓           | ✓      |
| Inställningar               | ✓     | ✗           | ✗      |
| Hantera användare           | ✓     | ✗           | ✗      |
| Se logg                     | ✓     | ✓           | ✓      |

---

## 6. Säkerhetsbrister (kritiska)

### 🔴 SQL-injektion – utbredd och kritisk

Praktiskt taget alla användarinmatningar sätts in direkt i SQL-strängar via stränginterpolation. Detta gör att en angripare kan manipulera databasfrågor.

**Exempel – inloggning kan kringgås helt (rad 36):**
```csharp
cmd.CommandText = $"SELECT Id, Roll FROM Användare WHERE Namn='{u}' AND Lösenord='{p}'";
```
Inmatning `admin' OR '1'='1' --` som användarnamn ger åtkomst utan lösenord.

**Alla drabbade platser:**
| Rad  | Funktion       | Indata               |
|------|----------------|----------------------|
| 36   | Inloggning     | u (Namn), p (Lösen)  |
| 192  | Kundsökning    | s (sökterm)          |
| 210  | Ny kund        | n, o, a, pn, ort, e, t, kt, rab |
| 220  | Redigera kund  | id                   |
| 241  | Redigera kund  | n, a, o, e, rab      |
| 255  | Inaktivera kund| id                   |
| 268  | Kundstatistik  | id                   |
| 382  | Ny faktura     | kid                  |
| 404  | Ny faktura     | not (Notering)       |
| 417  | Ny faktura     | pid                  |
| 480  | Kreditera      | id                   |
| 533  | Visa faktura   | id                   |
| 618  | Betalning      | fid                  |
| 630  | Betalning      | fid                  |
| 640  | Betalning      | met, reff            |
| 664  | Betalhist.     | id                   |
| 682  | Påminnelser    | (indirekt via queries)|
| 894  | Ny användare   | n, p, roll, e        |

**Åtgärd:** Använd parametriserade frågor för samtliga databasanrop:
```csharp
// Fel (nuläget):
cmd.CommandText = $"SELECT * FROM Kunder WHERE Id={id}";

// Rätt:
cmd.CommandText = "SELECT * FROM Kunder WHERE Id=@id";
cmd.Parameters.AddWithValue("@id", id);
```

---

### 🔴 Lösenord i klartext

Lösenord lagras och jämförs i klartext. Ett databas-läckage exponerar alla användares lösenord direkt.

**Berörda rader:** 98–100 (seed-data), 36 (inloggning), 894 (ny användare).

**Åtgärd:** Använd `BCrypt.Net-Next` eller `Microsoft.AspNetCore.Cryptography.KeyDerivation` för att hasha lösenord med salt:
```csharp
// Spara:
var hash = BCrypt.Net.BCrypt.HashPassword(lösenord);

// Verifiera:
var ok = BCrypt.Net.BCrypt.Verify(lösenord, hashFrånDB);
```

---

### 🟡 Ingen indata-validering

Ingen validering sker av inmatad data. Konsekvenser:

- `decimal.Parse(antal!)` (rad 427) kastar undantag om användaren skriver text
- `int.Parse(id!)` (rad 473, 874) kastar undantag vid felaktig inmatning
- Negativa belopp accepteras utan kontroll
- Ogiltiga roller kan anges vid skapande av användare (rad 891–892)
- E-postadresser valideras inte

---

### 🟡 Behörighetskontroll – inkonsekvent

I `KundMeny()` (rad 163–164) visas en varning till Läsare men menyn visas ändå och körningen fortsätter. Behörighetskontrollen i de enskilda valen (rad 199, 217 osv.) fungerar korrekt, men strukturen är förvirrande och felbenägen.

```csharp
// Rad 163 – saknar return:
if (_roll == "Läsare") { Console.WriteLine("Ej behörighet att hantera kunder i skrivläge."); }
// Menyn visas ändå!
```

---

### 🟡 Inget HTTPS/kryptering vid transport

Systemet är ett lokalt konsolprogram utan nätverkskommunikation, men om det byggas ut med nätverksåtkomst saknas kryptering.

---

## 7. Kodkvalitetsbrister

### 🔴 Monolitisk design – allt i en klass

Alla 918 rader finns i `Program.cs` i en enda klass utan separation av ansvar. Konsekvenser:

- Omöjligt att enhetstesta
- Svårt att förstå och underhålla
- Ändringar i en del kan bryta andra delar
- Koden kan inte återanvändas

**Rekommenderad arkitektur (3-lagersmodell):**
```
Presentationslager   → Konsol-UI, menyhantering
Tjänstelager         → Affärslogik, validering
Datalager            → Repository-pattern, SQL-frågor
```

---

### 🔴 Magiska tal och strängar

Numeriska statusvärden och typkoder används direkt utan namngivna konstanter eller enum:

```csharp
// Nuläget – vad betyder 2? 3?
var stat = i < 8 ? 2 : (i < 11 ? 1 : 0);
if (r.GetInt32(5) == 2) ...

// Bättre – tydlig intent:
enum FakturaStatus { Ny = 0, Skickad = 1, Betald = 2, Krediterad = 3 }
enum KundTyp { Privat = 0, Företag = 1, Kommun = 2 }
```

Hårdkodade avgifter (rad 695):
```csharp
var avgift = typ == 1 ? 60.0 : (typ == 2 ? 180.0 : 450.0);
```
Bör vara namngivna konstanter eller konfigurerbara värden.

---

### 🔴 Global mutable state

Åtta statiska variabler används som global state:

```csharp
static SqliteConnection? _c;          // Databasen
static int _usr = -1;                 // Inloggad användare
static string _roll = "";             // Användarens roll
static decimal _moms = 0.25m;         // Momssats
static bool _kör = true;              // Körflagga
static List<string> _logg = new();    // Revisionlogg (förloras vid omstart!)
static Dictionary<int, decimal> _cache = new(); // Priscache – ALDRIG LÄST
static int _maxFörfallodagar = 30;    // Konfiguration
static string _dbPath = "fakturor.db";
```

Problem:
- Svårt att testa (kan inte isoleras)
- `_logg` förloras vid omstart
- `_cache` är helt oanvänd (värden skrivs men läses aldrig)
- Databasen hanteras utan `using`-statement

---

### 🟡 Inga undantagshantering

Applikationen har noll `try-catch`-block. Vid felaktig inmatning eller databasfel kraschar programmet med en ful stack trace.

**Vanliga kraschpunkter:**
- Rad 427: `decimal.Parse(antal!)` – ogiltigt tal
- Rad 473: `int.Parse(id!)` – ogiltigt tal
- Rad 734–735: `int.Parse(mån!)`, `int.Parse(år!)` – ogiltigt tal/månad
- Rad 874: `int.Parse(Console.ReadLine()!)` – ogiltigt tal
- Rad 866: `decimal.Parse(m!)` – ogiltigt tal

---

### 🟡 Blandning av svenska och engelska

Variabelnamn och kommentarer blandar svenska och engelska utan konsekvent stil:
- `_maxFörfallodagar` (svenska), `_cache` (engelska), `tot` (förkortning), `kid` (customer ID)
- Kommentarer: "Kolla om kund finns och är aktiv" (svenska), kommentaren `// Kolla lager om produkt har lager` (svenska)

---

### 🟡 Resurser stängs inte korrekt

`SqliteConnection` och `SqliteCommand` implementerar `IDisposable` men hanteras inte med `using`:

```csharp
// Nuläget (kan läcka resurser):
var cmd = _c!.CreateCommand();
cmd.CommandText = "...";
var r = cmd.ExecuteReader();

// Bättre:
using var cmd = _c!.CreateCommand();
cmd.CommandText = "...";
using var r = cmd.ExecuteReader();
```

---

### 🟡 Inkonsekvent namngivning

- Klassen heter `Program` men bör brytas upp
- Metoder: `KundMeny()`, `BetMeny()`, `Inst()`, `Påminn()` – oklara förkortningar
- Variabler: `_c`, `_usr`, `_roll`, `_kör` – enkelbokstäver och förkortningar

---

### 🟡 Fakturanummergenerering är osäker

```csharp
// Rad 392-395:
nrc.CommandText = "SELECT MAX(Id) FROM Fakturor";
var maxId = nrc.ExecuteScalar();
var nextNr = maxId == DBNull.Value ? 1 : (int)(long)maxId + 1;
var fakNr = $"FAK-{DateTime.Now.Year}-{nextNr:D4}";
```

Problem: Race condition – om två användare skapar faktura samtidigt kan samma nummer genereras. Bör använda `AUTOINCREMENT` eller en sekvenstabbell.

---

### 🟡 Logg försvinner vid omstart

`_logg` är en in-memory lista. All revisionspårning försvinner när applikationen avslutas. Ingen persistent loggning finns.

---

### 🟡 Statustransitioner saknas delvis

Det finns inget sätt att manuellt markera en faktura som "Skickad" (Status=1) via menyn. Status 1 sätts bara i seed-data.

---

### 🟢 Vad som fungerar bra

- Affärslogiken är i huvudsak korrekt
- Delfakturering hanteras (`Σbetalningar ≥ Totalt - 0.01`)
- Kommunspeciella förfallodagar (60 dagar)
- Inaktiveringsskydd: kund med obetalda fakturor kan inte inaktiveras
- Databas-VACUUM via inställningar
- Lagerhantering med varning vid lågt saldo

---

## 8. Testbarhet

Koden är i princip **omöjlig att enhetstesta** i nuläget:

| Hinder                  | Konsekvens                                             |
|-------------------------|--------------------------------------------------------|
| Global statisk databas  | Kan inte mockas eller ersättas                         |
| Konsol-I/O i affärslogik| `Console.ReadLine()` i mitten av beräkningar          |
| Inga interfaces         | Inget att mocka                                        |
| Ingen dependency injection | Beroenden är hårdkodade                             |
| Allt i en metod         | Kan inte testa delar isolerat                          |

**För att göra koden testbar krävs:**

1. Separera UI från affärslogik
2. Introducera interfaces (`IKundRepository`, `IFakturaService` etc.)
3. Injicera beroenden via konstruktor
4. Ersätt `Console`-anrop med abstraktioner

---

## 9. Förbättringsförslag – prioriterat

### Prioritet 1 – Säkerhet (gör direkt)

1. **Parametriserade SQL-frågor** – Eliminerar SQL-injektion i alla 20+ platser
2. **Lösenordshashning** – BCrypt eller PBKDF2 med salt
3. **Indata-validering** – `int.TryParse`, `decimal.TryParse`, validera e-post, roller etc.

### Prioritet 2 – Arkitektur

4. **3-lagerarkitektur** – Separera UI, tjänster och data
5. **Repository-pattern** – `IKundRepository`, `IFakturaRepository` etc.
6. **Dependency injection** – Skicka beroenden via konstruktor
7. **Enum för statuskoder** – `FakturaStatus`, `KundTyp`, `PåminnelseTyp`

### Prioritet 3 – Stabilitet

8. **Felhantering** – `try-catch` med meningsfulla felmeddelanden
9. **`using`-statements** – Korrekt hantering av IDisposable
10. **Persistent logg** – Skriv till fil eller separat tabell i databasen

### Prioritet 4 – Underhållbarhet

11. **Ta bort oanvänd `_cache`** – Deklareras och rensas men värdena läses aldrig
12. **Namngivning** – Konsekvent svenska namn eller engelska, inga förkortningar
13. **Konstanter** – Påminnelseavgifter, standardmomssats, statusvärden
14. **Markera faktura som Skickad** – Saknas i menyn
15. **Redigera produkter** – Saknas (bara prisuppdatering finns)
16. **Ändra/ta bort användare** – Saknas

---

## 10. Exempelrefaktorering

### Före (SQL-injektion + ingen felhantering)

```csharp
// Rad 192
var s = Console.ReadLine();
var cmd = _c!.CreateCommand();
cmd.CommandText = $"SELECT * FROM Kunder WHERE Namn LIKE '%{s}%'";
var r = cmd.ExecuteReader();
```

### Efter (parametriserat + felhantering + separation)

```csharp
// I KundRepository:
public IEnumerable<Kund> Sök(string sökterm)
{
    using var cmd = _connection.CreateCommand();
    cmd.CommandText = "SELECT * FROM Kunder WHERE Namn LIKE @term OR OrgNr LIKE @term";
    cmd.Parameters.AddWithValue("@term", $"%{sökterm}%");
    using var r = cmd.ExecuteReader();
    var result = new List<Kund>();
    while (r.Read())
        result.Add(MapTillKund(r));
    return result;
}

// I KundUI:
public void SökKund()
{
    Console.Write("Sökterm: ");
    var term = Console.ReadLine() ?? "";
    var kunder = _kundRepository.Sök(term);
    foreach (var k in kunder)
        Console.WriteLine($"  [{k.Id}] {k.Namn} - {k.OrgNr} ({k.Ort})");
}
```

---

## 11. Refaktoreringsplan (steg-för-steg)

```
Steg 1: Skapa domänmodeller
        Kund.cs, Produkt.cs, Faktura.cs, FakturaRad.cs, Betalning.cs, Användare.cs

Steg 2: Skapa repository-interface och implementationer
        IKundRepository + KundRepository
        IFakturaRepository + FakturaRepository
        IBetalningsRepository + BetalningsRepository
        ...

Steg 3: Skapa tjänstelager
        KundService (affärslogik kring kunder)
        FakturaService (beräkningar, validering, flöden)
        BetalningsService (registrering, stämning mot faktura)
        PåminnesleService (trappa, avgifter)

Steg 4: Extrahera UI-lager
        KundUI, ProduktUI, FakturaUI, BetalningsUI, RapportUI

Steg 5: Säkra SQL-frågor
        Parametrisera alla frågor i repository-klasser

Steg 6: Säkra autentisering
        Lägg till lösenordshashning

Steg 7: Lägg till tester
        Enhetstester för tjänstelagret (med mockade repositories)
        Integrationstester mot test-SQLite-databas
```

---

*Dokumentet skapades 2026-03-08 som del av workshopen "Koda som ett Ess".*
