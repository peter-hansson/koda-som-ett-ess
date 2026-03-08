using Microsoft.Data.Sqlite;

namespace FakturaHanteraren.Tests.Helpers;

static class TestDb
{
    /// <summary>Skapar en in-memory SQLite-databas med schema, utan seed-data.</summary>
    public static SqliteConnection Create()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        new DatabaseSetup(conn, new AppKonfiguration()).FixaDB();
        return conn;
    }

    public static int InsertKund(SqliteConnection c,
        string namn = "Test AB",
        string orgNr = "556000-0001",
        string ort = "Stockholm",
        bool aktiv = true,
        double rabatt = 0.0,
        KundTyp typ = KundTyp.Företag)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Kunder (Namn,OrgNr,Adress,Postnr,Ort,Email,Tele,KundTyp,Rabatt,Aktiv,Skapad,Uppdaterad)
            VALUES (@n,@o,'Gatan 1','11111',@ort,'test@test.se','070-000000',@kt,@rab,@a,datetime('now'),datetime('now'))
            """;
        cmd.Parameters.AddWithValue("@n", namn);
        cmd.Parameters.AddWithValue("@o", orgNr);
        cmd.Parameters.AddWithValue("@ort", ort);
        cmd.Parameters.AddWithValue("@kt", (int)typ);
        cmd.Parameters.AddWithValue("@rab", rabatt);
        cmd.Parameters.AddWithValue("@a", aktiv ? 1 : 0);
        cmd.ExecuteNonQuery();
        return LastId(c);
    }

    public static int InsertProdukt(SqliteConnection c,
        string namn = "Testprodukt",
        double pris = 100.0,
        int lager = 10,
        bool aktiv = true,
        string kategori = "Tjänster")
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Produkter (Namn,Beskrivning,Pris,Enhet,MomsKod,Kategori,LagerSaldo,Aktiv)
            VALUES (@n,'Beskrivning',@p,'st',1,@k,@l,@a)
            """;
        cmd.Parameters.AddWithValue("@n", namn);
        cmd.Parameters.AddWithValue("@p", pris);
        cmd.Parameters.AddWithValue("@k", kategori);
        cmd.Parameters.AddWithValue("@l", lager);
        cmd.Parameters.AddWithValue("@a", aktiv ? 1 : 0);
        cmd.ExecuteNonQuery();
        return LastId(c);
    }

    public static int InsertFaktura(SqliteConnection c,
        int kundId,
        FakturaStatus status = FakturaStatus.Ny,
        string? förfallodatum = null,
        double totalt = 1000.0,
        double moms = 200.0,
        string? fakturaNr = null)
    {
        var datum = DateTime.Now.ToString("yyyy-MM-dd");
        var förfall = förfallodatum ?? DateTime.Now.AddDays(30).ToString("yyyy-MM-dd");
        var nr = fakturaNr ?? $"F{LastId(c) + 1:D6}";
        using var cmd = c.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Fakturor (KundId,FakturaNr,Datum,Förfallodatum,Status,Totalt,Moms,Rabatt,Notering,SkapadAv)
            VALUES (@kid,@nr,@datum,@förfall,@status,@tot,@moms,0,'',1)
            """;
        cmd.Parameters.AddWithValue("@kid", kundId);
        cmd.Parameters.AddWithValue("@nr", nr);
        cmd.Parameters.AddWithValue("@datum", datum);
        cmd.Parameters.AddWithValue("@förfall", förfall);
        cmd.Parameters.AddWithValue("@status", (int)status);
        cmd.Parameters.AddWithValue("@tot", totalt);
        cmd.Parameters.AddWithValue("@moms", moms);
        cmd.ExecuteNonQuery();
        return LastId(c);
    }

    public static int InsertBetalning(SqliteConnection c, int fakturaId, double belopp)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Betalningar (FakturaId,Belopp,Datum,Metod,Referens)
            VALUES (@fid,@bel,date('now'),'Bank','REF-001')
            """;
        cmd.Parameters.AddWithValue("@fid", fakturaId);
        cmd.Parameters.AddWithValue("@bel", belopp);
        cmd.ExecuteNonQuery();
        return LastId(c);
    }

    public static int InsertPåminnelse(SqliteConnection c, int fakturaId, PåminnelseTyp typ = PåminnelseTyp.Första)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Påminnelser (FakturaId,Datum,Typ,Avgift)
            VALUES (@fid,date('now'),@typ,60)
            """;
        cmd.Parameters.AddWithValue("@fid", fakturaId);
        cmd.Parameters.AddWithValue("@typ", (int)typ);
        cmd.ExecuteNonQuery();
        return LastId(c);
    }

    private static int LastId(SqliteConnection c)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT last_insert_rowid()";
        return (int)(long)cmd.ExecuteScalar()!;
    }
}
