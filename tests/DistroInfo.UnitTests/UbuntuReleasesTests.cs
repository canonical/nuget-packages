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

using Canonical.DistroInfo.Ubuntu;

namespace Canonical.DistroInfo.UnitTests;

public class UbuntuReleasesTests
{
    [Fact]
    public void FromSeries_WhenSeriesIsNoble_ReturnsNobleNumbatRelease()
    {
        // this test proofs that:
        // - the source generator seems to work
        // - the FromSeries logic works
        // - UbuntuRelease comparison works

        Assert.True(UbuntuReleases.Collection.TryGetBySeries("noble", out var release));
        Assert.Equal(actual: release, expected: UbuntuReleases.NobleNumbat);
    }

    [Fact]
    public void NobleNumbat_Has_ExpectedValues()
    {
        Assert.Equal(actual: UbuntuReleases.NobleNumbat.Version, expected: "24.04 LTS");
        Assert.Equal(actual: UbuntuReleases.NobleNumbat.ShortVersion, expected: "24.04");
        Assert.True(UbuntuReleases.NobleNumbat.IsLts);
        Assert.Equal(actual: UbuntuReleases.NobleNumbat.Codename, expected: "Noble Numbat");
        Assert.Equal(actual: UbuntuReleases.NobleNumbat.Series.Identifier, expected: "noble");
    }
}
