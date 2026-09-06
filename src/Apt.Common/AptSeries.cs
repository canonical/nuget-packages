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
using Canonical.Common;
using Canonical.Common.Parsing;

namespace Canonical.Apt;

/// <summary>
/// Represents an immutable instance of the name of a distribution series used in Debian packaging
/// to refer to specific release targets for a package.
/// </summary>
public readonly partial struct AptSeries : IIdentifier<AptSeries>
{
    /// <summary>
    /// Tries to parse a span of characters representing an apt series name and performs validation.
    /// </summary>
    /// <param name="identifierSpan">The string representation of the apt series name.</param>
    /// <param name="result">Will contain the parsed and validated apt series name.</param>
    /// <param name="annotations">Will contain additional context regarding the parsed result.</param>
    /// <param name="failEarly">
    /// <see langword="true"/> to abort parsing as soon as the first error gets detected.
    /// <paramref name="annotations"/> will be empty; <see langword="false"/> to process
    /// the entire value of <paramref name="identifierSpan"/>.
    /// </param>
    /// <returns><see langword="true"/> if <paramref name="identifierSpan"/> was parsed successfully; otherwise <see langword="false"/>.</returns>
    public static bool TryParse(
        ReadOnlySpan<char> identifierSpan,
        out AptSeries result,
        out ImmutableList<ParsingAnnotation> annotations,
        bool failEarly = false)
    {
        annotations = ImmutableList<ParsingAnnotation>.Empty;

        if (identifierSpan.Length < 1)
        {
            if (failEarly) goto abort;
            annotations += ParsingAnnotation.Create(
                descriptor: EmptyName,
                location: identifierSpan.ToLocation());
            goto abort;
        }

        List<char>? invalidCharacters = null;
        ImmutableList<Location>.Builder? invalidCharacterLocations = null;

        for (var position = 0; position < identifierSpan.Length; ++position)
        {
            char currentCharacter = identifierSpan[position];

            if (!char.IsAsciiLetterLower(currentCharacter) && currentCharacter != '-')
            {
                if (failEarly) goto abort;

                if (invalidCharacters is null)
                {
                    invalidCharacters = [currentCharacter];
                    invalidCharacterLocations = ImmutableList.CreateBuilder<Location>();
                }
                else if (!invalidCharacters.Contains(currentCharacter))
                {
                    invalidCharacters.Add(currentCharacter);
                }

                invalidCharacterLocations!.Add(position);
            }
        }

        if (invalidCharacters is not null)
        {
            annotations += ParsingAnnotation.Create(
                descriptor: InvalidCharacters,
                locations: invalidCharacterLocations!.ToImmutable(),
                messageArgs: [identifierSpan.ToString(), invalidCharacters.JoinAsCharacterLiteralList()]);
        }

        if (!annotations.IsEmpty) goto abort;

        result = new AptSeries(identifierSpan.ToString());
        return true;
abort:
        result = default;
        return false;
    }

    private static partial ParsingException CreateParsingException(
        ReadOnlySpan<char> identifierSpan,
        ImmutableList<ParsingAnnotation> annotations)
    {
        return new AptSeriesNameParsingException(identifierSpan.ToString(), annotations);
    }

    private static readonly ParsingAnnotationDescriptor EmptyName = new(
        Identifier: "APT-SERIES-001",
        Title: "Empty series name",
        MessageFormat: "Series name is empty.");

    private static readonly ParsingAnnotationDescriptor InvalidCharacters = new(
        Identifier: "APT-SERIES-002",
        Title: "Invalid characters",
        MessageFormat: "Series name '{0}' contains invalid character(s): {1}.",
        Description: "A series name must consist only of lowercase letters (a-z).");
}

public sealed class AptSeriesNameParsingException : ParsingException
{
    public AptSeriesNameParsingException(
        string value,
        ImmutableList<ParsingAnnotation> annotations)
        : base(
            message: $"Failed to parse apt series name '{value}'.",
            value: value,
            annotations: annotations)
    {
    }
}
