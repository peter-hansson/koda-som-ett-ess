namespace FakturaHanteraren.Tests.UI;

/// <summary>
/// Bas-klass som omdirigerar Console.In/Out för UI-tester.
/// Skapar en inmatningssträng och fångar skriven utmatning.
/// </summary>
public abstract class UITestBase : IDisposable
{
    private readonly TextWriter _originalOut = Console.Out;
    private readonly TextReader _originalIn = Console.In;

    protected StringWriter Output { get; } = new();

    protected void SetInput(params string[] rader)
    {
        var text = string.Join(Environment.NewLine, rader) + Environment.NewLine;
        Console.SetIn(new StringReader(text));
    }

    protected string GetOutput() => Output.ToString();

    protected UITestBase()
    {
        Console.SetOut(Output);
    }

    public void Dispose()
    {
        Console.SetOut(_originalOut);
        Console.SetIn(_originalIn);
        Output.Dispose();
    }
}
