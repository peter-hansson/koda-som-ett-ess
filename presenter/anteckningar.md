# Anders anteckningar — Pass 2 (efter middag)

Peters pass (före middag) tar "ny på jobbet"-scenariot: förklara kodbaser
och hitta/korrigera fel. Ditt pass bygger vidare med avancerade tekniker.

## Före sessionen

- [ ] Testa att Gemini CLI fungerar på en testmaskin
- [ ] Ha Claude Code redo med API-nyckel konfigurerad
- [ ] Ha ClawBot Jarvis igång i Discord-servern
- [ ] Testa att boten svarar i Discord
- [ ] Förbered Discord-inbjudningslänk att visa på skärmen: https://discord.gg/Dnr3865X
- [ ] Klona repot till en ren mapp för demon
- [ ] Ha en backup-lösning om nätet laggar (inspelade demos?)

## Intro (5 min)

**Kort recap — knyt an till Peters pass:**
- Peter visade hur AI hjälper dig *förstå* kod och *hitta fel*
- Nu: hur du får AI att jobba *med* dig — kontext, TDD, agentiska flöden
- "Peter visade AI som förklarare. Jag visar AI som medarbetare."

**Visa Discord-kopplingen:**
- Visa inbjudningslänk på skärmen: https://discord.gg/Dnr3865X
- "Gå med i Discord-servern — ni kan chatta med Jarvis under hela passet!"

### ClawBot Jarvis via Discord — deltagarinteraktion

Deltagarna chattar med ClawBot Jarvis i Discord under hela sessionen.
Jarvis har kontext om workshop-repot och kan:

- Svara på frågor om övningarna
- Förklara kod i repot
- Ge hints när någon kört fast

**Setup före sessionen:**
1. Verifiera att ClawBot Jarvis är aktiv i Discord-servern
2. Visa inbjudningslänken på projektorskärmen
3. Testa att ett meddelande i kanalen får svar

**Live-demo under introt (2 min):**
1. Öppna Discord (visa på skärmen)
2. Skriv: "Vad innehåller det här repot?"
3. Jarvis svarar med sammanfattning av övningarna
4. Skriv: "Visa mig testerna i övning 3"
5. Jarvis visar test_booking.py
6. "Alla kan göra samma sak — gå med via länken!"

**Under övningarna:**
- Uppmuntra deltagare att ställa frågor i Discord
- "Skriv till Jarvis om ni kört fast — det är som att ha en extra instruktör"
- Visa intressanta interaktioner på storskärm

**Varför Discord:**
- Lättillgängligt — fungerar i browser utan installation
- Alla deltagare ser varandras frågor och svar
- Visar att AI-kodning inte kräver terminal-skills
- Sänker tröskeln: "AI-hjälpen är en chatt bort"

## Övning 2: Kontext är kung (15 min)

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

## Övning 3: TDD med AI (15 min)

### Dina talking points

- Tester är den **mest exakta prompten** du kan skriva
- AI:n itererar snabbare än du: implementera → kör → fixa → kör
- Du fokuserar på *vad* (tester), AI:n på *hur* (implementation)
- Perfekt arbetsdelning: människa specificerar, AI implementerar

### Instruktion till deltagare

1. "Öppna 3-tdd-med-ai/"
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

## Övning 4: AI-review mot teamets kodstandard (20 min)

### Dina talking points

- **Automatisk AI-review** — en GitHub Action granskar varje PR automatiskt
- AI:n läser teamets kodstandard och blockerar merge om koden inte följer den
- Samma mönster som linting i CI — men för kodstil, arkitektur och säkerhet
- Samma AI som skrev koden hittar sällan sina egna misstag
- PR-review med AI är redan verklighet (GitHub Copilot, CodeRabbit)
- Git-flödet: fork → branch → commit → PR → automatisk review — så jobbar man på riktigt

### Instruktion till deltagare

**Steg-för-steg på skärmen:**

1. "Forka repot — klicka Fork på GitHub" (visa)
2. "Skapa en branch: `deltagare/<ert-namn>/ovning3`"
3. "Välj en uppgift — refaktorera väderappen, utöka bokningssystemet, eller bygg något eget"
4. "Låt AI:n jobba. Committa och pusha när ni är nöjda."
5. "Öppna en PR mot huvudrepot"
6. "Vänta... titta vad som händer på er PR!"

**Twist — AI-reviewern slår till:**

7. Inom någon minut dyker en utförlig review upp på PR:n
8. "Det här är Solsidan Tech AB:s AI-reviewer. Den har läst teamets kodstandard
   och granskar er kod mot den — automatiskt."
9. Visa kodstandard.md på skärmen — "Det här är reglerna den granskar mot."
10. "Fick ni 'Request changes'? Då blockeras merge. Fixa avvikelserna och pusha igen!"

### Demo med Claude Code

Kort demo medan deltagarna väntar på sin review (3 min):
1. Visa en befintlig PR (förbered en som *medvetet bryter* mot standarden)
2. Visa hur AI-reviewern automatiskt postat "Request changes"
3. Gå igenom feedbacken — specifika radnummer, citat ur kodstandarden
4. Visa att merge-knappen är blockerad

**Poängtera:**
- "Det här är som en linting-check — men den förstår *kontext*, inte bara syntax"
- "Den ser att `temp` bryter mot namnkonventionen, att `except:` utan
  specifikt fel inte är tillåtet, att testtäckningen saknas"
- Samma princip som övning 1 — kontext förändrar allt
- "Tänk er det här i ert team: varje PR granskas automatiskt mot er egen standard"

**Diskussionsfrågor efteråt:**
- "Följde AI:n som *skrev* koden standarden automatiskt? Eller fick ni 'Request changes'?"
- "Vad händer om ni ger kodstandarden till AI:n *innan* den skriver koden?"
- "Skulle ni använda det här i ert team?"

**Hjälp de som fastnar med git:**
- Visa fork + clone-flödet steg för steg
- Ha kommandon redo på en slide/i chatten
- Alternativ: de som inte vill forka kan visa sin kod direkt i Discord

### Förberedelser

- [ ] Skapa en demo-PR i förväg som medvetet bryter mot kodstandard.md
- [ ] Verifiera att GitHub Action `ai-review.yml` fungerar (kräver `ANTHROPIC_API_KEY` som repo secret)
- [ ] Testa att reviewen postar korrekt och blockerar merge
- [ ] Verifiera att Jarvis kan läsa GitHub-PR:er via länk (backup om Action inte funkar)
- [ ] Ha backup: om git-flödet tar för lång tid, låt deltagarna klistra kod direkt i Discord för review

## Diskussionspunkter — använd efter varje övning

### "AI är inte deterministisk"
- Fråga deltagarna: "Fick ni olika resultat?"
- Poängen: samma prompt → olika kod. Därför är granskning nödvändig.
- Bra diskussionsöppnare efter varje övning.

### Jämföra AI:er
- Idé: låt Gemini och Claude göra samma uppgift, jämför resultaten
- Visar att verktygets styrkor varierar — det finns inget "bästa" verktyg

### Remote Control (Claude Code)
- Demo-möjlighet: visa hur man fjärrstyr Claude Code via mobilappen
- "Jag kan ge instruktioner från telefonen medan CLI:n kör på laptopen"

### ClawBot Jarvis som timekeeper
- Jarvis kan rapportera tid och status i Discord-kanalen
- Användbart för att hålla koll på tidsbudgeten under workshopen

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
