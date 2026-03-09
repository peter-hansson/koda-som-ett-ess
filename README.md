# Koda som ett ess med AI

Praktisk workshop (60 min) om de bästa teknikerna med moderna AI-kodverktyg.

## Upplägg

| Pass | Vem | Innehåll |
|------|-----|----------|
| **Pass 1** (60 min) | Peter | AI som kodförklarare och felsökare |
| Middag | | |
| **Pass 2** (60 min) | Anders | Avancerade tekniker — kontext, TDD, agentisk kodning |

Det här repot innehåller material för **Pass 2 (Anders)**.

## Verktyg

| Verktyg | Roll |
|---------|------|
| **Gemini CLI** | Deltagarna kör detta på sina arbetsstationer |
| **Claude Code** | Demonstreras live av instruktören |
| **OpenClaw via WhatsApp** | Deltagarna chattar med Claude direkt i WhatsApp |

Alla verktygen är *agentiska* — de läser, skriver och kör kod självständigt.

### OpenClaw + WhatsApp

[OpenClaw](https://github.com/anthropics/openclaw) är en öppen källkods-agent
som använder Claude som backend. Under workshopen är OpenClaw kopplad till
detta repo och tillgänglig via WhatsApp. Skicka ett meddelande så svarar
Claude med full kontext om övningarna.

**Gå med:** Scanna QR-koden på skärmen eller lägg till numret i WhatsApp.

## Agenda

### Peters pass (före middag)

- "Ny på jobbet" — låt AI förklara en obekant kodbas
- Hitta och korrigera fel i befintlig kod

### Anders pass (efter middag)

| Tid | Block | Innehåll |
|-----|-------|----------|
| 0:00 | Intro | Kort recap + WhatsApp-setup |
| 0:05 | **Övning 1** | Kontext är kung — GEMINI.md / CLAUDE.md |
| 0:20 | **Övning 2** | TDD med AI — låt AI implementera från tester |
| 0:35 | **Övning 3** | Agentisk kodning — refaktorera legacy-kod |
| 0:50 | Avslutning | Tips, diskussion, frågor |

## Förutsättningar

- Python 3.12+
- `uv` (Python package manager) — <https://docs.astral.sh/uv/>
- Gemini CLI installerad och konfigurerad
- En terminal och en texteditor

## Kom igång

```bash
# Klona repot
git clone <repo-url>
cd koda-som-ett-ess

# Skapa en virtuell miljö
uv venv
source .venv/bin/activate  # Linux/macOS
# .venv\Scripts\activate   # Windows

# Installera beroenden (för övning 2 och 3)
uv pip install pytest
```

## Övningarna

1. [Kontext är kung](exercises/01-kontext-ar-kung/) — Lär dig ge AI rätt kontext
2. [TDD med AI](exercises/02-tdd-med-ai/) — Skriv tester, låt AI implementera
3. [Agentisk kodning](exercises/03-agentisk-kodning/) — Låt AI driva större förändringar

## Nyckelinsikter

- **Kontext > prompt** — En bra GEMINI.md/CLAUDE.md är viktigare än perfekta prompts
- **Tester är specifikationer** — AI läser dina tester som en spec och implementerar därefter
- **Agentisk = iterativ** — AI:n gör, kör, ser fel, fixar, upprepar. Låt den jobba.
- **Granska alltid** — AI är din snabbaste kollega, men du är fortfarande ansvarig
