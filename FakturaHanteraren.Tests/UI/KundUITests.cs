using FakturaHanteraren.Tests.Helpers;

namespace FakturaHanteraren.Tests.UI;

// Menyalternativ: 1=Lista, 2=Sök, 3=Ny, 4=Redigera, 5=Inaktivera, 6=Statistik
[Collection("UITests")]
public class KundUITests : UITestBase, IDisposable
{
    private readonly Microsoft.Data.Sqlite.SqliteConnection _db = TestDb.Create();
    private readonly List<string> _logg = [];

    private KundUI MakeUI(string roll = "Admin")
    {
        var session = new Session(1, roll);
        return new KundUI(new KundRepository(_db), session, _logg);
    }

    public new void Dispose()
    {
        _db.Dispose();
        base.Dispose();
    }

    // ── Rollkontroll ───────────────────────────────────────────────────────

    [Fact]
    public void VisaMeny_Läsare_FårEjSkapaKund()
    {
        SetInput("3"); // NyKund
        MakeUI("Läsare").VisaMeny();

        Assert.Contains("Ej behörighet", GetOutput());
    }

    [Fact]
    public void VisaMeny_Läsare_FårEjRedigeraKund()
    {
        var id = TestDb.InsertKund(_db);
        SetInput("4", id.ToString()); // RedigeraKund – ber om id, läsare nekas
        MakeUI("Läsare").VisaMeny();

        Assert.Contains("Ej behörighet", GetOutput());
    }

    [Fact]
    public void VisaMeny_Läsare_FårEjInaktiveraKund()
    {
        SetInput("5", "1"); // InaktiveraKund
        MakeUI("Läsare").VisaMeny();

        Assert.Contains("Endast admin", GetOutput());
    }

    [Fact]
    public void VisaMeny_Handläggare_FårEjInaktiveraKund()
    {
        SetInput("5", "1"); // InaktiveraKund
        MakeUI("Handläggare").VisaMeny();

        Assert.Contains("Endast admin", GetOutput());
    }

    // ── ListaKunder ────────────────────────────────────────────────────────

    [Fact]
    public void VisaMeny_ListaKunder_VisarKundnamn()
    {
        TestDb.InsertKund(_db, "Acme AB");
        SetInput("1");
        MakeUI().VisaMeny();

        Assert.Contains("Acme AB", GetOutput());
    }

    [Fact]
    public void VisaMeny_ListaKunder_TomDatabas_KastarEjUndantag()
    {
        SetInput("1");
        MakeUI().VisaMeny();

        Assert.DoesNotContain("Exception", GetOutput());
    }

    // ── SökKund ────────────────────────────────────────────────────────────

    [Fact]
    public void VisaMeny_SökKund_HittarResultat()
    {
        TestDb.InsertKund(_db, "Globex Corp");
        SetInput("2", "Globex");
        MakeUI().VisaMeny();

        Assert.Contains("Globex Corp", GetOutput());
    }

    [Fact]
    public void VisaMeny_SökKund_IngaMatch_VisarIngentingExtra()
    {
        SetInput("2", "XYZNOTFOUND");
        MakeUI().VisaMeny();

        Assert.DoesNotContain("Exception", GetOutput());
    }

    // ── NyKund ─────────────────────────────────────────────────────────────

    [Fact]
    public void VisaMeny_NyKund_Admin_SkaparKund()
    {
        SetInput("3",              // NyKund
            "Test Företag AB",    // namn
            "556999-9999",        // orgnr
            "Testgatan 1",        // adress
            "12345",              // postnr
            "Teststad",           // ort
            "info@test.se",       // email
            "070-123456",         // tele
            "1",                  // typ (1=Företag)
            "0");                 // rabatt

        MakeUI("Admin").VisaMeny();

        var kunder = new KundRepository(_db).HämtaAlla().ToList();
        Assert.Single(kunder);
        Assert.Equal("Test Företag AB", kunder[0].Namn);
    }

    [Fact]
    public void VisaMeny_NyKund_LoggarHändelse()
    {
        SetInput("3", "Logg AB", "556000-0001", "Gatan 1", "11111", "Stad",
                 "a@b.se", "070-000", "1", "0");

        MakeUI("Admin").VisaMeny();

        Assert.Contains(_logg, r => r.Contains("Logg AB"));
    }

    [Fact]
    public void VisaMeny_NyKund_OgiltigKundtyp_SkaparEj()
    {
        SetInput("3", "Fail AB", "556000-0000", "Gatan 1", "11111", "Stad",
                 "a@b.se", "070", "9"); // 9 är ogiltig typ

        MakeUI("Admin").VisaMeny();

        Assert.Empty(new KundRepository(_db).HämtaAlla());
        Assert.Contains("Ogiltig kundtyp", GetOutput());
    }

    [Fact]
    public void VisaMeny_NyKund_OgiltigRabatt_SkaparEj()
    {
        SetInput("3", "Fail AB", "556000-0000", "Gatan 1", "11111", "Stad",
                 "a@b.se", "070", "1", "150"); // 150% rabatt ogiltig

        MakeUI("Admin").VisaMeny();

        Assert.Empty(new KundRepository(_db).HämtaAlla());
        Assert.Contains("Ogiltig rabatt", GetOutput());
    }

    // ── InaktiveraKund ─────────────────────────────────────────────────────

    [Fact]
    public void VisaMeny_InaktiveraKund_MedObetaldFaktura_Nekas()
    {
        var kundId = TestDb.InsertKund(_db, "Skuld AB");
        TestDb.InsertFaktura(_db, kundId, FakturaStatus.Ny);

        SetInput("5", kundId.ToString());
        MakeUI("Admin").VisaMeny();

        Assert.Contains("obetalda", GetOutput());
        Assert.True(new KundRepository(_db).HämtaEfterId(kundId)!.Aktiv);
    }

    [Fact]
    public void VisaMeny_InaktiveraKund_UtanFakturor_Inaktiveras()
    {
        var kundId = TestDb.InsertKund(_db, "Ren Kund");

        SetInput("5", kundId.ToString());
        MakeUI("Admin").VisaMeny();

        Assert.False(new KundRepository(_db).HämtaEfterId(kundId)!.Aktiv);
        Assert.Contains("inaktiverad", GetOutput().ToLower());
    }

    [Fact]
    public void VisaMeny_InaktiveraKund_OgiltigtId_VisarFelmeddelande()
    {
        SetInput("5", "abc"); // ogiltigt id
        MakeUI("Admin").VisaMeny();

        Assert.Contains("Ogiltigt Id", GetOutput());
    }

    // ── VisaStatistik ──────────────────────────────────────────────────────

    [Fact]
    public void VisaMeny_VisaStatistik_FinnsEj_VisarFelmeddelande()
    {
        SetInput("6", "99999");
        MakeUI().VisaMeny();

        Assert.Contains("hittad", GetOutput().ToLower());
    }

    [Fact]
    public void VisaMeny_VisaStatistik_VisarKorrektData()
    {
        var kundId = TestDb.InsertKund(_db, "Statistik AB");
        TestDb.InsertFaktura(_db, kundId, FakturaStatus.Betald, totalt: 5000.0);

        SetInput("6", kundId.ToString());
        MakeUI().VisaMeny();

        var output = GetOutput();
        Assert.Contains("Statistik AB", output);
    }
}
