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

namespace Canonical.Dpkg;

public partial class DpkgVersion
{
    /// <inheritdoc cref="Object.Equals(object?)"/>
    public override bool Equals(object? other) => other switch
    {
        DpkgVersion debVersion => Equals(debVersion),
        _ => false
    };

    /// <inheritdoc cref="IEquatable{T}.Equals(T?)"/>
    public bool Equals(DpkgVersion? other) => CompareTo(other) == 0;

    /// <summary>
    /// Indicates whether the two <see cref="DpkgVersion"/> instances are equal to each other.
    /// </summary>
    /// <param name="a">The first <see cref="DpkgVersion"/> instance to compare with <paramref name="b"/>.</param>
    /// <param name="b">The second <see cref="DpkgVersion"/> instance to compare with <paramref name="a"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="a"/> is equal to <paramref name="b"/>;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public static bool operator ==(DpkgVersion? a, DpkgVersion? b)
    {
        if (a is null) return b is null;
        return a.Equals(b);
    }

    /// <summary>
    /// Indicates whether the two <see cref="DpkgVersion"/> instances are not equal to each other.
    /// </summary>
    /// <param name="a">The first <see cref="DpkgVersion"/> instance to compare with <paramref name="b"/>.</param>
    /// <param name="b">The second <see cref="DpkgVersion"/> instance to compare with <paramref name="a"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="a"/> is not equal to <paramref name="b"/>;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public static bool operator !=(DpkgVersion? a, DpkgVersion? b)
    {
        if (a is null) return b is not null;
        return !a.Equals(b);
    }

    /// <inheritdoc cref="IComparable.CompareTo(object?)" />
    public int CompareTo(object? other) => other switch
    {
        null => 1,
        DpkgVersion debVersion => CompareTo(debVersion),
        _ => throw new ArgumentException(
            paramName: nameof(other),
            message: $"Can't compare type {other.GetType().FullName} with type {typeof(DpkgVersion).FullName}.")
    };

    /// <inheritdoc cref="IComparable{T}.CompareTo(T?)" />
    /// <remarks>
    /// Based on <see href="https://git.launchpad.net/ubuntu/+source/dpkg/tree/lib/dpkg/version.c?id=1c6190706e06d4f577e378a7aeaa81f506cb49c6#n140">dpkg(1) implementation</see>.
    /// </remarks>
    public int CompareTo(DpkgVersion? other)
    {
        if (other is null) return 1;

        if (EpochValue > other.EpochValue) return 1;
        if (EpochValue < other.EpochValue) return -1;

        int weight = ComparePart(UpstreamVersion, other.UpstreamVersion);
        if (weight != 0) return weight;

        return ComparePart(Revision, other.Revision);

        static int ComparePart(string? a, string? b)
        {
            a ??= string.Empty;
            b ??= string.Empty;

            for (int i = 0, j = 0; i < a.Length || j < b.Length;)
            {
                while ((i < a.Length && !char.IsAsciiDigit(a[i])) ||
                       (j < b.Length && !char.IsAsciiDigit(b[j])))
                {
                    int weightA = GetCharacterWeight(a, i);
                    int weightB = GetCharacterWeight(b, j);

                    if (weightA != weightB)
                        return weightA - weightB;

                    i++;
                    j++;
                }

                // skip leading zeros;
                while (i < a.Length && a[i] == '0') i++;
                while (j < b.Length && b[j] == '0') j++;

                // stores the first numerical difference when comparing from left to right
                int mostSignificantNumericalDifference = 0;

                while (i < a.Length && char.IsAsciiDigit(a[i]) &&
                       j < b.Length && char.IsAsciiDigit(b[j]))
                {
                    if (mostSignificantNumericalDifference == 0)
                        mostSignificantNumericalDifference = a[i] - b[j];

                    ++i;
                    ++j;
                }

                // A has more digits than B, therefore A is larger
                if (i < a.Length && char.IsAsciiDigit(a[i]))
                    return 1;

                // A has more digits than A, therefore B is larger
                if (j < b.Length && char.IsAsciiDigit(b[j]))
                    return -1;

                if (mostSignificantNumericalDifference != 0)
                    return mostSignificantNumericalDifference;
            }

            return 0;
        }

        static int GetCharacterWeight(string value, int index)
        {
            if (index >= value.Length) return 0;

            char character = value[index];

            if (char.IsAsciiDigit(character))
                return 0;
            if (char.IsAsciiLetter(character))
                return character;
            if (character == '~')
                return -1;

            return character + 256;
        }
    }

    public static bool operator >(DpkgVersion? a, DpkgVersion? b)
    {
        if (a is null) return false;
        return a.CompareTo(b) > 0;
    }

    public static bool operator >=(DpkgVersion? a, DpkgVersion? b)
    {
        if (a is null) return b is null;
        return a.CompareTo(b) >= 0;
    }

    public static bool operator <(DpkgVersion? a, DpkgVersion? b)
    {
        if (a is null) return b is not null;
        return a.CompareTo(b) < 0;
    }

    public static bool operator <=(DpkgVersion? a, DpkgVersion? b)
    {
        if (a is null) return true;
        return a.CompareTo(b) <= 0;
    }
}
