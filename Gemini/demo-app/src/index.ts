import { searchProducts, getStockStatus } from './inventoryService.js';

console.log("--- Välkommen till Produkthanteraren ---");

const all = searchProducts("");
console.log(`Laddat ${all.length} produkter.`);

const electronics = searchProducts("Electronics");
console.log(`Hittade ${electronics.length} elektronikprodukter:`, electronics.map(p => p.name));

// Testa sökbuggen (detta kommer krascha programmet om det körs direkt)
try {
  console.log("Söker efter (v2)...");
  const special = searchProducts("(v2)"); 
  console.log("Resultat:", special);
} catch (e: any) {
  console.error("KRASCH i sökfunktionen:", e.message);
}
