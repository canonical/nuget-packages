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

namespace Canonical.DistroInfo.Ubuntu;

public sealed class UbuntuReleaseInfoCollection : DistroReleaseInfoCollection<UbuntuReleaseInfo>
{
    public static async Task<UbuntuReleaseInfoCollection> ReadFromDistroInfoDataAsync(
        string path = "/usr/share/distro-info/ubuntu.csv",
        CancellationToken cancellationToken = default)
    {
        var releases = await DistroInfo
            .ReadDistroInfoDataCsvFileAsync(path, ParseReleaseInfo, cancellationToken)
            .ConfigureAwait(false);

        return new UbuntuReleaseInfoCollection(releases);
    }

    private static UbuntuReleaseInfo ParseReleaseInfo(ImmutableDictionary<string, string> row)
    {
        string version = row.GetColumnValue("version", required: true)!;
        string codename = row.GetColumnValue("codename", required: true)!;
        AptSeries series = row.GetSeries();
        DateOnly created = row.GetColumnValueAsDate("created", required: true)!.Value;
        DateOnly release = row.GetColumnValueAsDate("release", required: true)!.Value;
        DateOnly eol = row.GetColumnValueAsDate("eol", required: true)!.Value;
        DateOnly? eolServer = row.GetColumnValueAsDate("eol-server");
        DateOnly? eolEsm = row.GetColumnValueAsDate("eol-esm");
        DateOnly? eolLegacy = row.GetColumnValueAsDate("eol-legacy");

        return new UbuntuReleaseInfo(
            version,
            codename,
            series,
            created,
            release,
            eol,
            eolServer,
            eolEsm,
            eolLegacy);
    }

    public UbuntuReleaseInfoCollection(ImmutableArray<UbuntuReleaseInfo> allReleases) : base(allReleases)
    {
    }

    protected override bool TryGetBySeriesAlias(
        string seriesAlias,
        DateOnly? date,
        [NotNullWhen(returnValue: true)] out UbuntuReleaseInfo? releaseInfo)
    {
        if (seriesAlias.Equals("devel", StringComparison.OrdinalIgnoreCase))
        {
            releaseInfo = GetDevel(date);
            return true;
        }

        releaseInfo = null;
        return false;
    }

    public override UbuntuReleaseInfo GetDevel(DateOnly date)
        => GetFirst(release => release.GetSupportStatus(date) is UbuntuSupportStatus.Development);

    public UbuntuReleaseInfo GetLatestReleasedLts()
        => GetLatestReleasedLts(DistroInfo.DefaultDateOrToday);
    public UbuntuReleaseInfo GetLatestReleasedLts(DateTime? date)
        => GetLatestReleasedLts(DistroInfo.GetDateOrDefault(date));
    public UbuntuReleaseInfo GetLatestReleasedLts(DateOnly? date)
        => GetLatestReleasedLts(DistroInfo.GetDateOrDefault(date));
    public UbuntuReleaseInfo GetLatestReleasedLts(DateOnly date)
        => GetFirst(release => release.IsLts && date >= release.Released);

    public override IEnumerable<UbuntuReleaseInfo> GetAllStandardSupported(DateOnly date)
        => AllReleases.Where(release => release.GetSupportStatus(date)
            is UbuntuSupportStatus.StandardSupport
            or UbuntuSupportStatus.StandardServerSupport);

    public IEnumerable<UbuntuReleaseInfo> GetAllEsmSupported()
        => GetAllEsmSupported(DistroInfo.DefaultDateOrToday);
    public IEnumerable<UbuntuReleaseInfo> GetAllEsmSupported(DateTime? date)
        => GetAllEsmSupported(DistroInfo.GetDateOrDefault(date));
    public IEnumerable<UbuntuReleaseInfo> GetAllEsmSupported(DateOnly? date)
        => GetAllEsmSupported(DistroInfo.GetDateOrDefault(date));
    public IEnumerable<UbuntuReleaseInfo> GetAllEsmSupported(DateOnly date)
        => AllReleases.Where(release => release.GetSupportStatus(date)
            is UbuntuSupportStatus.ExpandedSecurityMaintenance);

    public IEnumerable<UbuntuReleaseInfo> GetAllLegacySupported()
        => GetAllLegacySupported(DistroInfo.DefaultDateOrToday);
    public IEnumerable<UbuntuReleaseInfo> GetAllLegacySupported(DateTime? date)
        => GetAllLegacySupported(DistroInfo.GetDateOrDefault(date));
    public IEnumerable<UbuntuReleaseInfo> GetAllLegacySupported(DateOnly? date)
        => GetAllLegacySupported(DistroInfo.GetDateOrDefault(date));
    public IEnumerable<UbuntuReleaseInfo> GetAllLegacySupported(DateOnly date)
        => AllReleases.Where(release => release.GetSupportStatus(date)
            is UbuntuSupportStatus.LegacySupport);
}
