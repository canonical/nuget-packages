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

namespace Canonical.DistroInfo.Ubuntu.Dotnet.UnitTests;

public class DotnetVersionTests
{
    private static ImmutableArray<int> InvalidMajorValues = [ -1, 0 ];
    private static ImmutableArray<int> InvalidMinorValues = [ -1 ];
    private static ImmutableArray<int> InvalidRevisionValues = [ -1, 0, 1, 98, 99, 1_000, 1_001, 10_000, 100_000 ];
    private static ImmutableArray<int> InvalidFeatureBandValues = [ -1, 0, 10, 11, 100, 1000];
    private static ImmutableArray<int> InvalidPatchValues = [-1, 100, 101, 1_000, 10_000];

    private static ImmutableArray<int> ValidMajorValues = [ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 , 11 ];
    private static ImmutableArray<int> ValidMinorValues = [ 0, 1 ];
    private static ImmutableArray<(int Revison, int FeatureBand, int Patch)> ValidRevisionValues =
    [
        ( Revison: 100, FeatureBand: 100, Patch: 0 ),
        ( Revison: 101, FeatureBand: 100, Patch: 1 ),
        ( Revison: 301, FeatureBand: 300, Patch: 1 ),
        ( Revison: 999, FeatureBand: 900, Patch: 99 ),
    ];

    [Fact]
    public void Constructor_WithInvalidMajor_ThrowsArgumentOutOfRangeException()
    {
        const string INVALID_PARAM_NAME = "major";

        foreach (var major in InvalidMajorValues)
        {
            foreach (var minor in ValidMinorValues.Concat(InvalidMinorValues) )
            {
                foreach (var (revision, featureBand, patch) in ValidRevisionValues)
                {
                    AssertThrowsArgumentOutOfRangeException(major, minor, revision, INVALID_PARAM_NAME);
                    AssertThrowsArgumentOutOfRangeException(major, minor, featureBand, patch, INVALID_PARAM_NAME);
                }

                foreach (var revision in InvalidRevisionValues)
                {
                    AssertThrowsArgumentOutOfRangeException(major, minor, revision, INVALID_PARAM_NAME);
                }

                foreach (var featureBand in InvalidFeatureBandValues)
                {
                    foreach (var patch in InvalidPatchValues)
                    {
                        AssertThrowsArgumentOutOfRangeException(major, minor, featureBand, patch, INVALID_PARAM_NAME);
                    }
                }
            }
        }
    }

    private void AssertThrowsArgumentOutOfRangeException(int major, int minor, int revision, string? invalidParamName = null)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DotnetSdkVersion(
            major,
            minor,
            revision));
    }

    private void AssertThrowsArgumentOutOfRangeException(int major, int minor, int featureBand, int patch, string? invalidParamName = null)
    {
        Assert.Throws<ArgumentOutOfRangeException>(paramName: invalidParamName, () => new DotnetSdkVersion(
            major,
            minor,
            featureBand,
            patch));
    }
}
