# Övning 2: Kontext är kung

**Tid:** ~15 minuter

## Syfte

Visa hur *kontext* (inte bara prompten) avgör kvaliteten på AI-genererad kod.
Samma uppgift — två helt olika resultat.

## Steg 1: Utan kontext (2 min)

Öppna ditt AI-verktyg i en **tom mapp** och skriv:

```
Bygg en CLI-app i Python som genererar lösenord
```

Titta på resultatet. Det fungerar troligen — men följer det några specifika konventioner?

## Steg 2: Med kontext (5 min)

Skapa en ny mapp och lägg till en kontextfil för ditt verktyg:

```bash
mkdir losenord-app && cd losenord-app

# Gemini CLI:
mkdir -p .gemini && cp ../../material/ovning2-kontext/GEMINI.md .gemini/GEMINI.md

# Claude Code:
cp ../../material/ovning2-kontext/CLAUDE.md ./CLAUDE.md

# Cursor:
mkdir -p .cursor && cp ../../material/ovning2-kontext/cursor-rules .cursor/rules
```

**Eller be AI:n:**

```
Skapa en ny mapp som heter losenord-app. Kopiera rätt kontextfil
från material/ovning2-kontext/ till mappen (GEMINI.md, CLAUDE.md
eller cursor-rules beroende på vilket verktyg jag kör).
```

Ge nu **exakt samma prompt**:

```
Bygg en CLI-app i Python som genererar lösenord
```

## Steg 3: Jämför (3 min)

Jämför de två resultaten. Med kontextfilen borde du se:

- [x] `argparse` med tydliga hjälptexter (på svenska)
- [x] Felhantering med specifika exit-koder
- [x] Type hints överallt
- [x] Docstrings i Google-format
- [x] `if __name__ == "__main__":` -block
- [x] Inga externa beroenden

## Steg 4: Utöka (5 min)

Lägg till en ny regel i kontextfilen:

```
Lägg till följande i kontextfilen:
Alla CLI-appar ska stödja både --json och --text output-format.
Standardformat är text. JSON-output ska vara giltig JSON på stdout.
```

Be sedan AI:n:

```
Lägg till en --length flagga och en --no-special flagga
```

Se hur AI:n automatiskt följer JSON/text-konventionen utan att du behöver nämna det.

## Diskussion

- Kontextfilen är som en **onboarding-guide** för din AI-kollega
- Den behöver inte vara lång — 10-20 rader räcker
- Uppdatera den när konventioner ändras
- Alla i teamet delar samma kontextfil via git
