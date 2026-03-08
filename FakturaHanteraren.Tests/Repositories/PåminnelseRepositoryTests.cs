using FakturaHanteraren.Tests.Helpers;

namespace FakturaHanteraren.Tests.Repositories;

public class PåminnelseRepositoryTests : IDisposable
{
    private readonly Microsoft.Data.Sqlite.SqliteConnection _db = TestDb.Create();
    private PåminnelseRepository Repo => new(_db);

    public void Dispose() => _db.Dispose();

    [Fact]
    public void Registrera_FörstaTyp_InserterMedKorrektData()
    {
        var kundId = TestDb.InsertKund(_db);
        var fid = TestDb.InsertFaktura(_db, kundId);

        Repo.Registrera(fid, PåminnelseTyp.Första, Konstanter.FörstaAvgift);

        // Verifiera via FakturaRepository.HämtaDetaljer
        var detaljer = new FakturaRepository(_db).HämtaDetaljer(fid)!;
        Assert.Single(detaljer.Påminnelser);
        Assert.Equal(PåminnelseTyp.Första, detaljer.Påminnelser[0].Typ);
        Assert.Equal(Konstanter.FörstaAvgift, detaljer.Påminnelser[0].Avgift);
    }

    [Fact]
    public void Registrera_AndraTyp_SparrasKorrekt()
    {
        var kundId = TestDb.InsertKund(_db);
        var fid = TestDb.InsertFaktura(_db, kundId);

        Repo.Registrera(fid, PåminnelseTyp.Andra, Konstanter.AndraAvgift);

        var detaljer = new FakturaRepository(_db).HämtaDetaljer(fid)!;
        Assert.Equal(PåminnelseTyp.Andra, detaljer.Påminnelser[0].Typ);
        Assert.Equal(Konstanter.AndraAvgift, detaljer.Påminnelser[0].Avgift);
    }

    [Fact]
    public void Registrera_InkassoTyp_SparrasKorrekt()
    {
        var kundId = TestDb.InsertKund(_db);
        var fid = TestDb.InsertFaktura(_db, kundId);

        Repo.Registrera(fid, PåminnelseTyp.Inkasso, Konstanter.InkassoAvgift);

        var detaljer = new FakturaRepository(_db).HämtaDetaljer(fid)!;
        Assert.Equal(PåminnelseTyp.Inkasso, detaljer.Påminnelser[0].Typ);
        Assert.Equal(Konstanter.InkassoAvgift, detaljer.Påminnelser[0].Avgift);
    }

    [Fact]
    public void Registrera_SparrarDatumSomIdag()
    {
        var kundId = TestDb.InsertKund(_db);
        var fid = TestDb.InsertFaktura(_db, kundId);

        Repo.Registrera(fid, PåminnelseTyp.Första, Konstanter.FörstaAvgift);

        var detaljer = new FakturaRepository(_db).HämtaDetaljer(fid)!;
        Assert.Equal(DateTime.Now.ToString("yyyy-MM-dd"), detaljer.Påminnelser[0].Datum);
    }

    [Fact]
    public void Registrera_FleraPåminnelserSammaFaktura_AllaInsertas()
    {
        var kundId = TestDb.InsertKund(_db);
        var fid = TestDb.InsertFaktura(_db, kundId);

        Repo.Registrera(fid, PåminnelseTyp.Första, Konstanter.FörstaAvgift);
        Repo.Registrera(fid, PåminnelseTyp.Andra, Konstanter.AndraAvgift);
        Repo.Registrera(fid, PåminnelseTyp.Inkasso, Konstanter.InkassoAvgift);

        var detaljer = new FakturaRepository(_db).HämtaDetaljer(fid)!;
        Assert.Equal(3, detaljer.Påminnelser.Count);
    }

    [Fact]
    public void Registrera_PåminnelseRäknasIHämtaFörPåminnelse()
    {
        var kundId = TestDb.InsertKund(_db);
        var fid = TestDb.InsertFaktura(_db, kundId, FakturaStatus.Ny, förfallodatum: "2020-01-01");

        Repo.Registrera(fid, PåminnelseTyp.Första, Konstanter.FörstaAvgift);

        var förPåm = new FakturaRepository(_db).HämtaFörPåminnelse().Single();
        Assert.Equal(1, förPåm.AntalPåminnelser);
    }
}
