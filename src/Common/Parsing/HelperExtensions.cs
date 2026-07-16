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

namespace Canonical.Common.Parsing;

public static class HelperExtensions
{
    extension(ImmutableList<ParsingAnnotation>)
    {
        public static ImmutableList<ParsingAnnotation> operator +(ImmutableList<ParsingAnnotation> annotations, ParsingAnnotation annotation)
        {
            return annotations.Add(annotation);
        }

        public static ImmutableList<ParsingAnnotation> operator +(ImmutableList<ParsingAnnotation> a, ImmutableList<ParsingAnnotation> b)
        {
            if (a.IsEmpty) return b;
            if (b.IsEmpty) return a;
            return [..a, ..b];
        }
    }

    extension(IEnumerable<ParsingAnnotation> annotations)
    {
        public IEnumerable<ParsingAnnotation> OffsetLocations(int offset) =>
            annotations.Select(annotation => annotation with
            {
                Locations = [.. annotation.Locations.Select(location => location.Offset(offset))],
            });
    }

    extension(ReadOnlySpan<char> span)
    {
        public Location ToLocation() => new Location(start: 0, end: span.Length);
    }

    extension(IEnumerable<char> characters)
    {
        public string JoinAsCharacterLiteralList(string separator = ", ") =>
            string.Join(separator, characters.Select(c => $"'{c}'"));
    }
}
