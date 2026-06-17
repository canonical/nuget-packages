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

namespace Canonical.Common.Parsing;

public static class HelperExtensions
{
    extension(ImmutableList<ParsingAnnotation>)
    {
        public static ImmutableList<ParsingAnnotation> operator +(ImmutableList<ParsingAnnotation> annotations, ParsingAnnotation annotation)
        {
            return annotations.Add(annotation);
        }
    }

    /// <summary>
    /// Converts an index (represented by an <see langword="int"/>) to a <see cref="Range"/>.
    /// </summary>
    /// <param name="index">The index/position (represented by an <see langword="int"/>) within an array/span.</param>
    /// <returns>The range of the index.</returns>
    public static Range AsIndexToRange(this int index) => new(start: index, end: index + 1);

    extension(Range location)
    {
        public string ToLocationString()
        {
            if (location.Start.Equals(location.End) || location.End.Value == location.Start.Value + 1)
            {
                return location.Start.ToString();
            }

            return $"{location.Start.Value}..{location.End.Value}";
        }
    }

    extension(IEnumerable<Range> locations)
    {
        public IEnumerable<string> ToLocationStrings() => locations.Select(location => location.ToLocationString());
        public string JoinAsLocationStrings() => string.Join(", ", locations.ToLocationStrings());
    }

    extension(ReadOnlySpan<char> span)
    {
        public Range GetRange()
        {
            return new Range(start: 0, end: span.Length);
        }
    }

    extension(IEnumerable<char> characters)
    {
        public string JoinAsCharacterLiteralList() => string.Join(", ", characters.Select(c => $"'{c}'"));
    }
}
