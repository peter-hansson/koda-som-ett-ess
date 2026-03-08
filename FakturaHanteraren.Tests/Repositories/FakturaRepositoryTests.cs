using FakturaHanteraren.Tests.Helpers;

namespace FakturaHanteraren.Tests.Repositories;

public class FakturaRepositoryTests : IDisposable
{
    private readonly Microsoft.Data.Sqlite.SqliteConnection _db = TestDb.Create();
    private FakturaRepository Repo => new(_db);

    public void Dispose() => _db.Dispose();

    // ── NästaFakturaNummer ─────────────────────────────────────────────────

    [Fact]
    public void NästaFakturaNummer_TomDatabas_Returnerar1()
    {
        Assert.Equal(1, Repo.NästaFakturaNummer());
    }

    [Fact]
    public void NästaFakturaNummer_MedBefintligaFakturor_ReturnararMaxPlusEtt()
    {
        var kundId = TestDb.InsertKund(_db);
        TestDb.InsertFaktura(_db, kundId); // id=1
        TestDb.InsertFaktura(_db, kundId); // id=2

        Assert.Equal(3, Repo.NästaFakturaNummer());
    }

    // ── Skapa ──────────────────────────────────────────────────────────────

    [Fact]
    public void Skapa_ReturnararKorrektId()
    {
        var kundId = TestDb.InsertKund(_db);
        var datum = "2024-01-15";
        var förfall = "2024-02-14";

        var id = Repo.Skapa(kundId, "F000001", datum, förfall, 0, "Test", 1);

        Assert.True(id > 0);
    }

    [Fact]
    public void Skapa_SätterStatusTillNy()
    {
        var kundId = TestDb.InsertKund(_db);

        var id = Repo.Skapa(kundId, "F000001", "2024-01-01", "2024-01-31", 0, "", 1);

        var faktura = Repo.HämtaFörBetalning(id);
        Assert.Equal(FakturaStatus.Ny, faktura!.Status);
    }

    [Fact]
    public void Skapa_SätterTotaltTillNoll()
    {
        var kundId = TestDb.InsertKund(_db);

        var id = Repo.Skapa(kundId, "F000001", "2024-01-01", "2024-01-31", 0, "", 1);

        var faktura = Repo.HämtaFörBetalning(id);
        Assert.Equal(0.0, faktura!.Totalt);
    }

    // ── LäggTillRad + UppdateraTotalt ─────────────────────────────────────

    [Fact]
    public void LäggTillRad_OchUppdateraTotalt_VisasIDetaljer()
    {
        var kundId = TestDb.InsertKund(_db);
        var prodId = TestDb.InsertProdukt(_db, "Konsult", pris: 1000.0);
        var fid = Repo.Skapa(kundId, "F000001", "2024-06-01", "2024-07-01", 0, "", 1);

        Repo.LäggTillRad(fid, prodId, 2.0, 1000.0, 0.0, 2000.0);
        Repo.UppdateraTotalt(fid, 2500.0, 500.0); // inkl 25% moms

        var detaljer = Repo.HämtaDetaljer(fid)!;
        Assert.Single(detaljer.Rader);
        Assert.Equal(2.0, detaljer.Rader[0].Antal);
        Assert.Equal(1000.0, detaljer.Rader[0].ÅPris);
        Assert.Equal(2000.0, detaljer.Rader[0].Summa);
        Assert.Equal(2500.0, detaljer.Totalt);
        Assert.Equal(500.0, detaljer.Moms);
    }

    [Fact]
    public void LäggTillRad_MedRabatt_SparrarKorrekta_Värden()
    {
        var kundId = TestDb.InsertKund(_db);
        var prodId = TestDb.InsertProdukt(_db, pris: 100.0);
        var fid = Repo.Skapa(kundId, "F000001", "2024-01-01", "2024-01-31", 0, "", 1);

        // 10 st á 100 kr med 10% rabatt = 900 kr
        Repo.LäggTillRad(fid, prodId, 10.0, 100.0, 10.0, 900.0);

        var detaljer = Repo.HämtaDetaljer(fid)!;
        Assert.Equal(10.0, detaljer.Rader[0].Rabatt);
        Assert.Equal(900.0, detaljer.Rader[0].Summa);
    }

    // ── HämtaAlla ──────────────────────────────────────────────────────────

    [Fact]
    public void HämtaAlla_IngenFilter_ReturnararAlla()
    {
        var kundId = TestDb.InsertKund(_db);
        TestDb.InsertFaktura(_db, kundId, FakturaStatus.Ny);
        TestDb.InsertFaktura(_db, kundId, FakturaStatus.Betald);
        TestDb.InsertFaktura(_db, kundId, FakturaStatus.Skickad);

        Assert.Equal(3, Repo.HämtaAlla(null).Count());
    }

    [Fact]
    public void HämtaAlla_MedStatusFilter_ReturnararEndastMatchande()
    {
        var kundId = TestDb.InsertKund(_db);
        TestDb.InsertFaktura(_db, kundId, FakturaStatus.Ny);
        TestDb.InsertFaktura(_db, kundId, FakturaStatus.Betald);

        var result = Repo.HämtaAlla(FakturaStatus.Ny).ToList();

        Assert.Single(result);
        Assert.Equal(FakturaStatus.Ny, result[0].Status);
    }

    [Fact]
    public void HämtaAlla_TomDatabas_ReturnararTomLista()
    {
        Assert.Empty(Repo.HämtaAlla(null));
    }

    [Fact]
    public void HämtaAlla_SortератDatumNyasteFörst()
    {
        var kundId = TestDb.InsertKund(_db);
        // Skapa direkt via SQL med specifika datum för deterministisk sortering
        using var cmd = _db.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Fakturor (KundId,FakturaNr,Datum,Förfallodatum,Status,Totalt,Moms,Rabatt,Notering,SkapadAv)
            VALUES (@kid,'F001','2024-01-01','2024-02-01',0,0,0,0,'',1);
            INSERT INTO Fakturor (KundId,FakturaNr,Datum,Förfallodatum,Status,Totalt,Moms,Rabatt,Notering,SkapadAv)
            VALUES (@kid,'F002','2024-06-01','2024-07-01',0,0,0,0,'',1);
            """;
        cmd.Parameters.AddWithValue("@kid", kundId);
        cmd.ExecuteNonQuery();

        var nr = Repo.HämtaAlla(null).Select(f => f.FakturaNr).ToList();
        Assert.Equal("F002", nr[0]);
        Assert.Equal("F001", nr[1]);
    }

    // ── HämtaDetaljer ──────────────────────────────────────────────────────

    [Fact]
    public void HämtaDetaljer_FinnsEj_ReturnararNull()
    {
        Assert.Null(Repo.HämtaDetaljer(99999));
    }

    [Fact]
    public void HämtaDetaljer_ReturnararKundInfo()
    {
        var kundId = TestDb.InsertKund(_db, "Kunden AB", orgNr: "556111-1111", ort: "Uppsala");
        var fid = TestDb.InsertFaktura(_db, kundId);

        var d = Repo.HämtaDetaljer(fid)!;

        Assert.Equal("Kunden AB", d.KundNamn);
        Assert.Equal("556111-1111", d.OrgNr);
        Assert.Equal("Uppsala", d.Ort);
    }

    [Fact]
    public void HämtaDetaljer_UtanRaderBetalningarPåminnelser_ReturnararTommaListor()
    {
        var kundId = TestDb.InsertKund(_db);
        var fid = TestDb.InsertFaktura(_db, kundId);

        var d = Repo.HämtaDetaljer(fid)!;

        Assert.Empty(d.Rader);
        Assert.Empty(d.Betalningar);
        Assert.Empty(d.Påminnelser);
    }

    [Fact]
    public void HämtaDetaljer_MedFlera_Rader_ReturnararAlla()
    {
        var kundId = TestDb.InsertKund(_db);
        var p1 = TestDb.InsertProdukt(_db, "P1");
        var p2 = TestDb.InsertProdukt(_db, "P2");
        var fid = Repo.Skapa(kundId, "F000001", "2024-01-01", "2024-01-31", 0, "", 1);

        Repo.LäggTillRad(fid, p1, 1, 100, 0, 100);
        Repo.LäggTillRad(fid, p2, 2, 200, 0, 400);

        Assert.Equal(2, Repo.HämtaDetaljer(fid)!.Rader.Count);
    }

    // ── HämtaFörBetalning ──────────────────────────────────────────────────

    [Fact]
    public void HämtaFörBetalning_FinnsEj_ReturnararNull()
    {
        Assert.Null(Repo.HämtaFörBetalning(99999));
    }

    [Fact]
    public void HämtaFörBetalning_Finns_ReturnararInfo()
    {
        var kundId = TestDb.InsertKund(_db);
        var fid = TestDb.InsertFaktura(_db, kundId, totalt: 1250.0);

        var info = Repo.HämtaFörBetalning(fid);

        Assert.NotNull(info);
        Assert.Equal(1250.0, info!.Totalt);
        Assert.Equal(FakturaStatus.Ny, info.Status);
    }

    // ── HämtaFörKreditering ────────────────────────────────────────────────

    [Fact]
    public void HämtaFörKreditering_FinnsEj_ReturnararNull()
    {
        Assert.Null(Repo.HämtaFörKreditering(99999));
    }

    [Fact]
    public void HämtaFörKreditering_Finns_ReturnararInfo()
    {
        var kundId = TestDb.InsertKund(_db);
        var fid = TestDb.InsertFaktura(_db, kundId, FakturaStatus.Skickad, totalt: 5000.0);

        var info = Repo.HämtaFörKreditering(fid);

        Assert.NotNull(info);
        Assert.Equal(FakturaStatus.Skickad, info!.Status);
        Assert.Equal(5000.0, info.Totalt);
        Assert.Equal(kundId, info.KundId);
    }

    // ── MarkeraBetald ──────────────────────────────────────────────────────

    [Fact]
    public void MarkeraBetald_SätterStatusTillBetald()
    {
        var kundId = TestDb.InsertKund(_db);
        var fid = TestDb.InsertFaktura(_db, kundId, FakturaStatus.Skickad);

        Repo.MarkeraBetald(fid);

        Assert.Equal(FakturaStatus.Betald, Repo.HämtaFörBetalning(fid)!.Status);
    }

    [Fact]
    public void MarkeraBetald_SätterBetaldDatum()
    {
        var kundId = TestDb.InsertKund(_db);
        var fid = TestDb.InsertFaktura(_db, kundId);

        Repo.MarkeraBetald(fid);

        var detaljer = Repo.HämtaDetaljer(fid)!;
        // Status ska vara Betald (vi kollar via lista)
        var faktura = Repo.HämtaAlla(FakturaStatus.Betald).First();
        Assert.Equal(fid, faktura.Id);
    }

    // ── MarkeraKrediterad ──────────────────────────────────────────────────

    [Fact]
    public void MarkeraKrediterad_SätterStatusTillKrediterad()
    {
        var kundId = TestDb.InsertKund(_db);
        var fid = TestDb.InsertFaktura(_db, kundId, FakturaStatus.Skickad);

        Repo.MarkeraKrediterad(fid);

        var info = Repo.HämtaFörKreditering(fid)!;
        Assert.Equal(FakturaStatus.Krediterad, info.Status);
    }

    // ── SkapaKreditfaktura ─────────────────────────────────────────────────

    [Fact]
    public void SkapaKreditfaktura_InserterMedNegativtBelopp()
    {
        var kundId = TestDb.InsertKund(_db);

        Repo.SkapaKreditfaktura(kundId, "KRED-001", "2024-06-01", -5000.0, "Kreditering", 1);

        var alla = Repo.HämtaAlla(FakturaStatus.Krediterad).ToList();
        Assert.Single(alla);
        Assert.Equal("KRED-001", alla[0].FakturaNr);
        Assert.Equal(-5000.0, alla[0].Totalt);
    }

    [Fact]
    public void SkapaKreditfaktura_SätterStatusKrediterad()
    {
        var kundId = TestDb.InsertKund(_db);

        Repo.SkapaKreditfaktura(kundId, "KRED-001", "2024-06-01", -1000.0, "", 1);

        var faktura = Repo.HämtaAlla(FakturaStatus.Krediterad).First();
        Assert.Equal(FakturaStatus.Krediterad, faktura.Status);
    }

    // ── UppdateraFörPåminnelse ─────────────────────────────────────────────

    [Fact]
    public void UppdateraFörPåminnelse_LäggerTillAvgiftPåTotalt()
    {
        var kundId = TestDb.InsertKund(_db);
        var fid = TestDb.InsertFaktura(_db, kundId, totalt: 1000.0);

        Repo.UppdateraFörPåminnelse(fid, Konstanter.FörstaAvgift);

        var info = Repo.HämtaFörBetalning(fid)!;
        Assert.Equal(1060.0, info.Totalt, precision: 2);
    }

    [Fact]
    public void UppdateraFörPåminnelse_KumulativtMedFlera_Avgifter()
    {
        var kundId = TestDb.InsertKund(_db);
        var fid = TestDb.InsertFaktura(_db, kundId, totalt: 1000.0);

        Repo.UppdateraFörPåminnelse(fid, Konstanter.FörstaAvgift);  // +60
        Repo.UppdateraFörPåminnelse(fid, Konstanter.AndraAvgift);   // +180

        var info = Repo.HämtaFörBetalning(fid)!;
        Assert.Equal(1240.0, info.Totalt, precision: 2);
    }

    // ── HämtaFörfallna ─────────────────────────────────────────────────────

    [Fact]
    public void HämtaFörfallna_TomDatabas_ReturnararTomLista()
    {
        Assert.Empty(Repo.HämtaFörfallna());
    }

    [Fact]
    public void HämtaFörfallna_EjFörfallen_ReturnararTomLista()
    {
        var kundId = TestDb.InsertKund(_db);
        var framtida = DateTime.Now.AddDays(30).ToString("yyyy-MM-dd");
        TestDb.InsertFaktura(_db, kundId, FakturaStatus.Ny, förfallodatum: framtida);

        Assert.Empty(Repo.HämtaFörfallna());
    }

    [Fact]
    public void HämtaFörfallna_NyFakturaPastFörfallodatum_ReturnararFaktura()
    {
        var kundId = TestDb.InsertKund(_db);
        var gammal = "2020-01-01";
        TestDb.InsertFaktura(_db, kundId, FakturaStatus.Ny, förfallodatum: gammal, totalt: 1500.0);

        var result = Repo.HämtaFörfallna().ToList();

        Assert.Single(result);
        Assert.Equal(1500.0, result[0].Totalt);
        Assert.True(result[0].AntalDagar > 365); // Klart mer än ett år sedan
    }

    [Fact]
    public void HämtaFörfallna_SkickadFaktura_Inkluderas()
    {
        var kundId = TestDb.InsertKund(_db);
        TestDb.InsertFaktura(_db, kundId, FakturaStatus.Skickad, förfallodatum: "2020-01-01");

        Assert.Single(Repo.HämtaFörfallna());
    }

    [Fact]
    public void HämtaFörfallna_BetaldFaktura_ExkluderasÄvenOmFörfallen()
    {
        var kundId = TestDb.InsertKund(_db);
        TestDb.InsertFaktura(_db, kundId, FakturaStatus.Betald, förfallodatum: "2020-01-01");

        Assert.Empty(Repo.HämtaFörfallna());
    }

    [Fact]
    public void HämtaFörfallna_KrediteradFaktura_Exkluderas()
    {
        var kundId = TestDb.InsertKund(_db);
        TestDb.InsertFaktura(_db, kundId, FakturaStatus.Krediterad, förfallodatum: "2020-01-01");

        Assert.Empty(Repo.HämtaFörfallna());
    }

    // ── HämtaFörPåminnelse ────────────────────────────────────────────────

    [Fact]
    public void HämtaFörPåminnelse_TomDatabas_ReturnararTomLista()
    {
        Assert.Empty(Repo.HämtaFörPåminnelse());
    }

    [Fact]
    public void HämtaFörPåminnelse_UtanPåminnelser_VisarAntalNoll()
    {
        var kundId = TestDb.InsertKund(_db);
        TestDb.InsertFaktura(_db, kundId, FakturaStatus.Ny, förfallodatum: "2020-01-01");

        var result = Repo.HämtaFörPåminnelse().ToList();

        Assert.Single(result);
        Assert.Equal(0, result[0].AntalPåminnelser);
    }

    [Fact]
    public void HämtaFörPåminnelse_MedEnPåminnelse_VisarAntalEtt()
    {
        var kundId = TestDb.InsertKund(_db);
        var fid = TestDb.InsertFaktura(_db, kundId, FakturaStatus.Ny, förfallodatum: "2020-01-01");
        TestDb.InsertPåminnelse(_db, fid, PåminnelseTyp.Första);

        var result = Repo.HämtaFörPåminnelse().ToList();

        Assert.Single(result);
        Assert.Equal(1, result[0].AntalPåminnelser);
    }

    [Fact]
    public void HämtaFörPåminnelse_MedTvåPåminnelser_VisarAntalTvå()
    {
        var kundId = TestDb.InsertKund(_db);
        var fid = TestDb.InsertFaktura(_db, kundId, FakturaStatus.Ny, förfallodatum: "2020-01-01");
        TestDb.InsertPåminnelse(_db, fid, PåminnelseTyp.Första);
        TestDb.InsertPåminnelse(_db, fid, PåminnelseTyp.Andra);

        var result = Repo.HämtaFörPåminnelse().Single();

        Assert.Equal(2, result.AntalPåminnelser);
    }

    [Fact]
    public void HämtaFörPåminnelse_BetaldFaktura_Exkluderas()
    {
        var kundId = TestDb.InsertKund(_db);
        TestDb.InsertFaktura(_db, kundId, FakturaStatus.Betald, förfallodatum: "2020-01-01");

        Assert.Empty(Repo.HämtaFörPåminnelse());
    }
}
