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
using System.Diagnostics.CodeAnalysis;
using Canonical.Apt;

namespace Canonical.DistroInfo.Debian;

public sealed class DebianReleaseInfoCollection : DistroReleaseInfoCollection<DebianReleaseInfo>
{
    public static async Task<DebianReleaseInfoCollection> ReadFromDistroInfoDataAsync(
        string path = "/usr/share/distro-info/ubuntu.csv",
        CancellationToken cancellationToken = default)
    {
        var releases = await DistroInfo
            .ReadDistroInfoDataCsvFileAsync(path, ParseReleaseInfo, cancellationToken)
            .ConfigureAwait(false);

        return new DebianReleaseInfoCollection(releases);
    }

    private static DebianReleaseInfo ParseReleaseInfo(ImmutableDictionary<string, string> row)
    {
        string? version = row.GetColumnValue("version");
        string codename = row.GetColumnValue("codename", required: true)!;
        AptSeries series = row.GetSeries();
        DateOnly created = row.GetColumnValueAsDate("created", required: true)!.Value;
        DateOnly? release = row.GetColumnValueAsDate("release", required: false);
        DateOnly? eol = row.GetColumnValueAsDate("eol");
        DateOnly? eolLts = row.GetColumnValueAsDate("eol-lts");
        DateOnly? eolELts = row.GetColumnValueAsDate("eol-elts");

        return new DebianReleaseInfo(
            version,
            codename,
            series,
            created,
            release,
            eol,
            eolLts,
            eolELts);
    }

    public DebianReleaseInfoCollection(ImmutableArray<DebianReleaseInfo> allReleases) : base(allReleases)
    {
    }

    protected override bool TryGetBySeriesAlias(
        string seriesAlias,
        DateOnly? date,
        [NotNullWhen(returnValue: true)] out DebianReleaseInfo? releaseInfo)
    {
        if (seriesAlias.Equals("unstable", StringComparison.OrdinalIgnoreCase))
            releaseInfo = GetDevel(date);
        else if (seriesAlias.Equals("testing", StringComparison.OrdinalIgnoreCase))
            releaseInfo = GetTesting(date);
        else if (seriesAlias.Equals("stable", StringComparison.OrdinalIgnoreCase))
            releaseInfo = GetStable(date);
        else if (seriesAlias.Equals("oldstable", StringComparison.OrdinalIgnoreCase))
            releaseInfo = GetOldStable(date);
        else if (seriesAlias.Equals("oldoldstable", StringComparison.OrdinalIgnoreCase))
            releaseInfo = GetOldOldStable(date);
        else
        {
            releaseInfo = null;
            return false;
        }

        return true;
    }

    public override DebianReleaseInfo GetDevel(DateOnly date)
        => GetFirst(release => release.GetSupportStatus() is DebianSupportStatus.Development, skip: 1);

    public DebianReleaseInfo GetTesting() => GetTesting(DistroInfo.DefaultDateOrToday);
    public DebianReleaseInfo GetTesting(DateTime? date) => GetDevel(DistroInfo.GetDateOrDefault(date));
    public DebianReleaseInfo GetTesting(DateOnly? date) => GetDevel(DistroInfo.GetDateOrDefault(date));
    public DebianReleaseInfo GetTesting(DateOnly date)
        => GetFirst(release => date >= release.Created
                                && (release is { Released: null, Version: not null }
                                    || (release is { Released: { } released } && date < released)));

    public DebianReleaseInfo GetUnstable() => GetDevel();
    public DebianReleaseInfo GetUnstable(DateTime? date) => GetDevel(date);
    public DebianReleaseInfo GetUnstable(DateOnly? date) => GetDevel(date);
    public DebianReleaseInfo GetUnstable(DateOnly date) => GetDevel(date);

    public DebianReleaseInfo GetStable() => GetLatestReleased();
    public DebianReleaseInfo GetStable(DateTime? date) => GetLatestReleased(date);
    public DebianReleaseInfo GetStable(DateOnly? date) => GetLatestReleased(date);
    public DebianReleaseInfo GetStable(DateOnly date) => GetLatestReleased(date);

    public DebianReleaseInfo GetOldStable() => GetOldStable(DistroInfo.DefaultDateOrToday);
    public DebianReleaseInfo GetOldStable(DateTime? date) => GetOldStable(DistroInfo.GetDateOrDefault(date));
    public DebianReleaseInfo GetOldStable(DateOnly? date) => GetOldStable(DistroInfo.GetDateOrDefault(date));
    public DebianReleaseInfo GetOldStable(DateOnly date)
        => GetFirst(release => date >= release.Released, skip: 1);

    public DebianReleaseInfo GetOldOldStable() => GetOldOldStable(DistroInfo.DefaultDateOrToday);
    public DebianReleaseInfo GetOldOldStable(DateTime? date) => GetOldOldStable(DistroInfo.GetDateOrDefault(date));
    public DebianReleaseInfo GetOldOldStable(DateOnly? date) => GetOldOldStable(DistroInfo.GetDateOrDefault(date));
    public DebianReleaseInfo GetOldOldStable(DateOnly date)
        => GetFirst(release => date >= release.Released, skip: 2);

    public override IEnumerable<DebianReleaseInfo> GetAllStandardSupported(DateOnly date)
        => AllReleases.Where(release => release.GetSupportStatus(date) is DebianSupportStatus.StandardSupport);

    public IEnumerable<DebianReleaseInfo> GetAllLtsSupported()
        => GetAllLtsSupported(DistroInfo.DefaultDateOrToday);
    public IEnumerable<DebianReleaseInfo> GetAllLtsSupported(DateTime? date)
        => GetAllLtsSupported(DistroInfo.GetDateOrDefault(date));
    public IEnumerable<DebianReleaseInfo> GetAllLtsSupported(DateOnly? date)
        => GetAllLtsSupported(DistroInfo.GetDateOrDefault(date));
    public IEnumerable<DebianReleaseInfo> GetAllLtsSupported(DateOnly date)
        => AllReleases.Where(release => release.GetSupportStatus(date) is DebianSupportStatus.LongTermSupport);

    public IEnumerable<DebianReleaseInfo> GetAllELtsSupported()
        => GetAllELtsSupported(DistroInfo.DefaultDateOrToday);
    public IEnumerable<DebianReleaseInfo> GetAllELtsSupported(DateTime? date)
        => GetAllELtsSupported(DistroInfo.GetDateOrDefault(date));
    public IEnumerable<DebianReleaseInfo> GetAllELtsSupported(DateOnly? date)
        => GetAllELtsSupported(DistroInfo.GetDateOrDefault(date));
    public IEnumerable<DebianReleaseInfo> GetAllELtsSupported(DateOnly date)
        => AllReleases.Where(release => release.GetSupportStatus(date) is DebianSupportStatus.ExtendedLongTermSupport);
}
