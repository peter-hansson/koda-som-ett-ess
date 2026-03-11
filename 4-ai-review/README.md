# Övning 4: AI-review mot teamets kodstandard

**Tid:** ~20 minuter

## Syfte

Visa ett realistiskt arbetsflöde: du skriver kod med AI, checkar in,
öppnar en pull request — och låter en *annan* AI granska ditt arbete.

## Steg 1: Forka repot (2 min)

1. Gå till repots GitHub-sida
2. Klicka **Fork** (övre högra hörnet)
3. Klona din fork:

```bash
git clone https://github.com/<ditt-användarnamn>/koda-som-ett-ess.git
cd koda-som-ett-ess
```

**Osäker på git?** Be AI:n hjälpa dig:

```
Jag har forkat repot koda-som-ett-ess på GitHub. Klona min fork,
skapa en branch som heter deltagare/<mitt-namn>/ovning4 och byt till den.
```

## Steg 2: Skapa en branch och bygg något (10 min)

```bash
git checkout -b deltagare/<ditt-namn>/ovning4
```

**Eller be AI:n:**

```
Skapa en ny branch som heter deltagare/<mitt-namn>/ovning4 och byt till den.
```

**Viktigt:** All kod måste ligga under `4-ai-review/` — annars
triggas inte den automatiska AI-reviewen på din PR.

Välj **en** av dessa uppgifter (eller hitta på en egen):

### Alt A: Förbättra väderappen

`4-ai-review/src/weather_app.py` är en rörig legacy-app.
Be AI:n refaktorera den — med tester som skyddsnät:

```
Läs src/weather_app.py. Skriv tester som täcker befintlig funktionalitet
(mocka HTTP-anropen). Refaktorera sedan till ren kodstruktur med separata
moduler, type hints och konstanter. Testerna ska passera efter varje ändring.
```

### Alt B: Utöka bokningssystemet

Kopiera bokningssystemet hit först:

```bash
cp -r ../3-tdd-med-ai/src ../3-tdd-med-ai/tests .
```

**Eller be AI:n:**

```
Kopiera src/ och tests/ från 3-tdd-med-ai/ till 4-ai-review/
```

Lägg sedan till en ny funktion. Idéer:
- Statistik (mest bokade bana, populäraste tiden)
- Exportera bokningar till CSV
- Sök bland bokningar

Skriv tester först, låt AI:n implementera.

### Alt C: Bygg något helt nytt

Skapa en ny mapp `4-ai-review/src/<ditt-namn>/` och
bygg vad du vill. Förslag:
- En quiz om AI-kodning
- En CLI som hämtar nyheter
- Ett spel i terminalen

## Steg 3: Committa och pusha (2 min)

```bash
git add .
git commit -m "Övning 4: <kort beskrivning av vad du byggt>"
git push -u origin deltagare/<ditt-namn>/ovning4
```

**Eller be AI:n:**

```
Committa alla ändringar med ett beskrivande meddelande och pusha till origin.
```

## Steg 4: Öppna en Pull Request (1 min)

1. Gå till din fork på GitHub
2. Klicka **"Compare & pull request"**
3. Rikta PR:n mot huvudrepot (`peter-hansson/koda-som-ett-ess`, branch `main`)
4. Skriv en kort beskrivning av vad du gjort

**Eller be AI:n:**

```
Skapa en pull request mot peter-hansson/koda-som-ett-ess main.
Skriv en kort beskrivning av ändringarna.
```

## Steg 5: Vänta på AI-reviewern (5 min)

När du öppnar din PR händer något automatiskt — en AI-reviewer startar
och granskar din kod mot teamets kodstandard.

Men vi granskar inte "i allmänhet". I [`kodstandard.md`](../material/ovning4-kodstandard/kodstandard.md) finns
"Solsidan Tech AB:s" interna kodstandard — precis som på ett riktigt utvecklingsbolag.
AI-reviewern läser den och granskar din kod mot *den*.

**Vad händer i bakgrunden:**

1. En GitHub Action triggas automatiskt på din PR
2. AI:n läser `kodstandard.md` och din diff
3. Den postar en utförlig review med konkreta avvikelser
4. Om koden inte följer standarden: **merge blockeras** med "Request changes"
5. Om allt ser bra ut: PR:n godkänns

**Kolla din PR på GitHub** — inom någon minut dyker reviewen upp.

**Fick du "Request changes"?** Åtgärda feedbacken och pusha igen:

```
Läs AI-reviewerns feedback på min PR. Åtgärda alla avvikelser från
kodstandarden och pusha en ny commit.
```

En ny review körs automatiskt på den uppdaterade koden.

**Vill du också testa manuellt?** Be Jarvis i Discord eller öppna en
ny AI-session:

```
Granska den här PR:n mot kodstandard.md: <länk>

Gå igenom varje punkt i guiden och rapportera:
- Vad som följer standarden
- Vad som bryter mot standarden (med radnummer)
- Konkreta förslag för att åtgärda avvikelserna
```

## Diskussion

- **Automatisk kvalitetssäkring** — AI-reviewern körs på varje PR, precis som CI-tester.
  Ingen kod kommer in som inte följer standarden.
- **Kontext styr granskningen** — utan kodstandard.md ger AI:n generisk feedback.
  Med guiden pekar den ut specifika avvikelser mot *ert* regelverk.
- **Samma princip som övning 1** — kontext förändrar allt, även vid granskning
- Samma AI som skrev koden hittar sällan sina egna misstag — byt session!
- Följde AI:n som *skrev* koden guiden automatiskt? Eller behövde granskaren påpeka avvikelser?
- Vad händer om du ger guiden till AI:n *innan* den skriver koden?
- PR-review med AI används redan i verkligheten (GitHub Copilot, CodeRabbit, etc.)
- AI ersätter inte mänsklig granskning — men den fångar saker du missar
