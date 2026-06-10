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

public class DpkgNameUnitTests
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
        var dpkgName = DpkgName.Parse(name, formatProvider: null);
        Assert.Equal(expected: name, actual: dpkgName.Identifier);
    }

    [Theory]
    [MemberData(nameof(InvalidNames))]
    public void Parse_WithInvalidName_ThrowsMalformedDpkgNameException(string name)
    {
        Assert.Throws<MalformedDpkgNameException>(() => DpkgName.Parse(name, formatProvider: null));
    }

    [Fact]
    public void Parse_WithEmptyString_ThrowsMalformedDpkgNameException()
    {
        Assert.Throws<MalformedDpkgNameException>(() => DpkgName.Parse(string.Empty, formatProvider: null));
    }

    [Fact]
    public void Parse_WithNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => DpkgName.Parse(null!, formatProvider: null));
    }

    #endregion

    #region TryParse(string?, ..., out DpkgName)

    [Theory]
    [MemberData(nameof(ValidNames))]
    public void TryParse_WithValidName_ReturnsTrueAndPopulatesResult(string name)
    {
        var success = DpkgName.TryParse(name, out var result);

        Assert.True(success);
        Assert.Equal(expected: name, actual: result.Identifier);
    }

    [Theory]
    [MemberData(nameof(InvalidNames))]
    public void TryParse_WithInvalidName_ReturnsFalseAndDefaultResult(string name)
    {
        var success = DpkgName.TryParse(name, out var result);

        Assert.False(success);
        Assert.Equal(expected: default, actual: result);
    }

    [Fact]
    public void TryParse_WithNull_ReturnsFalse()
    {
        var success = DpkgName.TryParse(null, out var result);

        Assert.False(success);
        Assert.Equal(expected: default, actual: result);
    }

    [Fact]
    public void TryParse_WithFormatProviderOverload_BehavesIdentically()
    {
        var success = DpkgName.TryParse("dotnet8", formatProvider: null, out var result);

        Assert.True(success);
        Assert.Equal(expected: "dotnet8", actual: result.Identifier);
    }

    #endregion

    #region Span overloads

    [Fact]
    public void Parse_WithValidSpan_ReturnsDpkgName()
    {
        var dpkgName = DpkgName.Parse("dotnet8".AsSpan(), formatProvider: null);
        Assert.Equal(expected: "dotnet8", actual: dpkgName.Identifier);
    }

    [Fact]
    public void TryParse_WithValidSpan_ReturnsTrueAndPopulatesResult()
    {
        var success = DpkgName.TryParse("dotnet8".AsSpan(), out var result);

        Assert.True(success);
        Assert.Equal(expected: "dotnet8", actual: result.Identifier);
    }

    [Fact]
    public void TryParse_WithInvalidSpan_ReturnsFalse()
    {
        var success = DpkgName.TryParse("Foo".AsSpan(), out var result);

        Assert.False(success);
        Assert.Equal(expected: default, actual: result);
    }

    #endregion

    #region Parse(ReadOnlySpan<char>, throwOnError, throwOnFirstError)

    [Fact]
    public void Parse_WithThrowOnErrorFalseAndInvalidName_ReturnsNull()
    {
        var result = DpkgName.Parse("Foo".AsSpan(), throwOnError: false);
        Assert.Null(result);
    }

    [Fact]
    public void Parse_WithThrowOnErrorFalseAndValidName_ReturnsDpkgName()
    {
        var result = DpkgName.Parse("dotnet8".AsSpan(), throwOnError: false);

        Assert.NotNull(result);
        Assert.Equal(expected: "dotnet8", actual: result.Value.Identifier);
    }

    [Fact]
    public void Parse_WithThrowOnFirstErrorButThrowOnErrorFalse_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => DpkgName.Parse("foo".AsSpan(), throwOnError: false, throwOnFirstError: true));
    }

    [Fact]
    public void Parse_WithThrowOnFirstError_ThrowsOnTheFirstInvalidCharacterOnly()
    {
        // "a_b_c" has invalid characters at positions 1 and 3.
        var exception = Assert.Throws<MalformedDpkgNameException>(
            () => DpkgName.Parse("a_b_c".AsSpan(), throwOnError: true, throwOnFirstError: true));

        var invalidCharacter = Assert.Single(exception.InvalidCharacters);
        Assert.Equal(expected: ('_', 1), actual: invalidCharacter);
    }

    [Fact]
    public void Parse_WithoutThrowOnFirstError_CollectsAllInvalidCharacters()
    {
        // "a_b_c" has invalid characters at positions 1 and 3.
        var exception = Assert.Throws<MalformedDpkgNameException>(
            () => DpkgName.Parse("a_b_c".AsSpan(), throwOnError: true, throwOnFirstError: false));

        Assert.Equal(
            expected: [('_', 1), ('_', 3)],
            actual: exception.InvalidCharacters);
    }

    [Fact]
    public void Parse_WithInvalidLeadingCharacter_ReportsItAtPositionZero()
    {
        var exception = Assert.Throws<MalformedDpkgNameException>(
            () => DpkgName.Parse("-foo".AsSpan(), throwOnError: true));

        var invalidCharacter = Assert.Single(exception.InvalidCharacters);
        Assert.Equal(expected: ('-', 0), actual: invalidCharacter);
    }

    #endregion

    #region MalformedDpkgNameException

    [Fact]
    public void MalformedDpkgNameException_ExposesOffendingPackageName()
    {
        var exception = Assert.Throws<MalformedDpkgNameException>(
            () => DpkgName.Parse("foo_bar", formatProvider: null));

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
        var dpkgName = DpkgName.Parse("dotnet8", formatProvider: null);
        string asString = dpkgName;

        Assert.Equal(expected: "dotnet8", actual: asString);
    }

    [Fact]
    public void ExplicitConversionFromString_WithValidName_ReturnsDpkgName()
    {
        var dpkgName = (DpkgName)"dotnet8";
        Assert.Equal(expected: "dotnet8", actual: dpkgName.Identifier);
    }

    [Fact]
    public void ExplicitConversionFromString_WithInvalidName_ThrowsMalformedDpkgNameException()
    {
        Assert.Throws<MalformedDpkgNameException>(() => (DpkgName)"Foo");
    }

    #endregion

    #region Equality and formatting

    [Fact]
    public void ToString_ReturnsIdentifier()
    {
        var dpkgName = DpkgName.Parse("dotnet8", formatProvider: null);
        Assert.Equal(expected: "dotnet8", actual: dpkgName.ToString());
    }

    [Fact]
    public void Equals_WithSameIdentifier_ReturnsTrue()
    {
        var first = DpkgName.Parse("dotnet8", formatProvider: null);
        var second = DpkgName.Parse("dotnet8", formatProvider: null);

        Assert.Equal(expected: first, actual: second);
        Assert.Equal(expected: first.GetHashCode(), actual: second.GetHashCode());
    }

    [Fact]
    public void Equals_WithDifferentIdentifier_ReturnsFalse()
    {
        var first = DpkgName.Parse("dotnet8", formatProvider: null);
        var second = DpkgName.Parse("dotnet9", formatProvider: null);

        Assert.NotEqual(first, second);
    }

    #endregion
}