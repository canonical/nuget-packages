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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Canonical.Apt;

namespace Canonical.DistroInfo;

public abstract class DistroReleaseInfoCollection<T> where T : class, IDistroReleaseInfo
{
    protected internal DistroReleaseInfoCollection(ImmutableArray<T> allReleases)
    {
        if (allReleases.IsEmpty)
        {
            throw new ArgumentException("Release collection is empty.", paramName: nameof(allReleases));
        }

        AllReleases = allReleases;
    }


    /// <summary>
    /// All release infos contained in this collection.
    /// </summary>
    public readonly ImmutableArray<T> AllReleases;

    #region TryGetBySeries

    /// <summary>
    /// Tries to get the information for a release that matches a specific series.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if release information was found for the specified <paramref name="series"/>;
    /// otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Uses <see cref="DistroInfo.DefaultDateOrToday"/> when evaluating dynamic series aliases.
    /// </remarks>
    public bool TryGetBySeries(AptSeries series, [NotNullWhen(returnValue: true)] out T? releaseInfo)
        => TryGetBySeries(series.Identifier, out releaseInfo);

    /// <summary>
    /// Tries to get the information for a release that matches a specific series.
    /// </summary>
    /// <param name="date">
    /// The specific date to use when evaluating dynamic series aliases;
    /// when <see langword="null"/> <see cref="DistroInfo.DefaultDateOrToday"/>
    /// will be used.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if release information was found for the specified <paramref name="series"/>;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public bool TryGetBySeries(AptSeries series, DateTime? date, [NotNullWhen(returnValue: true)] out T? releaseInfo)
        => TryGetBySeries(series.Identifier, date, out releaseInfo);

    /// <inheritdoc cref="TryGetBySeries(AptSeries, DateTime?, out T?)"/>
    public bool TryGetBySeries(AptSeries series, DateOnly? date, [NotNullWhen(returnValue: true)] out T? releaseInfo)
        => TryGetBySeries(series.Identifier, date, out releaseInfo);

    /// <inheritdoc cref="TryGetBySeries(Canonical.Apt.AptSeries,out T?)"/>
    public bool TryGetBySeries(string series, [NotNullWhen(returnValue: true)] out T? releaseInfo)
        => TryGetBySeries(series, date: default(DateOnly?), out releaseInfo);

    /// <inheritdoc cref="TryGetBySeries(AptSeries, DateTime?, out T?)"/>
    public bool TryGetBySeries(string series, DateTime? date, [NotNullWhen(returnValue: true)] out T? releaseInfo)
        => date.HasValue
            ? TryGetBySeries(series, DateOnly.FromDateTime(date.Value), out releaseInfo)
            : TryGetBySeries(series, out releaseInfo);

    /// <inheritdoc cref="TryGetBySeries(AptSeries, DateTime?, out T?)"/>
    public bool TryGetBySeries(string series, DateOnly? date, [NotNullWhen(returnValue: true)] out T? releaseInfo)
    {
        if (TryGetBySeriesAlias(series, date, out releaseInfo)) return true;

        for (int i = AllReleases.Length - 1; i >= 0; i--)
        {
            if (AllReleases[i].Series.Identifier.Equals(series, StringComparison.OrdinalIgnoreCase))
            {
                releaseInfo = AllReleases[i];
                return true;
            }
        }

        releaseInfo = null;
        return false;
    }

    protected abstract bool TryGetBySeriesAlias(
        string seriesAlias,
        DateOnly? date,
        [NotNullWhen(returnValue: true)] out T? releaseInfo);

    #endregion

    public bool TryGetByVersion(string version, [NotNullWhen(returnValue: true)] out T? releaseInfo)
    {
        var alternativeVersion = version.EndsWith(" LTS", StringComparison.OrdinalIgnoreCase)
            ? version[..^4]
            : version + " LTS";

        for (int i = AllReleases.Length - 1; i >= 0; i--)
        {
            if (version.Equals(AllReleases[i].Version, StringComparison.OrdinalIgnoreCase)
                || alternativeVersion.Equals(AllReleases[i].Version, StringComparison.OrdinalIgnoreCase))
            {
                releaseInfo = AllReleases[i];
                return true;
            }
        }

        releaseInfo = null;
        return false;
    }

    public bool TryGetByCodename(string codename, [NotNullWhen(returnValue: true)] out T? releaseInfo)
    {
        for (int i = AllReleases.Length - 1; i >= 0; i--)
        {
            if (codename.Equals(AllReleases[i].Version, StringComparison.OrdinalIgnoreCase))
            {
                releaseInfo = AllReleases[i];
                return true;
            }
        }

        releaseInfo = null;
        return false;
    }

    #region GetDevel

    /// <summary>
    /// Get the development release.
    /// </summary>
    /// <remarks>
    /// Uses the date provided by <see cref="DistroInfo.DefaultDateOrToday"/>.
    /// </remarks>
    /// <exception cref="DistroDataOutdated">When no info for the development release was found.</exception>
    public T GetDevel() => GetDevel(DistroInfo.DefaultDateOrToday);

    /// <summary>
    /// Get the development release at the specified date.
    /// </summary>
    /// <param name="date">The specific date when to evaluate; when <see langword="null"/> <see cref="DistroInfo.DefaultDateOrToday"/> will be used.</param>
    /// <exception cref="DistroDataOutdated">When no info for the development release was found.</exception>
    public T GetDevel(DateTime? date) => GetDevel(DistroInfo.GetDateOrDefault(date));

    /// <inheritdoc cref="GetDevel(DateTime?)"/>
    public T GetDevel(DateOnly? date) => GetDevel(DistroInfo.GetDateOrDefault(date));

    /// <summary>
    /// Get the development release at the specified date.
    /// </summary>
    /// <param name="date">The specific date when to evaluate</param>
    /// <exception cref="DistroDataOutdated">When no info for the development release was found.</exception>
    public abstract T GetDevel(DateOnly date);

    #endregion

    #region GetLatestReleased

    public T GetLatestReleased() => GetLatestReleased(DistroInfo.DefaultDateOrToday);
    public T GetLatestReleased(DateTime? date) => GetLatestReleased(DistroInfo.GetDateOrDefault(date));
    public T GetLatestReleased(DateOnly? date) => GetLatestReleased(DistroInfo.GetDateOrDefault(date));
    public T GetLatestReleased(DateOnly date)
        => GetFirst(release => date >= release.Released);

    #endregion

    #region GetAllAvailable

    public IEnumerable<T> GetAllAvailable() => GetAllAvailable(DistroInfo.DefaultDateOrToday);
    public IEnumerable<T> GetAllAvailable(DateTime? date) => GetAllAvailable(DistroInfo.GetDateOrDefault(date));
    public IEnumerable<T> GetAllAvailable(DateOnly? date) => GetAllAvailable(DistroInfo.GetDateOrDefault(date));
    public IEnumerable<T> GetAllAvailable(DateOnly date)
        => AllReleases.Where(release => date >= release.Created);

    #endregion

    #region GetAllSupported

    public IEnumerable<T> GetAllSupported() => GetAllSupported(DistroInfo.DefaultDateOrToday);
    public IEnumerable<T> GetAllSupported(DateTime? date) => GetAllSupported(DistroInfo.GetDateOrDefault(date));
    public IEnumerable<T> GetAllSupported(DateOnly? date) => GetAllSupported(DistroInfo.GetDateOrDefault(date));
    public IEnumerable<T> GetAllSupported(DateOnly date)
        => AllReleases.Where(release => release.IsSupported(date));

    #endregion

    #region GetAllUnsupported

    public IEnumerable<T> GetAllUnsupported() => GetAllUnsupported(DistroInfo.DefaultDateOrToday);
    public IEnumerable<T> GetAllUnsupported(DateTime? date) => GetAllUnsupported(DistroInfo.GetDateOrDefault(date));
    public IEnumerable<T> GetAllUnsupported(DateOnly? date) => GetAllUnsupported(DistroInfo.GetDateOrDefault(date));
    public IEnumerable<T> GetAllUnsupported(DateOnly date)
        => AllReleases.Where(release => date >= release.Created && !release.IsSupported(date));

    #endregion

    #region GetAllStandardSupported

    public IEnumerable<T> GetAllStandardSupported() => GetAllStandardSupported(DistroInfo.DefaultDateOrToday);
    public IEnumerable<T> GetAllStandardSupported(DateTime? date) => GetAllStandardSupported(DistroInfo.GetDateOrDefault(date));
    public IEnumerable<T> GetAllStandardSupported(DateOnly? date) => GetAllStandardSupported(DistroInfo.GetDateOrDefault(date));
    public abstract IEnumerable<T> GetAllStandardSupported(DateOnly date);

    #endregion

    #region GetAllMaintained

    public IEnumerable<T> GetAllMaintained() => GetAllMaintained(DistroInfo.DefaultDateOrToday);
    public IEnumerable<T> GetAllMaintained(DateTime? date) => GetAllMaintained(DistroInfo.GetDateOrDefault(date));
    public IEnumerable<T> GetAllMaintained(DateOnly? date) => GetAllMaintained(DistroInfo.GetDateOrDefault(date));
    public IEnumerable<T> GetAllMaintained(DateOnly date)
        => AllReleases.Where(release => release.IsMaintained(date));

    #endregion

    #region GetAllUnmaintained

    public IEnumerable<T> GetAllUnmaintained() => GetAllUnmaintained(DistroInfo.DefaultDateOrToday);
    public IEnumerable<T> GetAllUnmaintained(DateTime? date) => GetAllUnmaintained(DistroInfo.GetDateOrDefault(date));
    public IEnumerable<T> GetAllUnmaintained(DateOnly? date) => GetAllUnmaintained(DistroInfo.GetDateOrDefault(date));
    public IEnumerable<T> GetAllUnmaintained(DateOnly date)
        => AllReleases.Where(release => date >= release.Created && !release.IsMaintained(date));

    #endregion

    protected T GetFirst(Func<T, bool> predicate)
    {
        for (int i = AllReleases.Length - 1; i >= 0; i--)
        {
            if (predicate(AllReleases[i])) return AllReleases[i];
        }

        throw new DistroDataOutdated();
    }

    protected T GetFirst(Func<T, bool> predicate, int skip)
    {
        Debug.Assert(skip > 0);

        for (int i = AllReleases.Length - 1; i >= 0; i--)
        {
            if (predicate(AllReleases[i]) && --skip == 0) return AllReleases[i];
        }

        throw new DistroDataOutdated();
    }
}
