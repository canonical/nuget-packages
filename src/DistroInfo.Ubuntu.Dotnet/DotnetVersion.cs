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

namespace Canonical.DistroInfo.Ubuntu.Dotnet;

public sealed record DotnetRuntimeVersion : DotnetVersion
{
    public DotnetRuntimeVersion(DotnetVersion dotnetVersion) : base(
        major: dotnetVersion.Major,
        minor: dotnetVersion.Minor,
        patch: dotnetVersion.Patch)
    {
    }

    public DotnetRuntimeVersion(int major, int minor, int patch) : base(major, minor, patch)
    {
        ValidateMajor(major);
        ValidateMinor(minor);
        ValidatePatch(patch);
    }

    public override string ToString() => $"{Major}.{Minor}.{Patch}";
}

public sealed record DotnetSdkVersion : DotnetVersion
{
    public readonly int FeatureBand;

    public int Revision => FeatureBand + Patch;

    public DotnetSdkVersion(DotnetVersion dotnetVersion, int featureBand) : base(
        dotnetVersion.Major,
        dotnetVersion.Minor,
        dotnetVersion.Patch)
    {
        ValidateFeatureBand(featureBand);

        FeatureBand = featureBand;
    }

    public DotnetSdkVersion(int major, int minor, int featureBand, int patch) : base(major, minor, patch: patch)
    {
        ValidateMajor(major);
        ValidateMinor(minor);
        ValidatePatch(patch);
        ValidateFeatureBand(featureBand);

        FeatureBand = featureBand;
    }

    public DotnetSdkVersion(int major, int minor, int revision) : base(major, minor, patch: revision % 100)
    {
        ValidateMajor(major);
        ValidateMinor(minor);
        ValidateRevision(revision);

        FeatureBand = revision - Patch;
    }

    public static bool IsValidFeatureBand(int featureBand) =>
        featureBand is >= 100 and <= 900 && featureBand % 100 == 0;

    public static bool IsValidRevision(int revision) =>
        revision is >= 100 and <= 900;

    public static void ValidateFeatureBand(int featureBand)
    {
        if (IsValidFeatureBand(featureBand)) return;

        throw new ArgumentOutOfRangeException(
            paramName: nameof(featureBand),
            actualValue: featureBand,
            message: "The .NET SDK feature band has to be a multiple of 100 in the range of 100-900.");
    }

    public static void ValidateRevision(int revision)
    {
        if (IsValidRevision(revision)) return;

        throw new ArgumentOutOfRangeException(
            paramName: nameof(revision),
            actualValue: revision,
            message: "The .NET SDK revision has to be in the range of 100-900.");
    }

    public override string ToString() => $"{Major}.{Minor}.{Revision}";
}


public abstract record DotnetVersion
{
    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }

    protected internal DotnetVersion(int major, int minor, int patch)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
    }

    public static bool IsValidMajor(int major) => major >= 1;
    public static bool IsValidMinor(int minor) => minor >= 0;
    public static bool IsValidPatch(int patch) => patch is >= 0 and <= 99;

    public static void ValidateMajor(int major)
    {
        if (IsValidMajor(major)) return;

        throw new ArgumentOutOfRangeException(
            paramName: nameof(major),
            actualValue: major,
            message: "The major version of a .NET version has to be a positive integer.");
    }

    public static void ValidateMinor(int minor)
    {
        if (IsValidMinor(minor)) return;

        throw new ArgumentOutOfRangeException(
            paramName: nameof(minor),
            actualValue: minor,
            message: "The minor version of a .NET version has to be a zero or a positive integer.");
    }

    public static void ValidatePatch(int patch)
    {
        if (IsValidPatch(patch)) return;

        throw new ArgumentOutOfRangeException(
            paramName: nameof(patch),
            actualValue: patch,
            message: "The patch version of a .NET version has to be in the range of 0-99.");
    }
}
