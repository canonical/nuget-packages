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

namespace Canonical.DistroInfo.Debian;

public sealed record DebianReleaseInfo : IDistroReleaseInfo
{
    private readonly string _stringRepresentation;

    /// <summary>
    /// The version identifier of Debian release.
    /// </summary>
    /// <example>
    /// <c>12</c>
    /// </example>
    /// <remarks>
    /// Non-stable releases do not have a version identifier.
    /// </remarks>
    public string? Version { get; }

    /// <summary>
    /// The codename of the Debian release.
    /// </summary>
    /// <example>
    /// <c>"Bookworm"</c>
    /// </example>
    public string Codename { get; }

    /// <summary>
    /// Represents the series identifier of a Debian release.
    /// </summary>
    /// <example>
    /// <c>bookworm</c>
    /// </example>
    public AptSeries Series { get; }

    /// <summary>
    /// The date when the development for the Debian release started.
    /// </summary>
    public DateOnly Created { get; }

    /// <summary>
    /// The date when the Debian version was initially released.
    /// </summary>
    public DateOnly? Released { get; }

    /// <summary>
    /// The date when the (standard) <see href="https://wiki.debian.org/DebianStable">Stable Support</see>
    /// for this Debian release ends.
    /// </summary>
    public DateOnly? EndOfStandardSupport { get; }

    /// <summary>
    /// The date when the <see href="https://wiki.debian.org/LTS">Long Term Support (LTS)</see>
    /// for this Debian release ends.
    /// </summary>
    public DateOnly? EndOfLongTermSupport { get; }

    /// <summary>
    /// The date when the <see href="https://wiki.debian.org/LTS/Extended">Extended Long Term Support (ELTS)</see>
    /// for this Debian release ends.
    /// </summary>
    public DateOnly? EndOfExtendedLongTermSupport { get; }

    /// <summary>
    /// The date representing the end of life for the Debian release.
    /// </summary>
    /// <remarks>
    /// This is the latest date of <see cref="EndOfStandardSupport"/>, <see cref="EndOfLongTermSupport"/> and
    /// <see cref="EndOfExtendedLongTermSupport"/>.
    /// </remarks>
    public DateOnly? EndOfLife { get; }

    public DebianReleaseInfo(
        string? version,
        string codename,
        AptSeries series,
        DateOnly created,
        DateOnly? released,
        DateOnly? endOfStandardSupport,
        DateOnly? endOfLongTermSupport,
        DateOnly? endOfExtendedLongTermSupport)
    {
        Version = version;
        Codename = codename;
        Series = series;
        Created = created;
        Released = released;
        EndOfStandardSupport = endOfStandardSupport;
        EndOfLongTermSupport = endOfLongTermSupport;
        EndOfExtendedLongTermSupport = endOfExtendedLongTermSupport;
        EndOfLife = endOfExtendedLongTermSupport ?? endOfLongTermSupport ?? endOfStandardSupport;

        var versionText = version is { Length: > 0 } ? $"{version} " : string.Empty;
        _stringRepresentation = $"Debian {versionText}({codename})";
    }

    /// <inheritdoc />
    public override string ToString() => _stringRepresentation;

    public DebianSupportStatus GetSupportStatus() =>
        GetSupportStatus(DistroInfo.DefaultDateOrToday);

    public DebianSupportStatus GetSupportStatus(DateTime? date) =>
        GetSupportStatus(DistroInfo.GetDateOrDefault(date));

    public DebianSupportStatus GetSupportStatus(DateOnly? date) =>
        GetSupportStatus(DistroInfo.GetDateOrDefault(date));

    public DebianSupportStatus GetSupportStatus(DateOnly date)
    {
        if (date < Created)
            return DebianSupportStatus.Unknown;
        if (Released is null || date < Released)
            return DebianSupportStatus.Development;
        if (EndOfLife is null || date <= EndOfStandardSupport)
            return DebianSupportStatus.StandardSupport;
        if (date <= EndOfLongTermSupport)
            return DebianSupportStatus.LongTermSupport;
        if (date <= EndOfExtendedLongTermSupport)
            return DebianSupportStatus.ExtendedLongTermSupport;

        return DebianSupportStatus.Unsupported;
    }
}

public enum DebianSupportStatus
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
    /// The release received long-term support (LTS) at the specified date.
    /// </summary>
    LongTermSupport,

    /// <summary>
    /// The release received extended long-term support (ELTS) at the specified date.
    /// </summary>
    ExtendedLongTermSupport,

    /// <summary>
    /// The release is no longer supported at the specified date.
    /// </summary>
    Unsupported,
}
