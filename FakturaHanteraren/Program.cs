using Microsoft.Data.Sqlite;

namespace FakturaHanteraren;

class Program
{
    static void Main(string[] args)
    {
        var dbPath = args.Length > 0 ? args[0] : "fakturor.db";
        var konfiguration = new AppKonfiguration();

        using var c = new SqliteConnection($"Data Source={dbPath}");
        c.Open();

        var dbSetup = new DatabaseSetup(c, konfiguration);
        dbSetup.FixaDB();
        dbSetup.SeedaOmTom();

        try { Console.Clear(); } catch { /* Ignorera i miljöer utan terminalbuffert */ }
        Console.WriteLine("╔══════════════════════════════════════╗");
        Console.WriteLine("║   FAKTURA-HANTERAREN ENTERPRISE 3.1 ║");
        Console.WriteLine("╚══════════════════════════════════════╝");
        Console.WriteLine();

        // Autentisering
        var session = LoggaIn(c, konfiguration.Logg);
        Console.WriteLine($"\nVälkommen! Roll: {session.Roll}\n");

        // Repositories
        var kundRepo = new KundRepository(c);
        var produktRepo = new ProduktRepository(c);
        var fakturaRepo = new FakturaRepository(c);
        var betRepo = new BetalningsRepository(c);
        var påmRepo = new PåminnelseRepository(c);

        // UI-lager
        var kundUI = new KundUI(kundRepo, session, konfiguration.Logg);
        var produktUI = new ProduktUI(produktRepo, session, konfiguration.Logg);
        var fakturaUI = new FakturaUI(fakturaRepo, kundRepo, produktRepo, konfiguration, session, konfiguration.Logg);
        var betUI = new BetalningsUI(betRepo, fakturaRepo, session, konfiguration.Logg);
        var påmUI = new PåminnelseUI(påmRepo, fakturaRepo, session, konfiguration.Logg);
        var rapportUI = new RapportUI(c);
        var instUI = new InställningarUI(c, konfiguration, session, konfiguration.Logg);

        // Huvudmenyloop
        bool kör = true;
        while (kör)
        {
            Console.WriteLine("--- HUVUDMENY ---");
            Console.WriteLine("1. Kunder");
            Console.WriteLine("2. Produkter");
            Console.WriteLine("3. Fakturor");
            Console.WriteLine("4. Rapporter");
            Console.WriteLine("5. Betalningar");
            Console.WriteLine("6. Påminnelser");
            Console.WriteLine("7. Inställningar");
            Console.WriteLine("8. Logg");
            Console.WriteLine("0. Avsluta");
            Console.Write("> ");
            switch (Console.ReadLine())
            {
                case "1": kundUI.VisaMeny(); break;
                case "2": produktUI.VisaMeny(); break;
                case "3": fakturaUI.VisaMeny(); break;
                case "4": rapportUI.VisaMeny(); break;
                case "5": betUI.VisaMeny(); break;
                case "6": påmUI.VisaMeny(); break;
                case "7": instUI.VisaMeny(); break;
                case "8": foreach (var rad in konfiguration.Logg) Console.WriteLine(rad); break;
                case "0": kör = false; break;
                default: Console.WriteLine("??"); break;
            }
        }
    }

    static Session LoggaIn(SqliteConnection c, List<string> logg)
    {
        while (true)
        {
            Console.Write("Användarnamn: ");
            var u = Console.ReadLine();
            Console.Write("Lösenord: ");
            var p = Console.ReadLine();

            using var cmd = c.CreateCommand();
            cmd.CommandText = "SELECT Id, Roll, Lösenord FROM Användare WHERE Namn=@namn";
            cmd.Parameters.AddWithValue("@namn", u ?? "");
            using var r = cmd.ExecuteReader();

            if (r.Read())
            {
                var hash = r.GetString(2);
                if (BCrypt.Net.BCrypt.Verify(p ?? "", hash))
                {
                    var session = new Session(r.GetInt32(0), r.GetString(1));
                    logg.Add($"{DateTime.Now}: {u} loggade in");
                    return session;
                }
            }
            Console.WriteLine("FEL! Försök igen.");
        }
    }
}
