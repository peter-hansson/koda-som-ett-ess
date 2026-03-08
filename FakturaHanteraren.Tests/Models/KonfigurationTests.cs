namespace FakturaHanteraren.Tests.Models;

public class KonfigurationTests
{
    [Fact]
    public void DefaultMoms_Är25Procent()
    {
        var k = new AppKonfiguration();
        Assert.Equal(0.25m, k.Moms);
    }

    [Fact]
    public void DefaultMaxFörfallodagar_ÄrStandard()
    {
        var k = new AppKonfiguration();
        Assert.Equal(Konstanter.StandardFörfallodag, k.MaxFörfallodagar);
    }

    [Fact]
    public void Logg_StarterTom()
    {
        var k = new AppKonfiguration();
        Assert.Empty(k.Logg);
    }

    [Fact]
    public void Moms_KanÄndras()
    {
        var k = new AppKonfiguration();
        k.Moms = 0.12m;
        Assert.Equal(0.12m, k.Moms);
    }

    [Fact]
    public void MaxFörfallodagar_KanÄndras()
    {
        var k = new AppKonfiguration();
        k.MaxFörfallodagar = 60;
        Assert.Equal(60, k.MaxFörfallodagar);
    }

    [Fact]
    public void Logg_KanLäggasTill()
    {
        var k = new AppKonfiguration();
        k.Logg.Add("händelse 1");
        k.Logg.Add("händelse 2");
        Assert.Equal(2, k.Logg.Count);
    }

    [Fact]
    public void Session_SkaparMedKorrektaVärden()
    {
        var s = new Session(42, "Admin");
        Assert.Equal(42, s.AnvändarId);
        Assert.Equal("Admin", s.Roll);
    }

    [Fact]
    public void Session_Record_EqualityFungerarKorrekt()
    {
        var s1 = new Session(1, "Admin");
        var s2 = new Session(1, "Admin");
        var s3 = new Session(2, "Admin");
        Assert.Equal(s1, s2);
        Assert.NotEqual(s1, s3);
    }
}
