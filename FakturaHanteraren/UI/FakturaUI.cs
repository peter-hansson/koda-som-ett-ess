using System.Globalization;

namespace FakturaHanteraren;

class FakturaUI(
    IFakturaRepository fakturaRepo,
    IKundRepository kundRepo,
    IProduktRepository produktRepo,
    AppKonfiguration konfiguration,
    Session session,
    List<string> logg)
{
    public void VisaMeny()
    {
        Console.WriteLine("\n-- FAKTUROR --");
        Console.WriteLine("1. Lista fakturor");
        Console.WriteLine("2. Skapa ny faktura");
        Console.WriteLine("3. Visa faktura");
        Console.WriteLine("4. Kreditera faktura");
        Console.WriteLine("5. Förfallna fakturor");
        Console.Write("> ");
        switch (Console.ReadLine())
        {
            case "1": ListaFakturor(); break;
            case "2": SkapaFaktura(); break;
            case "3": VisaFaktura(); break;
            case "4": KrederaFaktura(); break;
            case "5": VisaFörfallna(); break;
        }
    }

    private void ListaFakturor()
    {
        Console.Write("Filter (0=Ny,1=Skickad,2=Betald,*=Alla): ");
        var f = Console.ReadLine();
        FakturaStatus? filter = null;
        if (f != "*")
        {
            if (!int.TryParse(f, out var statusInt) || statusInt < 0 || statusInt > 3)
            { Console.WriteLine("Ogiltigt filter."); return; }
            filter = (FakturaStatus)statusInt;
        }

        Console.WriteLine($"\n{"Id",-5}{"FakturaNr",-16}{"Kund",-22}{"Datum",-13}{"Förfall",-13}{"Status",-10}{"Totalt",12}");
        Console.WriteLine(new string('-', 91));
        foreach (var rad in fakturaRepo.HämtaAlla(filter))
        {
            var st = StatusText(rad.Status);
            var förfall = DateTime.Parse(rad.Förfallodatum);
            var förfStr = rad.Förfallodatum + (rad.Status < FakturaStatus.Betald && förfall < DateTime.Now ? " ⚠️" : "");
            Console.WriteLine($"{rad.Id,-5}{rad.FakturaNr,-16}{rad.KundNamn,-22}{rad.Datum,-13}{förfStr,-13}{st,-10}{rad.Totalt,12:N2}");
        }
    }

    private void SkapaFaktura()
    {
        if (session.Roll == "Läsare") { Console.WriteLine("Ej behörighet!"); return; }
        Console.Write("Kund-Id: ");
        if (!int.TryParse(Console.ReadLine(), out var kid)) { Console.WriteLine("Ogiltigt kund-Id."); return; }

        var kund = kundRepo.HämtaEfterId(kid);
        if (kund == null || !kund.Aktiv) { Console.WriteLine("Kund ej hittad eller inaktiv!"); return; }

        var nextNr = fakturaRepo.NästaFakturaNummer();
        var fakNr = $"FAK-{DateTime.Now.Year}-{nextNr:D4}";
        var datum = DateTime.Now.ToString("yyyy-MM-dd");
        var förfall = kund.Typ == KundTyp.Kommun
            ? DateTime.Now.AddDays(Konstanter.KommunFörfallodag).ToString("yyyy-MM-dd")
            : DateTime.Now.AddDays(konfiguration.MaxFörfallodagar).ToString("yyyy-MM-dd");

        Console.Write("Notering: "); var notering = Console.ReadLine() ?? "";
        var fakturaId = fakturaRepo.Skapa(kid, fakNr, datum, förfall, kund.Rabatt, notering, session.AnvändarId);

        Console.WriteLine($"\nFaktura {fakNr} skapad för {kund.Namn}. Lägg till rader:");
        decimal totalExMoms = 0;
        while (true)
        {
            Console.Write("Produkt-Id (0=klar): ");
            if (!int.TryParse(Console.ReadLine(), out var pid)) { Console.WriteLine("Ogiltigt produkt-Id."); continue; }
            if (pid == 0) break;

            Console.Write("Antal: ");
            if (!decimal.TryParse(Console.ReadLine()?.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var antal) || antal <= 0)
            { Console.WriteLine("Ogiltigt antal."); continue; }

            var produkt = produktRepo.HämtaEfterId(pid);
            if (produkt == null) { Console.WriteLine("Produkt ej hittad!"); continue; }

            if (produkt.LagerSaldo > 0 && antal > produkt.LagerSaldo)
            { Console.WriteLine($"Otillräckligt lager! Tillgängligt: {produkt.LagerSaldo}"); continue; }

            var pris = produkt.Pris;
            Console.Write($"Pris ({pris:N2}/förslag, enter=behåll): ");
            var custPrisStr = Console.ReadLine();
            if (!string.IsNullOrEmpty(custPrisStr))
            {
                if (!double.TryParse(custPrisStr.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out pris))
                { Console.WriteLine("Ogiltigt pris."); continue; }
            }

            var rabatt = kund.Rabatt;
            Console.Write($"Radrabatt % ({kund.Rabatt}%/förslag): ");
            var custRabStr = Console.ReadLine();
            if (!string.IsNullOrEmpty(custRabStr))
            {
                if (!double.TryParse(custRabStr.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out rabatt))
                { Console.WriteLine("Ogiltig rabatt."); continue; }
            }

            var summa = (decimal)pris * antal * (1 - (decimal)rabatt / 100);
            totalExMoms += summa;

            fakturaRepo.LäggTillRad(fakturaId, pid, (double)antal, pris, rabatt, (double)summa);
            if (produkt.LagerSaldo > 0) produktRepo.MinskaLager(pid, (double)antal);

            Console.WriteLine($"  + {produkt.Namn} x{antal} {produkt.Enhet} = {summa:N2} kr");
        }

        var moms = totalExMoms * konfiguration.Moms;
        var total = totalExMoms + moms;
        fakturaRepo.UppdateraTotalt(fakturaId, (double)total, (double)moms);

        logg.Add($"{DateTime.Now}: Faktura {fakNr} skapad, totalt {total:N2} kr");
        Console.WriteLine($"\nFaktura klar! Totalt: {totalExMoms:N2} + moms {moms:N2} = {total:N2} kr");
    }

    private void VisaFaktura()
    {
        Console.Write("Faktura-Id: ");
        if (!int.TryParse(Console.ReadLine(), out var id)) { Console.WriteLine("Ogiltigt Id."); return; }
        VisaFakturaDetaljer(id);
    }

    public void VisaFakturaDetaljer(int id)
    {
        var f = fakturaRepo.HämtaDetaljer(id);
        if (f == null) { Console.WriteLine("Faktura ej hittad!"); return; }

        Console.WriteLine($"\n╔══════════════════════════════════════════╗");
        Console.WriteLine($"║  FAKTURA {f.FakturaNr,-31} ║");
        Console.WriteLine($"╠══════════════════════════════════════════╣");
        Console.WriteLine($"║  Kund: {f.KundNamn,-33}║");
        Console.WriteLine($"║  OrgNr: {f.OrgNr,-32}║");
        Console.WriteLine($"║  {f.Adress,-39}║");
        Console.WriteLine($"║  {f.Postnr} {f.Ort,-33}║");
        Console.WriteLine($"╠══════════════════════════════════════════╣");
        Console.WriteLine($"║  Datum: {f.Datum,-32}║");
        Console.WriteLine($"║  Förfall: {f.Förfallodatum,-30}║");
        Console.WriteLine($"║  Status: {StatusText(f.Status),-31}║");
        Console.WriteLine($"╠══════════════════════════════════════════╣");
        Console.WriteLine($"║  {"Produkt",-18}{"Antal",6}{"Pris",9}{"Summa",9} ║");
        Console.WriteLine($"║  {"--------",-18}{"-----",6}{"----",9}{"-----",9} ║");
        foreach (var rad in f.Rader)
            Console.WriteLine($"║  {rad.ProduktNamn,-18}{rad.Antal,6:F1}{rad.ÅPris,9:N0}{rad.Summa,9:N0} ║");
        Console.WriteLine($"╠══════════════════════════════════════════╣");
        Console.WriteLine($"║  Moms:{f.Moms,34:N2} ║");
        Console.WriteLine($"║  TOTALT:{f.Totalt,32:N2} ║");
        Console.WriteLine($"╚══════════════════════════════════════════╝");

        if (f.Betalningar.Count > 0)
        {
            Console.WriteLine("\nBetalningar:");
            foreach (var b in f.Betalningar)
                Console.WriteLine($"  {b.Datum}: {b.Belopp:N2} kr ({b.Metod}) ref: {b.Referens}");
        }
        if (f.Påminnelser.Count > 0)
        {
            Console.WriteLine("\nPåminnelser:");
            foreach (var p in f.Påminnelser)
            {
                var typText = p.Typ == PåminnelseTyp.Första ? "Första" : (p.Typ == PåminnelseTyp.Andra ? "Andra" : "Inkasso");
                Console.WriteLine($"  {p.Datum}: {typText} påminnelse, avgift {p.Avgift:N2} kr");
            }
        }
    }

    private void KrederaFaktura()
    {
        if (session.Roll != "Admin") { Console.WriteLine("Endast admin!"); return; }
        Console.Write("Faktura-Id att kreditera: ");
        if (!int.TryParse(Console.ReadLine(), out var id)) { Console.WriteLine("Ogiltigt Id."); return; }

        var info = fakturaRepo.HämtaFörKreditering(id);
        if (info == null) { Console.WriteLine("Faktura ej hittad!"); return; }
        if (info.Status == FakturaStatus.Krediterad) { Console.WriteLine("Redan krediterad!"); return; }

        var nextNr = fakturaRepo.NästaFakturaNummer();
        var kredNr = $"KRED-{DateTime.Now.Year}-{nextNr:D4}";
        fakturaRepo.SkapaKreditfaktura(info.KundId, kredNr, DateTime.Now.ToString("yyyy-MM-dd"), -info.Totalt, $"Kreditering av {info.FakturaNr}", session.AnvändarId);
        fakturaRepo.MarkeraKrediterad(id);

        logg.Add($"{DateTime.Now}: Faktura {info.FakturaNr} krediterad -> {kredNr}");
        Console.WriteLine($"Kreditfaktura {kredNr} skapad på {-info.Totalt:N2} kr");
    }

    private void VisaFörfallna()
    {
        Console.WriteLine("\nFörfallna fakturor:");
        Console.WriteLine($"{"FakturaNr",-16}{"Kund",-22}{"Förfall",-13}{"Dagar",-8}{"Belopp",12}");
        Console.WriteLine(new string('-', 71));
        decimal summa = 0; int antal = 0;
        foreach (var f in fakturaRepo.HämtaFörfallna())
        {
            Console.WriteLine($"{f.FakturaNr,-16}{f.KundNamn,-22}{f.Förfallodatum,-13}{f.AntalDagar,-8:F0}{f.Totalt,12:N2}");
            summa += (decimal)f.Totalt; antal++;
        }
        Console.WriteLine($"\nTotalt {antal} förfallna fakturor, {summa:N2} kr utestående");
    }

    private static string StatusText(FakturaStatus s) => s switch
    {
        FakturaStatus.Ny => "Ny",
        FakturaStatus.Skickad => "Skickad",
        FakturaStatus.Betald => "Betald",
        _ => "Krediterad"
    };
}
