using Microsoft.Data.Sqlite;

namespace FakturaHanteraren;

class DatabaseSetup(SqliteConnection c, AppKonfiguration konfiguration)
{
    public void FixaDB()
    {
        using var cmd = c.CreateCommand();
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

    public void SeedaOmTom()
    {
        using var checkCmd = c.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(*) FROM Användare";
        if ((long)checkCmd.ExecuteScalar()! > 0) return;

        // Lösenord hashade med BCrypt
        var adminHash = BCrypt.Net.BCrypt.HashPassword("admin123");
        var annaHash = BCrypt.Net.BCrypt.HashPassword("anna123");
        var erikHash = BCrypt.Net.BCrypt.HashPassword("erik123");

        using var anvCmd = c.CreateCommand();
        anvCmd.CommandText = "INSERT INTO Användare (Namn, Lösenord, Roll, Email, Skapad) VALUES (@n, @p, @r, @e, datetime('now'))";
        anvCmd.Parameters.AddWithValue("@n", "admin");
        anvCmd.Parameters.AddWithValue("@p", adminHash);
        anvCmd.Parameters.AddWithValue("@r", "Admin");
        anvCmd.Parameters.AddWithValue("@e", "admin@faktura.se");
        anvCmd.ExecuteNonQuery();

        anvCmd.Parameters["@n"].Value = "anna";
        anvCmd.Parameters["@p"].Value = annaHash;
        anvCmd.Parameters["@r"].Value = "Handläggare";
        anvCmd.Parameters["@e"].Value = "anna@faktura.se";
        anvCmd.ExecuteNonQuery();

        anvCmd.Parameters["@n"].Value = "erik";
        anvCmd.Parameters["@p"].Value = erikHash;
        anvCmd.Parameters["@r"].Value = "Läsare";
        anvCmd.Parameters["@e"].Value = "erik@faktura.se";
        anvCmd.ExecuteNonQuery();

        using var s = c.CreateCommand();
        s.CommandText = @"
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

        for (int i = 1; i <= 12; i++)
        {
            var kid = (i % 5) + 1;
            var dat = new DateTime(2024, Math.Max(1, Math.Min(12, i)), Math.Min(28, i + 5));
            var förfall = dat.AddDays(konfiguration.MaxFörfallodagar);
            var stat = i < 8 ? FakturaStatus.Betald : (i < 11 ? FakturaStatus.Skickad : FakturaStatus.Ny);
            var fnr = $"FAK-2024-{i:D4}";

            using var fc = c.CreateCommand();
            fc.CommandText = $"INSERT INTO Fakturor (KundId, FakturaNr, Datum, Förfallodatum, Status, Totalt, Moms, Rabatt, Notering, SkapadAv) VALUES ({kid}, '{fnr}', '{dat:yyyy-MM-dd}', '{förfall:yyyy-MM-dd}', {(int)stat}, 0, 0, 0, '', -1)";
            fc.ExecuteNonQuery();

            using var lastIdCmd = c.CreateCommand();
            lastIdCmd.CommandText = "SELECT last_insert_rowid()";
            var fid = (int)(long)lastIdCmd.ExecuteScalar()!;

            decimal tot = 0;
            for (int j = 0; j < (i % 3) + 1; j++)
            {
                var pid = ((i + j) % 8) + 1;
                var antal = (decimal)((j + 1) * (i % 4 == 0 ? 10 : 1));

                using var pc = c.CreateCommand();
                pc.CommandText = $"SELECT Pris FROM Produkter WHERE Id={pid}";
                var pris = (double)pc.ExecuteScalar()!;

                var rab = kid == 3 ? 10.0 : (kid == 2 ? 5.0 : 0.0);
                var summa = (decimal)pris * antal * (1 - (decimal)rab / 100);
                tot += summa;

                using var rc = c.CreateCommand();
                rc.CommandText = $"INSERT INTO FakturaRader (FakturaId, ProduktId, Antal, ÅPris, Rabatt, Summa) VALUES ({fid}, {pid}, {(double)antal}, {pris}, {rab}, {(double)summa})";
                rc.ExecuteNonQuery();
            }

            var moms = tot * konfiguration.Moms;
            using var uc = c.CreateCommand();
            uc.CommandText = $"UPDATE Fakturor SET Totalt={(double)(tot + moms)}, Moms={(double)moms} WHERE Id={fid}";
            uc.ExecuteNonQuery();

            if (stat == FakturaStatus.Betald)
            {
                using var bc = c.CreateCommand();
                bc.CommandText = $"INSERT INTO Betalningar (FakturaId, Belopp, Datum, Metod, Referens) VALUES ({fid}, {(double)(tot + moms)}, '{dat.AddDays(15):yyyy-MM-dd}', 'Bank', 'REF-{i:D4}')";
                bc.ExecuteNonQuery();

                using var bu = c.CreateCommand();
                bu.CommandText = $"UPDATE Fakturor SET Betald='{dat.AddDays(15):yyyy-MM-dd}' WHERE Id={fid}";
                bu.ExecuteNonQuery();
            }
        }
    }
}
