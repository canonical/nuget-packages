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
/// Represents an immutable instance of the name of a Debian machine architecture specification string.
/// </summary>
/// <seealso href="https://www.debian.org/doc/debian-policy/ch-controlfields.html#architecture"/>
public readonly partial struct DpkgMachineArchitecture : IIdentifier<DpkgMachineArchitecture>
{
    /// <summary>
    /// A special wildcard architecture specification string that matches all Debian machine architectures and is the most frequently used.
    /// </summary>
    /// <seealso href="https://www.debian.org/doc/debian-policy/ch-controlfields.html#architecture"/>
    public static readonly DpkgMachineArchitecture Any = new DpkgMachineArchitecture("any");

    /// <summary>
    /// A special architecture specification string that indicates a source package.
    /// </summary>
    /// <seealso href="https://www.debian.org/doc/debian-policy/ch-controlfields.html#architecture"/>
    public static readonly DpkgMachineArchitecture Source = new DpkgMachineArchitecture("source");

    /// <summary>
    /// A special architecture specification string that indicates an architecture-independent package.
    /// </summary>
    /// <seealso href="https://www.debian.org/doc/debian-policy/ch-controlfields.html#architecture"/>
    public static readonly DpkgMachineArchitecture All = new DpkgMachineArchitecture("all");

    public static bool TryParse(
        ReadOnlySpan<char> identifierSpan,
        out DpkgMachineArchitecture identifier,
        out ImmutableList<ParsingAnnotation> annotations,
        bool failFast = false)
    {
        annotations = ImmutableList<ParsingAnnotation>.Empty;

        if (identifierSpan.Length < 1)
        {
            if (failFast) goto abort;
            annotations += ParsingAnnotation.Create(
                descriptor: EmptyName,
                location: identifierSpan.GetRange());
            goto abort;
        }

        List<char>? invalidCharacters = null;
        ImmutableList<Range>.Builder? invalidCharacterLocations = null;
        for (var position = 0; position < identifierSpan.Length; ++position)
        {
            char currentCharacter = identifierSpan[position];

            if (!char.IsAsciiLetterLower(currentCharacter)
                && !char.IsAsciiDigit(currentCharacter))
            {
                if (failFast) goto abort;

                if (invalidCharacters is null)
                {
                    invalidCharacters = [ currentCharacter ];
                    invalidCharacterLocations = ImmutableList.CreateBuilder<Range>();
                }
                else if (!invalidCharacters.Contains(currentCharacter))
                {
                    invalidCharacters.Add(currentCharacter);
                }
                invalidCharacterLocations!.Add(position.AsIndexToRange());
            }
        }

        if (invalidCharacters is not null)
        {
            annotations += ParsingAnnotation.Create(InvalidCharacters,
                locations: invalidCharacterLocations!.ToImmutable(),
                messageArgs: invalidCharacters.JoinAsCharacterLiteralList());
            goto abort;
        }

        identifier = new DpkgMachineArchitecture(identifierSpan.ToString());
        return true;
abort:
        identifier = new DpkgMachineArchitecture();
        return false;
    }

    private static readonly ParsingAnnotationDescriptor EmptyName = new(
        Identifier: "DPKG-ARCH-001",
        Title: "Empty string",
        MessageFormat: "Machine architecture string is empty.",
        Description: "Debian machine architecture specification string can not be empty.",
        HelpLink: new Uri("https://www.debian.org/doc/debian-policy/ch-controlfields.html#architecture"));

    private static readonly ParsingAnnotationDescriptor InvalidCharacters = new(
        Identifier: "DPKG-ARCH-002",
        Title: "Invalid machine architecture string characters",
        MessageFormat: "Package name contains unusual character(s): {0}.",
        Description: "Debian machine architecture specification strings must consist only of " +
                     "lowercase letters (a-z) and digits (0-9).",
        HelpLink: new Uri("https://www.debian.org/doc/debian-policy/ch-controlfields.html#architecture"));

    private static partial ParsingException CreateParsingException(
        ReadOnlySpan<char> identifierSpan,
        ImmutableList<ParsingAnnotation> annotations)
    {
        return new DpkgMachineArchitectureParsingException(identifierSpan.ToString(), annotations);
    }
}

public sealed class DpkgMachineArchitectureParsingException : ParsingException
{
    public DpkgMachineArchitectureParsingException(
        string value,
        ImmutableList<ParsingAnnotation> annotations)
        : base(message: $"Failed to parse dpkg machine architecture name '{value}'.", value: value, annotations: annotations)
    {
        HelpLink = "https://www.debian.org/doc/debian-policy/ch-controlfields.html#architecture";
    }
}
