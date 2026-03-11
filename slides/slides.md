# Slides — Pass 2: Koda som ett ess med AI

En slide per övning. Fokus: metodikens vinster och vad deltagarna tar med sig.

---

## Övning 2: Kontext är kung

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│              KONTEXT > PROMPT                               │
│                                                             │
│  Samma prompt, olika resultat:                              │
│                                                             │
│  ┌─────────────────────┐   ┌──────────────────────────────┐ │
│  │  UTAN kontext        │   │  MED CLAUDE.md (15 rader)    │ │
│  │                      │   │                              │ │
│  │  - Engelska texter   │   │  + Svenska hjälptexter       │ │
│  │  - Inga type hints   │   │  + Type hints överallt       │ │
│  │  - Random struktur   │   │  + Tydliga exit-koder        │ │
│  │  - pip install X     │   │  + Bara standardbiblioteket  │ │
│  │  - Ingen felhantering│   │  + Docstrings, argparse      │ │
│  │                      │   │  + if __name__ == "__main__"  │ │
│  └─────────────────────┘   └──────────────────────────────┘ │
│                                                             │
│  VINSTER                                                    │
│  ─────────────────────────────────────────────────          │
│  - Teamet delar konventioner via git — alla får             │
│    samma AI-beteende, oavsett verktyg                       │
│  - 15 rader kontext > 150 rader prompt-engineering          │
│  - Ny regel i filen = AI följer den automatiskt             │
│    i alla framtida interaktioner                            │
│  - Onboarding för AI = onboarding för nya utvecklare        │
│                                                             │
│  METODIK: Investera 10 min i .md-filen,                     │
│           spara timmar i varje framtida session              │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**Talarpunkter:**
- CLAUDE.md / GEMINI.md / cursor-rules — samma konventioner, alla verktyg
- Kontexten skalar — skriv en gång, används hundratals gånger
- Steg 4 visar det tydligt: lägg till --json/--text-regeln, AI följer den utan att du nämner det
- Prompten är tillfällig, kontexten är bestående

---

## Övning 3: TDD med AI

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│          TESTER = KÖRBAR SPECIFIKATION                      │
│                                                             │
│  Traditionellt:             Med AI:                         │
│                                                             │
│  Människa skriver spec  →  Människa skriver TESTER          │
│  Människa implementerar →  AI implementerar                 │
│  Människa testar        →  AI kör + fixar + itererar        │
│  Människa fixar buggar  →  AI fixar tills grönt             │
│                                                             │
│  ┌─────────────────────────────────────────────────┐        │
│  │  26 tester ──→ AI läser ──→ implementerar       │        │
│  │       ↑                          │               │        │
│  │       └──── kör pytest ←─────────┘               │        │
│  │             (1-3 iterationer till grönt)          │        │
│  └─────────────────────────────────────────────────┘        │
│                                                             │
│  VINSTER                                                    │
│  ─────────────────────────────────────────────────          │
│  - Du fokuserar på VAD (affärsregler),                      │
│    AI:n på HUR (implementation)                             │
│  - Tester är den mest exakta prompten du kan skriva         │
│    — ingen tvetydighet, körbar, verifierbar                 │
│  - AI itererar snabbare än du: impl → test → fix → test    │
│  - Ny affärsregel? Skriv ETT test, AI implementerar        │
│  - Samma princip för logik OCH GUI (tkinter-tester)        │
│                                                             │
│  DEMO: 26 tester → booking.py på ~2 min                    │
│        13 UI-tester → booking_ui.py på ~1 min              │
│        test_weekend_surcharge → klar på 30 sek             │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**Talarpunkter:**
- Perfekt arbetsdelning: människa specificerar, AI implementerar
- Testerna ÄR specifikationen — inget "lost in translation"
- Agentisk iteration: AI gör, kör, ser fel, fixar, upprepar
- Steg 5: deltagaren skriver SJÄLV ett test — ser direkt att det funkar
- Fungerar för backend OCH GUI — tester definierar beteendet

---

## Övning 4: AI-review mot kodstandarden

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│        PROFESSIONELLT ARBETSFLÖDE MED AI                    │
│                                                             │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐   │
│  │ 1. FORK  │→ │ 2. KODA  │→ │ 3. PR    │→ │ 4. REVIEW│   │
│  │ + BRANCH │  │  med AI  │  │          │  │  av AI   │   │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘   │
│                     │                           │           │
│                     ▼                           ▼           │
│              AI som skribent             AI som granskare   │
│              (Gemini/Claude)          (Claude i GitHub Action)│
│                                                             │
│  TVÅSIDIGT AI-SKYDD                                        │
│  ─────────────────────────────────────────────────          │
│  - Skrivande AI följer kontextfilen (övning 2)             │
│  - Tester fångar logikfel (övning 3)                       │
│  - Granskande AI fångar stilfel mot kodstandard.md          │
│  - Merge blockeras vid avvikelser — människa godkänner     │
│                                                             │
│  VINSTER                                                    │
│  ─────────────────────────────────────────────────          │
│  - Automatisk kvalitetsgrind på varje PR                    │
│  - Kodstandarden genomdrivs konsekvent —                    │
│    ingen "det där låter vi gå den här gången"              │
│  - Olika AI vid skrivning vs granskning =                   │
│    fräscha ögon, fångar fler problem                       │
│  - Du är GRANSKARE, inte SKRIBENT — högre nivå             │
│  - AI ersätter inte mänsklig granskning —                   │
│    men den fångar det du missar                            │
│                                                             │
│  FLÖDE: fork → branch → koda → commit → PR →               │
│         AI-review → fixa → ny review → merge               │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**Talarpunkter:**
- Speglar riktigt utvecklararbetsflöde: fork → branch → PR → review
- kodstandard.md styr granskningen — utan den ger AI:n generisk feedback
- Samma princip som övning 2: kontext förändrar allt, även vid granskning
- AI:n som skrev koden hittar sällan sina egna misstag — byt AI-session!
- Övning 4 knyter ihop allt: kontext + TDD + agentiskt arbete + granskning

---

## Sammanfattning

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│              TRE PRINCIPER ATT TA MED                       │
│                                                             │
│                                                             │
│  1. KONTEXT FÖRST              (övning 2)                   │
│     15 rader i en .md-fil > clevera prompts                 │
│     Hela teamet får konsekvent AI-beteende                  │
│                                                             │
│                                                             │
│  2. TESTER SOM SPEC            (övning 3)                   │
│     Skriv vad du vill ha, låt AI skriva hur                 │
│     Människa specificerar, AI implementerar och itererar    │
│                                                             │
│                                                             │
│  3. AI SOM KOLLEGA, INTE VERKTYG   (övning 4)              │
│     Ge uppdraget, låt agenten jobba                         │
│     Du granskar — högre nivå, bättre resultat              │
│                                                             │
│                                                             │
│  Gemensam nämnare: du höjer dig från                        │
│  SKRIBENT till ARKITEKT och GRANSKARE                       │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```
