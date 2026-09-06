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

namespace Canonical.DistroInfo.Ubuntu;

public sealed record UbuntuReleaseInfo : IDistroReleaseInfo
{
    private readonly string _stringRepresentation;

    /// <summary>
    /// The version identifier of Ubuntu release.
    /// </summary>
    /// <example>
    /// <c>"24.04 LTS"</c>
    /// </example>
    public string Version { get; }

    /// <summary>
    /// The short version identifier of Ubuntu release.
    /// </summary>
    /// <example>
    /// <c>"24.04"</c>
    /// </example>
    public string ShortVersion { get; }

    /// <summary>
    /// Determines whether the Ubuntu release is a Long Term Support (LTS) release.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the Ubuntu release is an LTS release; otherwise, <see langword="false"/>.
    /// </value>
    public bool IsLts { get; }

    /// <summary>
    /// The codename of the Ubuntu release.
    /// </summary>
    /// <example>
    /// <c>"Noble Numbat"</c>
    /// </example>
    public string Codename { get; }

    /// <summary>
    /// Represents the series identifier of an Ubuntu release.
    /// </summary>
    /// <example>
    /// <c>noble</c>
    /// </example>
    public AptSeries Series { get; }

    /// <summary>
    /// The date when the development for the Ubuntu release started.
    /// </summary>
    public DateOnly Created { get; }

    /// <summary>
    /// The date when the Ubuntu version was initially released.
    /// </summary>
    public DateOnly Released { get; }
    DateOnly? IDistroReleaseInfo.Released => Released;

    /// <summary>
    /// The date when the standard support for the Ubuntu version ends.
    /// </summary>
    /// <remarks>
    /// If <see cref="EndOfServerStandardSupport"/> is not <see langword="null"/> than this
    /// date just represents when the standard support for the Ubuntu desktop version ends.
    /// This is just the case for older Ubuntu releases.
    /// </remarks>
    public DateOnly EndOfStandardSupport { get; }

    /// <summary>
    /// Represents the end of standard support date for Ubuntu server.
    /// </summary>
    /// <remarks>
    /// Older Ubuntu LTS versions supported the server version longer than the Desktop version.
    /// Ubuntu 10.04 LTS (Lucid Lynx) was the last Ubuntu version with that differentiation.
    /// For all later versions, <see cref="EndOfStandardSupport"/> has the same value or is
    /// <see langword="null"/>.
    /// </remarks>
    public DateOnly? EndOfServerStandardSupport { get; }

    /// <summary>
    /// The date when the expanded security maintenance (ESM) for the Ubuntu release ends.
    /// </summary>
    /// <remarks>
    /// Only newer Ubuntu LTS releases do have ESM. <see langword="null"/> will be returned for releases without ESM.
    /// </remarks>
    public DateOnly? EndOfLegacyMaintenance { get; }

    /// <summary>
    /// The date when the expanded security maintenance (ESM) for the Ubuntu release ends.
    /// </summary>
    /// <remarks>
    /// Only newer Ubuntu LTS releases do have ESM. <see langword="null"/> will be returned for releases without ESM.
    /// </remarks>
    public DateOnly? EndOfExpandedSecurityMaintenance { get; }

    /// <summary>
    /// The date representing the end of life for the Ubuntu release.
    /// </summary>
    /// <remarks>
    /// This is the latest date of <see cref="EndOfStandardSupport"/>, <see cref="EndOfServerStandardSupport"/>,
    /// <see cref="EndOfExpandedSecurityMaintenance"/> and <see cref="EndOfLegacyMaintenance"/> that is not null.
    /// </remarks>
    public DateOnly EndOfLife { get; }
    DateOnly? IDistroReleaseInfo.EndOfLife => EndOfLife;

    public UbuntuReleaseInfo(
        string version,
        string codename,
        AptSeries series,
        DateOnly created,
        DateOnly released,
        DateOnly endOfStandardSupport,
        DateOnly? endOfServerStandardSupport,
        DateOnly? endOfExpandedSecurityMaintenance,
        DateOnly? endOfLegacyMaintenance)
    {
        IsLts = version.EndsWith(" LTS");
        Version = version;
        ShortVersion = IsLts ? version[..^4] : version;
        Codename = codename;
        Series = series;
        Created = created;
        Released = released;
        EndOfStandardSupport = endOfStandardSupport;
        EndOfServerStandardSupport = endOfServerStandardSupport;
        EndOfExpandedSecurityMaintenance = endOfExpandedSecurityMaintenance;
        EndOfLegacyMaintenance = endOfLegacyMaintenance;
        EndOfLife = endOfLegacyMaintenance
                    ?? endOfExpandedSecurityMaintenance
                    ?? endOfServerStandardSupport
                    ?? endOfStandardSupport;

        _stringRepresentation = $"Ubuntu {version} ({codename})";
    }

    /// <inheritdoc />
    public override string ToString() => _stringRepresentation;

    public UbuntuSupportStatus GetSupportStatus() =>
        GetSupportStatus(DistroInfo.DefaultDateOrToday);

    public UbuntuSupportStatus GetSupportStatus(DateTime? date) =>
        GetSupportStatus(DistroInfo.GetDateOrDefault(date));

    public UbuntuSupportStatus GetSupportStatus(DateOnly? date) =>
        GetSupportStatus(DistroInfo.GetDateOrDefault(date));

    public UbuntuSupportStatus GetSupportStatus(DateOnly date)
    {
        if (date < Created)
            return UbuntuSupportStatus.Unknown;
        if (date < Released)
            return UbuntuSupportStatus.Development;
        if (date <= EndOfStandardSupport)
            return UbuntuSupportStatus.StandardSupport;
        if (date <= EndOfServerStandardSupport)
            return UbuntuSupportStatus.StandardServerSupport;
        if (date <= EndOfExpandedSecurityMaintenance)
            return UbuntuSupportStatus.ExpandedSecurityMaintenance;
        if (date <= EndOfLegacyMaintenance)
            return UbuntuSupportStatus.LegacySupport;

        return UbuntuSupportStatus.Unsupported;
    }
}

public enum UbuntuSupportStatus
{
    /// <summary>
    /// The release was unknown at the specified date.
    /// </summary>
    Unknown,

    /// <summary>
    /// The release was in active development at the specified date.
    /// </summary>
    Development,

    /// <summary>
    /// The release received standard support at the specified date.
    /// </summary>
    StandardSupport,

    /// <summary>
    /// The release received standard server support at the specified date.
    /// </summary>
    /// <remarks>
    /// Older Ubuntu versions supported the server version longer than the Desktop version.
    /// Ubuntu 10.04 LTS (Lucid Lynx) was the last Ubuntu version with that differentiation.
    /// </remarks>
    StandardServerSupport,

    /// <summary>
    /// The release received expanded security maintenance (ESM) at the specified date.
    /// </summary>
    ExpandedSecurityMaintenance,

    /// <summary>
    /// The release received expanded security maintenance (ESM) covered by the Legacy add-on
    /// at the specified date.
    /// </summary>
    LegacySupport,

    /// <summary>
    /// The release was no longer supported at the specified date.
    /// </summary>
    Unsupported,
}
