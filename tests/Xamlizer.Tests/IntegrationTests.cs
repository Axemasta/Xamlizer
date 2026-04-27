namespace Xamlizer.Tests;

public class IntegrationTests
{
    [Fact]
    public Task Colors_GeneratesExpectedSource()
    {
        var xaml = TestResources.GetColorsXaml();
        var parseResult = XamlParser.Parse("Colors", xaml);
        var source = CSharpCodeGenerator.Generate(parseResult, "MyApp");
        return Verify(source);
    }
}
