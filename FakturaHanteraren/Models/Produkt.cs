namespace FakturaHanteraren;

record Produkt(
    int Id,
    string Namn,
    string Beskrivning,
    double Pris,
    string Enhet,
    string Kategori,
    int LagerSaldo
);

record LagerStatusRad(string Namn, int LagerSaldo, string Kategori);
