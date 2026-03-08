using Microsoft.Data.Sqlite;
using System.Globalization;

namespace FakturaHanteraren;

interface IKundRepository
{
    IEnumerable<Kund> HämtaAlla();
    IEnumerable<Kund> Sök(string term);
    Kund? HämtaEfterId(int id);
    void Skapa(string namn, string orgNr, string adress, string postnr, string ort, string email, string tele, KundTyp typ, double rabatt);
    void Uppdatera(int id, string? namn, string? adress, string? ort, string? email, double? rabatt);
    bool HarObetaldaFakturor(int id);
    void Inaktivera(int id);
    KundStatistik? HämtaStatistik(int id);
}

class KundRepository(SqliteConnection c) : IKundRepository
{
    public IEnumerable<Kund> HämtaAlla()
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT * FROM Kunder WHERE Aktiv=1 ORDER BY Namn";
        using var r = cmd.ExecuteReader();
        var result = new List<Kund>();
        while (r.Read()) result.Add(MapTillKund(r));
        return result;
    }

    public IEnumerable<Kund> Sök(string term)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT * FROM Kunder WHERE Namn LIKE @term OR OrgNr LIKE @term OR Ort LIKE @term";
        cmd.Parameters.AddWithValue("@term", $"%{term}%");
        using var r = cmd.ExecuteReader();
        var result = new List<Kund>();
        while (r.Read()) result.Add(MapTillKund(r));
        return result;
    }

    public Kund? HämtaEfterId(int id)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT * FROM Kunder WHERE Id=@id";
        cmd.Parameters.AddWithValue("@id", id);
        using var r = cmd.ExecuteReader();
        return r.Read() ? MapTillKund(r) : null;
    }

    public void Skapa(string namn, string orgNr, string adress, string postnr, string ort, string email, string tele, KundTyp typ, double rabatt)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "INSERT INTO Kunder (Namn,OrgNr,Adress,Postnr,Ort,Email,Tele,KundTyp,Rabatt,Skapad,Uppdaterad) VALUES (@n,@o,@a,@pn,@ort,@e,@t,@kt,@rab,datetime('now'),datetime('now'))";
        cmd.Parameters.AddWithValue("@n", namn);
        cmd.Parameters.AddWithValue("@o", orgNr);
        cmd.Parameters.AddWithValue("@a", adress);
        cmd.Parameters.AddWithValue("@pn", postnr);
        cmd.Parameters.AddWithValue("@ort", ort);
        cmd.Parameters.AddWithValue("@e", email);
        cmd.Parameters.AddWithValue("@t", tele);
        cmd.Parameters.AddWithValue("@kt", (int)typ);
        cmd.Parameters.AddWithValue("@rab", rabatt);
        cmd.ExecuteNonQuery();
    }

    public void Uppdatera(int id, string? namn, string? adress, string? ort, string? email, double? rabatt)
    {
        var sets = new List<string>();
        using var cmd = c.CreateCommand();
        if (namn != null) { sets.Add("Namn=@namn"); cmd.Parameters.AddWithValue("@namn", namn); }
        if (adress != null) { sets.Add("Adress=@adress"); cmd.Parameters.AddWithValue("@adress", adress); }
        if (ort != null) { sets.Add("Ort=@ort"); cmd.Parameters.AddWithValue("@ort", ort); }
        if (email != null) { sets.Add("Email=@email"); cmd.Parameters.AddWithValue("@email", email); }
        if (rabatt != null) { sets.Add("Rabatt=@rabatt"); cmd.Parameters.AddWithValue("@rabatt", rabatt.Value); }
        if (sets.Count == 0) return;
        sets.Add("Uppdaterad=datetime('now')");
        cmd.CommandText = $"UPDATE Kunder SET {string.Join(",", sets)} WHERE Id=@id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public bool HarObetaldaFakturor(int id)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Fakturor WHERE KundId=@id AND Status<@betald";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@betald", (int)FakturaStatus.Betald);
        return (long)cmd.ExecuteScalar()! > 0;
    }

    public void Inaktivera(int id)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "UPDATE Kunder SET Aktiv=0, Uppdaterad=datetime('now') WHERE Id=@id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public KundStatistik? HämtaStatistik(int id)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT k.Namn, COUNT(f.Id), COALESCE(SUM(f.Totalt),0), COALESCE(SUM(CASE WHEN f.Status=@betald THEN f.Totalt ELSE 0 END),0), COALESCE(SUM(CASE WHEN f.Status<@betald THEN f.Totalt ELSE 0 END),0) FROM Kunder k LEFT JOIN Fakturor f ON k.Id=f.KundId WHERE k.Id=@id GROUP BY k.Namn";
        cmd.Parameters.AddWithValue("@betald", (int)FakturaStatus.Betald);
        cmd.Parameters.AddWithValue("@id", id);
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return new KundStatistik(r.GetString(0), r.GetInt32(1), r.GetDouble(2), r.GetDouble(3), r.GetDouble(4));
    }

    private static Kund MapTillKund(SqliteDataReader r) =>
        new(r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetString(3), r.GetString(4),
            r.GetString(5), r.GetString(6), r.GetString(7), (KundTyp)r.GetInt32(8), r.GetDouble(9), r.GetInt32(10) == 1);
}
