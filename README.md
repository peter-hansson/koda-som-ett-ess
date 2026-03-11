# Koda som ett ess med AI

Praktisk workshop om de bästa teknikerna med moderna AI-kodverktyg.

## Upplägg

| Pass | Vem | Innehåll |
|------|-----|----------|
| **Pass 1** (~60 min) | Peter | FakturaHanteraren: analysera, refaktorera, testa, dokumentera |
| Middag | | ~30 min |
| **Pass 2** (~75 min) | Anders | Kontext, TDD, agentisk kodning |

## Verktyg

| Verktyg | Roll |
|---------|------|
| **Gemini CLI** | Deltagarna kör detta på sina arbetsstationer |
| **Claude Code** | Demonstreras live av instruktören |
| **ClawBot Jarvis via Discord** | Deltagarna chattar med en interaktiv AI-bot i Discord |

Alla verktygen är *agentiska* — de läser, skriver och kör kod självständigt.

### ClawBot Jarvis — Discord

ClawBot Jarvis är en interaktiv AI-bot som är kopplad till detta repo
och tillgänglig via Discord. Ställ frågor om övningarna, be om hints,
eller diskutera kod — Jarvis har full kontext om workshopmaterialet.

**Gå med:** <https://discord.gg/Dnr3865X>

## Övningarna

| # | Övning | Beskrivning |
|---|--------|-------------|
| 1 | [Fakturasystemet](1-fakturasystemet/) | Peters C#-app — "ny på jobbet"-scenariot |
| 2 | [Kontext är kung](2-kontext-ar-kung/) | Lär dig ge AI rätt kontext |
| 3 | [TDD med AI](3-tdd-med-ai/) | Skriv tester, låt AI implementera |
| 4 | [AI-review](4-ai-review/) | Fork, bygg, öppna PR, låt AI granska |

## Agenda — Pass 2

| Tid | Block | Innehåll |
|-----|-------|----------|
| 0:00 | Intro | Kort recap + Discord-setup |
| 0:05 | **Övning 2** | Kontext är kung — GEMINI.md / CLAUDE.md |
| 0:25 | **Övning 3** | TDD med AI — låt AI implementera från tester |
| 0:45 | **Övning 4** | AI-review mot teamets kodstandard |
| 1:05 | Avslutning | Tips, diskussion, frågor |

## Format

Varje övning följer samma mönster:
1. Instruktören visar på storskärm
2. Deltagarna kör själva
3. Diskussion — "Fick ni olika resultat?" (AI:n är inte deterministisk)

## Förutsättningar

- Python 3.12+
- `uv` (Python package manager) — <https://docs.astral.sh/uv/>
- Gemini CLI installerad och konfigurerad — har en gratiskvot som kan räcka för workshopen
- En terminal och en texteditor
- Valfritt AI-verktyg fungerar (Gemini CLI, Claude Code, Cursor, etc.)

## Kom igång

```bash
git clone https://github.com/peter-hansson/koda-som-ett-ess.git
cd koda-som-ett-ess

# Installera beroenden (för övning 3 och 4)
uv venv
source .venv/bin/activate  # Linux/macOS
# .venv\Scripts\activate   # Windows
uv pip install pytest
```

## Nyckelinsikter

- **Kontext > prompt** — En bra GEMINI.md/CLAUDE.md är viktigare än perfekta prompts
- **Tester är specifikationer** — AI läser dina tester som en spec och implementerar därefter
- **Agentisk = iterativ** — AI:n gör, kör, ser fel, fixar, upprepar. Låt den jobba.
- **Granska alltid** — AI är din snabbaste kollega, men du är fortfarande ansvarig
