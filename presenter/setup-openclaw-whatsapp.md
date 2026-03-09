# Setup: OpenClaw + WhatsApp för workshopen

## Översikt

```
Deltagare (WhatsApp) → OpenClaw → Claude API → Repot (git)
```

Deltagarna skickar meddelanden i WhatsApp. OpenClaw tar emot dem,
skickar till Claude med repo-kontext, och svarar tillbaka.

## 1. Installera OpenClaw

```bash
# Klona och installera
git clone https://github.com/anthropics/openclaw.git
cd openclaw
npm install  # eller bun install
```

## 2. Konfigurera API-nycklar

```bash
cp .env.example .env
```

Redigera `.env`:
```env
ANTHROPIC_API_KEY=sk-ant-...
WHATSAPP_PHONE_NUMBER_ID=<från Meta Business>
WHATSAPP_ACCESS_TOKEN=<från Meta Business>
WHATSAPP_VERIFY_TOKEN=<valfri hemlig sträng>
```

## 3. WhatsApp Business API

För att ta emot meddelanden behövs Meta Business-konto:

1. Gå till https://developers.facebook.com
2. Skapa en app → Välj "Business" → Lägg till "WhatsApp"
3. Under WhatsApp > Getting Started: notera Phone Number ID och Access Token
4. Konfigurera webhook-URL till din OpenClaw-instans

**Alternativ (enklare för workshop):**
Använd en WhatsApp-bridge som `whatsapp-web.js` via OpenClaws
community-skill `whatsapp-bridge`. Kräver bara en QR-scan från
din telefon — ingen Meta Business-setup.

## 4. Koppla till repot

Konfigurera OpenClaw att ha tillgång till workshop-repot:

```yaml
# openclaw.config.yaml
skills:
  - git:
      repo_path: /path/to/koda-som-ett-ess
      allowed_operations: [read, write, branch, commit]

system_prompt: |
  Du är en AI-assistent för workshopen "Koda som ett ess med AI".
  Du har tillgång till workshop-repot med tre övningar:
  1. Kontext är kung (GEMINI.md/CLAUDE.md)
  2. TDD med AI (padelbanebokning)
  3. Agentisk kodning (refaktorering av weather_app.py)

  Svara på svenska. Hjälp deltagarna med övningarna.
  Ge hints snarare än kompletta lösningar.
  Du kan läsa och visa filer från repot.
```

## 5. Starta

```bash
# Starta OpenClaw
openclaw start --config openclaw.config.yaml
```

## 6. Testa

Skicka ett meddelande från din telefon:
- "Vad innehåller det här repot?"
- "Visa mig testerna i övning 2"
- "Hjälp mig förstå varför mitt test misslyckas"

## Tips för workshopen

- **Begränsa svarslängden** — WhatsApp-meddelanden ska vara korta
- **Rate-limitering** — Sätt en limit per deltagare så API-kostnaderna inte skenar
- **Monitorera** — Ha OpenClaw-loggen synlig på din maskin
- **Fallback** — Om WhatsApp strular, kan deltagarna använda Gemini CLI direkt
