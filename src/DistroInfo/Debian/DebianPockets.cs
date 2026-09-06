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

namespace Canonical.DistroInfo.Debian;

// See http://ftp.debian.org/debian/dists/ as a reference
public class DebianPockets
{
    public static readonly AptPocket Release = AptPocket.Release;
    public static readonly AptPocket Updates = AptPocket.Parse("updates");
    public static readonly AptPocket ProposedUpdates = AptPocket.Parse("proposed-updates");
    public static readonly AptPocket Backports = AptPocket.Parse("backports");

    public static readonly ImmutableArray<AptPocket> WellKnown;

    static DebianPockets()
    {
        WellKnown = [Release, Updates, ProposedUpdates, Backports];
    }
}
