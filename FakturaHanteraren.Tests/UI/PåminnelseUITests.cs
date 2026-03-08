using FakturaHanteraren.Tests.Helpers;

namespace FakturaHanteraren.Tests.UI;

[Collection("UITests")]
public class PåminnelseUITests : UITestBase, IDisposable
{
    private readonly Microsoft.Data.Sqlite.SqliteConnection _db = TestDb.Create();
    private readonly List<string> _logg = [];

    private PåminnelseUI MakeUI(string roll = "Admin")
    {
        var session = new Session(1, roll);
        return new PåminnelseUI(
            new PåminnelseRepository(_db),
            new FakturaRepository(_db),
            session,
            _logg);
    }

    public new void Dispose()
    {
        _db.Dispose();
        base.Dispose();
    }

    // ── Rollkontroll ───────────────────────────────────────────────────────

    [Fact]
    public void VisaMeny_Läsare_FårEjBehörighet()
    {
        SetInput();
        MakeUI("Läsare").VisaMeny();

        Assert.Contains("Ej behörighet", GetOutput());
    }

    [Fact]
    public void VisaMeny_Handläggare_FårTillträde()
    {
        SetInput(); // Inga förfallna → returnerar direkt
        MakeUI("Handläggare").VisaMeny();

        Assert.DoesNotContain("Ej behörighet", GetOutput());
    }

    // ── Inga förfallna ─────────────────────────────────────────────────────

    [Fact]
    public void VisaMeny_IngaFörfallnaFakturor_VisarMeddelande()
    {
        SetInput();
        MakeUI().VisaMeny();

        Assert.Contains("Inga förfallna", GetOutput());
    }

    [Fact]
    public void VisaMeny_EnFörfallenMenEjFörfallen_VisarIngaFörfallna()
    {
        var kundId = TestDb.InsertKund(_db);
        // Framtida förfallodatum – inte förfallen
        TestDb.InsertFaktura(_db, kundId, FakturaStatus.Ny,
            förfallodatum: DateTime.Now.AddDays(30).ToString("yyyy-MM-dd"));

        SetInput();
        MakeUI().VisaMeny();

        Assert.Contains("Inga förfallna", GetOutput());
    }

    // ── Påminnelsetyp bestäms av AntalPåminnelser ──────────────────────────

    [Fact]
    public void VisaMeny_NollPåminnelser_VisarFörstaTyp()
    {
        var kundId = TestDb.InsertKund(_db);
        TestDb.InsertFaktura(_db, kundId, FakturaStatus.Ny, förfallodatum: "2020-01-01");

        SetInput("n"); // Skicka inte påminnelse
        MakeUI().VisaMeny();

        Assert.Contains("Första påminnelse", GetOutput());
        Assert.Contains(Konstanter.FörstaAvgift.ToString("N2"), GetOutput());
    }

    [Fact]
    public void VisaMeny_EnPåminnelseTidigare_VisarAndraTyp()
    {
        var kundId = TestDb.InsertKund(_db);
        var fid = TestDb.InsertFaktura(_db, kundId, FakturaStatus.Ny, förfallodatum: "2020-01-01");
        TestDb.InsertPåminnelse(_db, fid, PåminnelseTyp.Första);

        SetInput("n");
        MakeUI().VisaMeny();

        Assert.Contains("Andra påminnelse", GetOutput());
        Assert.Contains(Konstanter.AndraAvgift.ToString("N2"), GetOutput());
    }

    [Fact]
    public void VisaMeny_TvåPåminnelserTidigare_VisarInkassoTyp()
    {
        var kundId = TestDb.InsertKund(_db);
        var fid = TestDb.InsertFaktura(_db, kundId, FakturaStatus.Ny, förfallodatum: "2020-01-01");
        TestDb.InsertPåminnelse(_db, fid, PåminnelseTyp.Första);
        TestDb.InsertPåminnelse(_db, fid, PåminnelseTyp.Andra);

        SetInput("n");
        MakeUI().VisaMeny();

        Assert.Contains("Inkassovarning", GetOutput());
        Assert.Contains(Konstanter.InkassoAvgift.ToString("N2"), GetOutput());
    }

    [Fact]
    public void VisaMeny_TrePåminnelserTidigare_FortfarandeInkasso()
    {
        // >= 2 räknas alltid som Inkasso
        var kundId = TestDb.InsertKund(_db);
        var fid = TestDb.InsertFaktura(_db, kundId, FakturaStatus.Ny, förfallodatum: "2020-01-01");
        TestDb.InsertPåminnelse(_db, fid, PåminnelseTyp.Första);
        TestDb.InsertPåminnelse(_db, fid, PåminnelseTyp.Andra);
        TestDb.InsertPåminnelse(_db, fid, PåminnelseTyp.Inkasso);

        SetInput("n");
        MakeUI().VisaMeny();

        Assert.Contains("Inkassovarning", GetOutput());
    }

    // ── Skicka påminnelse ──────────────────────────────────────────────────

    [Fact]
    public void VisaMeny_BekräftarPåminnelse_RegisterasPåminnelse()
    {
        var kundId = TestDb.InsertKund(_db);
        var fid = TestDb.InsertFaktura(_db, kundId, FakturaStatus.Ny, förfallodatum: "2020-01-01", totalt: 1000.0);

        SetInput("j"); // Bekräfta skicka påminnelse
        MakeUI().VisaMeny();

        var detaljer = new FakturaRepository(_db).HämtaDetaljer(fid)!;
        Assert.Single(detaljer.Påminnelser);
        Assert.Equal(PåminnelseTyp.Första, detaljer.Påminnelser[0].Typ);
    }

    [Fact]
    public void VisaMeny_BekräftarPåminnelse_UppdaterarTotalt()
    {
        var kundId = TestDb.InsertKund(_db);
        var fid = TestDb.InsertFaktura(_db, kundId, FakturaStatus.Ny, förfallodatum: "2020-01-01", totalt: 1000.0);

        SetInput("j");
        MakeUI().VisaMeny();

        var info = new FakturaRepository(_db).HämtaFörBetalning(fid)!;
        Assert.Equal(1000.0 + Konstanter.FörstaAvgift, info.Totalt, precision: 2);
    }

    [Fact]
    public void VisaMeny_AvslagarPåminnelse_RegisterasEj()
    {
        var kundId = TestDb.InsertKund(_db);
        var fid = TestDb.InsertFaktura(_db, kundId, FakturaStatus.Ny, förfallodatum: "2020-01-01");

        SetInput("n"); // Avslå
        MakeUI().VisaMeny();

        var detaljer = new FakturaRepository(_db).HämtaDetaljer(fid)!;
        Assert.Empty(detaljer.Påminnelser);
    }

    [Fact]
    public void VisaMeny_BekräftarPåminnelse_LoggarHändelse()
    {
        var kundId = TestDb.InsertKund(_db);
        TestDb.InsertFaktura(_db, kundId, FakturaStatus.Ny, förfallodatum: "2020-01-01");

        SetInput("j");
        MakeUI().VisaMeny();

        Assert.NotEmpty(_logg);
    }
}
