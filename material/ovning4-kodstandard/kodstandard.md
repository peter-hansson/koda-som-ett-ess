# Developer Guide — Solsidan Tech AB

Intern utvecklingsguide. Alla kodändringar granskas mot detta dokument.

## Vår stack

- **Backend:** Python 3.12+, FastAPI för API:er, SQLAlchemy för databas
- **Frontend:** Inte relevant för detta projekt
- **Test:** pytest, 80% kodtäckning krävs för merge
- **CI:** GitHub Actions, alla tester måste vara gröna före merge
- **Pakethantering:** `uv` (aldrig `pip install` direkt)
- **Linting:** `ruff` för linting och formattering
- **Type-checking:** `mypy` i strict mode

## Verktyg

### uv — pakethantering

- Alla projekt har `pyproject.toml` — aldrig `requirements.txt`
- `uv venv` för virtuella miljöer
- `uv pip install` för paket — aldrig systemets pip
- `uv run` för att köra skript i rätt miljö

### ruff — linting och formattering

- Ruff ersätter flake8, isort och black
- Kör `ruff check .` före commit — inga varningar tillåtna
- Kör `ruff format .` för automatisk formattering
- Konfiguration i `pyproject.toml`:

```toml
[tool.ruff]
line-length = 80
target-version = "py312"

[tool.ruff.lint]
select = [
    "E",    # pycodestyle errors
    "W",    # pycodestyle warnings
    "F",    # pyflakes
    "I",    # isort
    "N",    # pep8-naming
    "UP",   # pyupgrade
    "B",    # flake8-bugbear
    "S",    # flake8-bandit (säkerhet)
    "SIM",  # flake8-simplify
]

[tool.ruff.lint.isort]
known-first-party = ["src"]
```

### mypy — typkontroll

- Kör `mypy src/` före commit
- Strict mode — inga `Any` utan motivering
- Konfiguration i `pyproject.toml`:

```toml
[tool.mypy]
python_version = "3.12"
strict = true
warn_return_any = true
warn_unused_configs = true
```

### pytest — test

- `pytest -v --tb=short` som standard
- `pytest --cov=src --cov-report=term-missing` för täckningsrapport
- Minst 80% täckning krävs

## Kodstil

### Python

- Type hints på **alla** funktioner — parametrar och returvärden
- Docstrings i Google-format på alla publika funktioner
- Max 80 tecken per rad (undantag: URL:er och långa strängar)
- Variabelnamn och funktioner på **engelska**
- Användargränssnitt och felmeddelanden på **svenska**
- Inga envariabelnamn utom `i`, `j`, `k` i korta loopar
- `pathlib.Path` istället för `os.path`
- f-strängar, aldrig `format()` eller `%`-formattering
- Undvik `Any` — var specifik med typer

### Namnkonventioner

- Funktioner: `snake_case`
- Klasser: `PascalCase`
- Konstanter: `SCREAMING_SNAKE_CASE`
- Privata attribut: `_leading_underscore`
- Inga förkortningar — `temperature` inte `temp`, `configuration` inte `cfg`

### Imports

- Standardbibliotek först, sedan tredjepartsbibliotek, sedan lokala moduler
- En import per rad
- Absoluta imports — aldrig relativa (`from src.models import Booking`, inte `from .models import Booking`)

## Arkitektur

### Filstruktur

```
src/
├── models/          # Dataklasser och domänobjekt
├── services/        # Affärslogik
├── api/             # Endpoints (om API)
├── cli/             # CLI-kommandon (om CLI)
└── utils/           # Gemensamma hjälpfunktioner
tests/
├── test_models/
├── test_services/
└── conftest.py      # Delade fixtures
```

- En klass per fil (undantag: små relaterade dataklasser)
- Max 200 rader per fil — dela upp vid behov
- Ingen affärslogik i CLI- eller API-lagret

### Separation of concerns

- **Models:** Ren data, inga sidoeffekter, inga importer av externa tjänster
- **Services:** Affärslogik, kan använda models och utils
- **CLI/API:** Bara input/output — anropar services, formaterar resultat
- Beroenden injiceras, aldrig hårdkodade

## Felhantering

- Egna exception-klasser som ärver från en gemensam `AppError`
- Aldrig `except Exception` eller bara `except:` — fånga specifika fel
- Felmeddelanden på svenska, riktade till användaren
- Logga tekniska detaljer med `logging`, visa användarvänliga meddelanden
- Exit-koder: 0 = ok, 1 = användarfel, 2 = systemfel

## Test

### Krav

- Alla publika funktioner ska ha minst ett test
- Edge cases: tomma listor, None-värden, gränsvärden
- Mocka externa beroenden (HTTP, databas, filsystem)
- Testnamn beskriver beteendet: `test_reject_booking_outside_hours`, inte `test_booking_3`

### Struktur

```python
def test_<vad_som_testas>():
    """Beskrivning av förväntat beteende."""
    # Arrange — förbered data
    system = BookingSystem()

    # Act — utför handlingen
    result = system.book(...)

    # Assert — kontrollera resultatet
    assert result.price == 100
```

- Varje test testar **en** sak
- Inga beroenden mellan tester
- Fixtures i `conftest.py` för delad setup

## Säkerhet

- Aldrig hårdkodade lösenord, API-nycklar eller secrets i kod
- Validera all extern input (användarinput, API-svar, filinnehåll)
- SQL-parametrar via parametriserade queries, aldrig strängkonkatenering
- Inga `eval()`, `exec()`, eller `subprocess.shell=True`

## Git och PR

### Commits

- Imperativ form: "Lägg till bokningsvalidering", inte "La till..." eller "Lägger till..."
- En commit per logisk ändring
- Max 72 tecken i commit-rubriken

### Pull requests

- Titel: kort beskrivning av ändringen
- Beskrivning: vad, varför, och hur man testar
- Alla tester gröna
- Minst en godkänd review före merge

## Kodlukt — saker vi inte accepterar

- **Magiska värden** — använd namngivna konstanter
- **Copy-paste-kod** — extrahera till funktion
- **Globalt state** — använd klasser eller dependency injection
- **Kommenterad kod** — ta bort, vi har git
- **Print-debugging** — använd `logging`
- **Onödig komplexitet** — enklaste lösningen som fungerar
