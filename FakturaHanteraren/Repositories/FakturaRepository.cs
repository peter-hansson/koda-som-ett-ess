using Microsoft.Data.Sqlite;

namespace FakturaHanteraren;

interface IFakturaRepository
{
    IEnumerable<FakturaListRad> HämtaAlla(FakturaStatus? filter);
    FakturaDetaljer? HämtaDetaljer(int id);
    FakturaBetalningsInfo? HämtaFörBetalning(int id);
    FakturaKrediteringsInfo? HämtaFörKreditering(int id);
    IEnumerable<FörfallenFaktura> HämtaFörfallna();
    IEnumerable<FörfallenFakturaFörPåminnelse> HämtaFörPåminnelse();
    int NästaFakturaNummer();
    int Skapa(int kundId, string fakturaNr, string datum, string förfallodatum, double kundRabatt, string notering, int skapadAv);
    void LäggTillRad(int fakturaId, int produktId, double antal, double pris, double rabatt, double summa);
    void UppdateraTotalt(int fakturaId, double totalt, double moms);
    void SkapaKreditfaktura(int kundId, string kredNr, string datum, double belopp, string notering, int skapadAv);
    void MarkeraKrediterad(int id);
    void MarkeraBetald(int id);
    void UppdateraFörPåminnelse(int id, double avgiftTillägg);
}

class FakturaRepository(SqliteConnection c) : IFakturaRepository
{
    public IEnumerable<FakturaListRad> HämtaAlla(FakturaStatus? filter)
    {
        using var cmd = c.CreateCommand();
        if (filter == null)
            cmd.CommandText = "SELECT f.Id, f.FakturaNr, k.Namn, f.Datum, f.Förfallodatum, f.Status, f.Totalt FROM Fakturor f JOIN Kunder k ON f.KundId=k.Id ORDER BY f.Datum DESC";
        else
        {
            cmd.CommandText = "SELECT f.Id, f.FakturaNr, k.Namn, f.Datum, f.Förfallodatum, f.Status, f.Totalt FROM Fakturor f JOIN Kunder k ON f.KundId=k.Id WHERE f.Status=@status ORDER BY f.Datum DESC";
            cmd.Parameters.AddWithValue("@status", (int)filter.Value);
        }
        using var r = cmd.ExecuteReader();
        var result = new List<FakturaListRad>();
        while (r.Read())
            result.Add(new(r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetString(3), r.GetString(4), (FakturaStatus)r.GetInt32(5), r.GetDouble(6)));
        return result;
    }

    public FakturaDetaljer? HämtaDetaljer(int id)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT f.*, k.Namn, k.OrgNr, k.Adress, k.Postnr, k.Ort FROM Fakturor f JOIN Kunder k ON f.KundId=k.Id WHERE f.Id=@id";
        cmd.Parameters.AddWithValue("@id", id);
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;

        var fakturaNr = r.GetString(2);
        var datum = r.GetString(3);
        var förfallodatum = r.GetString(4);
        var status = (FakturaStatus)r.GetInt32(5);
        var totalt = r.GetDouble(6);
        var moms = r.GetDouble(7);
        var kundNamn = r.GetString(12);
        var orgNr = r.GetString(13);
        var adress = r.GetString(14);
        var postnr = r.GetString(15);
        var ort = r.GetString(16);
        r.Close();

        var rader = HämtaRader(id);
        var betalningar = HämtaBetalningar(id);
        var påminnelser = HämtaPåminnelser(id);

        return new(fakturaNr, datum, förfallodatum, status, kundNamn, orgNr, adress, postnr, ort, totalt, moms, rader, betalningar, påminnelser);
    }

    public FakturaBetalningsInfo? HämtaFörBetalning(int id)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT FakturaNr, Totalt, Status FROM Fakturor WHERE Id=@id";
        cmd.Parameters.AddWithValue("@id", id);
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return new(r.GetString(0), r.GetDouble(1), (FakturaStatus)r.GetInt32(2));
    }

    public FakturaKrediteringsInfo? HämtaFörKreditering(int id)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT FakturaNr, Status, Totalt, KundId FROM Fakturor WHERE Id=@id";
        cmd.Parameters.AddWithValue("@id", id);
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return new(r.GetString(0), (FakturaStatus)r.GetInt32(1), r.GetDouble(2), r.GetInt32(3));
    }

    public IEnumerable<FörfallenFaktura> HämtaFörfallna()
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT f.Id, f.FakturaNr, k.Namn, f.Förfallodatum, f.Totalt, julianday('now')-julianday(f.Förfallodatum) as Dagar FROM Fakturor f JOIN Kunder k ON f.KundId=k.Id WHERE f.Status<@betald AND f.Förfallodatum<date('now') ORDER BY f.Förfallodatum";
        cmd.Parameters.AddWithValue("@betald", (int)FakturaStatus.Betald);
        using var r = cmd.ExecuteReader();
        var result = new List<FörfallenFaktura>();
        while (r.Read())
            result.Add(new(r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetString(3), r.GetDouble(4), r.GetDouble(5)));
        return result;
    }

    public IEnumerable<FörfallenFakturaFörPåminnelse> HämtaFörPåminnelse()
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT f.Id, f.FakturaNr, k.Namn, k.Email, f.Förfallodatum, f.Totalt, julianday('now')-julianday(f.Förfallodatum) as Dagar, (SELECT COUNT(*) FROM Påminnelser WHERE FakturaId=f.Id) as AntPåm FROM Fakturor f JOIN Kunder k ON f.KundId=k.Id WHERE f.Status<@betald AND f.Förfallodatum<date('now') ORDER BY f.Förfallodatum";
        cmd.Parameters.AddWithValue("@betald", (int)FakturaStatus.Betald);
        using var r = cmd.ExecuteReader();
        var result = new List<FörfallenFakturaFörPåminnelse>();
        while (r.Read())
            result.Add(new(r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetString(3), r.GetDouble(6), r.GetDouble(5), r.GetInt32(7)));
        return result;
    }

    public int NästaFakturaNummer()
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT MAX(Id) FROM Fakturor";
        var maxId = cmd.ExecuteScalar();
        return maxId == null || maxId == DBNull.Value ? 1 : (int)(long)maxId + 1;
    }

    public int Skapa(int kundId, string fakturaNr, string datum, string förfallodatum, double kundRabatt, string notering, int skapadAv)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "INSERT INTO Fakturor (KundId,FakturaNr,Datum,Förfallodatum,Status,Totalt,Moms,Rabatt,Notering,SkapadAv) VALUES (@kid,@fnr,@datum,@förfall,@status,0,0,@rab,@not,@usr)";
        cmd.Parameters.AddWithValue("@kid", kundId);
        cmd.Parameters.AddWithValue("@fnr", fakturaNr);
        cmd.Parameters.AddWithValue("@datum", datum);
        cmd.Parameters.AddWithValue("@förfall", förfallodatum);
        cmd.Parameters.AddWithValue("@status", (int)FakturaStatus.Ny);
        cmd.Parameters.AddWithValue("@rab", kundRabatt);
        cmd.Parameters.AddWithValue("@not", notering);
        cmd.Parameters.AddWithValue("@usr", skapadAv);
        cmd.ExecuteNonQuery();

        using var lastId = c.CreateCommand();
        lastId.CommandText = "SELECT last_insert_rowid()";
        return (int)(long)lastId.ExecuteScalar()!;
    }

    public void LäggTillRad(int fakturaId, int produktId, double antal, double pris, double rabatt, double summa)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "INSERT INTO FakturaRader (FakturaId,ProduktId,Antal,ÅPris,Rabatt,Summa) VALUES (@fid,@pid,@antal,@pris,@rab,@summa)";
        cmd.Parameters.AddWithValue("@fid", fakturaId);
        cmd.Parameters.AddWithValue("@pid", produktId);
        cmd.Parameters.AddWithValue("@antal", antal);
        cmd.Parameters.AddWithValue("@pris", pris);
        cmd.Parameters.AddWithValue("@rab", rabatt);
        cmd.Parameters.AddWithValue("@summa", summa);
        cmd.ExecuteNonQuery();
    }

    public void UppdateraTotalt(int fakturaId, double totalt, double moms)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "UPDATE Fakturor SET Totalt=@totalt, Moms=@moms WHERE Id=@id";
        cmd.Parameters.AddWithValue("@totalt", totalt);
        cmd.Parameters.AddWithValue("@moms", moms);
        cmd.Parameters.AddWithValue("@id", fakturaId);
        cmd.ExecuteNonQuery();
    }

    public void SkapaKreditfaktura(int kundId, string kredNr, string datum, double belopp, string notering, int skapadAv)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "INSERT INTO Fakturor (KundId,FakturaNr,Datum,Förfallodatum,Status,Totalt,Moms,Rabatt,Notering,SkapadAv) VALUES (@kid,@fnr,@datum,@datum,@status,@totalt,0,0,@not,@usr)";
        cmd.Parameters.AddWithValue("@kid", kundId);
        cmd.Parameters.AddWithValue("@fnr", kredNr);
        cmd.Parameters.AddWithValue("@datum", datum);
        cmd.Parameters.AddWithValue("@status", (int)FakturaStatus.Krediterad);
        cmd.Parameters.AddWithValue("@totalt", belopp);
        cmd.Parameters.AddWithValue("@not", notering);
        cmd.Parameters.AddWithValue("@usr", skapadAv);
        cmd.ExecuteNonQuery();
    }

    public void MarkeraKrediterad(int id)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "UPDATE Fakturor SET Status=@status WHERE Id=@id";
        cmd.Parameters.AddWithValue("@status", (int)FakturaStatus.Krediterad);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public void MarkeraBetald(int id)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "UPDATE Fakturor SET Status=@status, Betald=@datum WHERE Id=@id";
        cmd.Parameters.AddWithValue("@status", (int)FakturaStatus.Betald);
        cmd.Parameters.AddWithValue("@datum", DateTime.Now.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public void UppdateraFörPåminnelse(int id, double avgiftTillägg)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "UPDATE Fakturor SET Totalt=Totalt+@avgift WHERE Id=@id";
        cmd.Parameters.AddWithValue("@avgift", avgiftTillägg);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    private IReadOnlyList<FakturaRadVy> HämtaRader(int fakturaId)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT fr.*, p.Namn FROM FakturaRader fr JOIN Produkter p ON fr.ProduktId=p.Id WHERE fr.FakturaId=@id";
        cmd.Parameters.AddWithValue("@id", fakturaId);
        using var r = cmd.ExecuteReader();
        var result = new List<FakturaRadVy>();
        while (r.Read())
            result.Add(new(r.GetDouble(3), r.GetDouble(4), r.GetDouble(5), r.GetDouble(6), r.GetString(7)));
        return result;
    }

    private IReadOnlyList<Betalning> HämtaBetalningar(int fakturaId)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT * FROM Betalningar WHERE FakturaId=@id";
        cmd.Parameters.AddWithValue("@id", fakturaId);
        using var r = cmd.ExecuteReader();
        var result = new List<Betalning>();
        while (r.Read())
            result.Add(new(r.GetDouble(2), r.GetString(3), r.GetString(4), r.GetString(5)));
        return result;
    }

    private IReadOnlyList<Påminnelse> HämtaPåminnelser(int fakturaId)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT * FROM Påminnelser WHERE FakturaId=@id";
        cmd.Parameters.AddWithValue("@id", fakturaId);
        using var r = cmd.ExecuteReader();
        var result = new List<Påminnelse>();
        while (r.Read())
            result.Add(new(r.GetString(2), (PåminnelseTyp)r.GetInt32(3), r.GetDouble(4)));
        return result;
    }
}
