namespace FakturaHanteraren;

// Körbar konfiguration – delas av alla lager som behöver inställningarna
class AppKonfiguration
{
    public decimal Moms { get; set; } = 0.25m;
    public int MaxFörfallodagar { get; set; } = Konstanter.StandardFörfallodag;
    public List<string> Logg { get; } = new();
}

// Inloggad session – sätts efter autentisering
record Session(int AnvändarId, string Roll);
