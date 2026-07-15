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

using Canonical.Common;

namespace Canonical.Dpkg.UnitTests;

public class IdentifierComparerTests
{
    [Fact]
    public void Compare_StringNulls_OrdersNullBeforeNonNull()
    {
        var comparer = IdentifierComparer.Default;

        Assert.Equal(expected: 0, actual: comparer.Compare(null as string, null as string));
        Assert.True(comparer.Compare(null, "dotnet8") < 0);
        Assert.True(comparer.Compare("dotnet8", null) > 0);
    }

    [Fact]
    public void Compare_IdentifierNulls_OrdersNullBeforeNonNull()
    {
        var comparer = IdentifierComparer.Default;
        IIdentifier? nullIdentifier = null;
        IIdentifier identifier = DpkgPackageName.Parse("dotnet8");

        Assert.Equal(expected: 0, actual: comparer.Compare(nullIdentifier, nullIdentifier));
        Assert.True(comparer.Compare(nullIdentifier, identifier) < 0);
        Assert.True(comparer.Compare(identifier, nullIdentifier) > 0);
    }

    [Fact]
    public void Compare_NullableIdentifierNulls_OrdersNullBeforeNonNull()
    {
        var comparer = IdentifierComparer<DpkgPackageName>.Default;
        DpkgPackageName? nullName = null;
        DpkgPackageName? name = DpkgPackageName.Parse("dotnet8");

        Assert.Equal(expected: 0, actual: comparer.Compare(nullName, nullName));
        Assert.True(comparer.Compare(nullName, name) < 0);
        Assert.True(comparer.Compare(name, nullName) > 0);
    }

    [Fact]
    public void Compare_Strings_UsesIdentifierOrdering()
    {
        var comparer = IdentifierComparer.Default;

        Assert.True(comparer.Compare("dotnet8", "dotnet09") < 0);
        Assert.True(comparer.Compare("dotnet09", "dotnet9") < 0);
        Assert.True(comparer.Compare("dotnet10", "dotnet9") > 0);
    }

    [Fact]
    public void Compare_Identifiers_UsesIdentifierOrdering()
    {
        var comparer = IdentifierComparer<DpkgPackageName>.Default;

        var dotnet8 = DpkgPackageName.Parse("dotnet8");
        var dotnet09 = DpkgPackageName.Parse("dotnet09");
        var dotnet9 = DpkgPackageName.Parse("dotnet9");
        var dotnet10 = DpkgPackageName.Parse("dotnet10");

        Assert.True(comparer.Compare(dotnet8, dotnet09) < 0);
        Assert.True(comparer.Compare(dotnet09, dotnet9) < 0);
        Assert.True(comparer.Compare(dotnet10, dotnet9) > 0);
    }
}
