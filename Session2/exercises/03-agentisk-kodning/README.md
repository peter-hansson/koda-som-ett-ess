# Övning 3: Agentisk kodning

**Tid:** ~15 minuter

## Syfte

Visa hur en AI-agent självständigt kan driva större förändringar:
läsa kod, skapa filer, skriva tester, köra dem, och fixa fel — allt i ett svep.

## Scenario

`src/weather_app.py` är en "legacy"-app skriven av en stressad utvecklare.
Allt ligger i en fil: API-anrop, databearbetning, formattering och utskrift.
Inga tester. Inga type hints. Hårdkodade värden.

Din uppgift: låt AI:n först säkra beteendet med tester, sedan refaktorera.

## Steg 1: Läs igenom koden (2 min)

```bash
cat src/weather_app.py
```

Notera problemen:
- Allt i en fil, blandade ansvar
- **Inga tester** — vi vet inte om något går sönder
- Magiska värden
- Copy-paste-kod
- Ingen felhantering

## Steg 2: Låt AI:n skriva tester först (5 min)

Innan vi rör koden måste vi ha tester som skyddsnät:

```
Läs src/weather_app.py och skriv tester som täcker all befintlig
funktionalitet. Mocka bort HTTP-anropen så att testerna kan köras
offline. Kör testerna och verifiera att alla passerar.
```

**Varför tester först?**
- Utan tester vet vi inte om refaktoreringen bevarar beteendet
- Testerna blir vår "kontrakt" — de definierar vad koden *ska* göra
- Samma princip som i övning 2, men nu skriver AI:n *både* tester och kod

## Steg 3: Refaktorera med testskyddsnät (5 min)

Nu när testerna är gröna, ge refaktoreringsuppdraget:

```
Refaktorera src/weather_app.py till en ren kodstruktur:

1. Dela upp i separata moduler (api, models, formatters)
2. Lägg till type hints och docstrings
3. Ersätt magiska värden med konstanter

Testerna måste fortfarande passera efter refaktoreringen.
Kör dem efter varje ändring.
```

**Titta på hur agenten arbetar:**
- Den läser först hela filen och de befintliga testerna
- Planerar uppdelningen
- Skapar moduler en efter en
- Kör testerna efter varje steg
- Fixar det som inte fungerar
- Kör testerna igen

## Steg 4: Granska resultatet (3 min)

```bash
# Se vilka filer som skapades
find . -name "*.py" | head -20

# Kör testerna
python -m pytest -v
```

Diskutera:
- Är uppdelningen vettig?
- Skulle du gjort det annorlunda?
- Vad sparade du tid på? Vad måste du fortfarande granska manuellt?

## Diskussion

- **Tester först, refaktorera sedan** — samma princip som utan AI
- Agentisk AI skiner vid **mekaniska uppgifter** med tydliga mål
- Tester ger agenten en **feedback-loop** — den kan verifiera sitt eget arbete
- Du styr *vad* som ska göras, agenten bestämmer *hur*
