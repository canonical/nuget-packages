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

namespace Canonical.Dpkg;

/// <summary>
/// Represents an immutable instance of the name of a debian package.
/// </summary>
public readonly partial struct DpkgPackageName : IIdentifier<DpkgPackageName>
{
    /// <summary>
    /// Tries to parse a span of characters representing a debian package name and performs validation.
    /// </summary>
    /// <param name="identifierSpan">The string representation of the debian package name.</param>
    /// <param name="identifier">Will contain the parsed and validated dpkg package name.</param>
    /// <param name="annotations">Will contain additional context regarding the parsed result.</param>
    /// <param name="failEarly">
    /// <see langword="true"/> to abort parsing as soon as the first error gets detected.
    /// <paramref name="annotations"/> will be empty; <see langword="false"/> to process
    /// the entire value of <paramref name="identifierSpan"/>.
    /// </param>
    /// <returns><see langword="true"/> if <paramref name="identifierSpan"/> was parsed successfully; otherwise <see langword="false"/>.</returns>
    /// <seealso href="https://www.debian.org/doc/debian-policy/ch-controlfields.html#source"/>
    public static bool TryParse(
        ReadOnlySpan<char> identifierSpan,
        out DpkgPackageName identifier,
        out ImmutableList<ParsingAnnotation> annotations,
        bool failEarly = false)
    {
        annotations = ImmutableList<ParsingAnnotation>.Empty;

        if (identifierSpan.Length < 2)
        {
            if (failEarly) goto abort;
            annotations += ParsingAnnotation.Create(
                descriptor: PackageNameTooShort,
                location: identifierSpan.ToLocation());
        }

        if (identifierSpan.Length > 0
            && !char.IsAsciiLetterLower(identifierSpan[0])
            && !char.IsAsciiDigit(identifierSpan[0]))
        {
            if (failEarly) goto abort;
            annotations += ParsingAnnotation.Create(
                descriptor: InvalidStartCharacter,
                location: 0,
                messageArgs: identifierSpan[0]);
        }

        List<char>? invalidCharacters = null;
        ImmutableList<Location>.Builder? invalidCharacterLocations = null;
        for (var position = 1; position < identifierSpan.Length; ++position)
        {
            char currentCharacter = identifierSpan[position];

            if (!char.IsAsciiLetterLower(currentCharacter)
                && !char.IsAsciiDigit(currentCharacter)
                && currentCharacter != '-'
                && currentCharacter != '.'
                && currentCharacter != '+')
            {
                if (failEarly) goto abort;

                if (invalidCharacters is null)
                {
                    invalidCharacters = [ currentCharacter ];
                    invalidCharacterLocations = ImmutableList.CreateBuilder<Location>();
                }
                else if (!invalidCharacters.Contains(currentCharacter))
                {
                    invalidCharacters.Add(currentCharacter);
                }
                invalidCharacterLocations!.Add(new Location(position));
            }
        }

        if (invalidCharacters is not null)
        {
            annotations += ParsingAnnotation.Create(InvalidCharacters,
                locations: invalidCharacterLocations!.ToImmutable(),
                messageArgs: invalidCharacters.JoinAsCharacterLiteralList());
        }

        if (!annotations.IsEmpty) goto abort;

        identifier = new DpkgPackageName(identifierSpan.ToString());
        return true;
abort:
        identifier = new DpkgPackageName();
        return false;
    }

    private static readonly ParsingAnnotationDescriptor PackageNameTooShort = new(
        Identifier: "DPKG-NAME-001",
        Title: "Short package name",
        MessageFormat: "Package name is too short.",
        Description: "Package names must be at least two characters long.",
        HelpLink: new Uri("https://www.debian.org/doc/debian-policy/ch-controlfields.html#source"));

    private static readonly ParsingAnnotationDescriptor InvalidStartCharacter = new (
        Identifier: "DPKG-NAME-002",
        Title: "Invalid start character",
        MessageFormat: "Package name can not start with the character '{0}'.",
        Description: "Package names must start with a lowercase alphanumeric character (a-z or 0-9).",
        HelpLink: new Uri("https://www.debian.org/doc/debian-policy/ch-controlfields.html#source"));

    private static readonly ParsingAnnotationDescriptor InvalidCharacters = new(
        Identifier: "DPKG-NAME-003",
        Title: "Invalid characters",
        MessageFormat: "Package name contains invalid character(s): {0}.",
        Description: "Package names must consist only of lowercase letters (a-z), " +
                     "digits (0-9), plus (+) and minus (-) signs, and periods (.).",
        HelpLink: new Uri("https://www.debian.org/doc/debian-policy/ch-controlfields.html#source"));

    private static partial ParsingException CreateParsingException(
        ReadOnlySpan<char> identifierSpan,
        ImmutableList<ParsingAnnotation> annotations)
    {
        return new DpkgPackageNameParsingException(identifierSpan.ToString(), annotations);
    }
}

public sealed class DpkgPackageNameParsingException : ParsingException
{
    public DpkgPackageNameParsingException(
        string value,
        ImmutableList<ParsingAnnotation> annotations)
        : base(message: $"Failed to parse dpkg package name '{value}'.", value: value, annotations: annotations)
    {
        HelpLink = "https://www.debian.org/doc/debian-policy/ch-controlfields.html#source";
    }
}
