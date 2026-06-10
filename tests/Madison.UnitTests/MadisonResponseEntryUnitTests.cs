namespace Canonical.Madison.UnitTests;

public class MadisonResponseEntryUnitTests
{
    [Fact]
    public void Parse_WithValidInput_ReturnsCorrectEntry()
    {
        var line = " dotnet8 | 8.0.100-8.0.0~rc1-0ubuntu1 | mantic/universe | source, amd64, arm64 ";

        var result = MadisonResponseEntry.Parse(line);

        Assert.Equal("dotnet8", result.PackageName);
        Assert.Equal("8.0.100-8.0.0~rc1-0ubuntu1", result.Version);
        Assert.Equal("mantic/universe", result.Suite);
        Assert.Equal("mantic", result.Series);
        Assert.Equal("universe", result.Component);
        Assert.Equal(["source", "amd64", "arm64"], result.Architectures);
    }

    [Fact]
    public void Parse_WithValidInputAndNoComponent_ReturnsCorrectEntry()
    {
        var line = "gcc | 11.2.0-1 | jammy | amd64, arm64";

        var result = MadisonResponseEntry.Parse(line);

        Assert.Equal("gcc", result.PackageName);
        Assert.Equal("11.2.0-1", result.Version);
        Assert.Equal("jammy", result.Suite);
        Assert.Equal("jammy", result.Series);
        Assert.Empty(result.Component);
        Assert.Equal(["amd64", "arm64"], result.Architectures);
    }

    [Fact]
    public void Parse_WithSingleArchitecture_ReturnsCorrectEntry()
    {
        var line = "package | 1.0.0 | focal/main | amd64";

        var result = MadisonResponseEntry.Parse(line);

        Assert.Single(result.Architectures);
        Assert.Equal("amd64", result.Architectures[0]);
    }

    [Fact]
    public void Parse_WithMinimalValidInput_ReturnsCorrectEntry()
    {
        var line = "a|b|c|d";

        var result = MadisonResponseEntry.Parse(line);

        Assert.Equal("a", result.PackageName);
        Assert.Equal("b", result.Version);
        Assert.Equal("c", result.Suite);
        Assert.Equal("c", result.Series);
        Assert.Empty(result.Component);
        Assert.Equal(["d"], result.Architectures);
    }

    [Fact]
    public void Parse_WithComplexPackageName_ReturnsCorrectEntry()
    {
        var line = "lib32-gcc-11-dev | 11.2.0-19ubuntu1 | jammy/universe | amd64";

        var result = MadisonResponseEntry.Parse(line);

        Assert.Equal("lib32-gcc-11-dev", result.PackageName);
    }

    [Fact]
    public void Parse_WithComplexVersion_ReturnsCorrectEntry()
    {
        var line = "golang-go | 2:1.18~2ubuntu1 | jammy/universe | amd64";

        var result = MadisonResponseEntry.Parse(line);

        Assert.Equal("2:1.18~2ubuntu1", result.Version);
    }

    [Fact]
    public void Parse_WithNullInput_ThrowsArgumentNullException()
    {
        string? line = null;

        Assert.Throws<ArgumentNullException>(() => MadisonResponseEntry.Parse(line!));
    }

    [Fact]
    public void Parse_WithEmptyString_ThrowsFormatException()
    {
        var line = "";

        Assert.Throws<FormatException>(() => MadisonResponseEntry.Parse(line));
    }

    [Fact]
    public void Parse_WithTooFewFields_ThrowsFormatException()
    {
        var line = "package | version | jammy";

        Assert.Throws<FormatException>(() => MadisonResponseEntry.Parse(line));
    }

    [Fact]
    public void Parse_WithTooManyFields_ThrowsFormatException()
    {
        var line = "package | version | jammy/main | amd64 | extra";

        Assert.Throws<FormatException>(() => MadisonResponseEntry.Parse(line));
    }

    [Fact]
    public void Parse_WithEmptyPackageName_ThrowsFormatException()
    {
        var line = "  | 1.0 | jammy/main | amd64";

        Assert.Throws<FormatException>(() => MadisonResponseEntry.Parse(line));
    }

    [Fact]
    public void Parse_WithEmptyVersion_ThrowsFormatException()
    {
        var line = "package |  | jammy/main | amd64";

        Assert.Throws<FormatException>(() => MadisonResponseEntry.Parse(line));
    }

    [Fact]
    public void Parse_WithEmptySuite_ThrowsFormatException()
    {
        var line = "package | 1.0 |  | amd64";

        Assert.Throws<FormatException>(() => MadisonResponseEntry.Parse(line));
    }

    [Fact]
    public void Parse_WithEmptyArchitectures_ThrowsFormatException()
    {
        var line = "package | 1.0 | jammy/main |  ";

        Assert.Throws<FormatException>(() => MadisonResponseEntry.Parse(line));
    }

    [Fact]
    public void Parse_WithSuiteLeadingSlash_ThrowsFormatException()
    {
        var line = "package | 1.0 | /main | amd64";
        
        Assert.Throws<FormatException>(() => MadisonResponseEntry.Parse(line));
    }

    [Fact]
    public void Parse_WithSuiteTrailingSlash_ThrowsFormatException()
    {
        var line = "package | 1.0 | jammy/ | amd64";

        Assert.Throws<FormatException>(() => MadisonResponseEntry.Parse(line));
    }

    [Fact]
    public void TryParse_WithValidStringInput_ReturnsTrueAndCorrectEntry()
    {
        var line = " dotnet8 | 8.0.100-8.0.0~rc1-0ubuntu1 | mantic/universe | source, amd64, arm64 ";

        var success = MadisonResponseEntry.TryParse(line, out var result);

        Assert.True(success);
        Assert.Equal("dotnet8", result.PackageName);
        Assert.Equal("8.0.100-8.0.0~rc1-0ubuntu1", result.Version);
        Assert.Equal("mantic/universe", result.Suite);
        Assert.Equal("mantic", result.Series);
        Assert.Equal("universe", result.Component);
        Assert.Equal(["source", "amd64", "arm64"], result.Architectures);
    }

    [Fact]
    public void TryParse_WithNullStringInput_ReturnsFalse()
    {
        string? line = null;

        var success = MadisonResponseEntry.TryParse(line, out var result);

        Assert.False(success);
        Assert.Equal(default, result);
    }

    [Fact]
    public void TryParse_WithEmptyString_ReturnsFalse()
    {
        var line = "";

        var success = MadisonResponseEntry.TryParse(line, out var result);

        Assert.False(success);
    }

    [Fact]
    public void TryParse_WithTooFewFields_ReturnsFalse()
    {
        var line = "package | version | jammy";

        var success = MadisonResponseEntry.TryParse(line, out var result);

        Assert.False(success);
    }

    [Fact]
    public void TryParse_WithTooManyFields_ReturnsFalse()
    {
        var line = "package | version | jammy/main | amd64 | extra";

        var success = MadisonResponseEntry.TryParse(line, out var result);

        Assert.False(success);
    }

    [Fact]
    public void TryParse_WithEmptyField_ReturnsFalse()
    {
        var line = "package |  | jammy/main | amd64";

        var success = MadisonResponseEntry.TryParse(line, out var result);

        Assert.False(success);
    }

    [Fact]
    public void TryParse_WithSuiteLeadingSlash_ReturnsFalse()
    {
        var line = "package | 1.0 | /main | amd64";

        var success = MadisonResponseEntry.TryParse(line, out var result);

        Assert.False(success);
    }

    [Fact]
    public void TryParseSpan_WithValidInput_ReturnsTrueAndCorrectEntry()
    {
        var line = " dotnet8 | 8.0.100-8.0.0~rc1-0ubuntu1 | mantic/universe | source, amd64, arm64 ";
        ReadOnlySpan<char> span = line.AsSpan();

        var success = MadisonResponseEntry.TryParse(span, out var result);

        Assert.True(success);
        Assert.Equal("dotnet8", result.PackageName);
        Assert.Equal("8.0.100-8.0.0~rc1-0ubuntu1", result.Version);
        Assert.Equal("mantic/universe", result.Suite);
        Assert.Equal("mantic", result.Series);
        Assert.Equal("universe", result.Component);
        Assert.Equal(["source", "amd64", "arm64"], result.Architectures);
    }

    [Fact]
    public void TryParseSpan_WithEmptyInput_ReturnsFalse()
    {
        ReadOnlySpan<char> span = string.Empty.AsSpan();

        var success = MadisonResponseEntry.TryParse(span, out var result);

        Assert.False(success);
    }

    [Fact]
    public void TryParseSpan_WithTooFewFields_ReturnsFalse()
    {
        ReadOnlySpan<char> span = "package | version | jammy".AsSpan();

        var success = MadisonResponseEntry.TryParse(span, out var result);

        Assert.False(success);
    }

    [Fact]
    public void TryParseSpan_WithFormatProvider_WorksCorrectly()
    {
        var line = "package | 1.0 | jammy/main | amd64";
        ReadOnlySpan<char> span = line.AsSpan();

        var success = MadisonResponseEntry.TryParse(span, formatProvider: null, out var result);

        Assert.True(success);
        Assert.Equal("package", result.PackageName);
    }

    [Fact]
    public void ParseSpan_WithValidInput_ReturnsCorrectEntry()
    {
        ReadOnlySpan<char> span = " dotnet8 | 8.0.100-8.0.0~rc1-0ubuntu1 | mantic/universe | source, amd64, arm64 ".AsSpan();

        var result = MadisonResponseEntry.Parse(span);

        Assert.Equal("dotnet8", result.PackageName);
        Assert.Equal("8.0.100-8.0.0~rc1-0ubuntu1", result.Version);
        Assert.Equal("mantic/universe", result.Suite);
        Assert.Equal("mantic", result.Series);
        Assert.Equal("universe", result.Component);
        Assert.Equal(["source", "amd64", "arm64"], result.Architectures);
    }

    [Fact]
    public void ParseSpan_WithEmptyInput_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => MadisonResponseEntry.Parse(string.Empty.AsSpan()));
    }

    [Fact]
    public void Parse_WithMultipleArchitecturesInDifferentOrder_ParsesCorrectly()
    {
        var line = "pkg | 1.0 | jammy | amd64, i386, ppc64el, armhf, s390x";

        var result = MadisonResponseEntry.Parse(line);

        Assert.Equal(["amd64", "i386", "ppc64el", "armhf", "s390x"], result.Architectures);
    }

    [Fact]
    public void Parse_PreservesSpecialCharactersInFields()
    {
        var line = "pkg-name_2 | 1.0+deb1~ubuntu | jammy-security/universe | amd64";

        var result = MadisonResponseEntry.Parse(line);

        Assert.Equal("pkg-name_2", result.PackageName);
        Assert.Equal("1.0+deb1~ubuntu", result.Version);
        Assert.Equal("jammy-security/universe", result.Suite);
    }

    [Fact]
    public void TryParseString_WithFormatProvider_WorksCorrectly()
    {
        var line = "package | 1.0 | jammy/main | amd64";

        var success = MadisonResponseEntry.TryParse(line, formatProvider: null, out var result);

        Assert.True(success);
        Assert.Equal("package", result.PackageName);
    }
}
