# Övning 2: TDD med AI

**Tid:** ~15 minuter

## Syfte

Visa hur tester fungerar som **specifikationer** för AI.
Du skriver testerna — AI:n implementerar koden.

## Scenario

Du bygger ett bokningssystem för padelbanor. Affärsreglerna är:

- Bokningar är 1 timme, mellan 07:00–22:00
- Ingen dubbelbokning på samma bana
- Max 2 bokningar per person och dag
- Avbokning måste ske minst 2 timmar före
- Högtrafik (17:00–20:00) kostar 200 kr, övrig tid 100 kr

Alla regler finns redan **kodade som tester** i `tests/test_booking.py`.

## Steg 1: Utforska testerna (3 min)

```bash
cd exercises/02-tdd-med-ai
cat tests/test_booking.py
```

Läs igenom testerna. Notera hur varje affärsregel är ett testfall.

## Steg 2: Kör testerna — allt ska misslyckas (1 min)

```bash
cd exercises/02-tdd-med-ai
python -m pytest tests/ -v
```

Alla tester ska vara röda (FAILED). Det är meningen.

## Steg 3: Låt AI implementera (8 min)

Be AI:n att implementera koden så att testerna passerar:

```
Läs testerna i tests/test_booking.py och implementera src/booking.py
så att alla tester passerar. Kör pytest för att verifiera.
```

Titta på hur AI:n:
1. Läser testerna
2. Förstår affärsreglerna
3. Skriver implementationen
4. Kör testerna
5. Fixar eventuella fel och kör igen

## Steg 4: Lägg till en ny regel (3 min)

Skriv **själv** ett nytt test i `tests/test_booking.py`:

```python
def test_weekend_surcharge():
    """Helger har 50% prispåslag"""
    system = BookingSystem()
    # En lördag
    booking = system.book("Bana 1", "anna@test.se",
                          datetime(2026, 3, 14, 10, 0))  # lördag
    assert booking.price == 150  # 100 * 1.5
```

Be sedan AI:n att implementera stöd för den nya regeln.

## Diskussion

- Tester är den **bästa prompten** — de är exakta och testbara
- AI:n itererar: implementera → kör → fixa → kör igen
- Du behöver inte specificera *hur* — bara *vad* (via tester)
- Kombination: du skriver tester, AI implementerar = snabbt och säkert
