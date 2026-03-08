// src/inventoryService.ts
// En enkel datakälla (hårdkodad för enkelhets skull)
let products = [
    { id: 1, name: "Laptop Pro", category: "Electronics", price: 15000, stock: 5 },
    { id: 2, name: "Ergonomic Chair", category: "Furniture", price: 3500, stock: 12 },
    { id: 3, name: "Coffee Machine", category: "Appliances", price: 1200, stock: 0 },
    { id: 4, name: "USB-C Hub (v2)", category: "Electronics", price: 450, stock: 25 },
];
/**
 * En lite "stökig" funktion som deltagarna ska refaktorera och testa.
 * Den innehåller även en bugg när man söker med specialtecken (t.ex. parenteser).
 */
export function searchProducts(query) {
    if (!query)
        return products;
    // BUGG: Vi skapar en RegExp direkt från användarens input utan att escape:a specialtecken.
    // Detta kommer krascha om man söker på t.ex. "(v2)".
    const regex = new RegExp(query, 'i');
    return products.filter(p => {
        // Lite onödigt krånglig logik
        let match = false;
        if (regex.test(p.name)) {
            match = true;
        }
        else if (regex.test(p.category)) {
            match = true;
        }
        return match;
    });
}
export function updateStock(id, quantity) {
    const p = products.find(x => x.id === id);
    if (p) {
        p.stock += quantity;
        return true;
    }
    return false;
}
export function getStockStatus(p) {
    // Saknar typer, svår att förstå vid första anblick
    if (p.stock > 10)
        return "HIGH";
    if (p.stock > 0)
        return "LOW";
    return "OUT_OF_STOCK";
}
