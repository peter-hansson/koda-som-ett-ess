using System.Globalization;

namespace FakturaHanteraren;

class KundUI(IKundRepository kundRepo, Session session, List<string> logg)
{
    public void VisaMeny()
    {
        if (session.Roll == "Läsare") Console.WriteLine("Ej behörighet att hantera kunder i skrivläge.");
        Console.WriteLine("\n-- KUNDER --");
        Console.WriteLine("1. Lista kunder");
        Console.WriteLine("2. Sök kund");
        Console.WriteLine("3. Ny kund");
        Console.WriteLine("4. Redigera kund");
        Console.WriteLine("5. Inaktivera kund");
        Console.WriteLine("6. Kundstatistik");
        Console.Write("> ");
        switch (Console.ReadLine())
        {
            case "1": ListaKunder(); break;
            case "2": SökKund(); break;
            case "3": NyKund(); break;
            case "4": RedigeraKund(); break;
            case "5": InaktiveraKund(); break;
            case "6": VisaStatistik(); break;
        }
    }

    private void ListaKunder()
    {
        Console.WriteLine($"\n{"Id",-5}{"Namn",-25}{"OrgNr",-16}{"Ort",-15}{"Typ",-10}{"Rabatt",-8}");
        Console.WriteLine(new string('-', 79));
        foreach (var k in kundRepo.HämtaAlla())
        {
            var typ = k.Typ == KundTyp.Privat ? "Privat" : (k.Typ == KundTyp.Företag ? "Företag" : "Kommun");
            Console.WriteLine($"{k.Id,-5}{k.Namn,-25}{k.OrgNr,-16}{k.Ort,-15}{typ,-10}{k.Rabatt,-8:F1}%");
        }
    }

    private void SökKund()
    {
        Console.Write("Sökterm: ");
        var term = Console.ReadLine() ?? "";
        foreach (var k in kundRepo.Sök(term))
            Console.WriteLine($"  [{k.Id}] {k.Namn} - {k.OrgNr} ({k.Ort})");
    }

    private void NyKund()
    {
        if (session.Roll == "Läsare") { Console.WriteLine("Ej behörighet!"); return; }
        Console.Write("Namn: "); var namn = Console.ReadLine() ?? "";
        Console.Write("OrgNr/PersNr: "); var orgNr = Console.ReadLine() ?? "";
        Console.Write("Adress: "); var adress = Console.ReadLine() ?? "";
        Console.Write("Postnr: "); var postnr = Console.ReadLine() ?? "";
        Console.Write("Ort: "); var ort = Console.ReadLine() ?? "";
        Console.Write("Email: "); var email = Console.ReadLine() ?? "";
        Console.Write("Telefon: "); var tele = Console.ReadLine() ?? "";
        Console.Write("Kundtyp (0=Privat,1=Företag,2=Kommun): ");
        if (!int.TryParse(Console.ReadLine(), out var kt) || kt < 0 || kt > 2)
        { Console.WriteLine("Ogiltig kundtyp."); return; }
        Console.Write("Rabatt %: ");
        if (!double.TryParse(Console.ReadLine()?.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var rab) || rab < 0 || rab > 100)
        { Console.WriteLine("Ogiltig rabatt."); return; }

        kundRepo.Skapa(namn, orgNr, adress, postnr, ort, email, tele, (KundTyp)kt, rab);
        logg.Add($"{DateTime.Now}: Kund '{namn}' skapad av {session.AnvändarId}");
        Console.WriteLine("Kund skapad!");
    }

    private void RedigeraKund()
    {
        if (session.Roll == "Läsare") { Console.WriteLine("Ej behörighet!"); return; }
        Console.Write("Kund-Id: ");
        if (!int.TryParse(Console.ReadLine(), out var id)) { Console.WriteLine("Ogiltigt Id."); return; }

        var kund = kundRepo.HämtaEfterId(id);
        if (kund == null) { Console.WriteLine("Kund ej hittad."); return; }

        Console.WriteLine($"Nuvarande: {kund.Namn}, {kund.Adress}, {kund.Ort}");
        Console.Write("Nytt namn (enter=behåll): "); var namn = Console.ReadLine();
        Console.Write("Ny adress (enter=behåll): "); var adress = Console.ReadLine();
        Console.Write("Ny ort (enter=behåll): "); var ort = Console.ReadLine();
        Console.Write("Ny email (enter=behåll): "); var email = Console.ReadLine();
        Console.Write("Ny rabatt (enter=behåll): "); var rabStr = Console.ReadLine();

        double? rabatt = null;
        if (!string.IsNullOrEmpty(rabStr))
        {
            if (!double.TryParse(rabStr.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var r))
            { Console.WriteLine("Ogiltig rabatt."); return; }
            rabatt = r;
        }

        kundRepo.Uppdatera(id,
            string.IsNullOrEmpty(namn) ? null : namn,
            string.IsNullOrEmpty(adress) ? null : adress,
            string.IsNullOrEmpty(ort) ? null : ort,
            string.IsNullOrEmpty(email) ? null : email,
            rabatt);
        logg.Add($"{DateTime.Now}: Kund {id} uppdaterad");
        Console.WriteLine("Uppdaterad!");
    }

    private void InaktiveraKund()
    {
        if (session.Roll != "Admin") { Console.WriteLine("Endast admin!"); return; }
        Console.Write("Kund-Id att inaktivera: ");
        if (!int.TryParse(Console.ReadLine(), out var id)) { Console.WriteLine("Ogiltigt Id."); return; }

        if (kundRepo.HarObetaldaFakturor(id)) { Console.WriteLine("Kan ej inaktivera – kunden har obetalda fakturor!"); return; }
        kundRepo.Inaktivera(id);
        logg.Add($"{DateTime.Now}: Kund {id} inaktiverad av {session.AnvändarId}");
        Console.WriteLine("Kund inaktiverad.");
    }

    private void VisaStatistik()
    {
        Console.Write("Kund-Id: ");
        if (!int.TryParse(Console.ReadLine(), out var id)) { Console.WriteLine("Ogiltigt Id."); return; }

        var stats = kundRepo.HämtaStatistik(id);
        if (stats == null) { Console.WriteLine("Kund ej hittad."); return; }

        Console.WriteLine($"\n  Kund: {stats.Namn}");
        Console.WriteLine($"  Antal fakturor: {stats.AntalFakturor}");
        Console.WriteLine($"  Totalt fakturerat: {stats.TotaltFakturerat:N2} kr");
        Console.WriteLine($"  Varav betalt: {stats.Betalt:N2} kr");
        Console.WriteLine($"  Utestående: {stats.Utestående:N2} kr");
    }
}
