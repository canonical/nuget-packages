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

namespace Canonical.DistroInfo;

public class DistroDataOutdated : ApplicationException
{
    public DistroDataOutdated() : base(message:
        "Distribution data outdated. Please check for an update for distro-info-data. " +
        "See /usr/share/doc/distro-info-data/README.Debian for details.")
    {
    }
}
