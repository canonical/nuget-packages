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

using System.ComponentModel;

namespace Canonical.DistroInfo.Ubuntu.Dotnet;

public sealed record DotnetPreReleaseVersion : IFormattable
{
    public required DotnetPreReleaseType Type { get; init; }

    public required int Revision
    {
        get;
        init
        {
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(
                    paramName: nameof(Revision), value,
                    "The revision value of a .NET pre-release version has to be positive.");
            }

            field = value;
        }
    }

    public string? Metadata
    {
        get;
        init
        {
            field = value switch
            {
                null or "" => null,
                _ => value,
            };
        }
    }

    public override string ToString()
    {
        var type = Type.ToUpperString();
        var metadata = Metadata is null ? string.Empty : $" ({Metadata})";

        return $"{type} {Revision}{metadata}";
    }

    public string ToUpstreamString(char? prefix = '-')
    {
        var type = Type.ToLowerString();

        return Metadata is null
            ? $"{prefix}{type}.{Revision}"
            : $"{prefix}{type}.{Revision}.{Metadata}";
    }

    public string ToDpkgString(char? prefix = '~')
    {
        var type = Type.ToLowerString();

        return Metadata is null
            ? $"{prefix}{type}{Revision}"
            : $"{prefix}{type}{Revision}.{Metadata}";
    }

    public string ToString(string? format, IFormatProvider? formatProvider = null) =>
        format switch
        {
            null or "" or "G" => ToString(),
            "U" => ToUpstreamString(prefix: null),
            "u" => ToUpstreamString(),
            "D" => ToDpkgString(prefix: null),
            "d" => ToDpkgString(),
            _ => throw new FormatException($"Invalid format '{format}'.")
        };
}

public enum DotnetPreReleaseType
{
    Alpha,
    Beta,
    Preview,
    ReleaseCandidate,
}

public static class DotnetPreReleaseTypeExtensions
{
    extension(DotnetPreReleaseType preReleaseType)
    {
        public string ToUpperString() =>
            preReleaseType switch
            {
                DotnetPreReleaseType.ReleaseCandidate => "RC",
                DotnetPreReleaseType.Preview => "Preview",
                DotnetPreReleaseType.Beta => "Beta",
                DotnetPreReleaseType.Alpha => "Alpha",
                _ => throw new InvalidEnumArgumentException(
                    argumentName: nameof(preReleaseType),
                    invalidValue: (int)preReleaseType,
                    typeof(DotnetPreReleaseType))
            };

        public string ToLowerString() =>
            preReleaseType switch
            {
                DotnetPreReleaseType.ReleaseCandidate => "rc",
                DotnetPreReleaseType.Preview => "preview",
                DotnetPreReleaseType.Beta => "beta",
                DotnetPreReleaseType.Alpha => "alpha",
                _ => throw new InvalidEnumArgumentException(
                    argumentName: nameof(preReleaseType),
                    invalidValue: (int)preReleaseType,
                    typeof(DotnetPreReleaseType))
            };
    }
}
