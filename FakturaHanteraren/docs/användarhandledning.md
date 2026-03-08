# Användarhandledning – FakturaHanteraren Enterprise 3.1

> Konsolbaserat fakturahanteringssystem för hantering av kunder, produkter, fakturor, betalningar och påminnelser.

---

## Komma igång

### Starta programmet

```bash
dotnet run
# eller
FakturaHanteraren.exe
# alternativt med anpassad databas:
FakturaHanteraren.exe min-databas.db
```

### Inloggning

Vid start visas en inloggningsprompt. Ange användarnamn och lösenord:

| Användare | Lösenord  | Roll          | Behörigheter                      |
|-----------|-----------|---------------|-----------------------------------|
| admin     | admin123  | Admin         | Full åtkomst                      |
| anna      | anna123   | Handläggare   | Läsa + skapa/redigera (ej admin)  |
| erik      | erik123   | Läsare        | Enbart läsåtkomst                 |

---

## Huvudmeny

```
--- HUVUDMENY ---
1. Kunder
2. Produkter
3. Fakturor
4. Rapporter
5. Betalningar
6. Påminnelser
7. Inställningar
8. Logg
0. Avsluta
```

Skriv siffran för önskat alternativ och tryck Enter.

---

## 1. Kunder

**Behörighet:** Alla kan lista och söka. Skapa/redigera kräver Handläggare eller Admin. Inaktivera kräver Admin.

### 1.1 Lista kunder
Visar alla aktiva kunder i tabellformat med Id, Namn, OrgNr, Ort, Typ och Rabatt.

### 1.2 Sök kund
Sök på namn, organisationsnummer eller ort. Sökningen är partiell (t.ex. "acme" hittar "Acme AB").

### 1.3 Ny kund
Fyll i följande uppgifter:
- **Namn** – Kundens fullständiga namn
- **OrgNr/PersNr** – Organisationsnummer (företag/kommun) eller personnummer (privat)
- **Adress**, **Postnr**, **Ort**
- **Email**, **Telefon**
- **Kundtyp:** `0` = Privat, `1` = Företag, `2` = Kommun
- **Rabatt %** – Standardrabatt för kunden (0–100)

> Kommuner (typ 2) får automatiskt 60 dagars betalningsfrist istället för 30.

### 1.4 Redigera kund
Ange kund-Id. Lämna fältet tomt (Enter) för att behålla befintligt värde.
Kan redigera: namn, adress, ort, e-post och rabatt.

### 1.5 Inaktivera kund
Kräver **Admin**. En kund med obetalda fakturor kan **inte** inaktiveras.

### 1.6 Kundstatistik
Visar per kund: antal fakturor, totalt fakturerat, varav betalt och utestående belopp.

---

## 2. Produkter

**Behörighet:** Alla kan lista. Skapa och prisändra kräver Handläggare eller Admin.

### 2.1 Lista produkter
Visar alla aktiva produkter grupperade per kategori med pris, enhet och lagersaldo.

### 2.2 Ny produkt
Fyll i:
- **Namn**, **Beskrivning**
- **Pris** (exkl. moms)
- **Enhet** – t.ex. `tim`, `st`, `mån`
- **Kategori** – t.ex. `Tjänster`, `Licenser`, `Hårdvara`
- **Lagersaldo** – Sätt 0 för tjänster utan lagerhantering

### 2.3 Uppdatera pris
Ange produkt-Id och nytt pris. Påverkar **inte** befintliga fakturarader.

### 2.4 Lagerstatus
Visar produkter med lagersaldo > 0, sorterat på lägsta saldo.

---

## 3. Fakturor

**Behörighet:** Alla kan lista och visa. Skapa kräver Handläggare eller Admin. Kreditera kräver Admin.

### 3.1 Lista fakturor
Välj filter:
- `0` = Ny
- `1` = Skickad
- `2` = Betald
- `*` = Alla

Förfallna och obetalda fakturor markeras med ⚠️ i listan.

### 3.2 Skapa ny faktura

**Steg-för-steg:**

1. Ange **Kund-Id** (se listan under Kunder)
2. Systemet anger förslag på fakturanummer (FAK-ÅÅÅÅ-NNNN) och förfallodatum
3. Ange valfri **Notering**
4. Lägg till **fakturarader** – upprepa tills klart:
   - Ange **Produkt-Id**
   - Ange **Antal**
   - Bekräfta eller justera **à-pris** (Enter = behåll listpris)
   - Bekräfta eller justera **radrabatt %** (Enter = kundens standardrabatt)
   - Skriv `0` som Produkt-Id för att avsluta
5. Systemet beräknar totalt exkl. moms + moms (25%) = totalt inkl. moms

> Produkter med lagerhantering minskar automatiskt sitt saldo vid fakturering.

### 3.3 Visa faktura
Ange faktura-Id för att se fullständig fakturainformation inklusive rader, betalningar och påminnelser.

### 3.4 Kreditera faktura
Kräver **Admin**. Skapar en kreditfaktura (KRED-ÅÅÅÅ-NNNN) med negativt belopp och markerar ursprungsfakturan som krediterad.

### 3.5 Förfallna fakturor
Visar alla fakturor med status Ny eller Skickad vars förfallodatum passerat, med antal förfallna dagar och utestående belopp.

---

## 4. Rapporter

**Behörighet:** Alla roller kan se rapporter.

### 4.1 Månadssammanställning
Ange år och månad. Visar:
- Antal fakturerade och totalt belopp (inkl. moms)
- Antal betalningar och inbetalt belopp
- Antal nya kunder
- Antal och belopp förfallna fakturor

### 4.2 Kundreskontra
Totaloversikt per aktiv kund: antal fakturor, utestående, betalt och totalt.

### 4.3 Produktförsäljning
Topplista på produkter sorterat på omsättning. Visar antal fakturarader, totalt sålt antal och summa.

### 4.4 Momsrapport
Ange år och kvartal (1–4). Visar:
- Netto (exkl. moms)
- Utgående moms
- Totalt inkl. moms
- Belopp att redovisa till Skatteverket

### 4.5 Årssammanställning
Visar månad för månad med antal fakturor och belopp, inklusive ett enkelt stapeldiagram. Summerar hela året.

---

## 5. Betalningar

**Behörighet:** Läsare kan se historik. Registrera betalning kräver Handläggare eller Admin.

### 5.1 Registrera betalning

1. Ange faktura-Id
2. Systemet visar fakturans totalt, redan betalt och kvarvarande belopp
3. Ange **belopp**, **metod** (`Bank`, `Swish` eller `Kort`) och **referens**
4. Om totalt inbetalt täcker fakturans belopp markeras fakturan automatiskt som **Betald**
5. Annars registreras en delbetalning och kvarvaliderat belopp visas

### 5.2 Betalningshistorik
Ange faktura-Id för specifik faktura, eller `*` för de 50 senaste betalningarna i systemet.

---

## 6. Påminnelser

**Behörighet:** Handläggare och Admin.

Systemet visar automatiskt alla förfallna och obetalda fakturor med föreslagen åtgärd:

| Tidigare påminnelser | Åtgärd            | Avgift   |
|----------------------|-------------------|----------|
| 0                    | Första påminnelse | 60 kr    |
| 1                    | Andra påminnelse  | 180 kr   |
| 2 eller fler         | Inkassovarning    | 450 kr   |

För varje faktura visas kunduppgifter, antal förfallna dagar och belopp. Bekräfta med `j` för att skicka påminnelse.

När en påminnelse registreras:
- Avgiften läggs till fakturans totalt
- Påminnelsen loggas i systemet
- En simulerad e-post visas i konsolen

---

## 7. Inställningar

**Behörighet:** Enbart Admin.

### 7.1 Momssats
Ändra momssatsen i procent (t.ex. `25` för 25%). Påverkar nya fakturor.

### 7.2 Förfallodagar
Ändra standardantalet dagar tills förfallodatum (standardvärde: 30). Kommuner har alltid 60 dagar.

### 7.3 Hantera användare
Visar befintliga användare och ger möjlighet att skapa ny användare med namn, lösenord, roll och e-post.

**Tillgängliga roller:** `Admin`, `Handläggare`, `Läsare`

### 7.4 Databasunderhåll
Kör SQLite VACUUM för att optimera och komprimera databasen. Visar antal poster i varje tabell.

---

## 8. Logg

Visar alla händelser som loggats under aktuell session, t.ex.:
- Inloggningar
- Skapade kunder och fakturor
- Registrerade betalningar
- Skickade påminnelser
- Administrativa ändringar

> Loggen sparas enbart under pågående session och nollställs vid omstart.

---

## Felmeddelanden

| Meddelande                                  | Orsak och åtgärd                                               |
|---------------------------------------------|----------------------------------------------------------------|
| `FEL! Försök igen.`                         | Felaktigt användarnamn eller lösenord                          |
| `Ej behörighet!`                            | Din roll saknar rättighet för denna åtgärd                    |
| `Kund ej hittad eller inaktiv!`             | Kontrollera kund-Id och att kunden är aktiv                    |
| `Otillräckligt lager! Tillgängligt: X`      | Minska antal eller kontakta lageransvarig                      |
| `Kan ej inaktivera – obetalda fakturor!`    | Fakturor måste betalas eller krediteras innan kunden inaktiveras |
| `Redan betald!`                             | Fakturan är redan fullständigt betald                           |
| `Krediterad – kan ej betalas!`             | Fakturan har krediterats och kan inte betalas                  |
| `Redan krediterad!`                         | Fakturan är redan krediterad                                   |
| `Ogiltigt filter.`                          | Ange 0, 1, 2 eller * som filter för fakturalistan             |

---

## Snabbreferens – Fakturastatus

| Status    | Värde | Innebörd                                         |
|-----------|-------|--------------------------------------------------|
| Ny        | 0     | Fakturan är skapad men ej skickad                |
| Skickad   | 1     | Fakturan är skickad till kund                    |
| Betald    | 2     | Fakturan är fullt betald                         |
| Krediterad| 3     | Fakturan har krediterats (negativ kreditfaktura skapad) |

---

*FakturaHanteraren Enterprise 3.1 – Dokumentation skapad 2026-03-08*
