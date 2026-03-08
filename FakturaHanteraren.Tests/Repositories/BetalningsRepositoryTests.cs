using FakturaHanteraren.Tests.Helpers;

namespace FakturaHanteraren.Tests.Repositories;

public class BetalningsRepositoryTests : IDisposable
{
    private readonly Microsoft.Data.Sqlite.SqliteConnection _db = TestDb.Create();
    private BetalningsRepository Repo => new(_db);

    public void Dispose() => _db.Dispose();

    // ── HämtaSummaBetalt ───────────────────────────────────────────────────

    [Fact]
    public void HämtaSummaBetalt_IngaBetalningar_ReturnararNoll()
    {
        var kundId = TestDb.InsertKund(_db);
        var fid = TestDb.InsertFaktura(_db, kundId);

        Assert.Equal(0.0, Repo.HämtaSummaBetalt(fid));
    }

    [Fact]
    public void HämtaSummaBetalt_EnBetalning_ReturnararBeloppet()
    {
        var kundId = TestDb.InsertKund(_db);
        var fid = TestDb.InsertFaktura(_db, kundId);
        TestDb.InsertBetalning(_db, fid, 500.0);

        Assert.Equal(500.0, Repo.HämtaSummaBetalt(fid));
    }

    [Fact]
    public void HämtaSummaBetalt_FleraBetalningar_ReturnararSumman()
    {
        var kundId = TestDb.InsertKund(_db);
        var fid = TestDb.InsertFaktura(_db, kundId, totalt: 1500.0);
        TestDb.InsertBetalning(_db, fid, 500.0);
        TestDb.InsertBetalning(_db, fid, 750.0);
        TestDb.InsertBetalning(_db, fid, 250.0);

        Assert.Equal(1500.0, Repo.HämtaSummaBetalt(fid), precision: 2);
    }

    [Fact]
    public void HämtaSummaBetalt_AnnanFaktura_PåverkarEj()
    {
        var kundId = TestDb.InsertKund(_db);
        var fid1 = TestDb.InsertFaktura(_db, kundId);
        var fid2 = TestDb.InsertFaktura(_db, kundId);
        TestDb.InsertBetalning(_db, fid1, 1000.0);
        TestDb.InsertBetalning(_db, fid2, 500.0);

        Assert.Equal(1000.0, Repo.HämtaSummaBetalt(fid1));
        Assert.Equal(500.0, Repo.HämtaSummaBetalt(fid2));
    }

    [Fact]
    public void HämtaSummaBetalt_EjBefintligFaktura_ReturnararNoll()
    {
        // COALESCE(SUM, 0) returnerar 0 när inga rader finns
        Assert.Equal(0.0, Repo.HämtaSummaBetalt(99999));
    }

    // ── Registrera ─────────────────────────────────────────────────────────

    [Fact]
    public void Registrera_InserterBetalning()
    {
        var kundId = TestDb.InsertKund(_db);
        var fid = TestDb.InsertFaktura(_db, kundId, totalt: 2000.0);

        Repo.Registrera(fid, 2000.0, "Bank", "REF-001");

        Assert.Equal(2000.0, Repo.HämtaSummaBetalt(fid));
    }

    [Fact]
    public void Registrera_SparrarMetodOchReferens()
    {
        var kundId = TestDb.InsertKund(_db);
        var fid = TestDb.InsertFaktura(_db, kundId);

        Repo.Registrera(fid, 100.0, "Kort", "KORTREF-007");

        var historik = Repo.HämtaHistorik(fid).ToList();
        Assert.Single(historik);
        Assert.Equal("Kort", historik[0].Metod);
        Assert.Equal("KORTREF-007", historik[0].Referens);
    }

    [Fact]
    public void Registrera_SparrarDatumSomIdag()
    {
        var kundId = TestDb.InsertKund(_db);
        var fid = TestDb.InsertFaktura(_db, kundId);

        Repo.Registrera(fid, 100.0, "Bank", "REF");

        var historik = Repo.HämtaHistorik(fid).First();
        Assert.Equal(DateTime.Now.ToString("yyyy-MM-dd"), historik.Datum);
    }

    [Fact]
    public void Registrera_DelbetalningMöjlig()
    {
        var kundId = TestDb.InsertKund(_db);
        var fid = TestDb.InsertFaktura(_db, kundId, totalt: 3000.0);

        Repo.Registrera(fid, 1000.0, "Bank", "DEL1");
        Repo.Registrera(fid, 2000.0, "Bank", "DEL2");

        Assert.Equal(3000.0, Repo.HämtaSummaBetalt(fid), precision: 2);
    }

    // ── HämtaHistorik ──────────────────────────────────────────────────────

    [Fact]
    public void HämtaHistorik_TomDatabas_ReturnararTomLista()
    {
        Assert.Empty(Repo.HämtaHistorik(null));
    }

    [Fact]
    public void HämtaHistorik_NullFakturaId_ReturnararAllaBetalningar()
    {
        var kundId = TestDb.InsertKund(_db);
        var fid1 = TestDb.InsertFaktura(_db, kundId);
        var fid2 = TestDb.InsertFaktura(_db, kundId);
        TestDb.InsertBetalning(_db, fid1, 100.0);
        TestDb.InsertBetalning(_db, fid2, 200.0);

        var result = Repo.HämtaHistorik(null).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void HämtaHistorik_MedFakturaId_FiltererarKorrekt()
    {
        var kundId = TestDb.InsertKund(_db);
        var fid1 = TestDb.InsertFaktura(_db, kundId);
        var fid2 = TestDb.InsertFaktura(_db, kundId);
        TestDb.InsertBetalning(_db, fid1, 100.0);
        TestDb.InsertBetalning(_db, fid2, 999.0); // ska inte inkluderas

        var result = Repo.HämtaHistorik(fid1).ToList();

        Assert.Single(result);
        Assert.Equal(100.0, result[0].Belopp);
    }

    [Fact]
    public void HämtaHistorik_VisarFakturaNr()
    {
        var kundId = TestDb.InsertKund(_db);
        var fid = TestDb.InsertFaktura(_db, kundId, fakturaNr: "F001234");
        TestDb.InsertBetalning(_db, fid, 500.0);

        var rad = Repo.HämtaHistorik(fid).First();

        Assert.Equal("F001234", rad.FakturaNr);
    }

    [Fact]
    public void HämtaHistorik_NullFakturaId_MaxFemtioRader()
    {
        var kundId = TestDb.InsertKund(_db);
        // Lägg till 55 fakturor med en betalning var
        for (int i = 0; i < 55; i++)
        {
            var fid = TestDb.InsertFaktura(_db, kundId, fakturaNr: $"F{i:D5}");
            TestDb.InsertBetalning(_db, fid, 100.0);
        }

        var result = Repo.HämtaHistorik(null).ToList();

        Assert.Equal(50, result.Count); // LIMIT 50 i SQL
    }
}
