# Facit: Koda som ett Ess (Instruktörsguide)

Detta dokument innehåller de förväntade resultaten och kodändringarna för övningarna i workshoppen.

---

## Övning 2: Skills (Lösning)

Deltagarna bör lära sig att `activate_skill` ger Gemini tillgång till specifika instruktioner.
- Svar på "Vilka skills finns?": Gemini listar t.ex. `azure-prepare`, `microsoft-foundry`, `skill-creator`, etc.
- Vid aktivering av `azure-prepare`: Gemini kommer börja ställa frågor om projektet (språk, databas, etc.) för att generera Bicep-filer eller `azure.yaml`.

## Övning 3: MCP (Lösning)

- **Google Search:** Gemini använder `google_web_search` för att hitta aktuell information.
- **Web Fetch:** Gemini använder `web_fetch` för att läsa innehåll från URL:er (t.ex. rå kod från GitHub).
- **Diskussion:** Förklara att MCP-servrar tillåter Gemini att agera på riktig data i realtid, snarare än att bara lita på sin träningsdata.

## Övning 4: Refactoring & Typer (Lösning)

Efter att ha bett Gemini om typer och refaktorering bör `src/inventoryService.ts` se ut ungefär så här:

```typescript
export interface Product {
  id: number;
  name: string;
  category: string;
  price: number;
  stock: number;
}

// ... products-arrayen bör nu använda Product-interfacet

export function searchProducts(query: string): Product[] {
  if (!query) return products;

  const regex = new RegExp(query, 'i');
  return products.filter(p => regex.test(p.name) || regex.test(p.category));
}

export function getStockStatus(p: Product): string {
  if (p.stock > 10) return "HIGH";
  if (p.stock > 0) return "LOW";
  return "OUT_OF_STOCK";
}
```

---

## Övning 3: Enhetstester (Lösning)

Här är ett exempel på hur `src/inventoryService.test.ts` bör se ut med Vitest:

```typescript
import { describe, it, expect } from 'vitest';
import { updateStock, searchProducts } from './inventoryService.js';

describe('Inventory Service', () => {
  it('should increase stock correctly', () => {
    const success = updateStock(1, 10);
    expect(success).toBe(true);
    // Verifiera att lagret faktiskt ökade (kräver export av products eller en getter)
  });

  it('should return false for non-existing products', () => {
    const success = updateStock(999, 10);
    expect(success).toBe(false);
  });

  it('should filter products by name', () => {
    const results = searchProducts('Laptop');
    expect(results.length).toBe(1);
    expect(results[0].name).toBe('Laptop Pro');
  });
});
```

---

## Övning 4: Ny Feature (Lösning)

Den nya funktionen för lagervärde:

```typescript
export function calculateTotalInventoryValue(): number {
  return products.reduce((total, p) => total + (p.price * p.stock), 0);
}
```

---

## Övning 5: Debugging (Lösning)

Felet beror på att specialtecken i `new RegExp(query)` måste "escapas". En lösning Gemini bör föreslå är:

```typescript
export function searchProducts(query: string) {
  if (!query) return products;

  // Fix: Escape specialtecken för att undvika krasch vid t.ex. "(v2)"
  const escapedQuery = query.replace(/[.*+?^${}()|[\]\]/g, '\$&');
  const regex = new RegExp(escapedQuery, 'i');

  return products.filter(p => regex.test(p.name) || regex.test(p.category));
}
```

---

## Tips till Instruktören (Du!)

1. **Live-demo:** Kör `gemini login` och `gemini config` inför gruppen först för att visa hur enkelt det är.
2. **Uppmuntra naturligt språk:** Påminn deltagarna om att de kan prata med Gemini som en kollega. "Gör den här koden snyggare" fungerar ofta utmärkt.
3. **Fokusera på 'Varför':** När Gemini föreslår en ändring, fråga deltagarna: "Varför tror ni Gemini valde den här lösningen?".
