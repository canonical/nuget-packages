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

namespace Canonical.DistroInfo.Ubuntu;

public class UbuntuComponents
{
    public static readonly AptComponent Main = AptComponent.Parse("main");
    public static readonly AptComponent Universe = AptComponent.Parse("universe");
    public static readonly AptComponent Restricted = AptComponent.Parse("restricted");
    public static readonly AptComponent Multiverse = AptComponent.Parse("multiverse");

    public static readonly ImmutableArray<AptComponent> WellKnown;
    public static readonly ImmutableArray<AptComponent> Canonical;
    public static readonly ImmutableArray<AptComponent> Community;

    static UbuntuComponents()
    {
        Canonical = [Main, Restricted];
        Community = [Universe, Multiverse];
        WellKnown = [Main, Restricted, Universe, Multiverse];
    }
}
