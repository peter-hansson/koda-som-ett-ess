using Microsoft.Data.Sqlite;

namespace FakturaHanteraren;

class Program
{
    static SqliteConnection? _c;
    static int _usr = -1;
    static string _roll = "";
    static decimal _moms = 0.25m;
    static bool _kör = true;
    static List<string> _logg = new();
    static Dictionary<int, decimal> _cache = new();
    static int _maxFörfallodagar = 30;
    static string _dbPath = "fakturor.db";

    static void Main(string[] args)
    {
        if (args.Length > 0) _dbPath = args[0];
        _c = new SqliteConnection($"Data Source={_dbPath}");
        _c.Open();
        FixaDB();
        SeedaOmTom();
        Console.Clear();
        Console.WriteLine("╔══════════════════════════════════════╗");
        Console.WriteLine("║   FAKTURA-HANTERAREN ENTERPRISE 3.1 ║");
        Console.WriteLine("╚══════════════════════════════════════╝");
        Console.WriteLine();
        while (_usr == -1)
        {
            Console.Write("Användarnamn: ");
            var u = Console.ReadLine();
            Console.Write("Lösenord: ");
            var p = Console.ReadLine();
            var cmd = _c.CreateCommand();
            cmd.CommandText = $"SELECT Id, Roll FROM Användare WHERE Namn='{u}' AND Lösenord='{p}'";
            var r = cmd.ExecuteReader();
            if (r.Read()) { _usr = r.GetInt32(0); _roll = r.GetString(1); _logg.Add($"{DateTime.Now}: {u} loggade in"); }
            else { Console.WriteLine("FEL! Försök igen."); }
            r.Close();
        }
        Console.WriteLine($"\nVälkommen! Roll: {_roll}\n");
        while (_kör)
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
            var val = Console.ReadLine();
            switch (val)
            {
                case "1": KundMeny(); break;
                case "2": ProduktMeny(); break;
                case "3": FakturaMeny(); break;
                case "4": RapportMeny(); break;
                case "5": BetMeny(); break;
                case "6": Påminn(); break;
                case "7": Inst(); break;
                case "8": foreach (var l in _logg) Console.WriteLine(l); break;
                case "0": _kör = false; break;
                default: Console.WriteLine("??"); break;
            }
        }
        _c.Close();
    }

    static void FixaDB()
    {
        var cmd = _c!.CreateCommand();
        cmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS Användare (Id INTEGER PRIMARY KEY, Namn TEXT, Lösenord TEXT, Roll TEXT, Email TEXT, Skapad TEXT);
        CREATE TABLE IF NOT EXISTS Kunder (Id INTEGER PRIMARY KEY, Namn TEXT, OrgNr TEXT, Adress TEXT, Postnr TEXT, Ort TEXT, Email TEXT, Tele TEXT, KundTyp INTEGER, Rabatt REAL, Aktiv INTEGER DEFAULT 1, Skapad TEXT, Uppdaterad TEXT);
        CREATE TABLE IF NOT EXISTS Produkter (Id INTEGER PRIMARY KEY, Namn TEXT, Beskrivning TEXT, Pris REAL, Enhet TEXT, MomsKod INTEGER, Kategori TEXT, Aktiv INTEGER DEFAULT 1, LagerSaldo INTEGER DEFAULT 0);
        CREATE TABLE IF NOT EXISTS Fakturor (Id INTEGER PRIMARY KEY, KundId INTEGER, FakturaNr TEXT, Datum TEXT, Förfallodatum TEXT, Status INTEGER, Totalt REAL, Moms REAL, Rabatt REAL, Notering TEXT, SkapadAv INTEGER, Betald TEXT);
        CREATE TABLE IF NOT EXISTS FakturaRader (Id INTEGER PRIMARY KEY, FakturaId INTEGER, ProduktId INTEGER, Antal REAL, ÅPris REAL, Rabatt REAL, Summa REAL);
        CREATE TABLE IF NOT EXISTS Betalningar (Id INTEGER PRIMARY KEY, FakturaId INTEGER, Belopp REAL, Datum TEXT, Metod TEXT, Referens TEXT);
        CREATE TABLE IF NOT EXISTS Påminnelser (Id INTEGER PRIMARY KEY, FakturaId INTEGER, Datum TEXT, Typ INTEGER, Avgift REAL);
        ";
        cmd.ExecuteNonQuery();
    }

    static void SeedaOmTom()
    {
        var cmd = _c!.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Användare";
        var cnt = (long)cmd.ExecuteScalar()!;
        if (cnt == 0)
        {
            var s = _c.CreateCommand();
            s.CommandText = @"
            INSERT INTO Användare (Namn, Lösenord, Roll, Email, Skapad) VALUES ('admin', 'admin123', 'Admin', 'admin@faktura.se', datetime('now'));
            INSERT INTO Användare (Namn, Lösenord, Roll, Email, Skapad) VALUES ('anna', 'anna123', 'Handläggare', 'anna@faktura.se', datetime('now'));
            INSERT INTO Användare (Namn, Lösenord, Roll, Email, Skapad) VALUES ('erik', 'erik123', 'Läsare', 'erik@faktura.se', datetime('now'));
            INSERT INTO Kunder (Namn, OrgNr, Adress, Postnr, Ort, Email, Tele, KundTyp, Rabatt, Skapad, Uppdaterad) VALUES ('Acme AB', '556123-4567', 'Storgatan 1', '11122', 'Stockholm', 'info@acme.se', '08-111111', 1, 0.0, datetime('now'), datetime('now'));
            INSERT INTO Kunder (Namn, OrgNr, Adress, Postnr, Ort, Email, Tele, KundTyp, Rabatt, Skapad, Uppdaterad) VALUES ('Globex Corp', '556987-6543', 'Lillvägen 42', '22233', 'Göteborg', 'order@globex.se', '031-222222', 1, 5.0, datetime('now'), datetime('now'));
            INSERT INTO Kunder (Namn, OrgNr, Adress, Postnr, Ort, Email, Tele, KundTyp, Rabatt, Skapad, Uppdaterad) VALUES ('Initech Solutions', '556456-7890', 'Datavägen 8', '33344', 'Malmö', 'faktura@initech.se', '040-333333', 2, 10.0, datetime('now'), datetime('now'));
            INSERT INTO Kunder (Namn, OrgNr, Adress, Postnr, Ort, Email, Tele, KundTyp, Rabatt, Skapad, Uppdaterad) VALUES ('Nisse Konsult', '19750101-1234', 'Hemvägen 3', '44455', 'Uppsala', 'nisse@konsult.se', '070-444444', 0, 0.0, datetime('now'), datetime('now'));
            INSERT INTO Kunder (Namn, OrgNr, Adress, Postnr, Ort, Email, Tele, KundTyp, Rabatt, Skapad, Uppdaterad) VALUES ('TechStart Innovation AB', '559012-3456', 'Innovationsparken 15', '55566', 'Linköping', 'hello@techstart.se', '013-555555', 1, 2.5, datetime('now'), datetime('now'));
            INSERT INTO Produkter (Namn, Beskrivning, Pris, Enhet, MomsKod, Kategori, LagerSaldo) VALUES ('Konsulttimme Senior', 'Senior konsulttimme', 1200.00, 'tim', 1, 'Tjänster', 0);
            INSERT INTO Produkter (Namn, Beskrivning, Pris, Enhet, MomsKod, Kategori, LagerSaldo) VALUES ('Konsulttimme Junior', 'Junior konsulttimme', 800.00, 'tim', 1, 'Tjänster', 0);
            INSERT INTO Produkter (Namn, Beskrivning, Pris, Enhet, MomsKod, Kategori, LagerSaldo) VALUES ('Licens Pro', 'Årslicens Professional', 24000.00, 'st', 1, 'Licenser', 50);
            INSERT INTO Produkter (Namn, Beskrivning, Pris, Enhet, MomsKod, Kategori, LagerSaldo) VALUES ('Licens Basic', 'Årslicens Basic', 12000.00, 'st', 1, 'Licenser', 100);
            INSERT INTO Produkter (Namn, Beskrivning, Pris, Enhet, MomsKod, Kategori, LagerSaldo) VALUES ('Support Guld', 'Supportpaket Guld per månad', 5000.00, 'mån', 1, 'Support', 0);
            INSERT INTO Produkter (Namn, Beskrivning, Pris, Enhet, MomsKod, Kategori, LagerSaldo) VALUES ('Support Silver', 'Supportpaket Silver per månad', 2500.00, 'mån', 1, 'Support', 0);
            INSERT INTO Produkter (Namn, Beskrivning, Pris, Enhet, MomsKod, Kategori, LagerSaldo) VALUES ('Hårdvara Server', 'Rack-server standard', 45000.00, 'st', 1, 'Hårdvara', 5);
            INSERT INTO Produkter (Namn, Beskrivning, Pris, Enhet, MomsKod, Kategori, LagerSaldo) VALUES ('Utbildning Grundkurs', 'Grundutbildning heldag', 8500.00, 'st', 1, 'Utbildning', 0);
            ";
            s.ExecuteNonQuery();
            // Seeda fakturor
            for (int i = 1; i <= 12; i++)
            {
                var kid = (i % 5) + 1;
                var dat = new DateTime(2024, Math.Max(1, Math.Min(12, i)), Math.Min(28, i + 5));
                var förfall = dat.AddDays(_maxFörfallodagar);
                var stat = i < 8 ? 2 : (i < 11 ? 1 : 0);
                var fnr = $"FAK-2024-{i:D4}";
                var fc = _c.CreateCommand();
                fc.CommandText = $"INSERT INTO Fakturor (KundId, FakturaNr, Datum, Förfallodatum, Status, Totalt, Moms, Rabatt, Notering, SkapadAv) VALUES ({kid}, '{fnr}', '{dat:yyyy-MM-dd}', '{förfall:yyyy-MM-dd}', {stat}, 0, 0, 0, '', {_usr})";
                fc.ExecuteNonQuery();
                var fid = (int)(long)new SqliteCommand("SELECT last_insert_rowid()", _c).ExecuteScalar()!;
                var antalRader = (i % 3) + 1;
                decimal tot = 0;
                for (int j = 0; j < antalRader; j++)
                {
                    var pid = ((i + j) % 8) + 1;
                    var antal = (decimal)((j + 1) * (i % 4 == 0 ? 10 : 1));
                    var pc = _c.CreateCommand();
                    pc.CommandText = $"SELECT Pris FROM Produkter WHERE Id={pid}";
                    var pris = (double)pc.ExecuteScalar()!;
                    var rab = kid == 3 ? 10.0 : (kid == 2 ? 5.0 : 0.0);
                    var summa = (decimal)pris * antal * (1 - (decimal)rab / 100);
                    tot += summa;
                    var rc = _c.CreateCommand();
                    rc.CommandText = $"INSERT INTO FakturaRader (FakturaId, ProduktId, Antal, ÅPris, Rabatt, Summa) VALUES ({fid}, {pid}, {(double)antal}, {pris}, {rab}, {(double)summa})";
                    rc.ExecuteNonQuery();
                }
                var moms = tot * _moms;
                var uc = _c.CreateCommand();
                uc.CommandText = $"UPDATE Fakturor SET Totalt={(double)(tot + moms)}, Moms={(double)moms} WHERE Id={fid}";
                uc.ExecuteNonQuery();
                if (stat == 2)
                {
                    var bc = _c.CreateCommand();
                    bc.CommandText = $"INSERT INTO Betalningar (FakturaId, Belopp, Datum, Metod, Referens) VALUES ({fid}, {(double)(tot + moms)}, '{dat.AddDays(15):yyyy-MM-dd}', 'Bank', 'REF-{i:D4}')";
                    bc.ExecuteNonQuery();
                    var bu = _c.CreateCommand();
                    bu.CommandText = $"UPDATE Fakturor SET Betald='{dat.AddDays(15):yyyy-MM-dd}' WHERE Id={fid}";
                    bu.ExecuteNonQuery();
                }
            }
        }
    }

    static void KundMeny()
    {
        if (_roll == "Läsare") { Console.WriteLine("Ej behörighet att hantera kunder i skrivläge."); }
        Console.WriteLine("\n-- KUNDER --");
        Console.WriteLine("1. Lista kunder");
        Console.WriteLine("2. Sök kund");
        Console.WriteLine("3. Ny kund");
        Console.WriteLine("4. Redigera kund");
        Console.WriteLine("5. Inaktivera kund");
        Console.WriteLine("6. Kundstatistik");
        Console.Write("> ");
        var v = Console.ReadLine();
        if (v == "1")
        {
            var cmd = _c!.CreateCommand();
            cmd.CommandText = "SELECT * FROM Kunder WHERE Aktiv=1 ORDER BY Namn";
            var r = cmd.ExecuteReader();
            Console.WriteLine($"\n{"Id",-5}{"Namn",-25}{"OrgNr",-16}{"Ort",-15}{"Typ",-10}{"Rabatt",-8}");
            Console.WriteLine(new string('-', 79));
            while (r.Read())
            {
                var typ = r.GetInt32(8) == 0 ? "Privat" : (r.GetInt32(8) == 1 ? "Företag" : "Kommun");
                Console.WriteLine($"{r.GetInt32(0),-5}{r.GetString(1),-25}{r.GetString(2),-16}{r.GetString(5),-15}{typ,-10}{r.GetDouble(9),-8:F1}%");
            }
            r.Close();
        }
        else if (v == "2")
        {
            Console.Write("Sökterm: ");
            var s = Console.ReadLine();
            var cmd = _c!.CreateCommand();
            cmd.CommandText = $"SELECT * FROM Kunder WHERE Namn LIKE '%{s}%' OR OrgNr LIKE '%{s}%' OR Ort LIKE '%{s}%'";
            var r = cmd.ExecuteReader();
            while (r.Read()) { Console.WriteLine($"  [{r.GetInt32(0)}] {r.GetString(1)} - {r.GetString(2)} ({r.GetString(5)})"); }
            r.Close();
        }
        else if (v == "3")
        {
            if (_roll == "Läsare") { Console.WriteLine("Ej behörighet!"); return; }
            Console.Write("Namn: "); var n = Console.ReadLine();
            Console.Write("OrgNr/PersNr: "); var o = Console.ReadLine();
            Console.Write("Adress: "); var a = Console.ReadLine();
            Console.Write("Postnr: "); var pn = Console.ReadLine();
            Console.Write("Ort: "); var ort = Console.ReadLine();
            Console.Write("Email: "); var e = Console.ReadLine();
            Console.Write("Telefon: "); var t = Console.ReadLine();
            Console.Write("Kundtyp (0=Privat,1=Företag,2=Kommun): "); var kt = Console.ReadLine();
            Console.Write("Rabatt %: "); var rab = Console.ReadLine();
            var cmd = _c!.CreateCommand();
            cmd.CommandText = $"INSERT INTO Kunder (Namn,OrgNr,Adress,Postnr,Ort,Email,Tele,KundTyp,Rabatt,Skapad,Uppdaterad) VALUES ('{n}','{o}','{a}','{pn}','{ort}','{e}','{t}',{kt},{rab?.Replace(",", ".")},datetime('now'),datetime('now'))";
            cmd.ExecuteNonQuery();
            _logg.Add($"{DateTime.Now}: Kund '{n}' skapad av användare {_usr}");
            Console.WriteLine("Kund skapad!");
        }
        else if (v == "4")
        {
            if (_roll == "Läsare") { Console.WriteLine("Ej behörighet!"); return; }
            Console.Write("Kund-Id: "); var id = Console.ReadLine();
            var cmd = _c!.CreateCommand();
            cmd.CommandText = $"SELECT * FROM Kunder WHERE Id={id}";
            var r = cmd.ExecuteReader();
            if (r.Read())
            {
                Console.WriteLine($"Nuvarande: {r.GetString(1)}, {r.GetString(3)}, {r.GetString(5)}");
                Console.Write("Nytt namn (enter=behåll): "); var n = Console.ReadLine();
                Console.Write("Ny adress (enter=behåll): "); var a = Console.ReadLine();
                Console.Write("Ny ort (enter=behåll): "); var o = Console.ReadLine();
                Console.Write("Ny email (enter=behåll): "); var e = Console.ReadLine();
                Console.Write("Ny rabatt (enter=behåll): "); var rab = Console.ReadLine();
                r.Close();
                var sets = new List<string>();
                if (!string.IsNullOrEmpty(n)) sets.Add($"Namn='{n}'");
                if (!string.IsNullOrEmpty(a)) sets.Add($"Adress='{a}'");
                if (!string.IsNullOrEmpty(o)) sets.Add($"Ort='{o}'");
                if (!string.IsNullOrEmpty(e)) sets.Add($"Email='{e}'");
                if (!string.IsNullOrEmpty(rab)) sets.Add($"Rabatt={rab.Replace(",", ".")}");
                if (sets.Count > 0)
                {
                    sets.Add("Uppdaterad=datetime('now')");
                    var uc = _c.CreateCommand();
                    uc.CommandText = $"UPDATE Kunder SET {string.Join(",", sets)} WHERE Id={id}";
                    uc.ExecuteNonQuery();
                    _logg.Add($"{DateTime.Now}: Kund {id} uppdaterad");
                    Console.WriteLine("Uppdaterad!");
                }
            }
            else { r.Close(); Console.WriteLine("Kund ej hittad."); }
        }
        else if (v == "5")
        {
            if (_roll != "Admin") { Console.WriteLine("Endast admin!"); return; }
            Console.Write("Kund-Id att inaktivera: "); var id = Console.ReadLine();
            // Kolla om kund har obetalda fakturor
            var chk = _c!.CreateCommand();
            chk.CommandText = $"SELECT COUNT(*) FROM Fakturor WHERE KundId={id} AND Status<2";
            var ob = (long)chk.ExecuteScalar()!;
            if (ob > 0) { Console.WriteLine($"Kan ej inaktivera - {ob} obetalda fakturor!"); return; }
            var cmd = _c.CreateCommand();
            cmd.CommandText = $"UPDATE Kunder SET Aktiv=0, Uppdaterad=datetime('now') WHERE Id={id}";
            cmd.ExecuteNonQuery();
            _logg.Add($"{DateTime.Now}: Kund {id} inaktiverad av {_usr}");
            Console.WriteLine("Kund inaktiverad.");
        }
        else if (v == "6")
        {
            Console.Write("Kund-Id: "); var id = Console.ReadLine();
            var cmd = _c!.CreateCommand();
            cmd.CommandText = $"SELECT k.Namn, COUNT(f.Id), SUM(f.Totalt), SUM(CASE WHEN f.Status=2 THEN f.Totalt ELSE 0 END), SUM(CASE WHEN f.Status<2 THEN f.Totalt ELSE 0 END) FROM Kunder k LEFT JOIN Fakturor f ON k.Id=f.KundId WHERE k.Id={id} GROUP BY k.Namn";
            var r = cmd.ExecuteReader();
            if (r.Read())
            {
                Console.WriteLine($"\n  Kund: {r.GetString(0)}");
                Console.WriteLine($"  Antal fakturor: {r.GetInt32(1)}");
                Console.WriteLine($"  Totalt fakturerat: {r.GetDouble(2):N2} kr");
                Console.WriteLine($"  Varav betalt: {r.GetDouble(3):N2} kr");
                Console.WriteLine($"  Utestående: {r.GetDouble(4):N2} kr");
            }
            r.Close();
        }
    }

    static void ProduktMeny()
    {
        Console.WriteLine("\n-- PRODUKTER --");
        Console.WriteLine("1. Lista");
        Console.WriteLine("2. Ny produkt");
        Console.WriteLine("3. Uppdatera pris");
        Console.WriteLine("4. Lagerstatus");
        Console.Write("> ");
        var v = Console.ReadLine();
        if (v == "1")
        {
            var cmd = _c!.CreateCommand();
            cmd.CommandText = "SELECT * FROM Produkter WHERE Aktiv=1 ORDER BY Kategori, Namn";
            var r = cmd.ExecuteReader();
            Console.WriteLine($"\n{"Id",-5}{"Namn",-25}{"Pris",-12}{"Enhet",-8}{"Kategori",-15}{"Lager",-8}");
            Console.WriteLine(new string('-', 73));
            while (r.Read())
            {
                Console.WriteLine($"{r.GetInt32(0),-5}{r.GetString(1),-25}{r.GetDouble(3),-12:N2}{r.GetString(4),-8}{r.GetString(6),-15}{r.GetInt32(8),-8}");
            }
            r.Close();
        }
        else if (v == "2")
        {
            if (_roll == "Läsare") { Console.WriteLine("Ej behörighet!"); return; }
            Console.Write("Namn: "); var n = Console.ReadLine();
            Console.Write("Beskrivning: "); var b = Console.ReadLine();
            Console.Write("Pris: "); var p = Console.ReadLine();
            Console.Write("Enhet (st/tim/mån): "); var e = Console.ReadLine();
            Console.Write("Kategori: "); var k = Console.ReadLine();
            Console.Write("Lagersaldo: "); var l = Console.ReadLine();
            var cmd = _c!.CreateCommand();
            cmd.CommandText = $"INSERT INTO Produkter (Namn,Beskrivning,Pris,Enhet,MomsKod,Kategori,LagerSaldo) VALUES ('{n}','{b}',{p?.Replace(",",".")},'{e}',1,'{k}',{l})";
            cmd.ExecuteNonQuery();
            _logg.Add($"{DateTime.Now}: Produkt '{n}' skapad");
            Console.WriteLine("Produkt skapad!");
        }
        else if (v == "3")
        {
            if (_roll == "Läsare") { Console.WriteLine("Ej behörighet!"); return; }
            Console.Write("Produkt-Id: "); var id = Console.ReadLine();
            Console.Write("Nytt pris: "); var p = Console.ReadLine();
            var cmd = _c!.CreateCommand();
            cmd.CommandText = $"UPDATE Produkter SET Pris={p?.Replace(",",".")} WHERE Id={id}";
            cmd.ExecuteNonQuery();
            _logg.Add($"{DateTime.Now}: Pris uppdaterat för produkt {id}");
            _cache.Clear();
            Console.WriteLine("Pris uppdaterat!");
        }
        else if (v == "4")
        {
            var cmd = _c!.CreateCommand();
            cmd.CommandText = "SELECT Namn, LagerSaldo, Kategori FROM Produkter WHERE Aktiv=1 AND LagerSaldo>0 ORDER BY LagerSaldo";
            var r = cmd.ExecuteReader();
            Console.WriteLine("\nLagerstatus:");
            while (r.Read())
            {
                var warn = r.GetInt32(1) < 10 ? " ⚠️ LÅGT!" : "";
                Console.WriteLine($"  {r.GetString(0),-25} {r.GetInt32(1),6} st  ({r.GetString(2)}){warn}");
            }
            r.Close();
        }
    }

    static void FakturaMeny()
    {
        Console.WriteLine("\n-- FAKTUROR --");
        Console.WriteLine("1. Lista fakturor");
        Console.WriteLine("2. Skapa ny faktura");
        Console.WriteLine("3. Visa faktura");
        Console.WriteLine("4. Kreditera faktura");
        Console.WriteLine("5. Förfallna fakturor");
        Console.Write("> ");
        var v = Console.ReadLine();
        if (v == "1")
        {
            Console.Write("Filter (0=Ny,1=Skickad,2=Betald,*=Alla): ");
            var f = Console.ReadLine();
            var cmd = _c!.CreateCommand();
            var where = f == "*" ? "" : $" AND f.Status={f}";
            cmd.CommandText = $"SELECT f.Id, f.FakturaNr, k.Namn, f.Datum, f.Förfallodatum, f.Status, f.Totalt FROM Fakturor f JOIN Kunder k ON f.KundId=k.Id WHERE 1=1{where} ORDER BY f.Datum DESC";
            var r = cmd.ExecuteReader();
            Console.WriteLine($"\n{"Id",-5}{"FakturaNr",-16}{"Kund",-22}{"Datum",-13}{"Förfall",-13}{"Status",-10}{"Totalt",12}");
            Console.WriteLine(new string('-', 91));
            while (r.Read())
            {
                var st = r.GetInt32(5) == 0 ? "Ny" : (r.GetInt32(5) == 1 ? "Skickad" : (r.GetInt32(5) == 2 ? "Betald" : "Krediterad"));
                var förfall = DateTime.Parse(r.GetString(4));
                var förfStr = r.GetString(4);
                if (r.GetInt32(5) < 2 && förfall < DateTime.Now) förfStr += " ⚠️";
                Console.WriteLine($"{r.GetInt32(0),-5}{r.GetString(1),-16}{r.GetString(2),-22}{r.GetString(3),-13}{förfStr,-13}{st,-10}{r.GetDouble(6),12:N2}");
            }
            r.Close();
        }
        else if (v == "2")
        {
            if (_roll == "Läsare") { Console.WriteLine("Ej behörighet!"); return; }
            Console.Write("Kund-Id: "); var kid = Console.ReadLine();
            // Kolla att kund finns och är aktiv
            var kc = _c!.CreateCommand();
            kc.CommandText = $"SELECT Namn, Rabatt, KundTyp FROM Kunder WHERE Id={kid} AND Aktiv=1";
            var kr = kc.ExecuteReader();
            if (!kr.Read()) { kr.Close(); Console.WriteLine("Kund ej hittad eller inaktiv!"); return; }
            var kundNamn = kr.GetString(0);
            var kundRabatt = kr.GetDouble(1);
            var kundTyp = kr.GetInt32(2);
            kr.Close();

            // Generera fakturanummer
            var nrc = _c.CreateCommand();
            nrc.CommandText = "SELECT MAX(Id) FROM Fakturor";
            var maxId = nrc.ExecuteScalar();
            var nextNr = maxId == DBNull.Value ? 1 : (int)(long)maxId + 1;
            var fakNr = $"FAK-{DateTime.Now.Year}-{nextNr:D4}";
            var datum = DateTime.Now.ToString("yyyy-MM-dd");
            var förfall = DateTime.Now.AddDays(_maxFörfallodagar).ToString("yyyy-MM-dd");

            // Speciella villkor för kommuner
            if (kundTyp == 2) { förfall = DateTime.Now.AddDays(60).ToString("yyyy-MM-dd"); }

            Console.Write("Notering: "); var not = Console.ReadLine();
            var fc = _c.CreateCommand();
            fc.CommandText = $"INSERT INTO Fakturor (KundId,FakturaNr,Datum,Förfallodatum,Status,Totalt,Moms,Rabatt,Notering,SkapadAv) VALUES ({kid},'{fakNr}','{datum}','{förfall}',0,0,0,{kundRabatt},'{not}',{_usr})";
            fc.ExecuteNonQuery();
            var fid = (int)(long)new SqliteCommand("SELECT last_insert_rowid()", _c).ExecuteScalar()!;

            Console.WriteLine($"\nFaktura {fakNr} skapad för {kundNamn}. Lägg till rader:");
            decimal totalExMoms = 0;
            bool läggTill = true;
            while (läggTill)
            {
                Console.Write("Produkt-Id (0=klar): "); var pid = Console.ReadLine();
                if (pid == "0") { läggTill = false; continue; }
                Console.Write("Antal: "); var antal = Console.ReadLine();
                var pc = _c.CreateCommand();
                pc.CommandText = $"SELECT Pris, Namn, LagerSaldo, Enhet FROM Produkter WHERE Id={pid}";
                var pr = pc.ExecuteReader();
                if (pr.Read())
                {
                    var pris = pr.GetDouble(0);
                    var pNamn = pr.GetString(1);
                    var lager = pr.GetInt32(2);
                    var enhet = pr.GetString(3);
                    pr.Close();

                    var a = decimal.Parse(antal!);
                    // Kolla lager om produkt har lager
                    if (lager > 0 && a > lager) { Console.WriteLine($"Otillräckligt lager! Tillgängligt: {lager}"); continue; }

                    Console.Write($"Pris ({pris:N2}/förslag, enter=behåll): ");
                    var custPris = Console.ReadLine();
                    if (!string.IsNullOrEmpty(custPris)) pris = double.Parse(custPris.Replace(",", "."));

                    var rabatt = kundRabatt;
                    Console.Write($"Radbatt % ({kundRabatt}%/förslag): ");
                    var custRab = Console.ReadLine();
                    if (!string.IsNullOrEmpty(custRab)) rabatt = double.Parse(custRab.Replace(",", "."));

                    var summa = (decimal)pris * a * (1 - (decimal)rabatt / 100);
                    totalExMoms += summa;

                    var rc = _c.CreateCommand();
                    rc.CommandText = $"INSERT INTO FakturaRader (FakturaId,ProduktId,Antal,ÅPris,Rabatt,Summa) VALUES ({fid},{pid},{antal?.Replace(",",".")},{pris},{rabatt},{(double)summa})";
                    rc.ExecuteNonQuery();

                    // Uppdatera lager
                    if (lager > 0)
                    {
                        var lc = _c.CreateCommand();
                        lc.CommandText = $"UPDATE Produkter SET LagerSaldo=LagerSaldo-{antal?.Replace(",",".")} WHERE Id={pid}";
                        lc.ExecuteNonQuery();
                    }

                    Console.WriteLine($"  + {pNamn} x{antal} {enhet} = {summa:N2} kr");
                }
                else { pr.Close(); Console.WriteLine("Produkt ej hittad!"); }
            }

            var moms = totalExMoms * _moms;
            var total = totalExMoms + moms;
            var uc = _c.CreateCommand();
            uc.CommandText = $"UPDATE Fakturor SET Totalt={(double)total}, Moms={(double)moms} WHERE Id={fid}";
            uc.ExecuteNonQuery();

            _cache[fid] = total;
            _logg.Add($"{DateTime.Now}: Faktura {fakNr} skapad, totalt {total:N2} kr");
            Console.WriteLine($"\nFaktura klar! Totalt: {totalExMoms:N2} + moms {moms:N2} = {total:N2} kr");
        }
        else if (v == "3")
        {
            Console.Write("Faktura-Id: "); var id = Console.ReadLine();
            VisaFaktura(int.Parse(id!));
        }
        else if (v == "4")
        {
            if (_roll != "Admin") { Console.WriteLine("Endast admin!"); return; }
            Console.Write("Faktura-Id att kreditera: "); var id = Console.ReadLine();
            var cmd = _c!.CreateCommand();
            cmd.CommandText = $"SELECT FakturaNr, Status, Totalt, KundId FROM Fakturor WHERE Id={id}";
            var r = cmd.ExecuteReader();
            if (r.Read())
            {
                if (r.GetInt32(1) == 3) { r.Close(); Console.WriteLine("Redan krediterad!"); return; }
                var origNr = r.GetString(0);
                var belopp = r.GetDouble(2);
                var kundId = r.GetInt32(3);
                r.Close();

                // Skapa kreditfaktura
                var nrc = _c.CreateCommand();
                nrc.CommandText = "SELECT MAX(Id) FROM Fakturor";
                var nextId = (int)(long)nrc.ExecuteScalar()! + 1;
                var kredNr = $"KRED-{DateTime.Now.Year}-{nextId:D4}";
                var kc = _c.CreateCommand();
                kc.CommandText = $"INSERT INTO Fakturor (KundId,FakturaNr,Datum,Förfallodatum,Status,Totalt,Moms,Rabatt,Notering,SkapadAv) VALUES ({kundId},'{kredNr}','{DateTime.Now:yyyy-MM-dd}','{DateTime.Now:yyyy-MM-dd}',3,{-belopp},0,0,'Kreditering av {origNr}',{_usr})";
                kc.ExecuteNonQuery();

                // Markera originalet
                var uc = _c.CreateCommand();
                uc.CommandText = $"UPDATE Fakturor SET Status=3 WHERE Id={id}";
                uc.ExecuteNonQuery();

                _logg.Add($"{DateTime.Now}: Faktura {origNr} krediterad -> {kredNr}");
                Console.WriteLine($"Kreditfaktura {kredNr} skapad på {-belopp:N2} kr");
            }
            else { r.Close(); Console.WriteLine("Faktura ej hittad!"); }
        }
        else if (v == "5")
        {
            var cmd = _c!.CreateCommand();
            cmd.CommandText = $"SELECT f.Id, f.FakturaNr, k.Namn, f.Förfallodatum, f.Totalt, julianday('now')-julianday(f.Förfallodatum) as Dagar FROM Fakturor f JOIN Kunder k ON f.KundId=k.Id WHERE f.Status<2 AND f.Förfallodatum<date('now') ORDER BY f.Förfallodatum";
            var r = cmd.ExecuteReader();
            Console.WriteLine("\nFörfallna fakturor:");
            Console.WriteLine($"{"FakturaNr",-16}{"Kund",-22}{"Förfall",-13}{"Dagar",-8}{"Belopp",12}");
            Console.WriteLine(new string('-', 71));
            decimal summa = 0;
            int antal = 0;
            while (r.Read())
            {
                Console.WriteLine($"{r.GetString(1),-16}{r.GetString(2),-22}{r.GetString(3),-13}{r.GetDouble(5),-8:F0}{r.GetDouble(4),12:N2}");
                summa += (decimal)r.GetDouble(4);
                antal++;
            }
            r.Close();
            Console.WriteLine($"\nTotalt {antal} förfallna fakturor, {summa:N2} kr utestående");
        }
    }

    static void VisaFaktura(int id)
    {
        var cmd = _c!.CreateCommand();
        cmd.CommandText = $"SELECT f.*, k.Namn, k.OrgNr, k.Adress, k.Postnr, k.Ort FROM Fakturor f JOIN Kunder k ON f.KundId=k.Id WHERE f.Id={id}";
        var r = cmd.ExecuteReader();
        if (r.Read())
        {
            var stat = r.GetInt32(5) == 0 ? "Ny" : (r.GetInt32(5) == 1 ? "Skickad" : (r.GetInt32(5) == 2 ? "Betald" : "Krediterad"));
            Console.WriteLine($"\n╔══════════════════════════════════════════╗");
            Console.WriteLine($"║  FAKTURA {r.GetString(2),-31} ║");
            Console.WriteLine($"╠══════════════════════════════════════════╣");
            Console.WriteLine($"║  Kund: {r.GetString(11),-33}║");
            Console.WriteLine($"║  OrgNr: {r.GetString(12),-32}║");
            Console.WriteLine($"║  {r.GetString(13),-39}║");
            Console.WriteLine($"║  {r.GetString(14)} {r.GetString(15),-33}║");
            Console.WriteLine($"╠══════════════════════════════════════════╣");
            Console.WriteLine($"║  Datum: {r.GetString(3),-32}║");
            Console.WriteLine($"║  Förfall: {r.GetString(4),-30}║");
            Console.WriteLine($"║  Status: {stat,-31}║");
            r.Close();

            // Rader
            var rc = _c.CreateCommand();
            rc.CommandText = $"SELECT fr.*, p.Namn FROM FakturaRader fr JOIN Produkter p ON fr.ProduktId=p.Id WHERE fr.FakturaId={id}";
            var rr = rc.ExecuteReader();
            Console.WriteLine($"╠══════════════════════════════════════════╣");
            Console.WriteLine($"║  {"Produkt",-18}{"Antal",6}{"Pris",9}{"Summa",9} ║");
            Console.WriteLine($"║  {"--------",-18}{"-----",6}{"----",9}{"-----",9} ║");
            while (rr.Read())
            {
                Console.WriteLine($"║  {rr.GetString(7),-18}{rr.GetDouble(3),6:F1}{rr.GetDouble(4),9:N0}{rr.GetDouble(6),9:N0} ║");
            }
            rr.Close();

            var tc = _c.CreateCommand();
            tc.CommandText = $"SELECT Totalt, Moms, Rabatt FROM Fakturor WHERE Id={id}";
            var tr = tc.ExecuteReader();
            tr.Read();
            Console.WriteLine($"╠══════════════════════════════════════════╣");
            Console.WriteLine($"║  Moms:{tr.GetDouble(1),34:N2} ║");
            Console.WriteLine($"║  TOTALT:{tr.GetDouble(0),32:N2} ║");
            Console.WriteLine($"╚══════════════════════════════════════════╝");
            tr.Close();

            // Betalningar
            var bc = _c.CreateCommand();
            bc.CommandText = $"SELECT * FROM Betalningar WHERE FakturaId={id}";
            var br = bc.ExecuteReader();
            if (br.HasRows)
            {
                Console.WriteLine("\nBetalningar:");
                while (br.Read())
                {
                    Console.WriteLine($"  {br.GetString(3)}: {br.GetDouble(2):N2} kr ({br.GetString(4)}) ref: {br.GetString(5)}");
                }
            }
            br.Close();

            // Påminnelser
            var pc = _c.CreateCommand();
            pc.CommandText = $"SELECT * FROM Påminnelser WHERE FakturaId={id}";
            var pr = pc.ExecuteReader();
            if (pr.HasRows)
            {
                Console.WriteLine("\nPåminnelser:");
                while (pr.Read())
                {
                    var typ = pr.GetInt32(3) == 1 ? "Första" : (pr.GetInt32(3) == 2 ? "Andra" : "Inkasso");
                    Console.WriteLine($"  {pr.GetString(2)}: {typ} påminnelse, avgift {pr.GetDouble(4):N2} kr");
                }
            }
            pr.Close();
        }
        else { r.Close(); Console.WriteLine("Faktura ej hittad!"); }
    }

    static void BetMeny()
    {
        Console.WriteLine("\n-- BETALNINGAR --");
        Console.WriteLine("1. Registrera betalning");
        Console.WriteLine("2. Betalningshistorik");
        Console.Write("> ");
        var v = Console.ReadLine();
        if (v == "1")
        {
            if (_roll == "Läsare") { Console.WriteLine("Ej behörighet!"); return; }
            Console.Write("Faktura-Id: "); var fid = Console.ReadLine();
            var cmd = _c!.CreateCommand();
            cmd.CommandText = $"SELECT FakturaNr, Totalt, Status FROM Fakturor WHERE Id={fid}";
            var r = cmd.ExecuteReader();
            if (r.Read())
            {
                if (r.GetInt32(2) == 2) { r.Close(); Console.WriteLine("Redan betald!"); return; }
                if (r.GetInt32(2) == 3) { r.Close(); Console.WriteLine("Krediterad - kan ej betalas!"); return; }
                var totalt = r.GetDouble(1);
                var fnr = r.GetString(0);
                r.Close();

                // Kolla redan betalat
                var sc = _c.CreateCommand();
                sc.CommandText = $"SELECT COALESCE(SUM(Belopp),0) FROM Betalningar WHERE FakturaId={fid}";
                var redan = (double)sc.ExecuteScalar()!;
                var kvar = totalt - redan;
                Console.WriteLine($"Faktura {fnr}: {totalt:N2} kr, redan betalt {redan:N2}, kvar {kvar:N2} kr");

                Console.Write("Belopp: "); var bel = Console.ReadLine();
                Console.Write("Metod (Bank/Swish/Kort): "); var met = Console.ReadLine();
                Console.Write("Referens: "); var reff = Console.ReadLine();

                var bc = _c.CreateCommand();
                bc.CommandText = $"INSERT INTO Betalningar (FakturaId,Belopp,Datum,Metod,Referens) VALUES ({fid},{bel?.Replace(",",".")},'{DateTime.Now:yyyy-MM-dd}','{met}','{reff}')";
                bc.ExecuteNonQuery();

                var betalt = redan + double.Parse(bel!.Replace(",", "."));
                if (betalt >= totalt - 0.01)
                {
                    var uc = _c.CreateCommand();
                    uc.CommandText = $"UPDATE Fakturor SET Status=2, Betald='{DateTime.Now:yyyy-MM-dd}' WHERE Id={fid}";
                    uc.ExecuteNonQuery();
                    Console.WriteLine("Faktura markerad som betald!");
                }
                else
                {
                    Console.WriteLine($"Delbetalning registrerad. Kvar att betala: {totalt - betalt:N2} kr");
                }
                _logg.Add($"{DateTime.Now}: Betalning {bel} kr registrerad på faktura {fnr}");
            }
            else { r.Close(); Console.WriteLine("Faktura ej hittad!"); }
        }
        else if (v == "2")
        {
            Console.Write("Faktura-Id (eller * för alla): "); var id = Console.ReadLine();
            var cmd = _c!.CreateCommand();
            if (id == "*") cmd.CommandText = "SELECT b.*, f.FakturaNr FROM Betalningar b JOIN Fakturor f ON b.FakturaId=f.Id ORDER BY b.Datum DESC LIMIT 50";
            else cmd.CommandText = $"SELECT b.*, f.FakturaNr FROM Betalningar b JOIN Fakturor f ON b.FakturaId=f.Id WHERE b.FakturaId={id} ORDER BY b.Datum DESC";
            var r = cmd.ExecuteReader();
            Console.WriteLine($"\n{"Datum",-13}{"Faktura",-16}{"Belopp",12}  {"Metod",-8}{"Referens",-15}");
            Console.WriteLine(new string('-', 64));
            while (r.Read())
            {
                Console.WriteLine($"{r.GetString(3),-13}{r.GetString(6),-16}{r.GetDouble(2),12:N2}  {r.GetString(4),-8}{r.GetString(5),-15}");
            }
            r.Close();
        }
    }

    static void Påminn()
    {
        if (_roll == "Läsare") { Console.WriteLine("Ej behörighet!"); return; }
        Console.WriteLine("\n-- PÅMINNELSER --");
        var cmd = _c!.CreateCommand();
        cmd.CommandText = "SELECT f.Id, f.FakturaNr, k.Namn, k.Email, f.Förfallodatum, f.Totalt, julianday('now')-julianday(f.Förfallodatum) as Dagar, (SELECT COUNT(*) FROM Påminnelser WHERE FakturaId=f.Id) as AntPåm FROM Fakturor f JOIN Kunder k ON f.KundId=k.Id WHERE f.Status<2 AND f.Förfallodatum<date('now') ORDER BY f.Förfallodatum";
        var r = cmd.ExecuteReader();
        var fakturor = new List<(int Id, string Nr, string Kund, string Email, double Dagar, double Belopp, int AntPåm)>();
        while (r.Read())
        {
            fakturor.Add((r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetString(3), r.GetDouble(6), r.GetDouble(5), r.GetInt32(7)));
        }
        r.Close();

        if (fakturor.Count == 0) { Console.WriteLine("Inga förfallna fakturor!"); return; }

        foreach (var f in fakturor)
        {
            var typ = f.AntPåm + 1;
            var avgift = typ == 1 ? 60.0 : (typ == 2 ? 180.0 : 450.0);
            var typStr = typ == 1 ? "Första påminnelse" : (typ == 2 ? "Andra påminnelse" : "Inkassovarning");
            Console.WriteLine($"\n{f.Nr} - {f.Kund} ({f.Email})");
            Console.WriteLine($"  Förfallen {f.Dagar:F0} dagar, belopp {f.Belopp:N2} kr");
            Console.WriteLine($"  Åtgärd: {typStr} (avgift {avgift:N2} kr)");
            Console.Write("  Skicka påminnelse? (j/n): ");
            if (Console.ReadLine()?.ToLower() == "j")
            {
                var pc = _c.CreateCommand();
                pc.CommandText = $"INSERT INTO Påminnelser (FakturaId,Datum,Typ,Avgift) VALUES ({f.Id},'{DateTime.Now:yyyy-MM-dd}',{typ},{avgift})";
                pc.ExecuteNonQuery();
                // Uppdatera fakturabelopp med avgift
                var uc = _c.CreateCommand();
                uc.CommandText = $"UPDATE Fakturor SET Totalt=Totalt+{avgift} WHERE Id={f.Id}";
                uc.ExecuteNonQuery();
                _logg.Add($"{DateTime.Now}: {typStr} skickad för {f.Nr} till {f.Email}");
                Console.WriteLine($"  ✓ Påminnelse registrerad (totalt nu {f.Belopp + avgift:N2} kr)");
                // Simulera mailutskick
                Console.WriteLine($"  [MAIL] Till: {f.Email}");
                Console.WriteLine($"  [MAIL] Ämne: {typStr} - {f.Nr}");
                Console.WriteLine($"  [MAIL] Belopp: {f.Belopp + avgift:N2} kr inkl avgift {avgift:N2} kr");
            }
        }
    }

    static void RapportMeny()
    {
        Console.WriteLine("\n-- RAPPORTER --");
        Console.WriteLine("1. Månadssammanställning");
        Console.WriteLine("2. Kundreskontra");
        Console.WriteLine("3. Produktförsäljning");
        Console.WriteLine("4. Momsrapport");
        Console.WriteLine("5. Årssammanställning");
        Console.Write("> ");
        var v = Console.ReadLine();
        if (v == "1")
        {
            Console.Write("År (t.ex. 2024): "); var år = Console.ReadLine();
            Console.Write("Månad (1-12): "); var mån = Console.ReadLine();
            var startDat = $"{år}-{int.Parse(mån!):D2}-01";
            var slutDat = $"{år}-{int.Parse(mån):D2}-{DateTime.DaysInMonth(int.Parse(år!), int.Parse(mån)):D2}";

            // Fakturerade
            var fc = _c!.CreateCommand();
            fc.CommandText = $"SELECT COUNT(*), COALESCE(SUM(Totalt),0), COALESCE(SUM(Moms),0) FROM Fakturor WHERE Datum BETWEEN '{startDat}' AND '{slutDat}' AND Status<>3";
            var fr = fc.ExecuteReader(); fr.Read();
            Console.WriteLine($"\nMånadsrapport {år}-{int.Parse(mån):D2}");
            Console.WriteLine(new string('=', 40));
            Console.WriteLine($"Fakturerade: {fr.GetInt32(0)} st, {fr.GetDouble(1):N2} kr (varav moms {fr.GetDouble(2):N2})");
            fr.Close();

            // Betalda
            var bc = _c.CreateCommand();
            bc.CommandText = $"SELECT COUNT(*), COALESCE(SUM(Belopp),0) FROM Betalningar WHERE Datum BETWEEN '{startDat}' AND '{slutDat}'";
            var br = bc.ExecuteReader(); br.Read();
            Console.WriteLine($"Betalningar: {br.GetInt32(0)} st, {br.GetDouble(1):N2} kr");
            br.Close();

            // Nya kunder
            var kc = _c.CreateCommand();
            kc.CommandText = $"SELECT COUNT(*) FROM Kunder WHERE Skapad BETWEEN '{startDat}' AND '{slutDat}'";
            Console.WriteLine($"Nya kunder: {(long)kc.ExecuteScalar()!}");

            // Förfallna
            var dc = _c.CreateCommand();
            dc.CommandText = $"SELECT COUNT(*), COALESCE(SUM(Totalt),0) FROM Fakturor WHERE Status<2 AND Förfallodatum<date('now') AND Datum BETWEEN '{startDat}' AND '{slutDat}'";
            var dr = dc.ExecuteReader(); dr.Read();
            Console.WriteLine($"Förfallna: {dr.GetInt32(0)} st, {dr.GetDouble(1):N2} kr");
            dr.Close();
        }
        else if (v == "2")
        {
            var cmd = _c!.CreateCommand();
            cmd.CommandText = @"SELECT k.Id, k.Namn, 
                COUNT(f.Id) as AntalFakt,
                COALESCE(SUM(CASE WHEN f.Status<2 THEN f.Totalt ELSE 0 END),0) as Utestående,
                COALESCE(SUM(CASE WHEN f.Status=2 THEN f.Totalt ELSE 0 END),0) as Betalt,
                COALESCE(SUM(f.Totalt),0) as Totalt
                FROM Kunder k LEFT JOIN Fakturor f ON k.Id=f.KundId WHERE k.Aktiv=1 AND f.Status<>3 GROUP BY k.Id, k.Namn ORDER BY Utestående DESC";
            var r = cmd.ExecuteReader();
            Console.WriteLine($"\nKundreskontra");
            Console.WriteLine($"{"Kund",-25}{"Fakturor",10}{"Utestående",15}{"Betalt",15}{"Totalt",15}");
            Console.WriteLine(new string('-', 80));
            decimal sumUte = 0, sumBet = 0;
            while (r.Read())
            {
                Console.WriteLine($"{r.GetString(1),-25}{r.GetInt32(2),10}{r.GetDouble(3),15:N2}{r.GetDouble(4),15:N2}{r.GetDouble(5),15:N2}");
                sumUte += (decimal)r.GetDouble(3);
                sumBet += (decimal)r.GetDouble(4);
            }
            r.Close();
            Console.WriteLine(new string('-', 80));
            Console.WriteLine($"{"SUMMA",-35}{sumUte,15:N2}{sumBet,15:N2}{sumUte + sumBet,15:N2}");
        }
        else if (v == "3")
        {
            var cmd = _c!.CreateCommand();
            cmd.CommandText = @"SELECT p.Namn, p.Kategori, COUNT(fr.Id), SUM(fr.Antal), SUM(fr.Summa) 
                FROM Produkter p JOIN FakturaRader fr ON p.Id=fr.ProduktId 
                JOIN Fakturor f ON fr.FakturaId=f.Id WHERE f.Status<>3
                GROUP BY p.Id, p.Namn, p.Kategori ORDER BY SUM(fr.Summa) DESC";
            var r = cmd.ExecuteReader();
            Console.WriteLine($"\nProduktförsäljning");
            Console.WriteLine($"{"Produkt",-25}{"Kategori",-15}{"Rader",8}{"Antal",10}{"Summa",15}");
            Console.WriteLine(new string('-', 73));
            while (r.Read())
            {
                Console.WriteLine($"{r.GetString(0),-25}{r.GetString(1),-15}{r.GetInt32(2),8}{r.GetDouble(3),10:F1}{r.GetDouble(4),15:N2}");
            }
            r.Close();
        }
        else if (v == "4")
        {
            Console.Write("År: "); var år = Console.ReadLine();
            Console.Write("Kvartal (1-4): "); var kv = Console.ReadLine();
            var startMån = (int.Parse(kv!) - 1) * 3 + 1;
            var slutMån = startMån + 2;
            var startDat = $"{år}-{startMån:D2}-01";
            var slutDat = $"{år}-{slutMån:D2}-{DateTime.DaysInMonth(int.Parse(år!), slutMån):D2}";

            var cmd = _c!.CreateCommand();
            cmd.CommandText = $"SELECT COALESCE(SUM(Totalt-Moms),0), COALESCE(SUM(Moms),0), COALESCE(SUM(Totalt),0) FROM Fakturor WHERE Datum BETWEEN '{startDat}' AND '{slutDat}' AND Status<>3";
            var r = cmd.ExecuteReader(); r.Read();
            Console.WriteLine($"\nMomsrapport Q{kv} {år}");
            Console.WriteLine(new string('=', 40));
            Console.WriteLine($"Netto (ex moms): {r.GetDouble(0):N2} kr");
            Console.WriteLine($"Utgående moms:   {r.GetDouble(1):N2} kr");
            Console.WriteLine($"Totalt (inkl):    {r.GetDouble(2):N2} kr");
            Console.WriteLine($"\nAtt redovisa till Skatteverket: {r.GetDouble(1):N2} kr");
            r.Close();
        }
        else if (v == "5")
        {
            Console.Write("År: "); var år = Console.ReadLine();
            Console.WriteLine($"\nÅrssammanställning {år}");
            Console.WriteLine(new string('=', 50));
            for (int m = 1; m <= 12; m++)
            {
                var sd = $"{år}-{m:D2}-01";
                var ed = $"{år}-{m:D2}-{DateTime.DaysInMonth(int.Parse(år!), m):D2}";
                var mc = _c!.CreateCommand();
                mc.CommandText = $"SELECT COUNT(*), COALESCE(SUM(Totalt),0) FROM Fakturor WHERE Datum BETWEEN '{sd}' AND '{ed}' AND Status<>3";
                var mr = mc.ExecuteReader(); mr.Read();
                var bar = new string('█', (int)(mr.GetDouble(1) / 5000));
                Console.WriteLine($"  {m:D2}/{år}: {mr.GetInt32(0),3} fakturor  {mr.GetDouble(1),12:N2} kr  {bar}");
                mr.Close();
            }
            // Årssumma
            var ac = _c!.CreateCommand();
            ac.CommandText = $"SELECT COUNT(*), SUM(Totalt), SUM(Moms) FROM Fakturor WHERE strftime('%Y',Datum)='{år}' AND Status<>3";
            var ar = ac.ExecuteReader(); ar.Read();
            Console.WriteLine(new string('-', 50));
            Console.WriteLine($"  TOTALT: {ar.GetInt32(0)} fakturor, {ar.GetDouble(1):N2} kr (moms {ar.GetDouble(2):N2})");
            ar.Close();
        }
    }

    static void Inst()
    {
        if (_roll != "Admin") { Console.WriteLine("Endast admin!"); return; }
        Console.WriteLine("\n-- INSTÄLLNINGAR --");
        Console.WriteLine($"1. Momssats (nu {_moms * 100}%)");
        Console.WriteLine($"2. Förfallodagar (nu {_maxFörfallodagar})");
        Console.WriteLine("3. Hantera användare");
        Console.WriteLine("4. Databasunderhåll");
        Console.Write("> ");
        var v = Console.ReadLine();
        if (v == "1")
        {
            Console.Write("Ny momssats (%): ");
            var m = Console.ReadLine();
            _moms = decimal.Parse(m!.Replace(",", ".")) / 100;
            _logg.Add($"{DateTime.Now}: Momssats ändrad till {_moms * 100}%");
            _cache.Clear();
            Console.WriteLine($"Momssats satt till {_moms * 100}%");
        }
        else if (v == "2")
        {
            Console.Write("Antal dagar: ");
            _maxFörfallodagar = int.Parse(Console.ReadLine()!);
            _logg.Add($"{DateTime.Now}: Förfallodagar ändrade till {_maxFörfallodagar}");
            Console.WriteLine($"Förfallodagar satt till {_maxFörfallodagar}");
        }
        else if (v == "3")
        {
            Console.WriteLine("\nAnvändare:");
            var cmd = _c!.CreateCommand();
            cmd.CommandText = "SELECT Id, Namn, Roll, Email FROM Användare";
            var r = cmd.ExecuteReader();
            while (r.Read()) Console.WriteLine($"  [{r.GetInt32(0)}] {r.GetString(1)} ({r.GetString(2)}) - {r.GetString(3)}");
            r.Close();
            Console.Write("\nNy användare? (j/n): ");
            if (Console.ReadLine()?.ToLower() == "j")
            {
                Console.Write("Namn: "); var n = Console.ReadLine();
                Console.Write("Lösenord: "); var p = Console.ReadLine();
                Console.Write("Roll (Admin/Handläggare/Läsare): "); var roll = Console.ReadLine();
                Console.Write("Email: "); var e = Console.ReadLine();
                var uc = _c.CreateCommand();
                uc.CommandText = $"INSERT INTO Användare (Namn,Lösenord,Roll,Email,Skapad) VALUES ('{n}','{p}','{roll}','{e}',datetime('now'))";
                uc.ExecuteNonQuery();
                _logg.Add($"{DateTime.Now}: Användare '{n}' skapad");
                Console.WriteLine("Användare skapad!");
            }
        }
        else if (v == "4")
        {
            Console.WriteLine("Kör VACUUM...");
            var cmd = _c!.CreateCommand();
            cmd.CommandText = "VACUUM";
            cmd.ExecuteNonQuery();
            Console.WriteLine("Databas optimerad!");

            // Räkna poster
            var tables = new[] { "Användare", "Kunder", "Produkter", "Fakturor", "FakturaRader", "Betalningar", "Påminnelser" };
            foreach (var t in tables)
            {
                var tc = _c.CreateCommand();
                tc.CommandText = $"SELECT COUNT(*) FROM {t}";
                Console.WriteLine($"  {t}: {(long)tc.ExecuteScalar()!} poster");
            }
        }
    }
}
