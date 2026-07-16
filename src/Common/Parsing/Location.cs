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

using System.Diagnostics.CodeAnalysis;

namespace Canonical.Common.Parsing;

public readonly struct Location : IEquatable<Location>, IEquatable<Location?>
{
    private readonly int _start;
    private readonly int _end;

    public Location(int index)
    {
        if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), index, "Index can not be negative");
        _start = index;
        _end = index;
    }

    public Location(int start, int end)
    {
        if (start < 0)  throw new ArgumentOutOfRangeException(nameof(start), start, "start index can not be negative");
        if (end < 0)  throw new ArgumentOutOfRangeException(nameof(end), end, "start index can not be negative");
        if (end < start) throw new ArgumentException($"End index can not be less than start index (start: {start}, end: {end})");
        _start = start;
        _end = end;
    }

    public int Start => _start;
    public int End => _end;
    public int Length => _end - _start;
    public bool IsIndex => Length == 0;

    public override int GetHashCode() => HashCode.Combine(_start, _end);

    public override bool Equals([NotNullWhen(true)] object? obj) => obj switch
    {
        null => false,
        Location location => Equals(location),
        Range range => range.Equals(this),
        int index => IsIndex && _start == index,
        _ => throw new ArgumentException(
            paramName: nameof(obj),
            message: $"Can not compare type {obj.GetType().FullName} with {typeof(Location).FullName}."),
    };

    public bool Equals(Location location) => location._start == _start && location._end == _end;
    public bool Equals([NotNullWhen(true)] Location? location) => location.HasValue && Equals(location.Value);

    public override string ToString() => Length <= 1 ? _start.ToString() : $"{_start}..{_end}";

    public Location Offset(int offset)
    {
        if (offset < 0)
            throw new ArgumentOutOfRangeException(nameof(offset), offset, "Index offset can not be negative.");

        if (offset == 0) return this;
        return new Location(checked(_start + offset), checked(_end + offset));
    }

    public static implicit operator Range(Location location) => new(location._start, location._end);
    public static implicit operator Location(int index) => new Location(index);

    public static explicit operator Location(Range range)
    {
        if (range.Start.IsFromEnd)
        {
            throw new ArgumentOutOfRangeException(nameof(range.Start), range.Start,
                "Start Index value is from end and can therefore not be converted to the index from start.");
        }
        if (range.End.IsFromEnd)
        {
            throw new ArgumentOutOfRangeException(nameof(range.End), range.End,
                "End Index value is from end and can therefore not be converted to the index from start.");
        }

        return new Location(range.Start.Value, range.End.Value);
    }

    public static bool operator ==(Location a, Location b) => a._start == b._start && a._end == b._end;
    public static bool operator !=(Location a, Location b) => a._start != b._start || a._end != b._end;

    public static bool operator ==(Location? a, Location? b) => a.HasValue == b.HasValue && (!a.HasValue || a.Value == b!.Value);
    public static bool operator !=(Location? a, Location? b) => a.HasValue != b.HasValue || (a.HasValue && a.Value != b!.Value);
}
