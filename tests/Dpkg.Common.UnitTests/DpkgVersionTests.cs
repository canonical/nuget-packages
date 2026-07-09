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

using Xunit.Abstractions;

namespace Canonical.Dpkg.UnitTests;

public class DpkgVersionTests
{
    private const string SortedDpkgVersionsFile = "valid-dpkg-versions.txt";

    private readonly ITestOutputHelper _testOutput;

    public DpkgVersionTests(ITestOutputHelper output)
    {
        _testOutput = output;
    }

    [Fact]
    public void ToString_OfEmptyDpkgVersion_Returns_EmptyString()
    {
        Assert.Equal(expected: string.Empty, actual: DpkgVersion.Empty.ToString());
        Assert.Null(DpkgVersion.Empty.Epoch);
        Assert.Equal(expected: 0u, actual: DpkgVersion.Empty.EpochValue);
        Assert.Equal(expected: string.Empty, actual: DpkgVersion.Empty.UpstreamVersion);
        Assert.Equal(expected: string.Empty, actual: DpkgVersion.Empty.EffectiveUpstreamVersion);
        Assert.Null(DpkgVersion.Empty.RevertedUpstreamVersion);
        Assert.Null(DpkgVersion.Empty.RealUpstreamVersion);
        Assert.Null(DpkgVersion.Empty.Revision);
        Assert.Null(DpkgVersion.Empty.DebianRevision);
        Assert.Null(DpkgVersion.Empty.UbuntuRevision);
    }

    [Fact]
    public void Parsing_EmptyString_Works()
    {
        Parse(string.Empty);
    }

    [Theory]
    [InlineData("a:1")]
    public void Parsing_InvalidVersionString_Fails(string invalidVersionString)
    {
        Assert.False(DpkgVersion.TryParse(invalidVersionString, formatProvider: null, out _));
    }

    [Theory]
    [InlineData("1", null, 0u, "1", "1", null, null, null, null, null)]
    [InlineData("1-1", null, 0u, "1", "1", null, null, "1", "1", null)]
    [InlineData("1-1ubuntu1", null, 0u, "1", "1", null, null, "1ubuntu1", "1", "1")]
    [InlineData("1-1-1ubuntu1", null, 0u, "1-1", "1-1", null, null, "1ubuntu1", "1", "1")]
    [InlineData("2+really1-1ubuntu1", null, 0u, "2+really1", "1", "2", "1", "1ubuntu1", "1", "1")]
    [InlineData("1:1", "1", 1u, "1", "1", null, null, null, null, null)]
    [InlineData("1:1-1", "1", 1u, "1", "1", null, null, "1", "1", null)]
    [InlineData("1:1-1ubuntu1", "1", 1u, "1", "1", null, null, "1ubuntu1", "1", "1")]
    [InlineData("1:1-1-1ubuntu1", "1", 1u, "1-1", "1-1", null, null, "1ubuntu1", "1", "1")]
    [InlineData("1:1:1-1:1:-1-1ubuntu1", "1", 1u, "1:1-1:1:-1", "1:1-1:1:-1", null, null, "1ubuntu1", "1", "1")]
    [InlineData("1:2+really1-1ubuntu1", "1", 1u, "2+really1", "1", "2", "1", "1ubuntu1", "1", "1")]
    [InlineData("001:1", "001", 1u, "1", "1", null, null, null, null, null)]
    [InlineData("9999:1", "9999", 9999u, "1", "1", null, null, null, null, null)]
    [InlineData("0009999:1", "0009999", 9999u, "1", "1", null, null, null, null, null)]
    [InlineData("999.999.999~preview999.999-999~prefix999ubuntu999~postfix999", null, 0u, "999.999.999~preview999.999", "999.999.999~preview999.999", null, null, "999~prefix999ubuntu999~postfix999", "999~prefix999", "999~postfix999")]
    [InlineData("1:8.0.104-8.0.4-1", "1", 1u, "8.0.104-8.0.4", "8.0.104-8.0.4", null, null, "1", "1", null)]
    [InlineData("1:6.0.129", "1", 1u, "6.0.129", "6.0.129", null, null, null, null, null)]
    [InlineData("1:8.0.104-8.0.4-0ubuntu1~23.10.1", "1", 1u, "8.0.104-8.0.4", "8.0.104-8.0.4", null, null, "0ubuntu1~23.10.1", "0", "1~23.10.1")]
    [InlineData("1:6.0.129-0ubuntu1~22.04.1", "1", 1u, "6.0.129", "6.0.129", null, null, "0ubuntu1~22.04.1", "0", "1~22.04.1")]
    [InlineData("6.0.129-0ubuntu1~22.04.1", null, 0u, "6.0.129", "6.0.129", null, null, "0ubuntu1~22.04.1", "0", "1~22.04.1")]
    [InlineData("8.0.104-8.0.4-0ubuntu1~23.10.1", null, 0u, "8.0.104-8.0.4", "8.0.104-8.0.4", null, null, "0ubuntu1~23.10.1", "0", "1~23.10.1")]
    [InlineData("8.0.104", null, 0u, "8.0.104", "8.0.104", null, null, null, null, null)]
    [InlineData("8.0.104+8.0.4", null, 0u, "8.0.104+8.0.4", "8.0.104+8.0.4", null, null, null, null, null)]
    [InlineData("1:999.999.999~preview999.999", "1", 1u, "999.999.999~preview999.999", "999.999.999~preview999.999", null, null, null, null, null)]
    [InlineData("1:999.999.999~preview999.999-999~prefix999ubuntu999~postfix999", "1", 1u, "999.999.999~preview999.999", "999.999.999~preview999.999", null, null, "999~prefix999ubuntu999~postfix999", "999~prefix999", "999~postfix999")]
    [InlineData("999.999.999~preview999.999", null, 0u, "999.999.999~preview999.999", "999.999.999~preview999.999", null, null, null, null, null)]
    [InlineData("8.0.100-8.0.0~rc1+really7.0.100-7.0.0~beta1~bootstrap+amd64-0ubuntu1", null, 0u, "8.0.100-8.0.0~rc1+really7.0.100-7.0.0~beta1~bootstrap+amd64", "7.0.100-7.0.0~beta1~bootstrap+amd64", "8.0.100-8.0.0~rc1", "7.0.100-7.0.0~beta1~bootstrap+amd64", "0ubuntu1", "0", "1")]
    [InlineData("8.0.100-8.0.0~rc1-0ubuntu1~test1ubuntu2ubuntu3", null, 0u, "8.0.100-8.0.0~rc1", "8.0.100-8.0.0~rc1", null, null, "0ubuntu1~test1ubuntu2ubuntu3", "0", "1~test1ubuntu2ubuntu3")]
    public void Parsing_ValidVersionString_MatchesExpectedValues(
        string validVersionString,
        string? expectedEpoch,
        uint expectedEpochValue,
        string expectedUpstreamVersion,
        string expectedEffectiveUpstreamVersion,
        string? expectedRevertedUpstreamVersion,
        string? expectedRealUpstreamVersion,
        string? expectedRevision,
        string? expectedDebianRevision,
        string? expectedUbuntuRevision)
    {
        var dpkgVersion = Parse(validVersionString);

        Assert.Equal(expected: expectedEpoch, actual: dpkgVersion.Epoch);
        Assert.Equal(expected: expectedEpochValue, actual: dpkgVersion.EpochValue);
        Assert.Equal(expected: expectedUpstreamVersion, actual: dpkgVersion.UpstreamVersion);
        Assert.Equal(expected: expectedEffectiveUpstreamVersion, actual: dpkgVersion.EffectiveUpstreamVersion);
        Assert.Equal(expected: expectedRevertedUpstreamVersion, actual: dpkgVersion.RevertedUpstreamVersion);
        Assert.Equal(expected: expectedRealUpstreamVersion, actual: dpkgVersion.RealUpstreamVersion);
        Assert.Equal(expected: expectedRevision, actual: dpkgVersion.Revision);
        Assert.Equal(expected: expectedDebianRevision, actual: dpkgVersion.DebianRevision);
        Assert.Equal(expected: expectedUbuntuRevision, actual: dpkgVersion.UbuntuRevision);
        Assert.Equal(expected: validVersionString, actual: dpkgVersion.ToString());
    }

    [Theory]
    [InlineData("0", "0")]
    [InlineData("00", "0")]
    [InlineData("0:0", "0")]
    public void VersionA_Equals_VersionB(string a, string b)
    {
        var alpha = Parse(a);
        var beta = Parse(b);

        Assert.Equal(expected: 0, actual: alpha.CompareTo((object?)beta));
        Assert.Equal(expected: 0, actual: alpha.CompareTo(beta));
        Assert.Equal(expected: 0, actual: beta.CompareTo((object?)alpha));
        Assert.Equal(expected: 0, actual: beta.CompareTo(alpha));

        Assert.True(alpha.Equals(beta));
        Assert.True(beta.Equals(alpha));
        Assert.True(alpha == beta);
        Assert.True(beta == alpha);
        Assert.False(alpha != beta);
        Assert.False(beta != alpha);

        Assert.False(alpha > beta);
        Assert.False(beta > alpha);
        Assert.True(alpha >= beta);
        Assert.True(beta >= alpha);

        Assert.False(alpha < beta);
        Assert.False(beta < alpha);
        Assert.True(alpha <= beta);
        Assert.True(beta <= alpha);
    }

    [Theory]
    [InlineData("2", "1")]
    [InlineData("1-2", "1-1")]
    [InlineData("1:1", "2")]
    public void VersionA_IsGreaterThan_VersionB(string a, string b)
    {
        var alpha = Parse(a);
        var beta = Parse(b);

        Assert.Equal(expected: 1, actual: alpha.CompareTo((object?)beta));
        Assert.Equal(expected: 1, actual: alpha.CompareTo(beta));
        Assert.Equal(expected: -1, actual: beta.CompareTo((object?)alpha));
        Assert.Equal(expected: -1, actual: beta.CompareTo(alpha));

        Assert.False(alpha.Equals(beta));
        Assert.False(beta.Equals(alpha));
        Assert.False(alpha == beta);
        Assert.False(beta == alpha);
        Assert.True(alpha != beta);
        Assert.True(beta != alpha);

        Assert.False(alpha < beta);
        Assert.True(beta < alpha);
        Assert.False(alpha <= beta);
        Assert.True(beta <= alpha);

        Assert.True(alpha > beta);
        Assert.False(beta > alpha);
        Assert.True(alpha >= beta);
        Assert.False(beta >= alpha);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1")]
    [InlineData("1:1")]
    [InlineData("1-1")]
    [InlineData("1:1-1")]
    [InlineData("1-1ubuntu1")]
    [InlineData("1:1-1ubuntu1")]
    public void Null_IsLessThan_AnyDpkgVersion(string version)
    {
        var dpkgVersion = Parse(version);
        Assert.True(null < dpkgVersion);

        Assert.Equal(expected: 1, actual: dpkgVersion.CompareTo((object?)null));
        Assert.Equal(expected: 1, actual: dpkgVersion.CompareTo((string?)null));
        Assert.Equal(expected: 1, actual: dpkgVersion.CompareTo((DpkgVersion?)null));

        Assert.False(dpkgVersion.Equals((object?)null));
        Assert.False(dpkgVersion.Equals((string?)null));
        Assert.False(dpkgVersion.Equals((DpkgVersion?)null));
        Assert.False(dpkgVersion == null);
        Assert.True(dpkgVersion != null);

        Assert.False(dpkgVersion < null);
        Assert.False(dpkgVersion <= null);
        Assert.True(dpkgVersion > null);
        Assert.True(dpkgVersion >= null);
    }

    [Fact]
    public void Sort()
    {
        var versions = new[] { Parse("2"), null, Parse("1"), Parse("3"), null };

        Array.Sort(versions);

        Assert.Null(versions[0]);
        Assert.Null(versions[1]);
        Assert.Equal("1", versions[2]?.ToString());
        Assert.Equal("2", versions[3]?.ToString());
        Assert.Equal("3", versions[4]?.ToString());
    }

    #region Regression tests

    [Fact]
    public void CompareTo_WithDifferentLeadingZeroCounts_OrdersCorrectly()
    {
        var alpha = Parse("01");
        var beta = Parse("1y");

        Assert.True(alpha < beta);
        Assert.True(beta > alpha);
    }

    // Equal versions should have the same hash code.
    // Violating the Equals/GetHashCode contract breaks HashSet/Dictionary keys.
    [Theory]
    [InlineData("00", "0")]
    [InlineData("0:0", "0")]
    [InlineData("01:1", "1:1")]
    public void GetHashCode_ForEqualVersions_IsConsistentWithEquals(string a, string b)
    {
        var alpha = Parse(a);
        var beta = Parse(b);

        Assert.Equal(alpha, beta);
        Assert.Equal(alpha.GetHashCode(), beta.GetHashCode());
    }

    [Fact]
    public void TryParse_ForInvalidRevisionCharacter_ReportsAtSingleCorrectLocation()
    {
        var success = DpkgVersion.TryParse("1-2_3".AsSpan(), out _, out var annotations);

        Assert.False(success);
        var annotation = Assert.Single(annotations);
        Assert.Equal("DPKG-VERSION-011", annotation.Identifier);
        Assert.Equal(3..4, Assert.Single(annotation.Locations));
    }

    [Fact]
    public void TryParse_ForInvalidEpochCharacter_ReportsAtSingleCorrectLocation()
    {
        var success = DpkgVersion.TryParse("a:1".AsSpan(), out _, out var annotations);

        Assert.False(success);
        var annotation = Assert.Single(annotations);
        Assert.Equal("DPKG-VERSION-003", annotation.Identifier);
        Assert.Equal(0..1, Assert.Single(annotation.Locations));
    }

    [Fact]
    public void TryParse_WithEpochExceedingMaxValue_IsRejected()
    {
        var success = DpkgVersion.TryParse("5000000000:1".AsSpan(), out _, out var annotations);

        Assert.False(success);
        Assert.Contains(annotations, annotation => annotation.Identifier == "DPKG-VERSION-004");
    }

    [Fact]
    public void CompareTo_DocumentationStringExample_Works()
    {
        var version = DpkgVersion.Parse("1");

        Assert.Equal(0, version.CompareTo(DpkgVersion.Parse("01")));
        Assert.Equal(0, version.CompareTo("01"));
        Assert.Equal(0, version.CompareTo(1));
        Assert.Equal(1, version.CompareTo(null as object));
        Assert.Equal(1, version.CompareTo(null as string));
        Assert.Equal(1, version.CompareTo(null as DpkgVersion));
    }

    [Fact]
    public void Sort_WithRandomizedVersionsOrder_MaintainsOriginalOrder()
    {
        // at time of writing SortedDpkgVersionsFile has 21647 entries
        // this gives some buffer is case the list grows
        var versions = new List<(int OriginalIndex, DpkgVersion Version)>(capacity: 23000);

        foreach (var versionString in File.ReadLines(SortedDpkgVersionsFile))
        {
            versions.Add((versions.Count, Parse(versionString)));
        }

        var random = new Random(Seed: 42); // Use fixed seed for reproducibility

        // Fisher–Yates shuffle (https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle)
        for (int i = versions.Count - 1; i > 0; i--)
        {
            int j = random.Next(maxValue: i + 1); // max value is exclusive
            (versions[i], versions[j]) = (versions[j], versions[i]);
        }

        var orderedVersions = versions
            .OrderBy(entry => entry.Version)
            .ThenBy(entry => entry.Version.ToString());

        string shuffledAndSortedFileName = Path.GetFileNameWithoutExtension(SortedDpkgVersionsFile) + ".shuffled-and-sorted.txt";
        using var file = new FileStream(shuffledAndSortedFileName, FileMode.Create, FileAccess.Write);
        using var writer = new StreamWriter(file);

        int index = 0;
        bool containsError = false;
        foreach (var entry in orderedVersions)
        {
            if (entry.OriginalIndex != index)
            {
                _testOutput.WriteLine($"'{entry.Version}' found at index {index} but expected at {entry.OriginalIndex}");
                containsError = true;
            }

            writer.WriteLine(entry.Version.ToString());
            ++index;
        }

        Assert.False(containsError, $"Shuffling and sorting the version list resulted in a different order. Compare {SortedDpkgVersionsFile} with {shuffledAndSortedFileName} to see the difference.");
    }

    #endregion

    private static DpkgVersion Parse(string version)
    {
        return DpkgVersion.Parse(version, failFast: false);
    }
}
