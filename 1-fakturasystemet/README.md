# 🃏 Koda som ett äss — Workshop

## Bakgrund
Du har ärvt "FakturaHanteraren Enterprise 3.1" — ett faktura- och kundhanteringssystem byggt som en ren konsolapplikation i .NET 8 med SQLite.

**Applikationen fungerar** men koden är... inte ideal. Din uppgift är att med hjälp av AI-verktyg förstå, förbättra och dokumentera systemet.

## Kom igång

```bash
dotnet restore
dotnet run
```

Logga in med: `admin` / `admin123`

## Uppgifter

### 1. 🔍 Förstå applikationen
Använd AI för att analysera koden och besvara:
- Vad gör applikationen?
- Vilka entiteter/domänobjekt finns?
- Vilka affärsregler finns (t.ex. rabatter, momssatser, förfallodagar)?
- Vilka säkerhetsproblem kan du identifiera?
- Vilka designproblem finns?

- Prompt: kan du förklara vad denna applikation gör?
gör en nogrann kodgranskning och skapa ett dokument under 1-fakturasystemet/docs
koncentrera dig på säkerhet, testbarhet, förbättringsmöjligheter och annat du tycker är viktigt


### 2. 🏗️ Refaktorera till 3-skiktslösning
Bryt upp koden i:
- **Presentation** — Konsolens UI (menyer, inmatning, utskrifter)
- **Affärslogik** — Regler, beräkningar, validering
- **Dataåtkomst** — Repositories, databasinteraktion

- Prompt: refaktorera applikationen enligt kodgranskningens rekommendationer
jag vill att du skapar en Nuxt4 app som frontend, stilen ska vara en modern snygg webapp i finansvärlden, 
använd färgschema från http://www.timepeoplegroup.se, skapa den i undermappen 1-fakturasystemet/frontend
som backend vill jag ha ett .net minimal web api, skapa den i  undermappen 1-fakturasystemet/backend
använd mcp-servern context7 om behöver hjälp med nuxt eller minimal web api


### 3. 🧪 Inför enhetstester
- Skapa ett testprojekt
- Skriv tester för affärslogiken (momsberäkningar, rabatter, påminnelseavgifter, etc.)
- Mål: Minst 80% kodtäckning på affärslogikskiktet

- Prompt: skapa omfattande enhetstester för backend, inkludera edge cases



### 4. 📝 Dokumentera
- XML-dokumentation på publika metoder
- En arkitekturbeskrivning (Mermaid-diagram eller liknande)
- API-dokumentation för affärslogiklagret

- Prompt: Dokumentera lösningen, skapa både omfattande användarhandledning och arkitekturdokument

## Kända problem i koden (hitta fler!)
- SQL-injection överallt
- Lösenord i klartext
- Allt i en enda fil
- Ingen separation of concerns
- Hårdkodade strängar och magic numbers
- Blandning av svenska och engelska
- Ingen felhantering
- Globalt state
- Omöjligt att enhetstesta

## Inloggningsuppgifter
| Användare | Lösenord | Roll |
|-----------|----------|------|
| admin | admin123 | Admin |
| anna | anna123 | Handläggare |
| erik | erik123 | Läsare |
