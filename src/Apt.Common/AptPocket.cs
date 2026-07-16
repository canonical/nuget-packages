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

public readonly partial struct AptPocket : IIdentifier<AptPocket>, IFormattable
{
    public static readonly AptPocket Release = new();

    public string DisplayName => _identifier.Length == 0 ? "release" : _identifier;

    public static bool TryParse(
        ReadOnlySpan<char> identifierSpan,
        out AptPocket identifier,
        out ImmutableList<ParsingAnnotation> annotations,
        bool failFast = false)
    {
        annotations = ImmutableList<ParsingAnnotation>.Empty;

        if (identifierSpan.Length == 0)
        {
            identifier = Release;
            return true;
        }

        if (!char.IsAsciiLetterLower(identifierSpan[0]))
        {
            if (failFast) goto abort;
            annotations += ParsingAnnotation.Create(
                descriptor: InvalidStartCharacter,
                location: 0,
                messageArgs: identifierSpan[0]);
        }

        if (!char.IsAsciiLetterLower(identifierSpan[^1]))
        {
            if (failFast) goto abort;
            annotations += ParsingAnnotation.Create(
                descriptor: InvalidEndCharacter,
                location: identifierSpan.Length - 1,
                messageArgs: identifierSpan[^1]);
        }

        List<char>? invalidCharacters = null;
        ImmutableList<Location>.Builder? invalidCharacterLocations = null;
        for (var position = 0; position < identifierSpan.Length; ++position)
        {
            char currentCharacter = identifierSpan[position];

            if (!char.IsAsciiLetterLower(currentCharacter)
                && currentCharacter != '-')
            {
                if (failFast) goto abort;

                if (invalidCharacters is null)
                {
                    invalidCharacters = [ currentCharacter ];
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
            annotations += ParsingAnnotation.Create(InvalidCharacters,
                locations: invalidCharacterLocations!.ToImmutable(),
                messageArgs: invalidCharacters.JoinAsCharacterLiteralList());
        }

        if (!annotations.IsEmpty) goto abort;

        identifier = new AptPocket(identifierSpan.ToString());
        return true;
abort:
        identifier = new AptPocket();
        return false;
    }

    private static readonly ParsingAnnotationDescriptor InvalidStartCharacter = new (
        Identifier: "APT-POCKET-001",
        Title: "Invalid start character",
        MessageFormat: "Pocket name can not start with the character '{0}'.",
        Description: "Pocket names must start with a lowercase letter (a-z).");

    private static readonly ParsingAnnotationDescriptor InvalidCharacters = new(
        Identifier: "DPKG-POCKET-002",
        Title: "Invalid characters",
        MessageFormat: "Pocket name contains invalid character(s): {0}.",
        Description: "Pocket names must consist only of lowercase letters (a-z), " +
                     "and minus (-) signs.");

    private static readonly ParsingAnnotationDescriptor InvalidEndCharacter = new (
        Identifier: "APT-POCKET-003",
        Title: "Invalid end character",
        MessageFormat: "Pocket name can not end with the character '{0}'.",
        Description: "Pocket names must end with a lowercase letter (a-z).");

    private static partial ParsingException CreateParsingException(
        ReadOnlySpan<char> identifierSpan,
        ImmutableList<ParsingAnnotation> annotations)
    {
        return new AptPocketParsingException(identifierSpan.ToString(), annotations);
    }

    public string ToString(string? format, IFormatProvider? formatProvider = null)
    {
        if (string.IsNullOrEmpty(format) || format == "G") return ToString();
        if (format == "D") return DisplayName;

        throw new FormatException($"The format string '{format}' is not in a correct format.");
    }
}

public sealed class AptPocketParsingException : ParsingException
{
    public AptPocketParsingException(
        string value,
        ImmutableList<ParsingAnnotation> annotations)
        : base(message: $"Failed to parse apt pocket name '{value}'.", value: value, annotations: annotations)
    {

    }
}
