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
using System.Globalization;
using System.Runtime.CompilerServices;
using Canonical.Apt;

namespace Canonical.DistroInfo;

public static class DistroInfo
{
    public static DateOnly? DefaultDate { get; set; } = null;

    public static DateOnly GetDateOrDefault(DateTime? date)
        => date.HasValue
           ? DateOnly.FromDateTime(date.Value)
           : DefaultDateOrToday;

    public static DateOnly GetDateOrDefault(DateOnly? date)
        => date ?? DefaultDateOrToday;

    public static DateOnly DefaultDateOrToday
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] get
        => DefaultDate ?? DateOnly.FromDateTime(DateTime.Now);
    }

    internal static async Task<ImmutableArray<T>> ReadDistroInfoDataCsvFileAsync<T>(
        string path,
        Func<ImmutableDictionary<string, string>, T> parser,
        CancellationToken cancellationToken)
        where T : class, IDistroReleaseInfo
    {
        using var csv = File.OpenText(path);

        var releases = ImmutableArray.CreateBuilder<T>();

        var line = await csv.ReadLineAsync(cancellationToken).ConfigureAwait(false);

        if (line is null)
        {
            throw new InvalidDataException($"Distro info csv file '{path}' is empty.");
        }

        var columns = line.Split(',');

        var readLineCount = 1;
        var row = ImmutableDictionary.CreateBuilder<string, string>();
        while ((line = await csv.ReadLineAsync(cancellationToken).ConfigureAwait(false)) is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ++readLineCount;

            var values = line.Split(',');
            for (int i = 0; i < values.Length; ++i)
            {
                if (i >= columns.Length)
                {
                    throw new InvalidDataException($"Line {readLineCount} of distro info csv file '{path}' contains more columns than defined in the header (Column Count (Header): {columns.Length}; Column Count (Current Row): {values.Length}).");
                }

                row.Add(columns[i], values[i]);
            }

            try
            {
                releases.Add(parser(row.ToImmutable()));
            }
            catch (Exception exception)
            {
                throw new InvalidDataException($"Line {readLineCount} of distro info csv file '{path}' could not be parsed: {exception.Message}", exception);
            }

            row.Clear();
        }

        return releases.ToImmutable();
    }

    internal static string? GetColumnValue(this ImmutableDictionary<string, string> row, string columnName, bool required = false)
    {
        if (!row.TryGetValue(columnName, out var value) && required)
        {
            throw new InvalidDataException($"Required value for column '{columnName}' is missing.");
        }
        if (required && string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidDataException($"Required value for column '{columnName}' is empty.");
        }

        return value;
    }

    internal static DateOnly? GetColumnValueAsDate(this ImmutableDictionary<string, string> row, string columnName, bool required = false)
    {
        if (!row.TryGetValue(columnName, out var dateString) && required)
        {
            throw new InvalidDataException($"Required value for column '{columnName}' is missing.");
        }

        if (string.IsNullOrWhiteSpace(dateString))
        {
            if (required) throw new InvalidDataException($"Required value for column '{columnName}' is empty.");
            return null;
        }

        if (DateOnly.TryParseExact(dateString,
                format: "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateOnly fullDate))
        {
            return fullDate;
        }

        if (DateOnly.TryParseExact(dateString,
                format: "yyyy-MM",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateOnly yearMonth))
        {
            int lastDay = DateTime.DaysInMonth(yearMonth.Year, yearMonth.Month);
            return new DateOnly(yearMonth.Year, yearMonth.Month, lastDay);
        }

        throw new FormatException($"Date '{dateString}' not in ISO 8601 format.");
    }

    internal static AptSeries GetSeries(this ImmutableDictionary<string, string> row)
    {
        var series = row.GetColumnValue("series", required: true);
        return AptSeries.Parse(series);
    }
}
