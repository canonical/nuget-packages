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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Canonical.DistroInfo.Static.SourceGeneration;

internal static class HelperExtensions
{
    public static string AsDateOnlyLiteral(this string? value)
    {
        if (string.IsNullOrEmpty(value)) return "null";

        var date = DateTime.Parse(value);
        return $"new DateOnly(year: {date.Year}, month: {date.Month}, day: {date.Day})";
    }

    public static Location GetLocation(this AdditionalText file) => Location.Create(
        file.Path,
        textSpan: new TextSpan(0, 0),
        lineSpan: new LinePositionSpan(
            new LinePosition(0, 0),
            new LinePosition(0, 0)));
}
