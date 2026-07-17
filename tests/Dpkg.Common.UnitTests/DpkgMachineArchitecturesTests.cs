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

public class DpkgMachineArchitecturesTests
{
    [Fact]
    public void WellKnownSet_IsNotEmpty()
    {
        Assert.NotEmpty(DpkgMachineArchitectures.WellKnown);
    }

    [Fact]
    public void TryParse_WithValidArch_ReturnsTrueAndIdentifier()
    {
        var success = DpkgMachineArchitecture.TryParse("amd64", out var result, out var annotations, failFast: false);

        Assert.True(success);
        Assert.Equal(expected: "amd64", actual: result.Identifier);
        Assert.Empty(annotations);
    }

    [Fact]
    public void TryParse_WellKnown_ReturnsTrueAndIdentifier()
    {
        foreach (var arch in DpkgMachineArchitectures.WellKnown)
        {
            var identifier = arch.Identifier;

            var success = DpkgMachineArchitecture.TryParse(identifier, out var result, out var annotations, failFast: false);

            Assert.True(success);
            Assert.Equal(expected: identifier, actual: result.Identifier);
            Assert.True(ReferenceEquals(identifier, result.Identifier), identifier);
            Assert.Empty(annotations);
        }
    }

    [Theory]
    [InlineData("foo")]
    [InlineData("foobar")]
    [InlineData("foobar1")]
    [InlineData("1234")]
    [InlineData("12345")]
    [InlineData("12345678")]
    public void TryParse_Unknown_ReturnsTrueAndIdentifier(string unknownIdentifier)
    {
        var success = DpkgMachineArchitecture.TryParse(unknownIdentifier, out var result, out var annotations, failFast: false);

        Assert.DoesNotContain(result, DpkgMachineArchitectures.WellKnown);
        Assert.True(success);
        Assert.Equal(expected: unknownIdentifier, actual: result.Identifier);
        Assert.Empty(annotations);
    }

    [Fact]
    public void TryParse_Empty_ReturnsFalse()
    {
        var success = DpkgMachineArchitecture.TryParse(string.Empty, out var result, out var annotations, failFast: false);

        Assert.False(success);
        Assert.Equal(expected: string.Empty, actual: result.Identifier);
        Assert.NotEmpty(annotations);
    }
}
