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

public enum DpkgParsingErrorHandling
{
    /// <summary>
    /// Return the <see langword="default"/> value if the value that is parsed is invalid.
    /// </summary>
    ReturnDefault,

    /// <summary>
    /// Throw a <see cref="FormatException"/> when the first error is recognized.
    /// </summary>
    ThrowAtFirstError,

    /// <summary>
    /// Throw a <see cref="FormatException"/> when the entire value was processed.
    /// This may help in finding multiple subsequent errors, but can increase the amount of false negatives reported
    /// in the error.
    /// </summary>
    ThrowAfterProcessingAll,
}
