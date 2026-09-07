// This file is part of DpkgSharper
// Copyright (C) 2026 Dominik Viererbe <hello@dviererbe.de>
//
// This program is free software: you can redistribute it and/or modify it
// under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License
// for more details.
//
// You should have received a copy of the GNU Affero General Public License along
// with this program. If not, see <https://www.gnu.org/licenses/>.

namespace Canonical.DistroInfo.Ubuntu.Dotnet;

public readonly record struct DotnetDpkgVersionFormat
{
    private const byte SDK = 0b01;
    private const byte RUNTIME = 0b10;
    private const byte SDK_AND_RUNTIME = SDK | RUNTIME;

    private const byte FO127 = 0b100;
    private const byte LEGACY = 0b1000;
    private const byte TYPE = FO127 | LEGACY;

    public static DotnetDpkgVersionFormat Empty = new();

    // ReSharper disable once InconsistentNaming
    public static DotnetDpkgVersionFormat FO127SdkPackage = new(FO127 | SDK);

    // ReSharper disable once InconsistentNaming
    public static DotnetDpkgVersionFormat FO127RuntimePackage = new(FO127 | RUNTIME);

    // ReSharper disable once InconsistentNaming
    public static DotnetDpkgVersionFormat FO127SdkAndRuntimePackage = new(FO127 | SDK_AND_RUNTIME);

    public static DotnetDpkgVersionFormat Legacy = new(LEGACY | SDK_AND_RUNTIME);

    public static DotnetDpkgVersionFormat MalformedDotnet9ReleaseSdkPackage = new(SDK);
    public static DotnetDpkgVersionFormat MalformedDotnet9ReleaseRuntimePackage = new(RUNTIME);

    private readonly byte _flags;

    public DotnetDpkgVersionFormat()
    {
        _flags = 0;
    }

    private DotnetDpkgVersionFormat(byte flags)
    {
        _flags = flags;
    }

    public bool DeclaresSdkVersion => (_flags & SDK) != 0;
    public bool DeclaresRuntimeVersion => (_flags & RUNTIME) != 0;

    public bool IsSdkPackage => (_flags & SDK_AND_RUNTIME) == SDK;
    public bool IsRuntimePackage => (_flags & SDK_AND_RUNTIME) == RUNTIME;
    public bool IsSdkAndRuntimePackage => (_flags & SDK_AND_RUNTIME) == SDK_AND_RUNTIME;

    // ReSharper disable once InconsistentNaming
    public bool IsFO127Compliant => (_flags & FO127) != 0;
    public bool IsLegacyVersion => (_flags & LEGACY) != 0;
    public bool IsMalformedDotnet9ReleaseVersion => (_flags & TYPE) == 0;
    public bool IsEmpty => _flags == 0;

    public override string ToString()
    {
        if (IsFO127Compliant)
        {
            if (IsSdkPackage) return ".NET SDK Package (FO127)";
            if (IsRuntimePackage) return ".NET Runtime Package (FO127)";
            return ".NET Package (FO127)";
        }
        if (IsLegacyVersion)
        {
            return ".NET Package (legacy)";
        }
        if (IsMalformedDotnet9ReleaseVersion)
        {
            if (IsRuntimePackage) return ".NET Runtime Package (malformed)";
            return ".NET SDK Package (malformed)";
        }

        return "Empty";
    }
}
