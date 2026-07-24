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

namespace Canonical.Apt.UnitTests;

public class AptComponentTests
{
    [Theory]
    [InlineData("main")]
    [InlineData("non-free")]
    [InlineData("universe")]
    public void TryParse_WithValidName_ReturnsTrueAndIdentifier(string validComponentName)
    {
        var success = AptComponent.TryParse(validComponentName, out var result, out var annotations, failEarly: false);

        Assert.True(success);
        Assert.Equal(expected: validComponentName, actual: result.Identifier);
        Assert.Empty(annotations);
    }

    [Theory]
    [InlineData("")]
    [InlineData("-")]
    [InlineData("non-")]
    [InlineData("-free")]
    [InlineData("non free")]
    public void TryParse_WithInvalidName_ReturnsFalseAndEmptyIdentifier(string invalidComponentName)
    {
        var success = AptComponent.TryParse(invalidComponentName, out var result, out var annotations, failEarly: false);

        Assert.False(success);
        Assert.Equal(expected: string.Empty, actual: result.Identifier);
        Assert.NotEmpty(annotations);
    }
}
