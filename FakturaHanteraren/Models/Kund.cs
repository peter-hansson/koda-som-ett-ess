namespace FakturaHanteraren;

record Kund(
    int Id,
    string Namn,
    string OrgNr,
    string Adress,
    string Postnr,
    string Ort,
    string Email,
    string Tele,
    KundTyp Typ,
    double Rabatt,
    bool Aktiv
);

record KundStatistik(
    string Namn,
    int AntalFakturor,
    double TotaltFakturerat,
    double Betalt,
    double Utestående
);
