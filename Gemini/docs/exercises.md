# Workshop-övningar: Koda som ett Ess (Detaljerade steg)

Dessa övningar utförs i mappen `demo-app`. Öppna din terminal och se till att du står i rätt mapp innan du börjar.

---

## Övning 1: Från "Stökig kod" till "Ess-kod" (45 min)
*Mål: Att lära sig hur man sätter upp spelregler för AI-agenten och använder den för att höja kodkvaliteten.*

### Steg 1: Skapa projektets grundlagar (GEMINI.md)
Istället för att berätta för Gemini i varje prompt vilket testramverk eller språk du använder, skapar vi en "konfigurationsfil" som Gemini läser varje gång.
1. Be Gemini skapa filen:
   > "Skapa en GEMINI.md i rotkatalogen. Reglerna är: Vi använder alltid TypeScript med strikta typer, ESNext-moduler och Vitest för alla enhetstester. Vi föredrar korta, rena funktioner."
2. Verifiera att filen skapades och läs igenom den.

### Steg 2: Analys och Refaktorering
Nu ska vi städa upp `src/inventoryService.ts`. Den fungerar, men är inte "Ess-kod".
1. Be Gemini analysera filen först:
   > "Läs src/inventoryService.ts. Vilka problem ser du med typerna och läsbarheten?"
2. Be om en total refaktorering baserat på reglerna i GEMINI.md:
   > "Refaktorera src/inventoryService.ts. Implementera ett Product-interface och se till att alla funktioner är strikt typade. Gör logiken i searchProducts mer modern."
3. Granska ändringarna. Ser du hur den använde reglerna från din GEMINI.md?

### Steg 3: Skapa ett säkerhetsnät (Tester)
Nu ser vi styrkan i att ha definierat Vitest i GEMINI.md.
1. Be Gemini skapa testerna:
   > "Skapa filen src/inventoryService.test.ts och skriv omfattande enhetstester för alla funktioner i inventoryService.ts. Inkludera kantfall som tomma sökningar och produkter som inte finns."
2. Kör testerna i terminalen: `npm test`.
3. Om något test misslyckas – be Gemini fixa koden eller testet!

---

## Övning 2: Den smarta felsökaren (60 min)
*Mål: Att använda externa verktyg (MCP) och expert-instruktioner (Skills) för att lösa svåra problem.*

### Steg 1: Reproducera felet
Innan vi kan laga något måste vi se det gå sönder.
1. Bygg och kör appen: `npm run build && npm run start`.
2. Notera kraschen när programmet försöker söka på `(v2)`. Varför kraschar det? (Tips: RegExp och specialtecken).

### Steg 2: Använd MCP för att hitta en lösning
Istället för att sitta och klura på ett komplicerat Regular Expression-mönster själv, låter vi Gemini göra researchen.
1. Be Gemini söka efter en lösning:
   > "Använd verktyget google_web_search för att hitta hur man säkert 'escapar' en sträng så att den kan användas som en del av ett JavaScript RegExp-objekt utan att krascha vid specialtecken."
2. Läs Geminis förklaring av lösningen.

### Steg 3: Applicera och verifiera fixen
1. Be Gemini laga buggen i `src/inventoryService.ts`:
   > "Baserat på din sökning, uppdatera searchProducts så att den hanterar specialtecken korrekt. Lägg även till ett nytt testfall i inventoryService.test.ts som specifikt testar sökning med parenteser."
2. Kör testerna igen (`npm test`) och kör appen (`npm run start`) för att bekräfta att det fungerar.

### Steg 4: Aktivera expert-hjälp (Skills)
Nu ska vi se hur vi kan få specifika råd om molndistribution.
1. Be Gemini visa vilka Skills som finns:
   > "Vilka skills har jag tillgång till?"
2. Aktivera Azure-skillen:
   > "Aktivera skill 'azure-prepare'."
3. Be om råd för framtiden:
   > "Om jag vill köra den här appen som en serverlös funktion i Azure, vilka filer behöver jag skapa och hur skulle arkitekturen se ut?"

---

*Bra jobbat! Du har nu gått från att skriva kod manuellt till att styra en intelligent agent.*
