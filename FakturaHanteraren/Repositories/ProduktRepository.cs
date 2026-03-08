using Microsoft.Data.Sqlite;
using System.Globalization;

namespace FakturaHanteraren;

interface IProduktRepository
{
    IEnumerable<Produkt> HämtaAlla();
    Produkt? HämtaEfterId(int id);
    void Skapa(string namn, string beskrivning, double pris, string enhet, string kategori, int lagerSaldo);
    void UppdateraPris(int id, double pris);
    void MinskaLager(int id, double antal);
    IEnumerable<LagerStatusRad> HämtaLagerStatus();
}

class ProduktRepository(SqliteConnection c) : IProduktRepository
{
    public IEnumerable<Produkt> HämtaAlla()
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT * FROM Produkter WHERE Aktiv=1 ORDER BY Kategori, Namn";
        using var r = cmd.ExecuteReader();
        var result = new List<Produkt>();
        while (r.Read()) result.Add(MapTillProdukt(r));
        return result;
    }

    public Produkt? HämtaEfterId(int id)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT * FROM Produkter WHERE Id=@id AND Aktiv=1";
        cmd.Parameters.AddWithValue("@id", id);
        using var r = cmd.ExecuteReader();
        return r.Read() ? MapTillProdukt(r) : null;
    }

    public void Skapa(string namn, string beskrivning, double pris, string enhet, string kategori, int lagerSaldo)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "INSERT INTO Produkter (Namn,Beskrivning,Pris,Enhet,MomsKod,Kategori,LagerSaldo) VALUES (@n,@b,@p,@e,1,@k,@l)";
        cmd.Parameters.AddWithValue("@n", namn);
        cmd.Parameters.AddWithValue("@b", beskrivning);
        cmd.Parameters.AddWithValue("@p", pris);
        cmd.Parameters.AddWithValue("@e", enhet);
        cmd.Parameters.AddWithValue("@k", kategori);
        cmd.Parameters.AddWithValue("@l", lagerSaldo);
        cmd.ExecuteNonQuery();
    }

    public void UppdateraPris(int id, double pris)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "UPDATE Produkter SET Pris=@p WHERE Id=@id";
        cmd.Parameters.AddWithValue("@p", pris);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public void MinskaLager(int id, double antal)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "UPDATE Produkter SET LagerSaldo=LagerSaldo-@antal WHERE Id=@id";
        cmd.Parameters.AddWithValue("@antal", antal);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public IEnumerable<LagerStatusRad> HämtaLagerStatus()
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT Namn, LagerSaldo, Kategori FROM Produkter WHERE Aktiv=1 AND LagerSaldo>0 ORDER BY LagerSaldo";
        using var r = cmd.ExecuteReader();
        var result = new List<LagerStatusRad>();
        while (r.Read()) result.Add(new(r.GetString(0), r.GetInt32(1), r.GetString(2)));
        return result;
    }

    private static Produkt MapTillProdukt(SqliteDataReader r) =>
        new(r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetDouble(3), r.GetString(4), r.GetString(6), r.GetInt32(8));
}
