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