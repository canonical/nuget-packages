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

public class UbuntuReleaseInfoCollectionTests
{
    [Fact]
    public async Task AllReleases_WhenReadFromReadFromDistroInfoData_EqualsStaticCollection()
    {
        var collection = await UbuntuReleaseInfoCollection.ReadFromDistroInfoDataAsync();

        Assert.Equal(
            actual: collection.AllReleases as IEnumerable<UbuntuReleaseInfo>,
            expected: UbuntuReleases.All);

        // known reference values to check that the test actually works
        var devel = collection.GetDevel(new DateOnly(2023, 02, 13));
        Assert.Equal(actual: devel.Series.Identifier, expected: "lunar");
        Assert.Equal(actual: devel, expected: UbuntuReleases.LunarLobster);
    }
}
