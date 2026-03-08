using FakturaHanteraren.Tests.Helpers;

namespace FakturaHanteraren.Tests.Repositories;

public class KundRepositoryTests : IDisposable
{
    private readonly Microsoft.Data.Sqlite.SqliteConnection _db = TestDb.Create();
    private KundRepository Repo => new(_db);

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
        TestDb.InsertKund(_db, "Aktiv AB", aktiv: true);
        TestDb.InsertKund(_db, "Inaktiv AB", aktiv: false);

        var result = Repo.HämtaAlla().ToList();

        Assert.Single(result);
        Assert.Equal("Aktiv AB", result[0].Namn);
    }

    [Fact]
    public void HämtaAlla_ReturnararSortератEfterNamn()
    {
        TestDb.InsertKund(_db, "Ö-bolaget");
        TestDb.InsertKund(_db, "A-bolaget");
        TestDb.InsertKund(_db, "M-bolaget");

        var namn = Repo.HämtaAlla().Select(k => k.Namn).ToList();

        Assert.Equal(new[] { "A-bolaget", "M-bolaget", "Ö-bolaget" }, namn);
    }

    // ── Sök ────────────────────────────────────────────────────────────────

    [Fact]
    public void Sök_EfterNamn_HittarPartiellMatch()
    {
        TestDb.InsertKund(_db, "Acme Consulting");
        TestDb.InsertKund(_db, "Beta Corp");

        var result = Repo.Sök("Acme").ToList();

        Assert.Single(result);
        Assert.Equal("Acme Consulting", result[0].Namn);
    }

    [Fact]
    public void Sök_EfterOrgNr_HittarMatch()
    {
        TestDb.InsertKund(_db, "Acme AB", orgNr: "556123-4567");

        var result = Repo.Sök("556123").ToList();

        Assert.Single(result);
        Assert.Equal("Acme AB", result[0].Namn);
    }

    [Fact]
    public void Sök_EfterOrt_HittarMatch()
    {
        TestDb.InsertKund(_db, "GBG-bolaget", ort: "Göteborg");
        TestDb.InsertKund(_db, "STH-bolaget", ort: "Stockholm");

        var result = Repo.Sök("Göteborg").ToList();

        Assert.Single(result);
        Assert.Equal("GBG-bolaget", result[0].Namn);
    }

    [Fact]
    public void Sök_IngenMatch_ReturnararTomLista()
    {
        TestDb.InsertKund(_db, "Acme AB");

        Assert.Empty(Repo.Sök("XYZNOTFOUND"));
    }

    [Fact]
    public void Sök_TomSträng_ReturnararAllaKunder()
    {
        // LIKE '%%' matchar allt – inkl. inaktiva
        TestDb.InsertKund(_db, "Aktiv AB", aktiv: true);
        TestDb.InsertKund(_db, "Inaktiv AB", aktiv: false);

        var result = Repo.Sök("").ToList();

        // Sök filtrerar INTE på Aktiv – returnerar alla
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Sök_InkluderarInaktivaKunder()
    {
        TestDb.InsertKund(_db, "Inaktiv AB", aktiv: false);

        // Sök har ingen Aktiv=1-filter – hittar ändå kunden
        var result = Repo.Sök("Inaktiv").ToList();

        Assert.Single(result);
    }

    // ── HämtaEfterId ───────────────────────────────────────────────────────

    [Fact]
    public void HämtaEfterId_Finns_ReturnararKund()
    {
        var id = TestDb.InsertKund(_db, "Acme AB");

        var kund = Repo.HämtaEfterId(id);

        Assert.NotNull(kund);
        Assert.Equal("Acme AB", kund!.Namn);
    }

    [Fact]
    public void HämtaEfterId_FinnsEj_ReturnararNull()
    {
        Assert.Null(Repo.HämtaEfterId(99999));
    }

    [Fact]
    public void HämtaEfterId_InaktivKund_ReturnararKund()
    {
        // HämtaEfterId filtrerar inte på Aktiv
        var id = TestDb.InsertKund(_db, "Inaktiv AB", aktiv: false);

        var kund = Repo.HämtaEfterId(id);

        Assert.NotNull(kund);
        Assert.False(kund!.Aktiv);
    }

    // ── Skapa ──────────────────────────────────────────────────────────────

    [Fact]
    public void Skapa_InserterNyKund()
    {
        Repo.Skapa("Ny Kund AB", "559000-0001", "Gatan 1", "11111", "Uppsala",
                   "info@nykund.se", "070-111111", KundTyp.Företag, 5.0);

        var alla = Repo.HämtaAlla().ToList();

        Assert.Single(alla);
        Assert.Equal("Ny Kund AB", alla[0].Namn);
    }

    [Fact]
    public void Skapa_SparrarAllaFält()
    {
        Repo.Skapa("Privat Person", "19900101-1234", "Hemvägen 3", "22222", "Malmö",
                   "privat@mail.se", "073-999999", KundTyp.Privat, 10.0);

        var k = Repo.HämtaAlla().First();

        Assert.Equal("Privat Person", k.Namn);
        Assert.Equal("19900101-1234", k.OrgNr);
        Assert.Equal("Hemvägen 3", k.Adress);
        Assert.Equal("22222", k.Postnr);
        Assert.Equal("Malmö", k.Ort);
        Assert.Equal("privat@mail.se", k.Email);
        Assert.Equal("073-999999", k.Tele);
        Assert.Equal(KundTyp.Privat, k.Typ);
        Assert.Equal(10.0, k.Rabatt);
        Assert.True(k.Aktiv);
    }

    [Fact]
    public void Skapa_NollRabatt_ÄrTillåten()
    {
        Repo.Skapa("Noll Rabatt AB", "556000-0000", "Vägen 1", "33333", "Lund",
                   "a@b.se", "070-000000", KundTyp.Företag, 0.0);

        var k = Repo.HämtaAlla().First();
        Assert.Equal(0.0, k.Rabatt);
    }

    // ── Uppdatera ──────────────────────────────────────────────────────────

    [Fact]
    public void Uppdatera_UppdaterarNamn()
    {
        var id = TestDb.InsertKund(_db, "Gammalt Namn");

        Repo.Uppdatera(id, namn: "Nytt Namn", null, null, null, null);

        Assert.Equal("Nytt Namn", Repo.HämtaEfterId(id)!.Namn);
    }

    [Fact]
    public void Uppdatera_UppdaterarEmail()
    {
        var id = TestDb.InsertKund(_db);

        Repo.Uppdatera(id, null, null, null, email: "ny@email.se", null);

        Assert.Equal("ny@email.se", Repo.HämtaEfterId(id)!.Email);
    }

    [Fact]
    public void Uppdatera_UppdaterarRabatt()
    {
        var id = TestDb.InsertKund(_db, rabatt: 0.0);

        Repo.Uppdatera(id, null, null, null, null, rabatt: 15.0);

        Assert.Equal(15.0, Repo.HämtaEfterId(id)!.Rabatt);
    }

    [Fact]
    public void Uppdatera_AllaNull_GörIngeting()
    {
        var id = TestDb.InsertKund(_db, "Orörd AB");

        Repo.Uppdatera(id, null, null, null, null, null);

        Assert.Equal("Orörd AB", Repo.HämtaEfterId(id)!.Namn);
    }

    [Fact]
    public void Uppdatera_FleraFält_UppdaterasSimultant()
    {
        var id = TestDb.InsertKund(_db, "Gammalt", ort: "Gamla Ort");

        Repo.Uppdatera(id, namn: "Nytt", null, ort: "Ny Ort", null, null);

        var k = Repo.HämtaEfterId(id)!;
        Assert.Equal("Nytt", k.Namn);
        Assert.Equal("Ny Ort", k.Ort);
    }

    // ── HarObetaldaFakturor ────────────────────────────────────────────────

    [Fact]
    public void HarObetaldaFakturor_IngaFakturor_ReturnararFalse()
    {
        var kundId = TestDb.InsertKund(_db);

        Assert.False(Repo.HarObetaldaFakturor(kundId));
    }

    [Fact]
    public void HarObetaldaFakturor_NyFaktura_ReturnararTrue()
    {
        var kundId = TestDb.InsertKund(_db);
        TestDb.InsertFaktura(_db, kundId, FakturaStatus.Ny);

        Assert.True(Repo.HarObetaldaFakturor(kundId));
    }

    [Fact]
    public void HarObetaldaFakturor_SkickadFaktura_ReturnararTrue()
    {
        var kundId = TestDb.InsertKund(_db);
        TestDb.InsertFaktura(_db, kundId, FakturaStatus.Skickad);

        Assert.True(Repo.HarObetaldaFakturor(kundId));
    }

    [Fact]
    public void HarObetaldaFakturor_BetaldFaktura_ReturnararFalse()
    {
        var kundId = TestDb.InsertKund(_db);
        TestDb.InsertFaktura(_db, kundId, FakturaStatus.Betald);

        Assert.False(Repo.HarObetaldaFakturor(kundId));
    }

    [Fact]
    public void HarObetaldaFakturor_KrediteradFaktura_ReturnararFalse()
    {
        var kundId = TestDb.InsertKund(_db);
        TestDb.InsertFaktura(_db, kundId, FakturaStatus.Krediterad);

        Assert.False(Repo.HarObetaldaFakturor(kundId));
    }

    [Fact]
    public void HarObetaldaFakturor_BlandadStatus_ReturnararTrue()
    {
        var kundId = TestDb.InsertKund(_db);
        TestDb.InsertFaktura(_db, kundId, FakturaStatus.Betald);
        TestDb.InsertFaktura(_db, kundId, FakturaStatus.Ny);

        Assert.True(Repo.HarObetaldaFakturor(kundId));
    }

    // ── Inaktivera ─────────────────────────────────────────────────────────

    [Fact]
    public void Inaktivera_SätterAktivTillFalse()
    {
        var id = TestDb.InsertKund(_db, aktiv: true);

        Repo.Inaktivera(id);

        var kund = Repo.HämtaEfterId(id)!;
        Assert.False(kund.Aktiv);
    }

    [Fact]
    public void Inaktivera_FörsvinneurHämtaAlla()
    {
        var id = TestDb.InsertKund(_db);

        Repo.Inaktivera(id);

        Assert.Empty(Repo.HämtaAlla());
    }

    [Fact]
    public void Inaktivera_EjBefintligtId_GörIngeting()
    {
        // Ska inte kasta undantag
        Repo.Inaktivera(99999);
    }

    // ── HämtaStatistik ─────────────────────────────────────────────────────

    [Fact]
    public void HämtaStatistik_FinnsEj_ReturnararNull()
    {
        Assert.Null(Repo.HämtaStatistik(99999));
    }

    [Fact]
    public void HämtaStatistik_MedBetaldaFakturor_ReturnararKorrektSummor()
    {
        var kundId = TestDb.InsertKund(_db, "Statistik AB");
        TestDb.InsertFaktura(_db, kundId, FakturaStatus.Betald, totalt: 1000.0);
        TestDb.InsertFaktura(_db, kundId, FakturaStatus.Betald, totalt: 2000.0);

        var stat = Repo.HämtaStatistik(kundId);

        Assert.NotNull(stat);
        Assert.Equal("Statistik AB", stat!.Namn);
        Assert.Equal(2, stat.AntalFakturor);
        Assert.Equal(3000.0, stat.TotaltFakturerat, precision: 2);
        Assert.Equal(3000.0, stat.Betalt, precision: 2);
        Assert.Equal(0.0, stat.Utestående, precision: 2);
    }

    [Fact]
    public void HämtaStatistik_MedObetaldaFakturor_ReturnararKorrektUtestående()
    {
        var kundId = TestDb.InsertKund(_db, "Skuld AB");
        TestDb.InsertFaktura(_db, kundId, FakturaStatus.Ny, totalt: 500.0);
        TestDb.InsertFaktura(_db, kundId, FakturaStatus.Betald, totalt: 1000.0);

        var stat = Repo.HämtaStatistik(kundId);

        Assert.NotNull(stat);
        Assert.Equal(500.0, stat!.Utestående, precision: 2);
        Assert.Equal(1000.0, stat.Betalt, precision: 2);
    }

    [Fact]
    public void HämtaStatistik_UtanFakturor_ReturnararNollor()
    {
        // Edge case: LEFT JOIN ger NULL för SUM(f.Totalt) – testar körtidsbeteende
        var kundId = TestDb.InsertKund(_db, "Tom Kund");

        var stat = Repo.HämtaStatistik(kundId);

        Assert.NotNull(stat);
        Assert.Equal(0, stat!.AntalFakturor);
        // SUM(f.Totalt) är NULL utan COALESCE – detta kan kasta undantag (känd bugg)
        // Om testet passerar är det OK, om det kastar är det en bugg att åtgärda
        Assert.Equal(0.0, stat.TotaltFakturerat, precision: 2);
    }
}
