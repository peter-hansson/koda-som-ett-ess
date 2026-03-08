using Microsoft.Data.Sqlite;

namespace FakturaHanteraren;

interface IPåminnelseRepository
{
    void Registrera(int fakturaId, PåminnelseTyp typ, double avgift);
}

class PåminnelseRepository(SqliteConnection c) : IPåminnelseRepository
{
    public void Registrera(int fakturaId, PåminnelseTyp typ, double avgift)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "INSERT INTO Påminnelser (FakturaId,Datum,Typ,Avgift) VALUES (@fid,@datum,@typ,@avgift)";
        cmd.Parameters.AddWithValue("@fid", fakturaId);
        cmd.Parameters.AddWithValue("@datum", DateTime.Now.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@typ", (int)typ);
        cmd.Parameters.AddWithValue("@avgift", avgift);
        cmd.ExecuteNonQuery();
    }
}
