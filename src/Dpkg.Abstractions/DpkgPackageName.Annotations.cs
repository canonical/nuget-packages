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
using Canonical.Common.Parsing;

namespace Canonical.Dpkg;

public readonly partial record struct DpkgPackageName
{
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

    private static readonly ParsingAnnotationDescriptor InvalidCharacter = new(
        Identifier: "DPKG-NAME-003",
        Title: "Invalid character",
        MessageFormat: "Package name contains invalid characters: {0}.",
        Description: "Package names must consist only of lowercase letters (a-z), " +
                     "digits (0-9), plus (+) and minus (-) signs, and periods (.).",
        HelpLink: new Uri("https://www.debian.org/doc/debian-policy/ch-controlfields.html#source"));
}

public class MalformedDpkgPackageNameException : ParsingException
{
    public MalformedDpkgPackageNameException(
        string value,
        ImmutableList<ParsingAnnotation> annotations)
        : base(message: $"Failed to parse dpkg package name '{value}'.", value: value, annotations: annotations)
    {
    }
}
