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
using System.Diagnostics.Contracts;
using Canonical.Common.Parsing;

namespace Canonical.Common;

public interface IIdentifier :
    IComparable,
    IComparable<string>,
    IEquatable<string>,
    IComparable<ReadOnlySpan<char>>,
    IEquatable<ReadOnlySpan<char>>
{
    [Pure]
    string Identifier { get; }

    [Pure]
    void Deconstruct(out string identifier);
}

public interface IIdentifier<TSelf> :
    IIdentifier,
    IComparable<TSelf?>,
    IEquatable<TSelf?>,
    ISpanParsable<TSelf>
    where TSelf : struct, IIdentifier<TSelf>
{
    [Pure]
    static abstract TSelf Parse(ReadOnlySpan<char> identifierSpan, bool failEarly = false);

    [Pure]
    static abstract TSelf Parse(ReadOnlySpan<char> identifierSpan, out ImmutableList<ParsingAnnotation> annotations, bool failEarly = false);

    [Pure]
    static abstract bool TryParse(ReadOnlySpan<char> identifierSpan, out TSelf result);

    [Pure]
    static abstract bool TryParse(ReadOnlySpan<char> identifierSpan, out TSelf result, out ImmutableList<ParsingAnnotation> annotations, bool failEarly = false);
}
