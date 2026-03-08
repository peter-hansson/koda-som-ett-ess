namespace FakturaHanteraren;

enum FakturaStatus { Ny = 0, Skickad = 1, Betald = 2, Krediterad = 3 }
enum KundTyp { Privat = 0, Företag = 1, Kommun = 2 }
enum PåminnelseTyp { Första = 1, Andra = 2, Inkasso = 3 }

static class Konstanter
{
    public const double FörstaAvgift = 60.0;
    public const double AndraAvgift = 180.0;
    public const double InkassoAvgift = 450.0;
    public const int StandardFörfallodag = 30;
    public const int KommunFörfallodag = 60;
}
