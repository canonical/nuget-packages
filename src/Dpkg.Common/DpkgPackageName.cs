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
using System.Diagnostics.CodeAnalysis;
using Canonical.Common.Parsing;

namespace Canonical.Dpkg;

/// <summary>
/// Represents an immutable instance of the name of a debian package.
/// </summary>
public readonly partial struct DpkgPackageName :
    ISpanParsable<DpkgPackageName>,
    IEquatable<string>,
    IEquatable<DpkgPackageName>,
    IEquatable<DpkgPackageName?>,
    IComparable,
    IComparable<string>,
    IComparable<DpkgPackageName>,
    IComparable<DpkgPackageName?>
{
    internal DpkgPackageName(string identifier)
    {
        Identifier = identifier;
    }

    /// <summary>
    /// Gets the identifier of the debian package.
    /// </summary>
    public string Identifier { get; }

    /// <inheritdoc />
    public override int GetHashCode() => Identifier.GetHashCode();

    /// <inheritdoc />
    public override string ToString() => Identifier;

    public static implicit operator string(DpkgPackageName dpkgPackageName) => dpkgPackageName.Identifier;
    public static explicit operator DpkgPackageName(string value) => Parse(value);
    public static explicit operator DpkgPackageName(Span<char> value) => Parse(value);

    /// <summary>
    /// Parses a span of characters representing a debian package name and performs validation.
    /// </summary>
    /// <param name="packageNameSpan">The span of characters representing a debian package name.</param>
    /// <param name="failFast">
    /// <see langword="true"/> to abort parsing as soon as the first error gets detected;
    /// <see langword="false"/> to process the entire value of <paramref name="packageNameSpan"/>.
    /// </param>
    /// <returns>The parsed and validated dpkg package name.</returns>
    /// <exception cref="MalformedDpkgPackageNameException">
    /// When <paramref name="packageNameSpan"/> does not represent a valid dpkg package name.
    /// </exception>
    /// <seealso href="https://www.debian.org/doc/debian-policy/ch-controlfields.html#source"/>
    public static DpkgPackageName Parse(ReadOnlySpan<char> packageNameSpan, bool failFast = false)
    {
        return TryParse(packageNameSpan, out var packageName, out var annotations, failFast)
            ? packageName
            : throw new MalformedDpkgPackageNameException(packageNameSpan.ToString(), annotations);
    }

    /// <summary>
    /// Tries to parse a span of characters representing a debian package name and performs validation.
    /// </summary>
    /// <param name="packageNameSpan">The string representation of the debian package name.</param>
    /// <param name="packageName">Will contain the parsed and validated dpkg package name.</param>
    /// <returns><see langword="true"/> if <paramref name="packageNameSpan"/> was parsed successfully; otherwise <see langword="false"/>.</returns>
    /// <seealso href="https://www.debian.org/doc/debian-policy/ch-controlfields.html#source"/>
    public static bool TryParse(
        ReadOnlySpan<char> packageNameSpan,
        out DpkgPackageName packageName)
    {
        return TryParse(packageNameSpan, out packageName, out _, failFast: true);
    }

    /// <summary>
    /// Tries to parse a span of characters representing a debian package name and performs validation.
    /// </summary>
    /// <param name="packageNameSpan">The string representation of the debian package name.</param>
    /// <param name="packageName">Will contain the parsed and validated dpkg package name.</param>
    /// <param name="annotations">Will contain additional context regarding the parsed result.</param>
    /// <param name="failFast">
    /// <see langword="true"/> to abort parsing as soon as the first error gets detected.
    /// <paramref name="annotations"/> will be empty; <see langword="false"/> to process
    /// the entire value of <paramref name="packageNameSpan"/>.
    /// </param>
    /// <returns><see langword="true"/> if <paramref name="packageNameSpan"/> was parsed successfully; otherwise <see langword="false"/>.</returns>
    /// <seealso href="https://www.debian.org/doc/debian-policy/ch-controlfields.html#source"/>
    public static bool TryParse(
        ReadOnlySpan<char> packageNameSpan,
        out DpkgPackageName packageName,
        out ImmutableList<ParsingAnnotation> annotations,
        bool failFast = false)
    {
        annotations = ImmutableList<ParsingAnnotation>.Empty;

        if (packageNameSpan.Length < 2)
        {
            if (failFast) goto abort;
            annotations += ParsingAnnotation.Create(
                descriptor: PackageNameTooShort,
                location: packageNameSpan.GetRange());
        }

        if (packageNameSpan.Length > 0
            && !char.IsAsciiLetterLower(packageNameSpan[0])
            && !char.IsAsciiDigit(packageNameSpan[0]))
        {
            if (failFast) goto abort;
            annotations += ParsingAnnotation.Create(
                descriptor: InvalidStartCharacter,
                location: 0.AsIndexToRange(),
                messageArgs: packageNameSpan[0]);
        }

        List<char>? invalidCharacters = null;
        ImmutableList<Range>.Builder? invalidCharacterLocations = null;
        for (var position = 1; position < packageNameSpan.Length; ++position)
        {
            char currentCharacter = packageNameSpan[position];

            if (!char.IsAsciiLetterLower(currentCharacter)
                && !char.IsAsciiDigit(currentCharacter)
                && currentCharacter != '-'
                && currentCharacter != '.'
                && currentCharacter != '+')
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
        }

        if (!annotations.IsEmpty) goto abort;

        packageName = new DpkgPackageName(packageNameSpan.ToString());
        return true;
abort:
        packageName = default;
        return false;
    }

    #region ISpanParsable<DpkgPackageName>

    /// <inheritdoc />
    public static DpkgPackageName Parse(string? value, IFormatProvider? formatProvider)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Parse(value.AsSpan());
    }

    /// <inheritdoc />
    public static bool TryParse([NotNullWhen(true)] string? value, IFormatProvider? formatProvider, out DpkgPackageName result)
    {
        if (value is null)
        {
            result = default;
            return false;
        }

        return TryParse(value, out result, out _, failFast: true);
    }

    /// <inheritdoc />
    public static DpkgPackageName Parse(ReadOnlySpan<char> value, IFormatProvider? formatProvider)
    {
        return Parse(value);
    }

    /// <inheritdoc />
    public static bool TryParse(ReadOnlySpan<char> value, IFormatProvider? formatProvider, out DpkgPackageName result)
    {
        return TryParse(value, out result, out _, failFast: true);
    }

    #endregion

    #region IComparable

    public int CompareTo(object? other)
    {
        return other switch
        {
            null => 1,
            DpkgPackageName packageName => CompareTo(packageName.Identifier),
            string name => CompareTo(name),
            _ => CompareTo(other.ToString())
        };
    }

    public int CompareTo(DpkgPackageName other) => CompareTo(other.Identifier);

    public int CompareTo(DpkgPackageName? other) => other.HasValue ? CompareTo(other.Value.Identifier) : 1;

    public int CompareTo(string? other)
    {
        if (other is null) return 1;
        if (ReferenceEquals(Identifier, other)) return 0;

        for (int i = 0, j = 0; i < Identifier.Length || j < other.Length;)
        {
            while ((i < Identifier.Length && !char.IsAsciiDigit(Identifier[i])) ||
                   (j < other.Length && !char.IsAsciiDigit(other[j])))
            {
                int weight = Identifier[i].CompareTo(other[i]);

                if (weight != 0)
                    return weight;

                i++;
                j++;
            }

            // skip leading zeros;
            while (i < Identifier.Length && Identifier[i] == '0') i++;
            while (j < other.Length && other[j] == '0') j++;

            // stores the first numerical difference when comparing from left to right
            int mostSignificantNumericalDifference = 0;

            while (i < Identifier.Length && char.IsAsciiDigit(Identifier[i]) &&
                   j < other.Length && char.IsAsciiDigit(other[j]))
            {
                if (mostSignificantNumericalDifference == 0)
                    mostSignificantNumericalDifference = Identifier[i] - other[j];

                ++i;
                ++j;
            }

            // A has more digits than B, therefore A is larger
            if (i < Identifier.Length && char.IsAsciiDigit(Identifier[i]))
                return 1;

            // A has more digits than A, therefore B is larger
            if (j < other.Length && char.IsAsciiDigit(other[j]))
                return -1;

            if (mostSignificantNumericalDifference != 0)
                return mostSignificantNumericalDifference;
        }

        return Identifier.CompareTo(other, StringComparison.Ordinal);
    }

    #endregion

    #region IEquatable

    public override bool Equals([NotNullWhen(returnValue: true)] object? other)
    {
        return other switch
        {
            null => false,
            DpkgPackageName packageName => Equals(packageName),
            string packageName => Equals(packageName),
            _ => Equals(other.ToString()),
        };
    }

    public bool Equals([NotNullWhen(returnValue: true)] string? other)
    {
        return string.Equals(Identifier, other, StringComparison.Ordinal);
    }

    public bool Equals(DpkgPackageName other) => Equals(Identifier, other.Identifier);

    public bool Equals([NotNullWhen(returnValue: true)] DpkgPackageName? other) => other.HasValue && Equals(other.Value);

    #endregion

    #region operators ==, !=

    public static bool operator ==(DpkgPackageName a, DpkgPackageName b) => a.Equals(b);
    public static bool operator !=(DpkgPackageName a, DpkgPackageName b) => !a.Equals(b);

    /// <summary>
    /// Indicates whether the two <see cref="DpkgPackageName"/> instances are equal to each other.
    /// </summary>
    /// <param name="a">The first <see cref="DpkgPackageName"/> instance to compare with <paramref name="b"/>.</param>
    /// <param name="b">The second <see cref="DpkgPackageName"/> instance to compare with <paramref name="a"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="a"/> is equal to <paramref name="b"/>;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public static bool operator ==(DpkgPackageName? a, DpkgPackageName? b)
    {
        if (a is null) return b is null;
        return a.Value.Equals(b);
    }

    /// <summary>
    /// Indicates whether the two <see cref="DpkgPackageName"/> instances are not equal to each other.
    /// </summary>
    /// <param name="a">The first <see cref="DpkgPackageName"/> instance to compare with <paramref name="b"/>.</param>
    /// <param name="b">The second <see cref="DpkgPackageName"/> instance to compare with <paramref name="a"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="a"/> is not equal to <paramref name="b"/>;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public static bool operator !=(DpkgPackageName? a, DpkgPackageName? b)
    {
        if (a is null) return b is not null;
        return !a.Value.Equals(b);
    }

    #endregion

    // NOTE: Although the IComparable interfaces are implemented we do not want to implement the <, <=, >, >= operators.
    //       IComparable was implemented to allow methods like Array.Sort, Linq Order to sort the elements in a more
    //       practical and consistent way.
}
