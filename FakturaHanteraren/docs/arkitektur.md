# Arkitekturdokumentation – FakturaHanteraren Enterprise 3.1

> Teknisk dokumentation för utvecklare. Beskriver systemets struktur, lager, databasschema, flöden och designbeslut.

---

## 1. Systembeskrivning

FakturaHanteraren är ett konsolbaserat fakturahanteringssystem byggt på:

- **Plattform:** .NET 8 (C#)
- **Databas:** SQLite via `Microsoft.Data.Sqlite`
- **Lösenordshashning:** `BCrypt.Net-Next`
- **Testramverk:** xUnit med in-memory SQLite

Systemet körs som ett konsoleprogram och hanterar kunder, produkter, fakturor, betalningar och påminnelser via textbaserade menyer.

---

## 2. Projektstruktur

```
FakturaHanteraren/
├── Program.cs                   # Startpunkt, autentisering, huvudmenyloop
├── AppKonfiguration.cs          # Körbar konfiguration (moms, förfallodagar, logg)
├── FakturaHanteraren.csproj
├── FakturaHanteraren.sln
│
├── Models/
│   ├── Enums.cs                 # FakturaStatus, KundTyp, PåminnelseTyp, Konstanter
│   ├── FakturaTyper.cs          # Records: FakturaListRad, FakturaDetaljer, Betalning, m.fl.
│   ├── Kund.cs                  # Records: Kund, KundStatistik
│   └── Produkt.cs               # Records: Produkt, LagerStatusRad
│
├── Data/
│   └── DatabaseSetup.cs         # Skapar tabeller (FixaDB) och fyller testdata (SeedaOmTom)
│
├── Repositories/
│   ├── KundRepository.cs        # IKundRepository + KundRepository
│   ├── ProduktRepository.cs     # IProduktRepository + ProduktRepository
│   ├── FakturaRepository.cs     # IFakturaRepository + FakturaRepository
│   ├── BetalningsRepository.cs  # IBetalningsRepository + BetalningsRepository
│   └── PåminnelseRepository.cs  # IPåminnelseRepository + PåminnelseRepository
│
├── UI/
│   ├── KundUI.cs
│   ├── ProduktUI.cs
│   ├── FakturaUI.cs
│   ├── BetalningsUI.cs
│   ├── PåminnelseUI.cs
│   ├── RapportUI.cs             # Direkt databastillgång (ingen repository)
│   └── InställningarUI.cs       # Direkt databastillgång (ingen repository)
│
└── docs/
    ├── kodgranskning.md
    ├── användarhandledning.md
    ├── arkitektur.md             # Detta dokument
    └── förbättringsförslag.md

FakturaHanteraren.Tests/
├── GlobalUsings.cs
├── Helpers/
│   └── TestDb.cs                # In-memory SQLite-hjälpare för tester
├── Models/
│   ├── EnumsTests.cs
│   └── KonfigurationTests.cs
├── Repositories/
│   ├── KundRepositoryTests.cs
│   ├── ProduktRepositoryTests.cs
│   ├── FakturaRepositoryTests.cs
│   ├── BetalningsRepositoryTests.cs
│   └── PåminnelseRepositoryTests.cs
└── UI/
    ├── UITestBase.cs
    ├── UITestsCollection.cs
    ├── KundUITests.cs
    ├── InställningarUITests.cs
    └── PåminnelseUITests.cs
```

---

## 3. Lagerarkitektur

```
┌─────────────────────────────────────────────────────┐
│                    Program.cs                       │
│  Main() · LoggaIn() · Huvudmenyloop                 │
└──────────────────────┬──────────────────────────────┘
                       │ instansierar
          ┌────────────▼────────────────────────┐
          │           UI-lager                  │
          │  KundUI · ProduktUI · FakturaUI     │
          │  BetalningsUI · PåminnelseUI        │
          │  RapportUI · InställningarUI        │
          └────────────┬────────────────────────┘
                       │ anropar via interface
          ┌────────────▼────────────────────────┐
          │        Repository-lager             │
          │  IKundRepository (KundRepository)   │
          │  IProduktRepository (...)           │
          │  IFakturaRepository (...)           │
          │  IBetalningsRepository (...)        │
          │  IPåminnelseRepository (...)        │
          └────────────┬────────────────────────┘
                       │ SQL via
          ┌────────────▼────────────────────────┐
          │         SQLite-databas              │
          │         fakturor.db                 │
          └─────────────────────────────────────┘
```

### Lageransvar

| Lager       | Ansvar                                                              |
|-------------|---------------------------------------------------------------------|
| Program     | Startsekvens, databaskoppling, autentisering, menyloopen           |
| UI          | Inmatning, utmatning, presentation, navigering                     |
| Repository  | All SQL – läsning, skrivning, uppdatering                          |
| Models      | Datastrukturer (records), enums, konstanter                        |
| Data        | Databasinitiering och seed                                          |

---

## 4. Domänmodeller

### Enums (`Models/Enums.cs`)

```csharp
enum FakturaStatus { Ny = 0, Skickad = 1, Betald = 2, Krediterad = 3 }
enum KundTyp       { Privat = 0, Företag = 1, Kommun = 2 }
enum PåminnelseTyp { Första = 1, Andra = 2, Inkasso = 3 }

static class Konstanter
{
    public const double FörstaAvgift    = 60.0;
    public const double AndraAvgift     = 180.0;
    public const double InkassoAvgift   = 450.0;
    public const int    StandardFörfallodag = 30;
    public const int    KommunFörfallodag   = 60;
}
```

### Nyckelmodeller

| Record                      | Användning                                         |
|-----------------------------|-----------------------------------------------------|
| `Kund`                      | Fullständig kundpost                               |
| `KundStatistik`             | Aggregerad faktureringsstatistik per kund          |
| `Produkt`                   | Fullständig produktpost                            |
| `LagerStatusRad`            | Lagervy (namn, saldo, kategori)                    |
| `FakturaListRad`            | Komprimerad vy för fakturalistan                   |
| `FakturaDetaljer`           | Fullständig faktura inkl. rader, betalningar, påminnelser |
| `FakturaBetalningsInfo`     | Minimal vy för betalningsflödet                    |
| `FakturaKrediteringsInfo`   | Minimal vy för krediteringsflödet                  |
| `FörfallenFaktura`          | Förfallna fakturor med antal dagar                 |
| `FörfallenFakturaFörPåminnelse` | Utökad vy med e-post och antal tidigare påminnelser |
| `Betalning`                 | Enskild betalningspost                             |
| `BetalningsHistorikRad`     | Betalning inkl. fakturanummer (för historikvy)     |
| `Påminnelse`                | Enskild påminnelsepost                             |

---

## 5. Databasschema

Databasen skapas automatiskt av `DatabaseSetup.FixaDB()` om tabellerna inte finns.

```sql
CREATE TABLE Användare (
    Id       INTEGER PRIMARY KEY,
    Namn     TEXT,           -- Användarnamn
    Lösenord TEXT,           -- BCrypt-hash
    Roll     TEXT,           -- Admin / Handläggare / Läsare
    Email    TEXT,
    Skapad   TEXT            -- ISO-datumstring
);

CREATE TABLE Kunder (
    Id         INTEGER PRIMARY KEY,
    Namn       TEXT,
    OrgNr      TEXT,
    Adress     TEXT,
    Postnr     TEXT,
    Ort        TEXT,
    Email      TEXT,
    Tele       TEXT,
    KundTyp    INTEGER,      -- 0=Privat, 1=Företag, 2=Kommun
    Rabatt     REAL,         -- Procent
    Aktiv      INTEGER DEFAULT 1,
    Skapad     TEXT,
    Uppdaterad TEXT
);

CREATE TABLE Produkter (
    Id          INTEGER PRIMARY KEY,
    Namn        TEXT,
    Beskrivning TEXT,
    Pris        REAL,        -- Exkl. moms
    Enhet       TEXT,        -- tim / st / mån
    MomsKod     INTEGER,     -- Alltid 1
    Kategori    TEXT,
    Aktiv       INTEGER DEFAULT 1,
    LagerSaldo  INTEGER DEFAULT 0
);

CREATE TABLE Fakturor (
    Id            INTEGER PRIMARY KEY,
    KundId        INTEGER,   -- FK → Kunder
    FakturaNr     TEXT,      -- FAK-YYYY-NNNN eller KRED-YYYY-NNNN
    Datum         TEXT,
    Förfallodatum TEXT,
    Status        INTEGER,   -- 0=Ny, 1=Skickad, 2=Betald, 3=Krediterad
    Totalt        REAL,      -- Inkl. moms + påminnelseavgifter
    Moms          REAL,
    Rabatt        REAL,      -- Kundrabatt vid fakturatillfället
    Notering      TEXT,
    SkapadAv      INTEGER,   -- FK → Användare
    Betald        TEXT       -- Betalningsdatum
);

CREATE TABLE FakturaRader (
    Id         INTEGER PRIMARY KEY,
    FakturaId  INTEGER,      -- FK → Fakturor
    ProduktId  INTEGER,      -- FK → Produkter
    Antal      REAL,
    ÅPris      REAL,         -- À-pris vid fakturatillfället
    Rabatt     REAL,         -- Radrabatt %
    Summa      REAL          -- Antal × ÅPris × (1 - Rabatt/100)
);

CREATE TABLE Betalningar (
    Id         INTEGER PRIMARY KEY,
    FakturaId  INTEGER,      -- FK → Fakturor
    Belopp     REAL,
    Datum      TEXT,
    Metod      TEXT,         -- Bank / Swish / Kort
    Referens   TEXT
);

CREATE TABLE Påminnelser (
    Id         INTEGER PRIMARY KEY,
    FakturaId  INTEGER,      -- FK → Fakturor
    Datum      TEXT,
    Typ        INTEGER,      -- 1=Första, 2=Andra, 3=Inkasso
    Avgift     REAL
);
```

### Relationsöversikt

```
Användare ──────────────── Fakturor (SkapadAv)
Kunder ─────────────────── Fakturor (KundId)
Fakturor ───────────────── FakturaRader (FakturaId)
Fakturor ───────────────── Betalningar (FakturaId)
Fakturor ───────────────── Påminnelser (FakturaId)
Produkter ──────────────── FakturaRader (ProduktId)
```

---

## 6. Autentisering och behörighet

### Autentisering

`Program.LoggaIn()` frågar efter användarnamn och lösenord i en loop tills rätt uppgifter anges. Lösenordet verifieras mot en BCrypt-hash lagrad i databasen:

```csharp
BCrypt.Net.BCrypt.Verify(angivetLösenord, hashFrånDB)
```

En `Session`-record skapas med `AnvändarId` och `Roll` och skickas vidare till alla UI-klasser.

### Behörighetsmodell

| Funktion                 | Admin | Handläggare | Läsare |
|--------------------------|:-----:|:-----------:|:------:|
| Lista/visa kunder        | ✓     | ✓           | ✓      |
| Söka kunder              | ✓     | ✓           | ✓      |
| Skapa kund               | ✓     | ✓           | ✗      |
| Redigera kund            | ✓     | ✓           | ✗      |
| Inaktivera kund          | ✓     | ✗           | ✗      |
| Lista/visa produkter     | ✓     | ✓           | ✓      |
| Skapa produkt            | ✓     | ✓           | ✗      |
| Uppdatera pris           | ✓     | ✓           | ✗      |
| Lista/visa fakturor      | ✓     | ✓           | ✓      |
| Skapa faktura            | ✓     | ✓           | ✗      |
| Kreditera faktura        | ✓     | ✗           | ✗      |
| Registrera betalning     | ✓     | ✓           | ✗      |
| Skicka påminnelser       | ✓     | ✓           | ✗      |
| Se rapporter             | ✓     | ✓           | ✓      |
| Inställningar            | ✓     | ✗           | ✗      |
| Hantera användare        | ✓     | ✗           | ✗      |
| Se logg                  | ✓     | ✓           | ✓      |

Behörighetskontroll sker i varje UI-metod med `session.Roll`-jämförelse.

---

## 7. Affärslogik och flöden

### Fakturaflöde

```
Skapa faktura (Status = Ny)
      │
      ▼
[Betala via Betalningar]
      │ om ΣBetalningar ≥ Totalt - 0.01
      ▼
Status = Betald
      │
 eller (Admin)
      ▼
SkapaKreditfaktura (KRED-YYYY-NNNN, negativt belopp)
      + MarkeraKrediterad på ursprungsfakturan
      ▼
Status = Krediterad
```

> Not: Status `Skickad` (1) saknar menyval för manuell uppdatering. Statusen förekommer i seed-data men kan inte sättas via UI.

### Beräkningsregler

| Beräkning           | Formel                                              |
|---------------------|-----------------------------------------------------|
| Radbelopp           | `Antal × ÅPris × (1 - Rabatt / 100)`               |
| Totalbelopp ex moms | `ΣRadbelopp`                                        |
| Moms                | `TotaltExMoms × Momssats` (default 25%)             |
| Totalt inkl. moms   | `TotaltExMoms + Moms`                               |
| Förfallodatum       | `Fakturadatum + MaxFörfallodagar` (30 dagar default)|
| Kommunförfall       | `Fakturadatum + 60 dagar` (KundTyp = Kommun)        |
| Delfakturering      | Faktura = Betald när `ΣBetalningar ≥ Totalt - 0.01` |

### Påminnelsetrappa

```
Antal registrerade påminnelser = 0 → Första påminnelse  (+60 kr)
Antal registrerade påminnelser = 1 → Andra påminnelse   (+180 kr)
Antal registrerade påminnelser ≥ 2 → Inkassovarning     (+450 kr)
```

Avgiften läggs direkt till `Fakturor.Totalt` i databasen via `FakturaRepository.UppdateraFörPåminnelse()`.

### Fakturanummergenerering

```csharp
// Nästa nummer = MAX(Id) i Fakturor + 1
var nextNr = fakturaRepo.NästaFakturaNummer();
var fakNr  = $"FAK-{DateTime.Now.Year}-{nextNr:D4}";   // t.ex. FAK-2025-0013
var kredNr = $"KRED-{DateTime.Now.Year}-{nextNr:D4}";  // t.ex. KRED-2025-0013
```

### Lagerhantering

- Produkter med `LagerSaldo > 0` kontrolleras vid fakturering
- `Antal > LagerSaldo` blockeras med felmeddelande
- Lagret minskas direkt vid fakturering (ingen reservation)
- `LagerSaldo = 0` = ingen lagerhantering (tjänster m.m.)

---

## 8. Konfiguration (`AppKonfiguration`)

```csharp
class AppKonfiguration
{
    public decimal Moms            { get; set; } = 0.25m;  // 25%
    public int MaxFörfallodagar    { get; set; } = 30;
    public List<string> Logg       { get; }      = new();
}
```

`AppKonfiguration` är ett delat objekt som skickas till alla lager som behöver det. Ändringar via `InställningarUI` slår igenom direkt för nya fakturor.

Loggen är en in-memory lista. Varje loggad händelse formateras: `{DateTime.Now}: {meddelande}`.

---

## 9. Beroendegraf

```
Program
├── AppKonfiguration
├── DatabaseSetup(c, konfiguration)
│
├── KundRepository(c)            ──→ IKundRepository
├── ProduktRepository(c)         ──→ IProduktRepository
├── FakturaRepository(c)         ──→ IFakturaRepository
├── BetalningsRepository(c)      ──→ IBetalningsRepository
├── PåminnelseRepository(c)      ──→ IPåminnelseRepository
│
├── KundUI(kundRepo, session, logg)
├── ProduktUI(produktRepo, session, logg)
├── FakturaUI(fakturaRepo, kundRepo, produktRepo, konfiguration, session, logg)
├── BetalningsUI(betRepo, fakturaRepo, session, logg)
├── PåminnelseUI(påmRepo, fakturaRepo, session, logg)
├── RapportUI(c)                 ──→ direkt SQLite-anslutning
└── InställningarUI(c, konfiguration, session, logg)
```

---

## 10. Testarkitektur

### Testprojekt: `FakturaHanteraren.Tests`

- **Testramverk:** xUnit
- **Databas:** In-memory SQLite (`:memory:`) via `TestDb`-hjälparen
- **UI-tester:** Konsolens in/ut mockas med `StringReader`/`StringWriter`

### TestDb

```csharp
class TestDb
{
    public static SqliteConnection Skapa()
    {
        var c = new SqliteConnection("Data Source=:memory:");
        c.Open();
        new DatabaseSetup(c, new AppKonfiguration()).FixaDB();
        return c;
    }
}
```

### Testtäckning

| Område                         | Täckt |
|--------------------------------|-------|
| Repository – Kund              | ✓     |
| Repository – Produkt           | ✓     |
| Repository – Faktura           | ✓     |
| Repository – Betalning         | ✓     |
| Repository – Påminnelse        | ✓     |
| UI – Kund                      | ✓     |
| UI – Inställningar             | ✓     |
| UI – Påminnelse                | ✓     |
| Models – Enums/Konstanter      | ✓     |
| Konfiguration                  | ✓     |

---

## 11. Startsekvens

```
1. Parsa argument: dbPath = args[0] ?? "fakturor.db"
2. Skapa AppKonfiguration
3. Öppna SqliteConnection
4. DatabaseSetup.FixaDB()      → Skapa tabeller om de saknas
5. DatabaseSetup.SeedaOmTom()  → Fyll testdata om databasen är tom
6. Visa välkomsthälsning
7. Program.LoggaIn()           → Loop tills korrekt inloggning
8. Instansiera alla Repositories
9. Instansiera alla UI-klasser
10. Kör huvudmenyloopen
```

---

*Arkitekturdokumentation skapad 2026-03-08 som del av workshopen "Koda som ett Ess".*
