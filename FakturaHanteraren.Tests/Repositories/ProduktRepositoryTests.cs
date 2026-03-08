using FakturaHanteraren.Tests.Helpers;

namespace FakturaHanteraren.Tests.Repositories;

public class ProduktRepositoryTests : IDisposable
{
    private readonly Microsoft.Data.Sqlite.SqliteConnection _db = TestDb.Create();
    private ProduktRepository Repo => new(_db);

    public void Dispose() => _db.Dispose();

    // ── HämtaAlla ──────────────────────────────────────────────────────────

    [Fact]
    public void HämtaAlla_TomDatabas_ReturnararTomLista()
    {
        Assert.Empty(Repo.HämtaAlla());
    }

    [Fact]
    public void HämtaAlla_ReturnararEndastAktiva()
    {
        TestDb.InsertProdukt(_db, "Aktiv", aktiv: true);
        TestDb.InsertProdukt(_db, "Inaktiv", aktiv: false);

        var result = Repo.HämtaAlla().ToList();

        Assert.Single(result);
        Assert.Equal("Aktiv", result[0].Namn);
    }

    [Fact]
    public void HämtaAlla_SortератEfterKategoriOchNamn()
    {
        TestDb.InsertProdukt(_db, "Z-tjänst", kategori: "Tjänster");
        TestDb.InsertProdukt(_db, "A-tjänst", kategori: "Tjänster");
        TestDb.InsertProdukt(_db, "Licens A", kategori: "Licenser");

        var namn = Repo.HämtaAlla().Select(p => p.Namn).ToList();

        // Licenser < Tjänster alfabetiskt
        Assert.Equal("Licens A", namn[0]);
        Assert.Equal("A-tjänst", namn[1]);
        Assert.Equal("Z-tjänst", namn[2]);
    }

    // ── HämtaEfterId ───────────────────────────────────────────────────────

    [Fact]
    public void HämtaEfterId_Finns_ReturnararProdukt()
    {
        var id = TestDb.InsertProdukt(_db, "Testprodukt", pris: 500.0);

        var p = Repo.HämtaEfterId(id);

        Assert.NotNull(p);
        Assert.Equal("Testprodukt", p!.Namn);
        Assert.Equal(500.0, p.Pris);
    }

    [Fact]
    public void HämtaEfterId_FinnsEj_ReturnararNull()
    {
        Assert.Null(Repo.HämtaEfterId(99999));
    }

    [Fact]
    public void HämtaEfterId_InaktivProdukt_ReturnararNull()
    {
        // HämtaEfterId filtrerar på Aktiv=1
        var id = TestDb.InsertProdukt(_db, "Inaktiv", aktiv: false);

        Assert.Null(Repo.HämtaEfterId(id));
    }

    // ── Skapa ──────────────────────────────────────────────────────────────

    [Fact]
    public void Skapa_InserterNyProdukt()
    {
        Repo.Skapa("Ny Produkt", "Bra produkt", 999.0, "st", "Licenser", 50);

        var alla = Repo.HämtaAlla().ToList();

        Assert.Single(alla);
        Assert.Equal("Ny Produkt", alla[0].Namn);
    }

    [Fact]
    public void Skapa_SparrarAllaFält()
    {
        Repo.Skapa("Server", "Rack-server", 45000.0, "st", "Hårdvara", 3);

        var p = Repo.HämtaAlla().First();

        Assert.Equal("Server", p.Namn);
        Assert.Equal(45000.0, p.Pris);
        Assert.Equal("st", p.Enhet);
        Assert.Equal("Hårdvara", p.Kategori);
        Assert.Equal(3, p.LagerSaldo);
    }

    [Fact]
    public void Skapa_NollLager_ÄrTillåten()
    {
        Repo.Skapa("Tjänst", "Konsulttimme", 1200.0, "tim", "Tjänster", 0);

        var p = Repo.HämtaAlla().First();
        Assert.Equal(0, p.LagerSaldo);
    }

    // ── UppdateraPris ──────────────────────────────────────────────────────

    [Fact]
    public void UppdateraPris_UppdaterarPriset()
    {
        var id = TestDb.InsertProdukt(_db, pris: 100.0);

        Repo.UppdateraPris(id, 250.0);

        Assert.Equal(250.0, Repo.HämtaEfterId(id)!.Pris);
    }

    [Fact]
    public void UppdateraPris_NollPris_ÄrTillåtet()
    {
        // Ingen validering i repot – 0 kr är tekniskt möjligt
        var id = TestDb.InsertProdukt(_db, pris: 100.0);

        Repo.UppdateraPris(id, 0.0);

        Assert.Equal(0.0, Repo.HämtaEfterId(id)!.Pris);
    }

    [Fact]
    public void UppdateraPris_NegativtPris_ÄrTillåtet()
    {
        // Repot validerar inte – negativt pris lagras
        var id = TestDb.InsertProdukt(_db, pris: 100.0);

        Repo.UppdateraPris(id, -50.0);

        Assert.Equal(-50.0, Repo.HämtaEfterId(id)!.Pris);
    }

    [Fact]
    public void UppdateraPris_EjBefintligtId_GörIngeting()
    {
        // Ska inte kasta undantag
        Repo.UppdateraPris(99999, 500.0);
    }

    // ── MinskaLager ────────────────────────────────────────────────────────

    [Fact]
    public void MinskaLager_MinskaarLagerSaldo()
    {
        var id = TestDb.InsertProdukt(_db, lager: 20);

        Repo.MinskaLager(id, 5);

        Assert.Equal(15, Repo.HämtaEfterId(id)!.LagerSaldo);
    }

    [Fact]
    public void MinskaLager_MedExaktaLagerSaldo_GerNoll()
    {
        var id = TestDb.InsertProdukt(_db, lager: 10);

        Repo.MinskaLager(id, 10);

        Assert.Equal(0, Repo.HämtaEfterId(id)!.LagerSaldo);
    }

    [Fact]
    public void MinskaLager_UnderNoll_TillåtsAvRepot()
    {
        // Ingen begränsning i repot – negativt saldo är möjligt
        var id = TestDb.InsertProdukt(_db, lager: 5);

        Repo.MinskaLager(id, 10);

        Assert.Equal(-5, Repo.HämtaEfterId(id)!.LagerSaldo);
    }

    [Fact]
    public void MinskaLager_NollAntal_ÄndrarIngenting()
    {
        var id = TestDb.InsertProdukt(_db, lager: 10);

        Repo.MinskaLager(id, 0);

        Assert.Equal(10, Repo.HämtaEfterId(id)!.LagerSaldo);
    }

    // ── HämtaLagerStatus ───────────────────────────────────────────────────

    [Fact]
    public void HämtaLagerStatus_ReturnararEndastMedLager()
    {
        TestDb.InsertProdukt(_db, "Med Lager", lager: 5);
        TestDb.InsertProdukt(_db, "Utan Lager", lager: 0);

        var result = Repo.HämtaLagerStatus().ToList();

        Assert.Single(result);
        Assert.Equal("Med Lager", result[0].Namn);
    }

    [Fact]
    public void HämtaLagerStatus_TomDatabas_ReturnararTomLista()
    {
        Assert.Empty(Repo.HämtaLagerStatus());
    }

    [Fact]
    public void HämtaLagerStatus_SortератEfterLagerSaldo_Stigande()
    {
        TestDb.InsertProdukt(_db, "Hög", lager: 100);
        TestDb.InsertProdukt(_db, "Låg", lager: 2);
        TestDb.InsertProdukt(_db, "Mellan", lager: 20);

        var saldo = Repo.HämtaLagerStatus().Select(r => r.LagerSaldo).ToList();

        Assert.Equal(new[] { 2, 20, 100 }, saldo);
    }

    [Fact]
    public void HämtaLagerStatus_InaktivaEjMedräknade()
    {
        TestDb.InsertProdukt(_db, "Inaktiv med lager", lager: 50, aktiv: false);

        Assert.Empty(Repo.HämtaLagerStatus());
    }
}
