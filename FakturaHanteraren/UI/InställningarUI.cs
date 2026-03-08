using Microsoft.Data.Sqlite;
using System.Globalization;

namespace FakturaHanteraren;

class InställningarUI(SqliteConnection c, AppKonfiguration konfiguration, Session session, List<string> logg)
{
    public void VisaMeny()
    {
        if (session.Roll != "Admin") { Console.WriteLine("Endast admin!"); return; }
        Console.WriteLine("\n-- INSTÄLLNINGAR --");
        Console.WriteLine($"1. Momssats (nu {konfiguration.Moms * 100}%)");
        Console.WriteLine($"2. Förfallodagar (nu {konfiguration.MaxFörfallodagar})");
        Console.WriteLine("3. Hantera användare");
        Console.WriteLine("4. Databasunderhåll");
        Console.Write("> ");
        switch (Console.ReadLine())
        {
            case "1": ÄndraMomssats(); break;
            case "2": ÄndraFörfallodagar(); break;
            case "3": HanteraAnvändare(); break;
            case "4": Databasunderhåll(); break;
        }
    }

    private void ÄndraMomssats()
    {
        Console.Write("Ny momssats (%): ");
        if (!decimal.TryParse(Console.ReadLine()?.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var moms) || moms <= 0)
        { Console.WriteLine("Ogiltig momssats."); return; }
        konfiguration.Moms = moms / 100;
        logg.Add($"{DateTime.Now}: Momssats ändrad till {konfiguration.Moms * 100}%");
        Console.WriteLine($"Momssats satt till {konfiguration.Moms * 100}%");
    }

    private void ÄndraFörfallodagar()
    {
        Console.Write("Antal dagar: ");
        if (!int.TryParse(Console.ReadLine(), out var dagar) || dagar <= 0)
        { Console.WriteLine("Ogiltigt värde."); return; }
        konfiguration.MaxFörfallodagar = dagar;
        logg.Add($"{DateTime.Now}: Förfallodagar ändrade till {dagar}");
        Console.WriteLine($"Förfallodagar satt till {dagar}");
    }

    private void HanteraAnvändare()
    {
        Console.WriteLine("\nAnvändare:");
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT Id, Namn, Roll, Email FROM Användare";
        using var r = cmd.ExecuteReader();
        while (r.Read())
            Console.WriteLine($"  [{r.GetInt32(0)}] {r.GetString(1)} ({r.GetString(2)}) - {r.GetString(3)}");
        r.Close();

        Console.Write("\nNy användare? (j/n): ");
        if (Console.ReadLine()?.ToLower() != "j") return;

        Console.Write("Namn: "); var namn = Console.ReadLine() ?? "";
        Console.Write("Lösenord: "); var lösenord = Console.ReadLine() ?? "";
        Console.Write("Roll (Admin/Handläggare/Läsare): "); var roll = Console.ReadLine() ?? "";
        Console.Write("Email: "); var email = Console.ReadLine() ?? "";

        var hash = BCrypt.Net.BCrypt.HashPassword(lösenord);
        using var uc = c.CreateCommand();
        uc.CommandText = "INSERT INTO Användare (Namn,Lösenord,Roll,Email,Skapad) VALUES (@n,@p,@r,@e,datetime('now'))";
        uc.Parameters.AddWithValue("@n", namn);
        uc.Parameters.AddWithValue("@p", hash);
        uc.Parameters.AddWithValue("@r", roll);
        uc.Parameters.AddWithValue("@e", email);
        uc.ExecuteNonQuery();
        logg.Add($"{DateTime.Now}: Användare '{namn}' skapad");
        Console.WriteLine("Användare skapad!");
    }

    private void Databasunderhåll()
    {
        Console.WriteLine("Kör VACUUM...");
        using var cmd = c.CreateCommand();
        cmd.CommandText = "VACUUM";
        cmd.ExecuteNonQuery();
        Console.WriteLine("Databas optimerad!");
        foreach (var tabell in new[] { "Användare", "Kunder", "Produkter", "Fakturor", "FakturaRader", "Betalningar", "Påminnelser" })
        {
            using var tc = c.CreateCommand();
            tc.CommandText = $"SELECT COUNT(*) FROM {tabell}";
            Console.WriteLine($"  {tabell}: {(long)tc.ExecuteScalar()!} poster");
        }
    }
}
