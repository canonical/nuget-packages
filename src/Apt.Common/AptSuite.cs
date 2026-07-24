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
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using Canonical.Common;
using Canonical.Common.Parsing;

namespace Canonical.Apt;

public readonly partial struct AptSuite : IIdentifier<AptSuite>, IFormattable
{
    // because of CS0282 we need to define all fields in one place
    private readonly string _identifier;
    public readonly AptSeries Series;
    public readonly AptPocket Pocket;

    public AptSuite()
    {
        _identifier = "";
        Series = new AptSeries();
        Pocket = AptPocket.Release;
    }

    public AptSuite(AptSeries series)
    {
        _identifier = series.Identifier;
        Series = series;
        Pocket = AptPocket.Release;
    }

    public AptSuite(AptSeries series, AptPocket pocket)
    {
        _identifier = pocket == AptPocket.Release
            ? series.Identifier
            : $"{series}-{pocket}";
        Series = series;
        Pocket = pocket;
    }

    internal AptSuite(string identifier, AptSeries series, AptPocket pocket)
    {
        _identifier = identifier;
        Series = series;
        Pocket = pocket;
    }

    [Pure]
    public void Deconstruct(out string identifier, out AptSeries series, out AptPocket pocket)
    {
        identifier = _identifier;
        series = Series;
        pocket = Pocket;
    }

    public string ToString(string? format, IFormatProvider? formatProvider) => format switch
    {
        null or "" or "G" => ToString(),
        "g" => $"{Series}-{Pocket.DisplayName}",
        "S" => Series.ToString(),
        "P" => Pocket.ToString(),
        "p" => Pocket.DisplayName,
        _ => throw new FormatException($"The format string '{format}' is not in a correct format.")
    };

    public static bool TryParse(
        ReadOnlySpan<char> identifierSpan,
        out AptSuite identifier,
        out ImmutableList<ParsingAnnotation> annotations,
        bool failEarly = false)
    {
        annotations = ImmutableList<ParsingAnnotation>.Empty;

        if (identifierSpan.Length < 1)
        {
            if (failEarly) goto abort;
            annotations = [ ParsingAnnotation.Create(
                descriptor: NameEmpty,
                location: identifierSpan.ToLocation()) ];
            goto abort;
        }

        int separationIndex = identifierSpan.IndexOf('-');

        ReadOnlySpan<char> seriesSpan;
        var pocket = AptPocket.Release;

        if (separationIndex < 0)
        {
            seriesSpan = identifierSpan;
        }
        else
        {
            seriesSpan = identifierSpan[..separationIndex];
            var pocketSpan = identifierSpan[(separationIndex + 1)..];

            if (pocketSpan.IsEmpty)
            {
                if (failEarly) goto abort;
                annotations = [ ParsingAnnotation.Create(
                    descriptor: EmptyPocket,
                    location: separationIndex) ];
            }
            else if (!AptPocket.TryParse(pocketSpan, out pocket, out var pocketAnnotations, failEarly))
            {
                if (failEarly) goto abort;
                annotations = [.. pocketAnnotations.OffsetLocations(offset: separationIndex)];
            }
        }

        if (!AptSeries.TryParse(seriesSpan, out var series, out var seriesAnnotations, failEarly))
        {
            if (failEarly) goto abort;
            annotations += seriesAnnotations;
            goto abort;
        }

        if (annotations.Count > 0) goto abort;

        identifier = new AptSuite(identifierSpan.ToString(), series, pocket);
        return true;
abort:
        identifier = new();
        return false;
    }

    private static readonly ParsingAnnotationDescriptor NameEmpty = new(
        Identifier: "APT-SUITE-001",
        Title: "Empty suite name",
        MessageFormat: "Suite name is empty.");

    private static readonly ParsingAnnotationDescriptor EmptyPocket = new(
        Identifier: "APT-SUITE-002",
        Title: "Empty pocket name",
        MessageFormat: "Pocket name after '-' is empty.");


    private static partial ParsingException CreateParsingException(
        ReadOnlySpan<char> identifierSpan,
        ImmutableList<ParsingAnnotation> annotations)
    {
        return new AptSuiteParsingException(identifierSpan.ToString(), annotations);
    }
}

public sealed class AptSuiteParsingException : ParsingException
{
    public AptSuiteParsingException(
        string value,
        ImmutableList<ParsingAnnotation> annotations)
        : base(message: $"Failed to parse apt suite '{value}'.", value: value, annotations: annotations)
    {
    }
}
