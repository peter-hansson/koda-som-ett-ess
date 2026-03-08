namespace FakturaHanteraren.Tests.Models;

public class EnumsTests
{
    [Fact]
    public void FakturaStatus_HarKorrekta_Värden()
    {
        Assert.Equal(0, (int)FakturaStatus.Ny);
        Assert.Equal(1, (int)FakturaStatus.Skickad);
        Assert.Equal(2, (int)FakturaStatus.Betald);
        Assert.Equal(3, (int)FakturaStatus.Krediterad);
    }

    [Fact]
    public void KundTyp_HarKorrekta_Värden()
    {
        Assert.Equal(0, (int)KundTyp.Privat);
        Assert.Equal(1, (int)KundTyp.Företag);
        Assert.Equal(2, (int)KundTyp.Kommun);
    }

    [Fact]
    public void PåminnelseTyp_HarKorrekta_Värden()
    {
        Assert.Equal(1, (int)PåminnelseTyp.Första);
        Assert.Equal(2, (int)PåminnelseTyp.Andra);
        Assert.Equal(3, (int)PåminnelseTyp.Inkasso);
    }

    [Fact]
    public void Konstanter_FörstaAvgift_Är60()
    {
        Assert.Equal(60.0, Konstanter.FörstaAvgift);
    }

    [Fact]
    public void Konstanter_AndraAvgift_Är180()
    {
        Assert.Equal(180.0, Konstanter.AndraAvgift);
    }

    [Fact]
    public void Konstanter_InkassoAvgift_Är450()
    {
        Assert.Equal(450.0, Konstanter.InkassoAvgift);
    }

    [Fact]
    public void Konstanter_StandardFörfallodag_Är30()
    {
        Assert.Equal(30, Konstanter.StandardFörfallodag);
    }

    [Fact]
    public void Konstanter_KommunFörfallodag_Är60()
    {
        Assert.Equal(60, Konstanter.KommunFörfallodag);
    }

    [Fact]
    public void FakturaStatus_BetaldStörreÄnSkickad()
    {
        // HarObetaldaFakturor och HämtaFörfallna använder Status < Betald
        Assert.True((int)FakturaStatus.Betald > (int)FakturaStatus.Skickad);
        Assert.True((int)FakturaStatus.Betald > (int)FakturaStatus.Ny);
    }
}
