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

namespace Canonical.Dpkg;

public partial class DpkgVersion
{
    #region IEquatable

    /// <inheritdoc cref="Object.Equals(object?)"/>
    public override bool Equals([NotNullWhen(returnValue: true)] object? other) => other switch
    {
        null => false,
        DpkgVersion debVersion => Equals(debVersion),
        string stringVersion => Equals(stringVersion),
        _ => Equals(other.ToString())
    };

    /// <inheritdoc cref="IEquatable{T}.Equals(T?)"/>
    public bool Equals([NotNullWhen(returnValue: true)] string? other) => CompareTo(other) == 0;

    /// <inheritdoc cref="IEquatable{T}.Equals(T?)"/>
    public bool Equals([NotNullWhen(returnValue: true)] DpkgVersion? other) => CompareTo(other) == 0;

    #endregion

    #region IComparable

    /// <summary>
    /// Compares the current instance with the string representation of another object and returns
    /// an integer that indicates whether the current instance precedes, follows, or occurs in the
    /// same position in the sort order as the other object.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>
    /// A value that indicates the relative order of the objects being compared.
    /// The return value has these meanings:
    ///
    /// <list type="table">
    /// <listheader><term> Value</term><description> Meaning</description></listheader>
    /// <item><term> Less than zero</term><description> This instance precedes <paramref name="other" /> in the sort order.</description></item>
    /// <item><term> Zero</term><description> This instance occurs in the same position in the sort order as <paramref name="other" />.</description></item>
    /// <item><term> Greater than zero</term><description> This instance follows <paramref name="other" /> in the sort order.</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="MalformedDpkgVersionException">When the string representation of <paramref name="other"/> is not parsable.</exception>
    /// <remarks>
    /// The comparison algorithm that is a port of the
    /// <see href="https://git.launchpad.net/ubuntu/+source/dpkg/tree/lib/dpkg/version.c?id=1c6190706e06d4f577e378a7aeaa81f506cb49c6#n140">dpkg(1) implementation</see>.
    /// <see langword="null"/> always returns <c>1</c>.
    /// </remarks>
    /// <example>
    /// <code>
    /// var version = DpkgVersion.Parse("1");
    /// version.CompareTo(DpkgVersion.Parse("01")); // = 0
    /// version.CompareTo("01"); // = 0
    /// version.CompareTo(1); // = 0
    /// version.CompareTo(null as object); // = 1
    /// version.CompareTo(null as string); // = 1
    /// version.CompareTo(null as DpkgVersion); // = 1
    /// </code>
    /// </example>
    public int CompareTo(object? other) => other switch
    {
        null => 1,
        DpkgVersion debVersion => CompareTo(debVersion),
        string stringVersion => CompareTo(stringVersion),
        _ => CompareTo(other.ToString())
    };

    /// <inheritdoc cref="CompareTo(object?)" />
    public int CompareTo(string? other)
    {
        if (other is null) return 1;
        if (ReferenceEquals(_originalString , other)) return 0;
        var version = Parse(other, out _, failEarly: true);
        return CompareTo(version);
    }

    /// <inheritdoc cref="CompareTo(object?)" />
    public int CompareTo(DpkgVersion? other)
    {
        if (other is null) return 1;
        if (ReferenceEquals(this, other) || ReferenceEquals(_originalString, other._originalString)) return 0;

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

    #endregion

    #region operators ==, !=

    /// <summary>
    /// Indicates whether the logical value of two <see cref="DpkgVersion"/> instances are equal to each other.
    /// </summary>
    /// <param name="a">The first <see cref="DpkgVersion"/> instance to compare with <paramref name="b"/>.</param>
    /// <param name="b">The second <see cref="DpkgVersion"/> instance to compare with <paramref name="a"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the logical value of <paramref name="a"/> is equal to the logical value of
    /// <paramref name="b"/>; otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// "logical value" refers to the interpretation by <see cref="DpkgVersion.Equals(DpkgVersion?)"/>
    /// e.g. <c>"0.1" == "0.01"</c>.
    /// </remarks>
    public static bool operator ==(DpkgVersion? a, DpkgVersion? b)
    {
        if (a is null) return b is null;
        return a.Equals(b);
    }

    /// <summary>
    /// Indicates whether the logical value of two <see cref="DpkgVersion"/> instances are not equal to each other.
    /// </summary>
    /// <param name="a">The first <see cref="DpkgVersion"/> instance to compare with <paramref name="b"/>.</param>
    /// <param name="b">The second <see cref="DpkgVersion"/> instance to compare with <paramref name="a"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the logical value of <paramref name="a"/> is not equal to the logical value of
    /// <paramref name="b"/>; otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// "logical value" refers to the interpretation by <see cref="DpkgVersion.Equals(DpkgVersion?)"/>
    /// e.g. <c>"0.1" == "0.01"</c>.
    /// </remarks>
    public static bool operator !=(DpkgVersion? a, DpkgVersion? b)
    {
        if (a is null) return b is not null;
        return !a.Equals(b);
    }

    #endregion

    #region operators >, >=, <, <=

    /// <summary>
    /// Indicates whether the logical value of one <see cref="DpkgVersion"/> instance is greater than another.
    /// </summary>
    /// <param name="a">The first <see cref="DpkgVersion"/> instance to compare with <paramref name="b"/>.</param>
    /// <param name="b">The second <see cref="DpkgVersion"/> instance to compare with <paramref name="a"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the logical value of <paramref name="a"/> is greater than the logical value of
    /// <paramref name="b"/>; otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// "logical value" refers to the interpretation by <see cref="DpkgVersion.CompareTo(DpkgVersion?)"/>
    /// e.g. <c>"0.010" &gt; "0.1"</c>.
    /// </remarks>
    public static bool operator >(DpkgVersion? a, DpkgVersion? b)
    {
        if (a is null) return false;
        return a.CompareTo(b) > 0;
    }

    /// <summary>
    /// Indicates whether the logical value of one <see cref="DpkgVersion"/> instance is greater than or equal to another.
    /// </summary>
    /// <param name="a">The first <see cref="DpkgVersion"/> instance to compare with <paramref name="b"/>.</param>
    /// <param name="b">The second <see cref="DpkgVersion"/> instance to compare with <paramref name="a"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the logical value of <paramref name="a"/> is greater than or equal to the logical value of
    /// <paramref name="b"/>; otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// "logical value" refers to the interpretation by <see cref="DpkgVersion.CompareTo(DpkgVersion?)"/>
    /// e.g. <c>"0.010" &gt;= "0.10"</c>.
    /// </remarks>
    public static bool operator >=(DpkgVersion? a, DpkgVersion? b)
    {
        if (a is null) return b is null;
        return a.CompareTo(b) >= 0;
    }

    /// <summary>
    /// Indicates whether the logical value of one <see cref="DpkgVersion"/> instance is less than another.
    /// </summary>
    /// <param name="a">The first <see cref="DpkgVersion"/> instance to compare with <paramref name="b"/>.</param>
    /// <param name="b">The second <see cref="DpkgVersion"/> instance to compare with <paramref name="a"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the logical value of <paramref name="a"/> is less than the logical value of
    /// <paramref name="b"/>; otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// "logical value" refers to the interpretation by <see cref="DpkgVersion.CompareTo(DpkgVersion?)"/>
    /// e.g. <c>"0.1" &lt; "0.010"</c>.
    /// </remarks>
    public static bool operator <(DpkgVersion? a, DpkgVersion? b)
    {
        if (a is null) return b is not null;
        return a.CompareTo(b) < 0;
    }

    /// <summary>
    /// Indicates whether the logical value of one <see cref="DpkgVersion"/> instance is less than or equal to another.
    /// </summary>
    /// <param name="a">The first <see cref="DpkgVersion"/> instance to compare with <paramref name="b"/>.</param>
    /// <param name="b">The second <see cref="DpkgVersion"/> instance to compare with <paramref name="a"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the logical value of <paramref name="a"/> is less than or equal to the logical value of
    /// <paramref name="b"/>; otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// "logical value" refers to the interpretation by <see cref="DpkgVersion.CompareTo(DpkgVersion?)"/>
    /// e.g. <c>"0.10" &lt;= "0.010"</c>.
    /// </remarks>
    public static bool operator <=(DpkgVersion? a, DpkgVersion? b)
    {
        if (a is null) return true;
        return a.CompareTo(b) <= 0;
    }

    #endregion
}
