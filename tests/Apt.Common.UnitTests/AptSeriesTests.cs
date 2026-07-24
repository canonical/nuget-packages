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

using System.Collections.Immutable;
using Canonical.Apt;
using Canonical.Common.Parsing;

namespace Canonical.Apt.UnitTests;

public sealed class AptSeriesTests
{
    #region TryParse(ReadOnlySpan<char>, out, out annotations, failEarly)

    [Fact]
    public void TryParse_WithValidName_ReturnsTrueAndIdentifier()
    {
        var success = AptSeries.TryParse(
            "trixie".AsSpan(), out var result, out var annotations, failEarly: false);

        Assert.True(success);
        Assert.Equal(expected: "trixie", actual: result.Identifier);
        Assert.Empty(annotations);
    }

    [Fact]
    public void TryParse_WithEmptySpan_ReturnsFalseAndEmptyAnnotation()
    {
        var success = AptSeries.TryParse(
            ReadOnlySpan<char>.Empty, out var result, out var annotations, failEarly: false);

        Assert.False(success);
        Assert.Equal(expected: default, actual: result);

        var annotation = Assert.Single(annotations);
        Assert.Equal(expected: "APT-SERIES-001", actual: annotation.Identifier);
        Assert.Equal(expected: [ new Location(0) ], actual: annotation.Locations);
    }

    [Fact]
    public void TryParse_WithfailEarlyAndEmptySpan_AbortsWithoutAnnotations()
    {
        var success = AptSeries.TryParse(
            ReadOnlySpan<char>.Empty, out var result, out var annotations, failEarly: true);

        Assert.False(success);
        Assert.Equal(expected: default, actual: result);
        Assert.Empty(annotations);
    }

    [Fact]
    public void TryParse_WithoutfailEarly_CollectsAllInvalidCharacterLocations()
    {
        // "a_b_c" has invalid characters at positions 1 and 3.
        var success = AptSeries.TryParse(
            "a_b_c".AsSpan(), out var result, out var annotations, failEarly: false);

        Assert.False(success);
        Assert.Equal(expected: default, actual: result);

        var annotation = Assert.Single(annotations);
        Assert.Equal(expected: "APT-SERIES-002", actual: annotation.Identifier);
        Assert.Equal(expected: [new Location(1), new Location(3)], actual: annotation.Locations);
    }

    [Fact]
    public void TryParse_WithfailEarlyAndInvalidCharacter_AbortsWithoutAnnotations()
    {
        var success = AptSeries.TryParse(
            "a_b".AsSpan(), out var result, out var annotations, failEarly: true);

        Assert.False(success);
        Assert.Equal(expected: default, actual: result);
        Assert.Empty(annotations);
    }

    [Fact]
    public void TryParse_DistinctInvalidCharacters_ListsThemInMessage()
    {
        var success = AptSeries.TryParse(
            "a1B".AsSpan(), out _, out var annotations, failEarly: false);

        Assert.False(success);
        var annotation = Assert.Single(annotations);
        Assert.Contains("'1'", annotation.Message);
        Assert.Contains("'B'", annotation.Message);
    }

    #endregion

    #region AptSeriesNameParsingException

    [Fact]
    public void Parse_WithInvalidCharacter_ThrowsAptSeriesNameParsingException()
    {
        var exception = Assert.Throws<AptSeriesNameParsingException>(
            () => AptSeries.Parse("foo_bar".AsSpan()));

        Assert.Equal(expected: "foo_bar", actual: exception.Value);
        Assert.Single(exception.Annotations);
    }

    [Fact]
    public void AptSeriesNameParsingException_IsAFormatException()
    {
        var exception = new AptSeriesNameParsingException(
            "value", ImmutableList<ParsingAnnotation>.Empty);

        Assert.IsAssignableFrom<FormatException>(exception);
    }

    #endregion
}
