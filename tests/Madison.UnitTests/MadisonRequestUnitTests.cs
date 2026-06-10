namespace Canonical.Madison.UnitTests;

public class MadisonRequestUnitTests
{
    [Fact]
    public void BuildRequestUri_WithMultipleFilters_ConstructsCorrectUri()
    {
        var request = new MadisonRequest(
            PackageNames: ["dotnet8"],
            Architectures: ["source", "amd64", "arm64"],
            Suites: ["jammy-updates", "jammy-security"],
            Components: ["universe", "main"],
            IncludeBinaryPackagesOfSourcePackages: true);

        var requestUri = request.BuildRequestUri(WellKnownMadisonEndpoints.Ubuntu);
        Assert.Equal(expected: new Uri("https://ubuntu-archive-team.ubuntu.com/madison.cgi?text=on&package=dotnet8&a=source,amd64,arm64&c=universe,main&s=jammy-updates,jammy-security&S=on"), actual: requestUri);
    }
}