using Microsoft.Data.Sqlite;
using System.Globalization;

namespace FakturaHanteraren;

class RapportUI(SqliteConnection c)
{
    public void VisaMeny()
    {
        Console.WriteLine("\n-- RAPPORTER --");
        Console.WriteLine("1. Månadssammanställning");
        Console.WriteLine("2. Kundreskontra");
        Console.WriteLine("3. Produktförsäljning");
        Console.WriteLine("4. Momsrapport");
        Console.WriteLine("5. Årssammanställning");
        Console.Write("> ");
        switch (Console.ReadLine())
        {
            case "1": Månadsrapport(); break;
            case "2": Kundreskontra(); break;
            case "3": Produktförsäljning(); break;
            case "4": Momsrapport(); break;
            case "5": Årssammanställning(); break;
        }
    }

    private void Månadsrapport()
    {
        Console.Write("År (t.ex. 2024): ");
        if (!int.TryParse(Console.ReadLine(), out var år) || år < 2000 || år > 2100) { Console.WriteLine("Ogiltigt år."); return; }
        Console.Write("Månad (1-12): ");
        if (!int.TryParse(Console.ReadLine(), out var mån) || mån < 1 || mån > 12) { Console.WriteLine("Ogiltig månad."); return; }
        var start = $"{år:D4}-{mån:D2}-01";
        var slut = $"{år:D4}-{mån:D2}-{DateTime.DaysInMonth(år, mån):D2}";

        Console.WriteLine($"\nMånadsrapport {år:D4}-{mån:D2}");
        Console.WriteLine(new string('=', 40));

        using var fc = c.CreateCommand();
        fc.CommandText = "SELECT COUNT(*), COALESCE(SUM(Totalt),0), COALESCE(SUM(Moms),0) FROM Fakturor WHERE Datum BETWEEN @s AND @e AND Status<>@kred";
        fc.Parameters.AddWithValue("@s", start); fc.Parameters.AddWithValue("@e", slut);
        fc.Parameters.AddWithValue("@kred", (int)FakturaStatus.Krediterad);
        using var fr = fc.ExecuteReader(); fr.Read();
        Console.WriteLine($"Fakturerade: {fr.GetInt32(0)} st, {fr.GetDouble(1):N2} kr (varav moms {fr.GetDouble(2):N2})");

        using var bc = c.CreateCommand();
        bc.CommandText = "SELECT COUNT(*), COALESCE(SUM(Belopp),0) FROM Betalningar WHERE Datum BETWEEN @s AND @e";
        bc.Parameters.AddWithValue("@s", start); bc.Parameters.AddWithValue("@e", slut);
        using var br = bc.ExecuteReader(); br.Read();
        Console.WriteLine($"Betalningar: {br.GetInt32(0)} st, {br.GetDouble(1):N2} kr");

        using var kc = c.CreateCommand();
        kc.CommandText = "SELECT COUNT(*) FROM Kunder WHERE Skapad BETWEEN @s AND @e";
        kc.Parameters.AddWithValue("@s", start); kc.Parameters.AddWithValue("@e", slut);
        Console.WriteLine($"Nya kunder: {(long)kc.ExecuteScalar()!}");

        using var dc = c.CreateCommand();
        dc.CommandText = "SELECT COUNT(*), COALESCE(SUM(Totalt),0) FROM Fakturor WHERE Status<@betald AND Förfallodatum<date('now') AND Datum BETWEEN @s AND @e";
        dc.Parameters.AddWithValue("@betald", (int)FakturaStatus.Betald);
        dc.Parameters.AddWithValue("@s", start); dc.Parameters.AddWithValue("@e", slut);
        using var dr = dc.ExecuteReader(); dr.Read();
        Console.WriteLine($"Förfallna: {dr.GetInt32(0)} st, {dr.GetDouble(1):N2} kr");
    }

    private void Kundreskontra()
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = @"SELECT k.Namn, COUNT(f.Id),
            COALESCE(SUM(CASE WHEN f.Status<@betald THEN f.Totalt ELSE 0 END),0),
            COALESCE(SUM(CASE WHEN f.Status=@betald THEN f.Totalt ELSE 0 END),0),
            COALESCE(SUM(f.Totalt),0)
            FROM Kunder k LEFT JOIN Fakturor f ON k.Id=f.KundId
            WHERE k.Aktiv=1 AND f.Status<>@kred GROUP BY k.Id, k.Namn ORDER BY 3 DESC";
        cmd.Parameters.AddWithValue("@betald", (int)FakturaStatus.Betald);
        cmd.Parameters.AddWithValue("@kred", (int)FakturaStatus.Krediterad);
        using var r = cmd.ExecuteReader();
        Console.WriteLine($"\nKundreskontra");
        Console.WriteLine($"{"Kund",-25}{"Fakturor",10}{"Utestående",15}{"Betalt",15}{"Totalt",15}");
        Console.WriteLine(new string('-', 80));
        decimal sumUte = 0, sumBet = 0;
        while (r.Read())
        {
            Console.WriteLine($"{r.GetString(0),-25}{r.GetInt32(1),10}{r.GetDouble(2),15:N2}{r.GetDouble(3),15:N2}{r.GetDouble(4),15:N2}");
            sumUte += (decimal)r.GetDouble(2); sumBet += (decimal)r.GetDouble(3);
        }
        Console.WriteLine(new string('-', 80));
        Console.WriteLine($"{"SUMMA",-35}{sumUte,15:N2}{sumBet,15:N2}{sumUte + sumBet,15:N2}");
    }

    private void Produktförsäljning()
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = @"SELECT p.Namn, p.Kategori, COUNT(fr.Id), SUM(fr.Antal), SUM(fr.Summa)
            FROM Produkter p JOIN FakturaRader fr ON p.Id=fr.ProduktId
            JOIN Fakturor f ON fr.FakturaId=f.Id WHERE f.Status<>@kred
            GROUP BY p.Id, p.Namn, p.Kategori ORDER BY SUM(fr.Summa) DESC";
        cmd.Parameters.AddWithValue("@kred", (int)FakturaStatus.Krediterad);
        using var r = cmd.ExecuteReader();
        Console.WriteLine($"\nProduktförsäljning");
        Console.WriteLine($"{"Produkt",-25}{"Kategori",-15}{"Rader",8}{"Antal",10}{"Summa",15}");
        Console.WriteLine(new string('-', 73));
        while (r.Read())
            Console.WriteLine($"{r.GetString(0),-25}{r.GetString(1),-15}{r.GetInt32(2),8}{r.GetDouble(3),10:F1}{r.GetDouble(4),15:N2}");
    }

    private void Momsrapport()
    {
        Console.Write("År: ");
        if (!int.TryParse(Console.ReadLine(), out var år) || år < 2000 || år > 2100) { Console.WriteLine("Ogiltigt år."); return; }
        Console.Write("Kvartal (1-4): ");
        if (!int.TryParse(Console.ReadLine(), out var kv) || kv < 1 || kv > 4) { Console.WriteLine("Ogiltigt kvartal."); return; }
        var startMån = (kv - 1) * 3 + 1;
        var slutMån = startMån + 2;
        var start = $"{år:D4}-{startMån:D2}-01";
        var slut = $"{år:D4}-{slutMån:D2}-{DateTime.DaysInMonth(år, slutMån):D2}";

        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT COALESCE(SUM(Totalt-Moms),0), COALESCE(SUM(Moms),0), COALESCE(SUM(Totalt),0) FROM Fakturor WHERE Datum BETWEEN @s AND @e AND Status<>@kred";
        cmd.Parameters.AddWithValue("@s", start); cmd.Parameters.AddWithValue("@e", slut);
        cmd.Parameters.AddWithValue("@kred", (int)FakturaStatus.Krediterad);
        using var r = cmd.ExecuteReader(); r.Read();
        Console.WriteLine($"\nMomsrapport Q{kv} {år:D4}");
        Console.WriteLine(new string('=', 40));
        Console.WriteLine($"Netto (ex moms): {r.GetDouble(0):N2} kr");
        Console.WriteLine($"Utgående moms:   {r.GetDouble(1):N2} kr");
        Console.WriteLine($"Totalt (inkl):    {r.GetDouble(2):N2} kr");
        Console.WriteLine($"\nAtt redovisa till Skatteverket: {r.GetDouble(1):N2} kr");
    }

    private void Årssammanställning()
    {
        Console.Write("År: ");
        if (!int.TryParse(Console.ReadLine(), out var år) || år < 2000 || år > 2100) { Console.WriteLine("Ogiltigt år."); return; }
        Console.WriteLine($"\nÅrssammanställning {år:D4}");
        Console.WriteLine(new string('=', 50));
        for (int m = 1; m <= 12; m++)
        {
            using var mc = c.CreateCommand();
            mc.CommandText = "SELECT COUNT(*), COALESCE(SUM(Totalt),0) FROM Fakturor WHERE Datum BETWEEN @s AND @e AND Status<>@kred";
            mc.Parameters.AddWithValue("@s", $"{år:D4}-{m:D2}-01");
            mc.Parameters.AddWithValue("@e", $"{år:D4}-{m:D2}-{DateTime.DaysInMonth(år, m):D2}");
            mc.Parameters.AddWithValue("@kred", (int)FakturaStatus.Krediterad);
            using var mr = mc.ExecuteReader(); mr.Read();
            var bar = new string('█', (int)(mr.GetDouble(1) / 5000));
            Console.WriteLine($"  {m:D2}/{år:D4}: {mr.GetInt32(0),3} fakturor  {mr.GetDouble(1),12:N2} kr  {bar}");
        }
        using var ac = c.CreateCommand();
        ac.CommandText = "SELECT COUNT(*), SUM(Totalt), SUM(Moms) FROM Fakturor WHERE strftime('%Y',Datum)=@år AND Status<>@kred";
        ac.Parameters.AddWithValue("@år", år.ToString("D4"));
        ac.Parameters.AddWithValue("@kred", (int)FakturaStatus.Krediterad);
        using var ar = ac.ExecuteReader(); ar.Read();
        Console.WriteLine(new string('-', 50));
        Console.WriteLine($"  TOTALT: {ar.GetInt32(0)} fakturor, {ar.GetDouble(1):N2} kr (moms {ar.GetDouble(2):N2})");
    }
}
