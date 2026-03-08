using Microsoft.Data.Sqlite;

namespace FakturaHanteraren;

interface IBetalningsRepository
{
    double HämtaSummaBetalt(int fakturaId);
    void Registrera(int fakturaId, double belopp, string metod, string referens);
    IEnumerable<BetalningsHistorikRad> HämtaHistorik(int? fakturaId);
}

class BetalningsRepository(SqliteConnection c) : IBetalningsRepository
{
    public double HämtaSummaBetalt(int fakturaId)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT COALESCE(SUM(Belopp),0) FROM Betalningar WHERE FakturaId=@id";
        cmd.Parameters.AddWithValue("@id", fakturaId);
        return (double)cmd.ExecuteScalar()!;
    }

    public void Registrera(int fakturaId, double belopp, string metod, string referens)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "INSERT INTO Betalningar (FakturaId,Belopp,Datum,Metod,Referens) VALUES (@fid,@bel,@datum,@met,@reff)";
        cmd.Parameters.AddWithValue("@fid", fakturaId);
        cmd.Parameters.AddWithValue("@bel", belopp);
        cmd.Parameters.AddWithValue("@datum", DateTime.Now.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@met", metod);
        cmd.Parameters.AddWithValue("@reff", referens);
        cmd.ExecuteNonQuery();
    }

    public IEnumerable<BetalningsHistorikRad> HämtaHistorik(int? fakturaId)
    {
        using var cmd = c.CreateCommand();
        if (fakturaId == null)
        {
            cmd.CommandText = "SELECT b.*, f.FakturaNr FROM Betalningar b JOIN Fakturor f ON b.FakturaId=f.Id ORDER BY b.Datum DESC LIMIT 50";
        }
        else
        {
            cmd.CommandText = "SELECT b.*, f.FakturaNr FROM Betalningar b JOIN Fakturor f ON b.FakturaId=f.Id WHERE b.FakturaId=@id ORDER BY b.Datum DESC";
            cmd.Parameters.AddWithValue("@id", fakturaId.Value);
        }
        using var r = cmd.ExecuteReader();
        var result = new List<BetalningsHistorikRad>();
        while (r.Read())
            result.Add(new(r.GetString(3), r.GetString(6), r.GetDouble(2), r.GetString(4), r.GetString(5)));
        return result;
    }
}
