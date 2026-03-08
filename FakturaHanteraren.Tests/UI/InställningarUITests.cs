using FakturaHanteraren.Tests.Helpers;

namespace FakturaHanteraren.Tests.UI;

[Collection("UITests")]
public class InställningarUITests : UITestBase, IDisposable
{
    private readonly Microsoft.Data.Sqlite.SqliteConnection _db = TestDb.Create();
    private readonly List<string> _logg = [];
    private readonly AppKonfiguration _konfig = new();

    private InställningarUI MakeUI(string roll = "Admin") =>
        new(_db, _konfig, new Session(1, roll), _logg);

    public new void Dispose()
    {
        _db.Dispose();
        base.Dispose();
    }

    // ── Rollkontroll ───────────────────────────────────────────────────────

    [Fact]
    public void VisaMeny_EjAdmin_VisarFelmeddelande()
    {
        SetInput();
        MakeUI("Handläggare").VisaMeny();

        Assert.Contains("Endast admin", GetOutput());
    }

    [Fact]
    public void VisaMeny_Läsare_VisarFelmeddelande()
    {
        SetInput();
        MakeUI("Läsare").VisaMeny();

        Assert.Contains("Endast admin", GetOutput());
    }

    [Fact]
    public void VisaMeny_Admin_FårTillträde()
    {
        SetInput("0"); // Avbryt direkt
        MakeUI("Admin").VisaMeny();

        Assert.DoesNotContain("Endast admin", GetOutput());
    }

    // ── ÄndraMomssats ──────────────────────────────────────────────────────

    [Fact]
    public void VisaMeny_ÄndraMomssats_UppdaterarKonfiguration()
    {
        SetInput("1", "12");
        MakeUI().VisaMeny();

        Assert.Equal(0.12m, _konfig.Moms);
    }

    [Fact]
    public void VisaMeny_ÄndraMomssats_OgiltigtVärde_ÄndrarEj()
    {
        SetInput("1", "abc");
        MakeUI().VisaMeny();

        Assert.Equal(0.25m, _konfig.Moms); // default-värde oförändrat
        Assert.Contains("Ogiltig momssats", GetOutput());
    }

    [Fact]
    public void VisaMeny_ÄndraMomssats_NollEjTillåtet()
    {
        SetInput("1", "0");
        MakeUI().VisaMeny();

        Assert.Equal(0.25m, _konfig.Moms);
        Assert.Contains("Ogiltig momssats", GetOutput());
    }

    [Fact]
    public void VisaMeny_ÄndraMomssats_NegativtEjTillåtet()
    {
        SetInput("1", "-5");
        MakeUI().VisaMeny();

        Assert.Equal(0.25m, _konfig.Moms);
    }

    [Fact]
    public void VisaMeny_ÄndraMomssats_LoggarHändelse()
    {
        SetInput("1", "6");
        MakeUI().VisaMeny();

        Assert.Contains(_logg, r => r.Contains("Momssats"));
    }

    // ── ÄndraFörfallodagar ─────────────────────────────────────────────────

    [Fact]
    public void VisaMeny_ÄndraFörfallodagar_UppdaterarKonfiguration()
    {
        SetInput("2", "45");
        MakeUI().VisaMeny();

        Assert.Equal(45, _konfig.MaxFörfallodagar);
    }

    [Fact]
    public void VisaMeny_ÄndraFörfallodagar_OgiltigtVärde_ÄndrarEj()
    {
        SetInput("2", "text");
        MakeUI().VisaMeny();

        Assert.Equal(Konstanter.StandardFörfallodag, _konfig.MaxFörfallodagar);
        Assert.Contains("Ogiltigt värde", GetOutput());
    }

    [Fact]
    public void VisaMeny_ÄndraFörfallodagar_NollEjTillåtet()
    {
        SetInput("2", "0");
        MakeUI().VisaMeny();

        Assert.Equal(Konstanter.StandardFörfallodag, _konfig.MaxFörfallodagar);
        Assert.Contains("Ogiltigt värde", GetOutput());
    }

    [Fact]
    public void VisaMeny_ÄndraFörfallodagar_LoggarHändelse()
    {
        SetInput("2", "14");
        MakeUI().VisaMeny();

        Assert.Contains(_logg, r => r.Contains("Förfallodagar"));
    }
}
