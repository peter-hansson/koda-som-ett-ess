using System.Globalization;

namespace FakturaHanteraren;

class ProduktUI(IProduktRepository produktRepo, Session session, List<string> logg)
{
    public void VisaMeny()
    {
        Console.WriteLine("\n-- PRODUKTER --");
        Console.WriteLine("1. Lista");
        Console.WriteLine("2. Ny produkt");
        Console.WriteLine("3. Uppdatera pris");
        Console.WriteLine("4. Lagerstatus");
        Console.Write("> ");
        switch (Console.ReadLine())
        {
            case "1": ListaProdukter(); break;
            case "2": NyProdukt(); break;
            case "3": UppdateraPris(); break;
            case "4": VisaLagerStatus(); break;
        }
    }

    private void ListaProdukter()
    {
        Console.WriteLine($"\n{"Id",-5}{"Namn",-25}{"Pris",-12}{"Enhet",-8}{"Kategori",-15}{"Lager",-8}");
        Console.WriteLine(new string('-', 73));
        foreach (var p in produktRepo.HämtaAlla())
            Console.WriteLine($"{p.Id,-5}{p.Namn,-25}{p.Pris,-12:N2}{p.Enhet,-8}{p.Kategori,-15}{p.LagerSaldo,-8}");
    }

    private void NyProdukt()
    {
        if (session.Roll == "Läsare") { Console.WriteLine("Ej behörighet!"); return; }
        Console.Write("Namn: "); var namn = Console.ReadLine() ?? "";
        Console.Write("Beskrivning: "); var beskr = Console.ReadLine() ?? "";
        Console.Write("Pris: ");
        if (!double.TryParse(Console.ReadLine()?.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var pris) || pris < 0)
        { Console.WriteLine("Ogiltigt pris."); return; }
        Console.Write("Enhet (st/tim/mån): "); var enhet = Console.ReadLine() ?? "";
        Console.Write("Kategori: "); var kategori = Console.ReadLine() ?? "";
        Console.Write("Lagersaldo: ");
        if (!int.TryParse(Console.ReadLine(), out var lager) || lager < 0)
        { Console.WriteLine("Ogiltigt lagersaldo."); return; }

        produktRepo.Skapa(namn, beskr, pris, enhet, kategori, lager);
        logg.Add($"{DateTime.Now}: Produkt '{namn}' skapad");
        Console.WriteLine("Produkt skapad!");
    }

    private void UppdateraPris()
    {
        if (session.Roll == "Läsare") { Console.WriteLine("Ej behörighet!"); return; }
        Console.Write("Produkt-Id: ");
        if (!int.TryParse(Console.ReadLine(), out var id)) { Console.WriteLine("Ogiltigt Id."); return; }
        Console.Write("Nytt pris: ");
        if (!double.TryParse(Console.ReadLine()?.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var pris) || pris < 0)
        { Console.WriteLine("Ogiltigt pris."); return; }

        produktRepo.UppdateraPris(id, pris);
        logg.Add($"{DateTime.Now}: Pris uppdaterat för produkt {id}");
        Console.WriteLine("Pris uppdaterat!");
    }

    private void VisaLagerStatus()
    {
        Console.WriteLine("\nLagerstatus:");
        foreach (var rad in produktRepo.HämtaLagerStatus())
        {
            var warn = rad.LagerSaldo < 10 ? " ⚠️ LÅGT!" : "";
            Console.WriteLine($"  {rad.Namn,-25} {rad.LagerSaldo,6} st  ({rad.Kategori}){warn}");
        }
    }
}
