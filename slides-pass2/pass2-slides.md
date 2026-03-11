# Slides — Pass 2: Koda som ett ess med AI

Each slide is one exercise. Focus: methodology benefits and measurable gains.

---

## Slide 1: Kontext ar kung

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
│  │  - Engelska texter   │   │  + Svenska hjalptexter       │ │
│  │  - Inga type hints   │   │  + Type hints overallt       │ │
│  │  - Random struktur   │   │  + Tydliga exit-koder        │ │
│  │  - pip install X     │   │  + Bara standardbiblioteket  │ │
│  │  - Ingen felhantering│   │  + Docstrings, argparse      │ │
│  │                      │   │  + if __name__ == "__main__"  │ │
│  └─────────────────────┘   └──────────────────────────────┘ │
│                                                             │
│  VINSTER                                                    │
│  ─────────────────────────────────────────────────          │
│  - Teamet delar konventioner via git — alla far             │
│    samma AI-beteende, oavsett verktyg                       │
│  - 15 rader kontext > 150 rader prompt-engineering          │
│  - Ny regel i filen = AI foljer den automatiskt             │
│    i alla framtida interaktioner                            │
│  - Onboarding for AI = onboarding for nya utvecklare        │
│                                                             │
│  METODIK: Investera 10 min i .md-filen,                     │
│           spar timmar i varje framtida session               │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**Talarpunkter:**
- CLAUDE.md/GEMINI.md ar som `.editorconfig` fast for AI
- Kontext skalar — en gang skriven, anvands hundratals ganger
- Prompten ar tillfallig, kontexten ar bestående
- Teamkonventioner blir automatiskt genomdrivna

---

## Slide 2: TDD med AI

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│          TESTER = EXECUTABLE SPECIFICATION                  │
│                                                             │
│  Traditionellt:             Med AI:                         │
│                                                             │
│  Manniska skriver spec  →  Manniska skriver TESTER          │
│  Manniska implementerar →  AI implementerar                 │
│  Manniska testar        →  AI kor + fixar + itererar        │
│  Manniska fixar buggar  →  AI fixar tills gront             │
│                                                             │
│  ┌─────────────────────────────────────────────────┐        │
│  │  26 tester ──→ AI lasar ──→ implementerar       │        │
│  │       ↑                          │               │        │
│  │       └──── kor pytest ←─────────┘               │        │
│  │             (1-3 iterationer till gront)          │        │
│  └─────────────────────────────────────────────────┘        │
│                                                             │
│  VINSTER                                                    │
│  ─────────────────────────────────────────────────          │
│  - Du fokuserar pa VAD (affarsregler),                      │
│    AI:n pa HUR (implementation)                             │
│  - Tester ar den mest exakta prompten du kan skriva         │
│    — ingen tvetydighet, koerbar, verifierbar                │
│  - AI itererar snabbare an du: impl → test → fix → test    │
│  - Ny affarsregel? Skriv ETT test, AI implementerar         │
│  - 100% testtackning fran dag 1                             │
│                                                             │
│  DEMO: 26 tester → booking.py pa ~2 min                    │
│        Lagg till test_weekend_surcharge → klar pa 30 sek    │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**Talarpunkter:**
- Perfekt arbetsdelning: manniska specificerar, AI implementerar
- Testerna AR specifikationen — inget "lost in translation"
- Agentisk iteration: AI gor, kor, ser fel, fixar, upprepar
- Lagg till en ny regel = skriv ett test + "fix it"
- Deltagarna ser AI:n arbeta i realtid

---

## Slide 3: Agentisk kodning och AI-review

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│        PROFESSIONELLT ARBETSFLODE MED AI                    │
│                                                             │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐   │
│  │ 1. BRANCH│→ │ 2. KODA  │→ │ 3. PR    │→ │ 4. REVIEW│   │
│  │          │  │  med AI  │  │          │  │  av AI   │   │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘   │
│                     │                           │           │
│                     ▼                           ▼           │
│              AI som skribent             AI som granskare   │
│              (Gemini/Claude)             (Claude i CI/CD)   │
│                                                             │
│  TVASIDIGT AI-SKYDD                                         │
│  ─────────────────────────────────────────────────          │
│  - Skrivande AI foljer kontextfilen (ovning 1)              │
│  - Tester fangar logikfel (ovning 2)                        │
│  - Granskande AI fangar stilfel mot kodstandarden            │
│  - Manniskan gor slutgranskning                             │
│                                                             │
│  VINSTER                                                    │
│  ─────────────────────────────────────────────────          │
│  - AI skiner vid mekaniska uppgifter:                       │
│    refaktorering, tester, migrering                         │
│  - Du ar GRANSKARE, inte SKRIBENT — hogre niva              │
│  - Automatisk kvalitetsgrind i CI/CD                        │
│  - Kodstandarden genomdrivs konsekvent —                    │
│    ingen "det dar later vi ga den har gangen"               │
│  - Olika AI:er vid skrivning vs granskning =                │
│    farska ogon, fangar fler problem                         │
│                                                             │
│  RESULTAT: fran messy legacy-kod till ren struktur          │
│            med tester — pa 10 minuter                       │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**Talarpunkter:**
- Speglar riktigt utvecklararbetsflode: branch → kod → PR → review
- Tester forst skapar skyddsnat innan refaktorering
- AI-review mot kodstandard ar som en extra senior kollega som aldrig ar trott
- Merge-block vid regelbrott — kvalitet genomdrivs automatiskt
- Ovning 3 knyter ihop allt: kontext + TDD + agentiskt arbete + granskning

---

## Sammanfattningsslide

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│              TRE PRINCIPER ATT TA MED                       │
│                                                             │
│                                                             │
│  1. KONTEXT FORST                                           │
│     15 rader i en .md-fil > clevera prompts                 │
│     Hela teamet far konsekvent AI-beteende                  │
│                                                             │
│                                                             │
│  2. TESTER SOM SPEC                                         │
│     Skriv vad du vill ha, lat AI skriva hur                 │
│     Manniska specificerar, AI implementerar och itererar     │
│                                                             │
│                                                             │
│  3. AI SOM KOLLEGA, INTE VERKTYG                            │
│     Ge uppdraget, lat agenten jobba                         │
│     Du granskar — hogre niva, battre resultat               │
│                                                             │
│                                                             │
│  Gemensam namnare: du hojer dig fran                        │
│  SKRIBENT till ARKITEKT och GRANSKARE                       │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```
