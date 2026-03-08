namespace FakturaHanteraren;

class PåminnelseUI(
    IPåminnelseRepository påmRepo,
    IFakturaRepository fakturaRepo,
    Session session,
    List<string> logg)
{
    public void VisaMeny()
    {
        if (session.Roll == "Läsare") { Console.WriteLine("Ej behörighet!"); return; }
        Console.WriteLine("\n-- PÅMINNELSER --");

        var fakturor = fakturaRepo.HämtaFörPåminnelse().ToList();
        if (fakturor.Count == 0) { Console.WriteLine("Inga förfallna fakturor!"); return; }

        foreach (var f in fakturor)
        {
            var påmTyp = f.AntalPåminnelser >= 2 ? PåminnelseTyp.Inkasso
                       : f.AntalPåminnelser == 1 ? PåminnelseTyp.Andra
                       : PåminnelseTyp.Första;
            var avgift = påmTyp == PåminnelseTyp.Första ? Konstanter.FörstaAvgift
                       : påmTyp == PåminnelseTyp.Andra ? Konstanter.AndraAvgift
                       : Konstanter.InkassoAvgift;
            var typStr = påmTyp == PåminnelseTyp.Första ? "Första påminnelse"
                       : påmTyp == PåminnelseTyp.Andra ? "Andra påminnelse"
                       : "Inkassovarning";

            Console.WriteLine($"\n{f.Nr} - {f.Kund} ({f.Email})");
            Console.WriteLine($"  Förfallen {f.Dagar:F0} dagar, belopp {f.Belopp:N2} kr");
            Console.WriteLine($"  Åtgärd: {typStr} (avgift {avgift:N2} kr)");
            Console.Write("  Skicka påminnelse? (j/n): ");
            if (Console.ReadLine()?.ToLower() != "j") continue;

            påmRepo.Registrera(f.Id, påmTyp, avgift);
            fakturaRepo.UppdateraFörPåminnelse(f.Id, avgift);

            logg.Add($"{DateTime.Now}: {typStr} skickad för {f.Nr} till {f.Email}");
            Console.WriteLine($"  ✓ Påminnelse registrerad (totalt nu {f.Belopp + avgift:N2} kr)");
            Console.WriteLine($"  [MAIL] Till: {f.Email}");
            Console.WriteLine($"  [MAIL] Ämne: {typStr} - {f.Nr}");
            Console.WriteLine($"  [MAIL] Belopp: {f.Belopp + avgift:N2} kr inkl avgift {avgift:N2} kr");
        }
    }
}
