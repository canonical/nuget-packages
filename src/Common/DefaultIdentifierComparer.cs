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

namespace Canonical.Common;

public sealed class IdentifierComparer :
    IComparer<string>,
    IComparer<ReadOnlySpan<char>>,
    IComparer<IIdentifier>,
    IEqualityComparer<string>,
    IEqualityComparer<ReadOnlySpan<char>>,
    IEqualityComparer<IIdentifier>
{
    public static readonly IdentifierComparer Default = new();

    public int GetHashCode(ReadOnlySpan<char> id) => id.GetHashCode();
    public int GetHashCode(string id) => id.GetHashCode();
    public int GetHashCode(IIdentifier id) => id.Identifier.GetHashCode();

    public bool Equals(ReadOnlySpan<char> a, ReadOnlySpan<char> b) => a.Equals(b, StringComparison.Ordinal);
    public bool Equals(string? a, string? b) => string.Equals(a, b, StringComparison.Ordinal);
    public bool Equals(IIdentifier? x, IIdentifier? y) =>
        x is null == y is null &&
        (x is null || string.Equals(x.Identifier, y!.Identifier, StringComparison.Ordinal));

    public int Compare(ReadOnlySpan<char> x, ReadOnlySpan<char> y)
    {
        for (int i = 0, j = 0; i < x.Length || j < y.Length;)
        {
            while ((i < x.Length && !char.IsAsciiDigit(x[i])) ||
                   (j < y.Length && !char.IsAsciiDigit(y[j])))
            {
                int weight = GetCharacterWeight(x, i) - GetCharacterWeight(y, j);
                if (weight != 0) return weight;

                i++;
                j++;
            }

            // skip leading zeros;
            while (i < x.Length && x[i] == '0') i++;
            while (j < y.Length && y[j] == '0') j++;

            // stores the first numerical difference when comparing from left to right
            int mostSignificantNumericalDifference = 0;

            while (i < x.Length && char.IsAsciiDigit(x[i]) &&
                   j < y.Length && char.IsAsciiDigit(y[j]))
            {
                if (mostSignificantNumericalDifference == 0)
                    mostSignificantNumericalDifference = x[i] - y[j];

                ++i;
                ++j;
            }

            // X has more digits than Y, therefore X is larger
            if (i < x.Length && char.IsAsciiDigit(x[i]))
                return 1;

            // Y has more digits than X, therefore Y is larger
            if (j < y.Length && char.IsAsciiDigit(y[j]))
                return -1;

            if (mostSignificantNumericalDifference != 0)
                return mostSignificantNumericalDifference;
        }

        return x.CompareTo(y, StringComparison.Ordinal);

        static int GetCharacterWeight(ReadOnlySpan<char> value, int index)
        {
            if (index >= value.Length) return 0;

            char character = value[index];

            if (char.IsAsciiDigit(character))
                return 0;
            if (char.IsAsciiLetter(character))
                return character;

            return character + 256;
        }
    }

    public int Compare(string? x, string? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x is null) return y is null ? 0 : -1;
        if (y is null) return 1;

        return Compare(x.AsSpan(), y.AsSpan());
    }

    public int Compare(IIdentifier? x, IIdentifier? y)
    {
        if (x is null) return y is null ? 0 : -1;
        if (y is null) return 1;

        return Compare(x.Identifier, y.Identifier);
    }
}

public sealed class IdentifierComparer<T> :
    IComparer<T>,
    IEqualityComparer<T>,
    IComparer<T?>,
    IEqualityComparer<T?>,
    IAlternateEqualityComparer<ReadOnlySpan<char>, T>,
    IAlternateEqualityComparer<string?, T>,
    IAlternateEqualityComparer<T?, T>,
    IAlternateEqualityComparer<ReadOnlySpan<char>, T?>,
    IAlternateEqualityComparer<string?, T?>
    where T : struct, IIdentifier<T>
{
    public static readonly IdentifierComparer<T> Default = new();

    public int GetHashCode(T obj) => obj.Identifier.GetHashCode();

    int IEqualityComparer<T?>.GetHashCode([DisallowNull] T? obj)
    {
        return obj.HasValue
            ? obj.Value.Identifier.GetHashCode()
            : throw new ArgumentNullException(nameof(obj));
    }

    public int GetHashCode(T? obj) =>
        obj.HasValue
        ? obj.Value.Identifier.GetHashCode()
        : 0;

    public int GetHashCode(ReadOnlySpan<char> alternate) => string.GetHashCode(alternate);

    public int GetHashCode(string? alternate) =>
        alternate is not null
        ? alternate.GetHashCode()
        : 0;

    public bool Equals(T x, T y) =>
        ReferenceEquals(x.Identifier, y.Identifier) ||
        string.Equals(x.Identifier, y.Identifier, StringComparison.Ordinal);

    public bool Equals(T? x, T? y) =>
        x.HasValue == y.HasValue &&
        (!x.HasValue || string.Equals(x.Value.Identifier, y!.Value.Identifier, StringComparison.Ordinal));

    public bool Equals(T? alternate, T other) =>
        alternate.HasValue &&
        string.Equals(alternate.Value.Identifier, other.Identifier, StringComparison.Ordinal);

    public bool Equals(T alternate, T? other) =>
        other.HasValue &&
        string.Equals(alternate.Identifier, other.Value.Identifier, StringComparison.Ordinal);

    public bool Equals(ReadOnlySpan<char> alternate, T other) =>
        alternate.Equals(other.Identifier, StringComparison.Ordinal);

    public bool Equals(string? alternate, T other) =>
        alternate is not null &&
        string.Equals(alternate, other.Identifier, StringComparison.Ordinal);

    public bool Equals(ReadOnlySpan<char> alternate, T? other) =>
        other.HasValue && alternate.Equals(other.Value.Identifier, StringComparison.Ordinal);

    public bool Equals(string? alternate, T? other) =>
        alternate is not null == other.HasValue &&
        (alternate is null || string.Equals(alternate, other!.Value.Identifier, StringComparison.Ordinal));

    public int Compare(T x, T y) =>
        IdentifierComparer.Default.Compare(x.Identifier, y.Identifier);

    public int Compare(T? x, T? y)
    {
        if (!x.HasValue) return y.HasValue ? -1 : 0;
        if (!y.HasValue) return 1;

        return IdentifierComparer.Default.Compare(x.Value.Identifier, y.Value.Identifier);
    }

    public T Create(T? alternate) =>
        alternate ?? throw new ArgumentNullException(nameof(alternate));

    public T Create(ReadOnlySpan<char> alternate) => T.Parse(alternate);

    public T Create(string? alternate) =>
        alternate is not null
        ? T.Parse(alternate)
        : throw new ArgumentNullException(nameof(alternate));

    public T? Create(T alternate) => alternate;

    T? IAlternateEqualityComparer<string?, T?>.Create(string? alternate) =>
        alternate is not null
        ? T.Parse(alternate)
        : null;

    T? IAlternateEqualityComparer<ReadOnlySpan<char>, T?>.Create(ReadOnlySpan<char> alternate) => T.Parse(alternate);
}
