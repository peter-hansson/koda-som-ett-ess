# Instruktörsguide: Gemensam genomgång av 'Koda som ett Ess'

Denna guide är optimerad för en interaktiv session där du leder gruppen genom övningarna.

## Tips för en lyckad session
- **Live-fail:** Var inte rädd om Gemini ger ett konstigt svar. Det är ett perfekt tillfälle att visa hur man "styr om" AI:n.
- **Dela skärm:** Ha din terminal och VS Code synliga samtidigt.
- **Kör testerna ofta:** Visa hur `npm test` går från rött till grönt efter en fix.

## Steg-för-steg (Gemensamt)

### Intro: Setup (15 min)
- Kör `gemini login` live.
- Visa `gemini config` och förklara vad en "modell" är.

### Modul 1: Utforskning (20 min)
- **Demo:** Kör `codebase_investigator`.
- **Diskutera:** "Ser ni hur den faktiskt 'läser' filerna?"
- **Uppgift:** Låt deltagarna hitta den dolda buggen i `inventoryService.ts` genom att bara ställa frågor till Gemini.

### Modul 2: Skills & MCP (30 min) - *Showstopper*
- **Demo:** Aktivera en skill (t.ex. `azure-prepare`).
- **Demo:** Gör en webbsökning med `google_web_search`.
- **Key Point:** Förklara att Gemini CLI inte bara är en chatt, utan har "händer och fötter" (verktyg).

### Modul 3: Refactoring (40 min)
- **Interaktion:** Be gruppen föreslå förbättringar för `getStockStatus`.
- **Live-kodning:** Applicera Geminis förslag live.
- **Diskutera:** "Är koden verkligen bättre nu? Varför/Varför inte?"

### Modul 4: Debugging (30 min)
- **Gemensam krasch:** Be alla köra `npm run start` och se den krascha på `(v2)`.
- **Problemlösning:** Be Gemini förklara felet.
- **Fix:** Låt deltagarna applicera fixen och verifiera med `npm run start` igen.

## Avslutning (15 min)
- Sammanfatta: Gemini CLI är en partner, inte en ersättare.
- Fråga: "Vilken feature kommer ni använda mest imorgon på jobbet?"
