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

using Canonical.Apt;

namespace Canonical.DistroInfo;

public interface IDistroReleaseInfo
{
    /// <summary>
    /// The version number assigned to the distro series.
    /// </summary>
    string? Version { get; }

    /// <summary>
    /// The official development codename of the distro series.
    /// </summary>
    string Codename { get; }

    /// <summary>
    /// The name of the distro series used by APT.
    /// </summary>
    AptSeries Series { get; }

    /// <summary>
    /// The date when this distro series was officially created.
    /// </summary>
    DateOnly Created { get; }

    /// <summary>
    /// The date when this distro series was officially release.
    /// </summary>
    DateOnly? Released { get; }

    /// <summary>
    /// The release is no longer supported after this date.
    /// It's the date on which the latest maintenance program ends.
    /// </summary>
    DateOnly? EndOfLife { get; }

    /// <summary>
    /// The release is maintained (in development or a supported stable release) at the specified date.
    /// </summary>
    /// <remarks>
    /// When <paramref name="date" /> is between <see cref="Created"/> and <see cref="EndOfLife"/>.
    /// </remarks>
    public bool IsMaintained(DateOnly date) => date >= Created && (EndOfLife is null || date <= EndOfLife.Value);

    /// <summary>
    /// The release is a supported stable release at the specified date.
    /// </summary>
    /// <remarks>
    /// When <paramref name="date" /> is between <see cref="Released"/> and <see cref="EndOfLife"/>.
    /// </remarks>
    public bool IsSupported(DateOnly date) => date >= Released && (EndOfLife is null || date <= EndOfLife.Value);
}
