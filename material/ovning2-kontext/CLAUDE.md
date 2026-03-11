# Projektkonventioner

## Språk
- Alla CLI-hjälptexter och felmeddelanden på svenska
- Kod, variabelnamn och kommentarer på engelska

## Python-stil
- Python 3.12+
- Type hints på alla funktioner och returvärden
- Docstrings i Google-format
- Inga externa beroenden — använd bara standardbiblioteket
- Felhantering med specifika exit-koder (1 = användarfel, 2 = systemfel)
- `argparse` för CLI-argument med tydliga hjälptexter

## Struktur
- Alla CLI-appar har `if __name__ == "__main__":` -block
- En fil per verktyg, max 150 rader
- Funktioner ska göra en sak och vara testbara
