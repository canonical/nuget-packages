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

namespace Canonical.Apt.UnitTests;

public class AptSuiteTests
{
    [Theory]
    [InlineData("")]
    [InlineData("-updates")]
    [InlineData("noble-")]
    [InlineData("noble-proposed-")]
    [InlineData("noble1-proposed")]
    [InlineData("noble-proposed1")]
    [InlineData("noble1-proposed1")]
    [InlineData("noble1-proposed1-")]
    public void TryParse_WithInvalid_Fails(string suite)
    {
        var success = AptSuite.TryParse(suite, out var result, out var annotations, failFast: false);

        Assert.False(success);
        Assert.Equal(expected: string.Empty, actual: result.Identifier);
        Assert.Equal(expected: string.Empty, actual: result.Series.Identifier);
        Assert.Equal(expected: string.Empty, actual: result.Pocket.Identifier);
        Assert.Equal(expected: "release", actual: result.Pocket.DisplayName);
        Assert.Equal(expected: AptPocket.Release, actual: result.Pocket);
        Assert.NotEmpty(annotations);
    }

    [Fact]
    public void TryParse_WithOnlySeries_ReturnsTrueAndIdentifierAndReleasePocket()
    {
        var success = AptSuite.TryParse("noble", out var result, out var annotations, failFast: false);

        Assert.True(success);
        Assert.Equal(expected: "noble", actual: result.Identifier);
        Assert.Equal(expected: "noble", actual: result.Series.Identifier);
        Assert.Equal(expected: string.Empty, actual: result.Pocket.Identifier);
        Assert.Equal(expected: "release", actual: result.Pocket.DisplayName);
        Assert.Equal(expected: AptPocket.Release, actual: result.Pocket);
        Assert.Empty(annotations);
    }

    [Fact]
    public void TryParse_WithSeriesAndPocket_ReturnsTrueAndIdentifier()
    {
        var success = AptSuite.TryParse("noble-updates", out var result, out var annotations, failFast: false);

        Assert.True(success);
        Assert.Equal(expected: "noble-updates", actual: result.Identifier);
        Assert.Equal(expected: "noble", actual: result.Series.Identifier);
        Assert.Equal(expected: "updates", actual: result.Pocket.Identifier);
        Assert.Equal(expected: "updates", actual: result.Pocket.DisplayName);
        Assert.Empty(annotations);
    }

    [Fact]
    public void TryParse_WithMultipleDashes_ReturnsTrueAndIdentifier()
    {
        var success = AptSuite.TryParse("bullseye-proposed-updates", out var result, out var annotations, failFast: false);

        Assert.True(success);
        Assert.Equal(expected: "bullseye-proposed-updates", actual: result.Identifier);
        Assert.Equal(expected: "bullseye", actual: result.Series.Identifier);
        Assert.Equal(expected: "proposed-updates", actual: result.Pocket.Identifier);
        Assert.Equal(expected: "proposed-updates", actual: result.Pocket.DisplayName);
        Assert.Empty(annotations);
    }
}
