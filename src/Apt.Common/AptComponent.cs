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

public readonly partial struct AptComponent : IIdentifier<AptComponent>
{
    public static bool TryParse(
        ReadOnlySpan<char> identifierSpan,
        out AptComponent identifier,
        out ImmutableList<ParsingAnnotation> annotations,
        bool failEarly = false)
    {
        annotations = ImmutableList<ParsingAnnotation>.Empty;

        if (identifierSpan.Length == 0)
        {
            if (failEarly) goto abort;
            annotations += ParsingAnnotation.Create(
                descriptor: EmptyName,
                location: identifierSpan.ToLocation());
            goto abort;
        }

        if (!char.IsAsciiLetterLower(identifierSpan[0]))
        {
            if (failEarly) goto abort;
            annotations += ParsingAnnotation.Create(
                descriptor: InvalidStartCharacter,
                location: 0,
                messageArgs: identifierSpan[0]);
        }

        if (!char.IsAsciiLetterLower(identifierSpan[^1]))
        {
            if (failEarly) goto abort;
            annotations += ParsingAnnotation.Create(
                descriptor: InvalidEndCharacter,
                location: identifierSpan.Length - 1,
                messageArgs: identifierSpan[0]);
        }

        List<char>? invalidCharacters = null;
        ImmutableList<Location>.Builder? invalidCharacterLocations = null;
        for (var position = 0; position < identifierSpan.Length; ++position)
        {
            char currentCharacter = identifierSpan[position];

            if (!char.IsAsciiLetterLower(currentCharacter)
                && currentCharacter != '-')
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
        }

        if (!annotations.IsEmpty) goto abort;

        identifier = new AptComponent(identifierSpan.ToString());
        return true;
abort:
        identifier = new AptComponent();
        return false;
    }

    private static readonly ParsingAnnotationDescriptor EmptyName = new (
        Identifier: "APT-COMPONENT-001",
        Title: "Empty name",
        MessageFormat: "Component name is empty.");

    private static readonly ParsingAnnotationDescriptor InvalidStartCharacter = new (
        Identifier: "APT-COMPONENT-002",
        Title: "Invalid start character",
        MessageFormat: "Component name can not start with the character '{0}'.",
        Description: "Component names must start with a lowercase letter (a-z).");

    private static readonly ParsingAnnotationDescriptor InvalidCharacters = new(
        Identifier: "DPKG-COMPONENT-003",
        Title: "Invalid characters",
        MessageFormat: "Component name contains invalid character(s): {0}.",
        Description: "Component names must consist only of lowercase letters (a-z), " +
                     "and minus (-) signs.");

    private static readonly ParsingAnnotationDescriptor InvalidEndCharacter = new (
        Identifier: "APT-COMPONENT-004",
        Title: "Invalid end character",
        MessageFormat: "Component name can not end with the character '{0}'.",
        Description: "Component names must end with a lowercase letter (a-z).");

    private static partial ParsingException CreateParsingException(
        ReadOnlySpan<char> identifierSpan,
        ImmutableList<ParsingAnnotation> annotations)
    {
        return new AptPocketParsingException(identifierSpan.ToString(), annotations);
    }
}

public sealed class AptComponentParsingException : ParsingException
{
    public AptComponentParsingException(
        string value,
        ImmutableList<ParsingAnnotation> annotations)
        : base(message: $"Failed to parse apt component name '{value}'.", value: value, annotations: annotations)
    {

    }
}

