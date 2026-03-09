# Anders anteckningar — Pass 2 (efter middag)

Peters pass (före middag) tar "ny på jobbet"-scenariot: förklara kodbaser
och hitta/korrigera fel. Ditt pass bygger vidare med avancerade tekniker.

## Före sessionen

- [ ] Testa att Gemini CLI fungerar på en testmaskin
- [ ] Ha Claude Code redo med API-nyckel konfigurerad
- [ ] Ha OpenClaw igång, kopplad till repot och WhatsApp
- [ ] Testa att deltagare kan skicka meddelanden via WhatsApp
- [ ] Förbered QR-kod / telefonnummer att visa på skärmen
- [ ] Klona repot till en ren mapp för demon
- [ ] Ha en backup-lösning om nätet laggar (inspelade demos?)

## Intro (5 min)

**Kort recap — knyt an till Peters pass:**
- Peter visade hur AI hjälper dig *förstå* kod och *hitta fel*
- Nu: hur du får AI att jobba *med* dig — kontext, TDD, agentiska flöden
- "Peter visade AI som förklarare. Jag visar AI som medarbetare."

**Visa WhatsApp-kopplingen:**
- Visa QR-kod / nummer på skärmen
- "Ni kan chatta med Claude under hela passet — testa nu!"

### OpenClaw via WhatsApp — deltagarinteraktion

Deltagarna chattar med Claude via WhatsApp under hela sessionen.
OpenClaw är kopplad till workshop-repot och kan:

- Svara på frågor om övningarna
- Förklara kod i repot
- Ge hints när någon kört fast
- Göra kodändringar i repot (visa på skärmen)

**Setup före sessionen:**
1. Starta OpenClaw med WhatsApp-integration och git-skill kopplad till repot
2. Visa QR-koden på projektorskärmen så deltagare kan scanna
3. Testa att ett meddelande från din egen telefon får svar

**Live-demo under introt (2 min):**
1. Öppna WhatsApp på telefonen (visa på skärmen)
2. Skicka: "Vad innehåller det här repot?"
3. Claude svarar med sammanfattning av övningarna
4. Skicka: "Visa mig testerna i övning 2"
5. Claude visar test_booking.py
6. "Vem som helst av er kan göra samma sak — scanna QR-koden!"

**Under övningarna:**
- Uppmuntra deltagare att ställa frågor i WhatsApp
- "Skriv till Claude om ni kört fast — det är som att ha en extra instruktör"
- Visa intressanta WhatsApp-interaktioner på storskärm

**Varför WhatsApp:**
- Alla har det — inget att installera
- Visar att AI-kodning inte kräver terminal-skills
- Sänka tröskeln: "Claude är en chatt bort"
- Wow-faktor: från mobilen till kodändringar i repot

**Viktigt att nämna:**
- OpenClaw skickar data till Anthropics API — kolla era sekretess-policies
- Granska alltid ändringarna innan merge

## Övning 1: Kontext är kung (15 min)

### Dina talking points

- GEMINI.md / CLAUDE.md är som `.editorconfig` fast för AI
- Teamet delar filen via git — alla får samma AI-beteende
- Konventioner som AI:n följer: kodstil, namngivning, språk, struktur
- Det räcker med 10-20 rader för att se stor skillnad

### Demo med Claude Code

Visa sida vid sida:
1. Tom mapp → "bygg en CLI-app för lösenord"
2. Med CLAUDE.md → exakt samma prompt

Peka på skillnaderna:
- Svenska hjälptexter
- Type hints
- Exit-koder
- Inga beroenden
- Strukturerad kod

### Vanliga frågor

**F: Måste alla i teamet använda samma AI-verktyg?**
S: Nej — GEMINI.md och CLAUDE.md följer samma mönster. Konventionerna är
desamma, bara filnamnet skiljer.

**F: Hur lång ska kontextfilen vara?**
S: Börja med 10-20 rader. Lägg till när ni hittar mönster som AI:n missar.

## Övning 2: TDD med AI (15 min)

### Dina talking points

- Tester är den **mest exakta prompten** du kan skriva
- AI:n itererar snabbare än du: implementera → kör → fixa → kör
- Du fokuserar på *vad* (tester), AI:n på *hur* (implementation)
- Perfekt arbetsdelning: människa specificerar, AI implementerar

### Instruktion till deltagare

1. "Öppna exercises/02-tdd-med-ai/"
2. "Läs testerna — de beskriver ett bokningssystem för padelbanor"
3. "Kör pytest — allt är rött"
4. "Be nu AI:n: 'Läs testerna och implementera src/booking.py så att alla passerar. Kör pytest.'"
5. "Titta på hur AI:n arbetar"

### Förväntad tidsåtgång

- AI:n borde klara alla 18 tester på 1-3 iterationer
- Typiskt: första försöket ~15 tester gröna, fixar resten på iteration 2

### Om någon blir klar tidigt

Be dem lägga till testet `test_weekend_surcharge` från README:n
och låt AI:n implementera den nya regeln.

## Övning 3: Agentisk kodning (15 min)

### Dina talking points

- **Tester först** — samma princip som utan AI, men AI skriver dem åt dig
- Utan tester vet vi inte om refaktoreringen bevarar beteendet
- Agentisk AI skiner vid **mekaniska uppgifter**: refaktorering, migrering
- Nyckeln: ge ett tydligt mål, låt agenten bestämma vägen
- AI:n skapar sin egen feedback-loop via tester
- Du är **granskare**, inte **skribent**

### Demo med Claude Code

Två steg — poängtera uppdelningen:

**Steg A: Tester först**
1. Läser hela weather_app.py
2. Skriver tester med mockade HTTP-anrop
3. Kör testerna — allt grönt
4. "Nu har vi ett skyddsnät. Först nu vågar vi ändra."

**Steg B: Refaktorera**
5. Planerar uppdelningen
6. Skapar modul för modul
7. Kör testerna efter varje ändring
8. Fixar det som gått sönder
9. Kör igen tills allt är grönt

Pausa och kommentera medan agenten jobbar.

### Instruktion till deltagare

Be dem göra samma sak med Gemini CLI — ge hela uppdraget
och titta på hur agenten arbetar.

## Avslutning (5 min)

### Tips att ta med sig

1. **Börja med kontext, inte prompts** — GEMINI.md/CLAUDE.md först
2. **Skriv tester, inte specifikationer** — tester är exakta och verifierbara
3. **Låt agenten iterera** — avbryt inte för tidigt
4. **Granska alltid** — AI är snabb men inte ofelbar
5. **Versionshantera allt** — du kan alltid gå tillbaka

### Resurser

- Gemini CLI: https://github.com/google-gemini/gemini-cli
- Claude Code: https://docs.anthropic.com/en/docs/claude-code
- Repot med övningarna: (dela länken)

### Frågestund

Ha 2-3 min för frågor. Vanliga:
- "Hur hanterar ni sekretess / företagsdata?" → Gemini CLI kör lokalt,
  men skickar prompts till Googles API. Samma för Claude Code → Anthropic.
  Kolla era policies.
- "Ersätter det kodgranskning?" → Nej, det *kompletterar*. AI skriver,
  människan granskar.
- "Vilket verktyg är bäst?" → Prova båda. Verktygens styrkor varierar.
