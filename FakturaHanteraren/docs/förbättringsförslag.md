# Förbättringsförslag – FakturaHanteraren Enterprise 3.1

> Analys av den nuvarande koden med konkreta förbättringsförslag, prioriterade efter påverkan och implementationskostnad.

---

## Nulägesbedömning

Koden har genomgått en tydlig refaktorering från en monolitisk design och följer nu ett välstrukturerat 3-lagermönster. Säkerhetsfrågorna från ursprunglig design (SQL-injektion, klartext-lösenord) är åtgärdade. Nedan listas kvarstående förbättringsmöjligheter.

### Vad som fungerar bra

| Område                     | Kommentar                                                         |
|----------------------------|-------------------------------------------------------------------|
| Säkerhet – SQL             | Alla databasanrop använder parametriserade frågor                |
| Säkerhet – Lösenord        | BCrypt-hashning med salt används korrekt                         |
| Arkitektur                 | Tydlig separation: Models / Repositories / UI                    |
| Interfaces                 | Alla repositories har interface – möjliggör mockning i tester    |
| Testprojekt                | Separat testprojekt med in-memory SQLite täcker repon och UI     |
| Resurshantering            | `using var` används genomgående för IDisposable                  |
| Domänmodeller              | Records och enums är väl definierade och namngivna               |
| Indata-validering          | `int.TryParse`, `decimal.TryParse` används konsekvent            |
| Affärslogik                | Delfakturering, kommunförfall, inaktiveringsskydd är korrekt     |

---

## Prioritet 1 – Funktionella luckor

### 1.1 Saknat menyval: Markera faktura som "Skickad"

**Problem:** `FakturaStatus.Skickad` (1) existerar i koden och seed-data, men det finns inget menyval för att manuellt ändra en faktura från Ny till Skickad. Fakturor fastnar i status Ny.

**Förslag:**
```csharp
// I FakturaUI.VisaMeny():
Console.WriteLine("6. Markera som skickad");

// Ny metod:
private void MarkeraSomSkickad()
{
    if (session.Roll == "Läsare") { Console.WriteLine("Ej behörighet!"); return; }
    Console.Write("Faktura-Id: ");
    if (!int.TryParse(Console.ReadLine(), out var id)) return;
    // Lägg till MarkeraSkickad() i IFakturaRepository
}
```

### 1.2 Saknade CRUD-operationer för Produkter

**Problem:** Det finns inget sätt att redigera produktens namn/beskrivning/kategori/enhet eller inaktivera en produkt via UI. Bara pris kan uppdateras.

**Förslag:** Lägg till `RedigeraProdukt()` och `InaktiveraProdukt()` i `ProduktUI` samt motsvarande metoder i `IProduktRepository`.

### 1.3 Saknar hantering: Ta bort/inaktivera användare

**Problem:** `InställningarUI.HanteraAnvändare()` kan skapa nya användare men inte redigera eller inaktivera befintliga.

**Förslag:** Lägg till möjlighet att byta lösenord och inaktivera användarkonton.

---

## Prioritet 2 – Arkitektur och design

### 2.1 RapportUI och InställningarUI kringgår repository-lagret

**Problem:** `RapportUI` och `InställningarUI` tar direkt `SqliteConnection` som beroende och kör råa SQL-frågor – till skillnad från övriga UI-klasser som går via repository-interface. Detta bryter mot lagersepareringen och försvårar testning.

**Påverkan:** `RapportUI` har inga enhetstester i dagsläget.

**Förslag:**
```csharp
// Skapa:
interface IRapportRepository {
    MånadsRapportData HämtaMånadsdata(int år, int mån);
    IEnumerable<KundReskontraRad> HämtaKundreskontra();
    // ...
}

// Uppdatera RapportUI:
class RapportUI(IRapportRepository rapportRepo)
```

### 2.2 Ingen tjänstelager (Service Layer)

**Problem:** Affärslogik som t.ex. beräkning av fakturatotalt, påminnelsetrappan och krediteringsflödet ligger spridd i UI-lagret (`FakturaUI`, `PåminnelseUI`). UI-lagret bör inte innehålla affärslogik.

**Förslag:** Extrahera affärslogik till ett tjänstelager:
```
FakturaService   – skapar faktura, beräknar totalt, hanterar kreditering
BetalningsService – registrerar betalning, kontrollerar om fakturan är betald
PåminnelseService – bestämmer typ och avgift, registrerar påminnelse
```

### 2.3 Fakturanummergenerering är osäker vid parallell körning

**Problem:**
```csharp
// MAX(Id) + 1 är inte atomärt – race condition vid samtidiga insättningar
var nextNr = fakturaRepo.NästaFakturaNummer();
```
Om två användare skapar fakturor exakt samtidigt kan de få samma fakturanummer.

**Förslag:** Använd SQLite `last_insert_rowid()` direkt efter INSERT, eller en separat sekvens-tabell med `SELECT ... FOR UPDATE`-semantik via BEGIN EXCLUSIVE transaction.

---

## Prioritet 3 – Stabilitet och datakvalitet

### 3.1 Ingen validering av e-postformat

**Problem:** E-postadresser accepteras utan formatvalidering. `anna@` är en giltig e-postadress enligt nuvarande kod.

**Förslag:**
```csharp
static bool GiltigEmail(string email) =>
    !string.IsNullOrWhiteSpace(email) && email.Contains('@') && email.Contains('.');
```

### 3.2 Ingen validering av organisationsnummer/personnummer

**Problem:** OrgNr-fältet saknar formatvalidering. Systemet accepterar valfri sträng.

**Förslag:** Lägg till grundläggande formatvalidering:
- Organisationsnummer: `XXXXXX-YYYY` (10 siffror + bindestreck)
- Personnummer: `YYYYMMDD-XXXX` eller Luhn-kontroll

### 3.3 Momssatsen persisteras inte

**Problem:** `AppKonfiguration.Moms` är in-memory. Om adminstratören ändrar momssatsen under session förloras ändringen vid omstart och nästa session kör med 25% igen.

**Förslag:** Spara konfiguration till en `Inställningar`-tabell i databasen:
```sql
CREATE TABLE Inställningar (Nyckel TEXT PRIMARY KEY, Värde TEXT);
INSERT INTO Inställningar VALUES ('Momssats', '0.25');
INSERT INTO Inställningar VALUES ('MaxFörfallodagar', '30');
```

### 3.4 Logg försvinner vid omstart

**Problem:** `AppKonfiguration.Logg` är en in-memory lista. All revisionsloggning förloras när programmet avslutas.

**Förslag (alternativ a):** Skriv till fil:
```csharp
File.AppendAllText("faktura-logg.txt", $"{DateTime.Now}: {meddelande}\n");
```

**Förslag (alternativ b):** Lagra i databas:
```sql
CREATE TABLE Logg (Id INTEGER PRIMARY KEY, Datum TEXT, Händelse TEXT, AnvändarId INTEGER);
```

### 3.5 Inga FK-constraints i databasen

**Problem:** SQLite skapar tabellerna utan `FOREIGN KEY`-constraints och SQLite har FK-stöd avaktiverat by default. Databas-integriteten skyddas enbart av applikationslogiken.

**Förslag:** Aktivera FK-stöd och deklarera relationer:
```csharp
// I DatabaseSetup.FixaDB(), efter c.Open():
using var pragma = c.CreateCommand();
pragma.CommandText = "PRAGMA foreign_keys = ON";
pragma.ExecuteNonQuery();
```
```sql
-- I CREATE TABLE-satserna:
FOREIGN KEY (KundId) REFERENCES Kunder(Id),
FOREIGN KEY (SkapadAv) REFERENCES Användare(Id)
```

---

## Prioritet 4 – Användarupplevelse

### 4.1 Bekräftelsesteget saknas vid destruktiva operationer

**Problem:** Kreditering och inaktivering av kunder sker utan bekräftelsefråga ("Är du säker? j/n").

**Förslag:** Lägg till bekräftelseprompt:
```csharp
Console.Write($"Kreditera faktura {info.FakturaNr} på {info.Totalt:N2} kr? (j/n): ");
if (Console.ReadLine()?.ToLower() != "j") { Console.WriteLine("Avbruten."); return; }
```

### 4.2 Ingen paginering i listor

**Problem:** Listor (fakturor, kunder, betalningshistorik) skrivs ut i sin helhet. Med många poster blir utskriften oläslig.

**Förslag:** Lägg till enkel paginering:
```csharp
const int PageSize = 20;
var sida = 0;
var rader = fakturaRepo.HämtaAlla(filter).ToList();
// Visa rader[sida*PageSize .. (sida+1)*PageSize - 1]
// Fråga: "Nästa sida? (enter / q)"
```

### 4.3 Sökning på fakturanummer saknas

**Problem:** Det finns inget sätt att söka upp en faktura på fakturanummer (t.ex. "FAK-2024-0007"). Man måste känna till faktura-Id.

**Förslag:** Lägg till `HämtaEfterNummer(string fakturaNr)` i `IFakturaRepository` och ett sökalternativ i `FakturaUI`.

### 4.4 Ingen sortering eller filtrering på kunder

**Problem:** Kundlistan sorteras enbart på namn. Det finns inget sätt att filtrera på kundtyp.

---

## Prioritet 5 – Testbarhet

### 5.1 Konsol-I/O är svårt att mocka i UI-tester

**Problem:** UI-klasserna anropar `Console.ReadLine()` och `Console.WriteLine()` direkt. Testerna i `UITestBase` löser detta genom att omdirigera `Console.In`/`Console.Out`, men det är sprött och kontextberoende.

**Förslag:** Introducera en `IKonsol`-abstraktion:
```csharp
interface IKonsol {
    string? LäsRad();
    void SkrivRad(string text);
}
```
Injicera i UI-klasser. I produktion används en implementation som delegerar till `Console`. I tester används en mock.

### 5.2 FakturaUI saknar tester

**Problem:** `FakturaUI` är en av de mest komplexa UI-klasserna men saknar tester i testprojektet.

**Förslag:** Lägg till tester för:
- Skapande av faktura med giltiga och ogiltiga indata
- Behörighetskontroller
- Kreditering
- Förfallna fakturor

---

## Sammanfattning – Prioriterad åtgärdslista

| # | Förslag                                      | Prioritet | Insats  |
|---|----------------------------------------------|-----------|---------|
| 1 | Lägg till "Markera som Skickad"              | Hög       | Liten   |
| 2 | Redigera/inaktivera produkter                | Hög       | Liten   |
| 3 | Repository för Rapport och Inställningar     | Hög       | Medium  |
| 4 | Persistera konfiguration i databas           | Hög       | Liten   |
| 5 | Persistent logg (fil eller DB)               | Hög       | Liten   |
| 6 | Aktivera FK-constraints i SQLite             | Medium    | Liten   |
| 7 | Bekräftelse vid destruktiva operationer      | Medium    | Liten   |
| 8 | E-postvalidering                             | Medium    | Liten   |
| 9 | Tjänstelager för affärslogik                 | Medium    | Stor    |
| 10| IKonsol-abstraktion för bättre testbarhet    | Låg       | Medium  |
| 11| Paginering i listor                          | Låg       | Medium  |
| 12| Sökning på fakturanummer                     | Låg       | Liten   |
| 13| OrgNr-validering                             | Låg       | Liten   |
| 14| Hantera/inaktivera användare                 | Låg       | Liten   |
| 15| Tester för FakturaUI                         | Låg       | Medium  |

---

*Förbättringsförslag skapade 2026-03-08 som del av workshopen "Koda som ett Ess".*
