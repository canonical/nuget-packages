// Copyright (C) 2026 Canonical Ltd.
//
// SPDX-License-Identifier: GPL-3.0-only
//
// This program is free software: you can redistribute it and/or modify it under the terms of
// the GNU General Public License version 3, as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY
// WARRANTY; without even the implied warranties of MERCHANTABILITY, SATISFACTORY
// QUALITY, or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License
// for more details.
//
// You should have received a copy of the GNU General Public License along with this
// program.  If not, see http://www.gnu.org/licenses/.

namespace Canonical.Dpkg.UnitTests;

public class DpkgPackageNameUnitTests
{
    public static TheoryData<string> ValidNames =>
    [
        "a",                 // single lowercase letter
        "0",                 // single digit
        "z9",
        "dotnet8",
        "lib-foo",           // hyphen
        "foo.bar",           // dot
        "g++",               // plus
        "libstdc++6",
        "9pack",             // may start with a digit
        "a-b.c+d",           // all permitted special characters together
    ];

    public static TheoryData<string> InvalidNames =>
    [
        "Foo",               // uppercase letter
        "fooBar",            // uppercase letter in the middle
        "-foo",              // may not start with a hyphen
        ".foo",              // may not start with a dot
        "+foo",              // may not start with a plus
        "foo_bar",           // underscore is not permitted
        "foo bar",           // whitespace is not permitted
        "foo/bar",           // slash is not permitted
        "föö",               // non-ASCII letter
    ];

    #region Parse(string, IFormatProvider?)

    [Theory]
    [MemberData(nameof(ValidNames))]
    public void Parse_WithValidName_ReturnsDpkgNameWithMatchingIdentifier(string name)
    {
        var dpkgName = DpkgPackageName.Parse(name, formatProvider: null);
        Assert.Equal(expected: name, actual: dpkgName.Identifier);
    }

    [Theory]
    [MemberData(nameof(InvalidNames))]
    public void Parse_WithInvalidName_ThrowsMalformedDpkgNameException(string name)
    {
        Assert.Throws<MalformedDpkgNameException>(() => DpkgPackageName.Parse(name, formatProvider: null));
    }

    [Fact]
    public void Parse_WithEmptyString_ThrowsMalformedDpkgNameException()
    {
        Assert.Throws<MalformedDpkgNameException>(() => DpkgPackageName.Parse(string.Empty, formatProvider: null));
    }

    [Fact]
    public void Parse_WithNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => DpkgPackageName.Parse(null!, formatProvider: null));
    }

    #endregion

    #region TryParse(string?, ..., out DpkgName)

    [Theory]
    [MemberData(nameof(ValidNames))]
    public void TryParse_WithValidName_ReturnsTrueAndPopulatesResult(string name)
    {
        var success = DpkgPackageName.TryParse(name, out var result);

        Assert.True(success);
        Assert.Equal(expected: name, actual: result.Identifier);
    }

    [Theory]
    [MemberData(nameof(InvalidNames))]
    public void TryParse_WithInvalidName_ReturnsFalseAndDefaultResult(string name)
    {
        var success = DpkgPackageName.TryParse(name, out var result);

        Assert.False(success);
        Assert.Equal(expected: default, actual: result);
    }

    [Fact]
    public void TryParse_WithNull_ReturnsFalse()
    {
        var success = DpkgPackageName.TryParse(null, out var result);

        Assert.False(success);
        Assert.Equal(expected: default, actual: result);
    }

    [Fact]
    public void TryParse_WithFormatProviderOverload_BehavesIdentically()
    {
        var success = DpkgPackageName.TryParse("dotnet8", formatProvider: null, out var result);

        Assert.True(success);
        Assert.Equal(expected: "dotnet8", actual: result.Identifier);
    }

    #endregion

    #region Span overloads

    [Fact]
    public void Parse_WithValidSpan_ReturnsDpkgName()
    {
        var dpkgName = DpkgPackageName.Parse("dotnet8".AsSpan(), formatProvider: null);
        Assert.Equal(expected: "dotnet8", actual: dpkgName.Identifier);
    }

    [Fact]
    public void TryParse_WithValidSpan_ReturnsTrueAndPopulatesResult()
    {
        var success = DpkgPackageName.TryParse("dotnet8".AsSpan(), out var result);

        Assert.True(success);
        Assert.Equal(expected: "dotnet8", actual: result.Identifier);
    }

    [Fact]
    public void TryParse_WithInvalidSpan_ReturnsFalse()
    {
        var success = DpkgPackageName.TryParse("Foo".AsSpan(), out var result);

        Assert.False(success);
        Assert.Equal(expected: default, actual: result);
    }

    #endregion

    #region Parse(ReadOnlySpan<char>, DpkgParsingErrorHandling)

    [Fact]
    public void Parse_WithReturnDefaultAndInvalidName_ReturnsNull()
    {
        var result = DpkgPackageName.Parse("Foo".AsSpan(), DpkgParsingErrorHandling.ReturnDefault);
        Assert.Null(result);
    }

    [Fact]
    public void Parse_WithReturnDefaultAndValidName_ReturnsDpkgName()
    {
        var result = DpkgPackageName.Parse("dotnet8".AsSpan(), DpkgParsingErrorHandling.ReturnDefault);

        Assert.NotNull(result);
        Assert.Equal(expected: "dotnet8", actual: result.Value.Identifier);
    }

    [Fact]
    public void Parse_WithThrowAtFirstError_ThrowsOnTheFirstInvalidCharacterOnly()
    {
        // "a_b_c" has invalid characters at positions 1 and 3.
        var exception = Assert.Throws<MalformedDpkgNameException>(
            () => DpkgPackageName.Parse("a_b_c".AsSpan(), DpkgParsingErrorHandling.ThrowAtFirstError));

        var invalidCharacter = Assert.Single(exception.InvalidCharacters);
        Assert.Equal(expected: ('_', 1), actual: invalidCharacter);
    }

    [Fact]
    public void Parse_WithThrowAfterProcessingAll_CollectsAllInvalidCharacters()
    {
        // "a_b_c" has invalid characters at positions 1 and 3.
        var exception = Assert.Throws<MalformedDpkgNameException>(
            () => DpkgPackageName.Parse("a_b_c".AsSpan(), DpkgParsingErrorHandling.ThrowAfterProcessingAll));

        Assert.Equal(
            expected: [('_', 1), ('_', 3)],
            actual: exception.InvalidCharacters);
    }

    [Fact]
    public void Parse_WithInvalidLeadingCharacter_ReportsItAtPositionZero()
    {
        var exception = Assert.Throws<MalformedDpkgNameException>(
            () => DpkgPackageName.Parse("-foo".AsSpan(), DpkgParsingErrorHandling.ThrowAfterProcessingAll));

        var invalidCharacter = Assert.Single(exception.InvalidCharacters);
        Assert.Equal(expected: ('-', 0), actual: invalidCharacter);
    }

    #endregion

    #region MalformedDpkgNameException

    [Fact]
    public void MalformedDpkgNameException_ExposesOffendingPackageName()
    {
        var exception = Assert.Throws<MalformedDpkgNameException>(
            () => DpkgPackageName.Parse("foo_bar", formatProvider: null));

        Assert.Equal(expected: "foo_bar", actual: exception.PackageName);
    }

    [Fact]
    public void MalformedDpkgNameException_IsAFormatException()
    {
        var exception = new MalformedDpkgNameException("message", "pkg", []);
        Assert.IsAssignableFrom<FormatException>(exception);
    }

    #endregion

    #region Conversions

    [Fact]
    public void ImplicitStringConversion_ReturnsIdentifier()
    {
        var dpkgName = DpkgPackageName.Parse("dotnet8", formatProvider: null);
        string asString = dpkgName;

        Assert.Equal(expected: "dotnet8", actual: asString);
    }

    [Fact]
    public void ExplicitConversionFromString_WithValidName_ReturnsDpkgName()
    {
        var dpkgName = (DpkgPackageName)"dotnet8";
        Assert.Equal(expected: "dotnet8", actual: dpkgName.Identifier);
    }

    [Fact]
    public void ExplicitConversionFromString_WithInvalidName_ThrowsMalformedDpkgNameException()
    {
        Assert.Throws<MalformedDpkgNameException>(() => (DpkgPackageName)"Foo");
    }

    #endregion

    #region Equality and formatting

    [Fact]
    public void ToString_ReturnsIdentifier()
    {
        var dpkgName = DpkgPackageName.Parse("dotnet8", formatProvider: null);
        Assert.Equal(expected: "dotnet8", actual: dpkgName.ToString());
    }

    [Fact]
    public void Equals_WithSameIdentifier_ReturnsTrue()
    {
        var first = DpkgPackageName.Parse("dotnet8", formatProvider: null);
        var second = DpkgPackageName.Parse("dotnet8", formatProvider: null);

        Assert.Equal(expected: first, actual: second);
        Assert.Equal(expected: first.GetHashCode(), actual: second.GetHashCode());
    }

    [Fact]
    public void Equals_WithDifferentIdentifier_ReturnsFalse()
    {
        var first = DpkgPackageName.Parse("dotnet8", formatProvider: null);
        var second = DpkgPackageName.Parse("dotnet9", formatProvider: null);

        Assert.NotEqual(first, second);
    }

    #endregion
}
