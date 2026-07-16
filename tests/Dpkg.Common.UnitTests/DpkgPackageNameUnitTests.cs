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

using Canonical.Common.Parsing;

namespace Canonical.Dpkg.UnitTests;

public class DpkgPackageNameUnitTests
{
    public static TheoryData<string> ValidNames =>
    [
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
        "a",                 // too short: must be at least two characters long
        "0",                 // too short: must be at least two characters long
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

    [Fact]
    public void Sort()
    {
        DpkgPackageName?[] names =
        [
            DpkgPackageName.Parse("dotnet10"),
            DpkgPackageName.Parse("dotnet09"),
            null,
            DpkgPackageName.Parse("dotnet8"),
            DpkgPackageName.Parse("dotnet9"),
            null,
        ];

        Array.Sort(names);

        Assert.Null(names[0]);
        Assert.Null(names[1]);
        Assert.True(names[2].HasValue);
        Assert.Equal("dotnet8", names[2]!.Value.Identifier);
        Assert.True(names[3].HasValue);
        Assert.Equal("dotnet09", names[3]!.Value.Identifier);
        Assert.True(names[4].HasValue);
        Assert.Equal("dotnet9", names[4]!.Value.Identifier);
        Assert.True(names[5].HasValue);
        Assert.Equal("dotnet10", names[5]!.Value.Identifier);
    }

    #region Parse(string, IFormatProvider?)

    [Theory]
    [MemberData(nameof(ValidNames))]
    [FileData("dpkg-names.txt")]
    public void Parse_WithValidName_ReturnsDpkgNameWithMatchingIdentifier(string name)
    {
        var dpkgName = DpkgPackageName.Parse(name);
        Assert.Equal(expected: name, actual: dpkgName.Identifier);
    }

    [Theory]
    [MemberData(nameof(InvalidNames))]
    public void Parse_WithInvalidName_ThrowsMalformedDpkgNameException(string name)
    {
        Assert.Throws<DpkgPackageNameParsingException>(() => DpkgPackageName.Parse(name));
    }

    [Fact]
    public void Parse_WithEmptyString_ThrowsMalformedDpkgNameException()
    {
        Assert.Throws<DpkgPackageNameParsingException>(() => DpkgPackageName.Parse(string.Empty));
    }

    [Fact]
    public void Parse_WithNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => DpkgPackageName.Parse(null as string, provider: null));
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
        Assert.Equal(expected: string.Empty, actual: result.Identifier);
    }

    [Fact]
    public void TryParse_WithNull_ReturnsFalse()
    {
        var success = DpkgPackageName.TryParse(null, out var result);

        Assert.False(success);
        Assert.Equal(expected: string.Empty, actual: result.Identifier);
    }

    [Fact]
    public void TryParse_WithFormatProviderOverload_BehavesIdentically()
    {
        var success = DpkgPackageName.TryParse("dotnet8", out var result);

        Assert.True(success);
        Assert.Equal(expected: "dotnet8", actual: result.Identifier);
    }

    #endregion

    #region Span overloads

    [Fact]
    public void Parse_WithValidSpan_ReturnsDpkgName()
    {
        var dpkgName = DpkgPackageName.Parse("dotnet8".AsSpan());
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
        Assert.Equal(expected: string.Empty, actual: result.Identifier);
    }

    #endregion

    #region TryParse(ReadOnlySpan<char>, out, out annotations, failFast)

    [Fact]
    public void TryParse_WithFailFastAndInvalidName_AbortsWithoutAnnotations()
    {
        var success = DpkgPackageName.TryParse(
            "a_b_c".AsSpan(), out var result, out var annotations, failFast: true);

        Assert.False(success);
        Assert.Equal(expected: string.Empty, actual: result.Identifier);
        Assert.Empty(annotations);
    }

    [Fact]
    public void TryParse_WithoutFailFastAndValidName_ReturnsTrueWithoutAnnotations()
    {
        var success = DpkgPackageName.TryParse(
            "dotnet8".AsSpan(), out var result, out var annotations, failFast: false);

        Assert.True(success);
        Assert.Equal(expected: "dotnet8", actual: result.Identifier);
        Assert.Empty(annotations);
    }

    [Fact]
    public void TryParse_WithoutFailFast_CollectsAllInvalidCharacterLocations()
    {
        // "a_b_c" has invalid characters at positions 1 and 3.
        var success = DpkgPackageName.TryParse(
            "a_b_c".AsSpan(), out _, out var annotations, failFast: false);

        Assert.False(success);
        var annotation = Assert.Single(annotations);
        Assert.Equal(expected: "DPKG-NAME-003", actual: annotation.Identifier);
        Assert.Equal(expected: [new Location(1), new Location(3)], actual: annotation.Locations);
    }

    [Fact]
    public void Parse_WithInvalidLeadingCharacter_ReportsItAtPositionZero()
    {
        var exception = Assert.Throws<DpkgPackageNameParsingException>(
            () => DpkgPackageName.Parse("-foo".AsSpan()));

        var annotation = Assert.Single(exception.Annotations);
        Assert.Equal(expected: new Location(0), actual: Assert.Single(annotation.Locations));
    }

    #endregion

    #region MalformedDpkgNameException

    [Fact]
    public void MalformedDpkgNameException_ExposesOffendingPackageName()
    {
        var exception = Assert.Throws<DpkgPackageNameParsingException>(
            () => DpkgPackageName.Parse("foo_bar"));

        Assert.Equal(expected: "foo_bar", actual: exception.Value);
    }

    [Fact]
    public void MalformedDpkgNameException_IsAFormatException()
    {
        var exception = new DpkgPackageNameParsingException("pkg", []);
        Assert.IsAssignableFrom<FormatException>(exception);
    }

    #endregion

    #region Conversions

    [Fact]
    public void ImplicitStringConversion_ReturnsIdentifier()
    {
        var dpkgName = DpkgPackageName.Parse("dotnet8");
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
        Assert.Throws<DpkgPackageNameParsingException>(() => (DpkgPackageName)"Foo");
    }

    #endregion

    #region Equality and formatting

    [Fact]
    public void ToString_ReturnsIdentifier()
    {
        var dpkgName = DpkgPackageName.Parse("dotnet8");
        Assert.Equal(expected: "dotnet8", actual: dpkgName.ToString());
    }

    [Fact]
    public void Equals_WithSameIdentifier_ReturnsTrue()
    {
        var first = DpkgPackageName.Parse("dotnet8");
        var second = DpkgPackageName.Parse("dotnet8");

        Assert.Equal(expected: first, actual: second);
        Assert.Equal(expected: first.GetHashCode(), actual: second.GetHashCode());
    }

    [Fact]
    public void Equals_WithDifferentIdentifier_ReturnsFalse()
    {
        var first = DpkgPackageName.Parse("dotnet8");
        var second = DpkgPackageName.Parse("dotnet9");

        Assert.NotEqual(first, second);
    }

    #endregion
}
