namespace FakturaHanteraren;

// Faktura i listvy
record FakturaListRad(
    int Id,
    string FakturaNr,
    string KundNamn,
    string Datum,
    string Förfallodatum,
    FakturaStatus Status,
    double Totalt
);

// Rad i en faktura (för visning)
record FakturaRadVy(double Antal, double ÅPris, double Rabatt, double Summa, string ProduktNamn);

// Betalning kopplad till faktura
record Betalning(double Belopp, string Datum, string Metod, string Referens);

// Betalning i historikvy (inkl fakturanummer)
record BetalningsHistorikRad(string Datum, string FakturaNr, double Belopp, string Metod, string Referens);

// Påminnelse kopplad till faktura
record Påminnelse(string Datum, PåminnelseTyp Typ, double Avgift);

// Komplett fakturavy med alla detaljer
record FakturaDetaljer(
    string FakturaNr,
    string Datum,
    string Förfallodatum,
    FakturaStatus Status,
    string KundNamn,
    string OrgNr,
    string Adress,
    string Postnr,
    string Ort,
    double Totalt,
    double Moms,
    IReadOnlyList<FakturaRadVy> Rader,
    IReadOnlyList<Betalning> Betalningar,
    IReadOnlyList<Påminnelse> Påminnelser
);

// Förfallen faktura (för förfallolistan och påminnelseflödet)
record FörfallenFaktura(
    int Id,
    string FakturaNr,
    string KundNamn,
    string Förfallodatum,
    double Totalt,
    double AntalDagar
);

record FörfallenFakturaFörPåminnelse(
    int Id,
    string Nr,
    string Kund,
    string Email,
    double Dagar,
    double Belopp,
    int AntalPåminnelser
);

// Fakturainformation för betalningsflödet
record FakturaBetalningsInfo(string FakturaNr, double Totalt, FakturaStatus Status);

// Fakturainformation för krediteringsflödet
record FakturaKrediteringsInfo(string FakturaNr, FakturaStatus Status, double Totalt, int KundId);
