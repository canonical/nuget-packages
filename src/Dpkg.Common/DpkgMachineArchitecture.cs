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
using Canonical.Common;
using Canonical.Common.Parsing;

namespace Canonical.Dpkg;

/// <summary>
/// Represents an immutable instance of the name of a Debian machine architecture specification string.
/// </summary>
/// <seealso href="https://www.debian.org/doc/debian-policy/ch-controlfields.html#architecture"/>
public readonly partial struct DpkgMachineArchitecture : IIdentifier<DpkgMachineArchitecture>
{
    public static bool TryParse(
        ReadOnlySpan<char> identifierSpan,
        out DpkgMachineArchitecture identifier,
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

        var match = SearchWellKnownIdentifier(identifierSpan);
        if (match.HasValue)
        {
            identifier = match.Value;
            return true;
        }

        List<char>? invalidCharacters = null;
        ImmutableList<Location>.Builder? invalidCharacterLocations = null;
        for (var position = 0; position < identifierSpan.Length; ++position)
        {
            char currentCharacter = identifierSpan[position];

            if (!char.IsAsciiLetterLower(currentCharacter)
                && !char.IsAsciiDigit(currentCharacter))
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
                invalidCharacterLocations!.Add(position);
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

    private static DpkgMachineArchitecture? SearchWellKnownIdentifier(ReadOnlySpan<char> identifierSpan)
    {
        Debug.Assert(identifierSpan.Length > 0);

        // first, try match against the most common identifiers
        switch (identifierSpan)
        {
            // I sorted them by my assumptions of probability, but the compiler will
            // probably use some optimizations (like checking string length) anyway.
            case "any": return DpkgMachineArchitectures.any;
            case "all": return DpkgMachineArchitectures.all;
            case "amd64": return DpkgMachineArchitectures.amd64;
            case "amd64v3": return DpkgMachineArchitectures.amd64v3;
            case "arm64": return DpkgMachineArchitectures.arm64;
            case "source": return DpkgMachineArchitectures.source;
            case "s390x": return DpkgMachineArchitectures.s390x;
            case "ppc64el": return DpkgMachineArchitectures.ppc64el;
            case "riscv64": return DpkgMachineArchitectures.riscv64;
            case "armhf": return DpkgMachineArchitectures.armhf;
            case "i386": return DpkgMachineArchitectures.i386;
            case "armel": return DpkgMachineArchitectures.armel;
            case "mips64el": return DpkgMachineArchitectures.mips64el;
        }

        // then binary search for an existing entry in DpkgMachineArchitectures.WellKnown
        int lowIndex = 0;
        int highIndex = DpkgMachineArchitectures.WellKnown.Count - 1;
        while (lowIndex <= highIndex)
        {
            int midIndex = lowIndex + (highIndex - lowIndex) / 2;

            var mid = DpkgMachineArchitectures.WellKnown[midIndex];
            var weight = mid.CompareTo(identifierSpan);

            if (weight == 0) return mid;

            if (weight < 0)
                lowIndex = midIndex + 1;
            else
                highIndex = midIndex - 1;
        }

        return null;
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
        MessageFormat: "Machine architecture string contains invalid character(s): {0}.",
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
