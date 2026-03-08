using System.Globalization;

namespace FakturaHanteraren;

class BetalningsUI(
    IBetalningsRepository betRepo,
    IFakturaRepository fakturaRepo,
    Session session,
    List<string> logg)
{
    public void VisaMeny()
    {
        Console.WriteLine("\n-- BETALNINGAR --");
        Console.WriteLine("1. Registrera betalning");
        Console.WriteLine("2. Betalningshistorik");
        Console.Write("> ");
        switch (Console.ReadLine())
        {
            case "1": RegistreraBetalning(); break;
            case "2": VisaHistorik(); break;
        }
    }

    private void RegistreraBetalning()
    {
        if (session.Roll == "Läsare") { Console.WriteLine("Ej behörighet!"); return; }
        Console.Write("Faktura-Id: ");
        if (!int.TryParse(Console.ReadLine(), out var fid)) { Console.WriteLine("Ogiltigt Id."); return; }

        var info = fakturaRepo.HämtaFörBetalning(fid);
        if (info == null) { Console.WriteLine("Faktura ej hittad!"); return; }
        if (info.Status == FakturaStatus.Betald) { Console.WriteLine("Redan betald!"); return; }
        if (info.Status == FakturaStatus.Krediterad) { Console.WriteLine("Krediterad – kan ej betalas!"); return; }

        var redan = betRepo.HämtaSummaBetalt(fid);
        var kvar = info.Totalt - redan;
        Console.WriteLine($"Faktura {info.FakturaNr}: {info.Totalt:N2} kr, redan betalt {redan:N2}, kvar {kvar:N2} kr");

        Console.Write("Belopp: ");
        if (!double.TryParse(Console.ReadLine()?.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var bel) || bel <= 0)
        { Console.WriteLine("Ogiltigt belopp."); return; }
        Console.Write("Metod (Bank/Swish/Kort): "); var metod = Console.ReadLine() ?? "";
        Console.Write("Referens: "); var referens = Console.ReadLine() ?? "";

        betRepo.Registrera(fid, bel, metod, referens);

        var totaltBetalt = redan + bel;
        if (totaltBetalt >= info.Totalt - 0.01)
        {
            fakturaRepo.MarkeraBetald(fid);
            Console.WriteLine("Faktura markerad som betald!");
        }
        else
        {
            Console.WriteLine($"Delbetalning registrerad. Kvar att betala: {info.Totalt - totaltBetalt:N2} kr");
        }
        logg.Add($"{DateTime.Now}: Betalning {bel:N2} kr registrerad på faktura {info.FakturaNr}");
    }

    private void VisaHistorik()
    {
        Console.Write("Faktura-Id (eller * för alla): ");
        var input = Console.ReadLine();
        int? fakturaId = null;
        if (input != "*")
        {
            if (!int.TryParse(input, out var fidInt)) { Console.WriteLine("Ogiltigt Id."); return; }
            fakturaId = fidInt;
        }

        Console.WriteLine($"\n{"Datum",-13}{"Faktura",-16}{"Belopp",12}  {"Metod",-8}{"Referens",-15}");
        Console.WriteLine(new string('-', 64));
        foreach (var rad in betRepo.HämtaHistorik(fakturaId))
            Console.WriteLine($"{rad.Datum,-13}{rad.FakturaNr,-16}{rad.Belopp,12:N2}  {rad.Metod,-8}{rad.Referens,-15}");
    }
}
