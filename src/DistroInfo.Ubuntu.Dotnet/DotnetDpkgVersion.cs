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
using System.Text;
using Canonical.Dpkg;

namespace Canonical.DistroInfo.Ubuntu.Dotnet;

public sealed partial class DotnetDpkgVersion : DpkgVersion
{
    public const int DEFAULT_FEATURE_BAND = 100;

    /// <summary>
    /// An <see cref="DpkgVersion"/> instance with an empty string representation.
    /// </summary>
    public new static readonly DotnetDpkgVersion Empty = new DotnetDpkgVersion(
        originalString: string.Empty,
        epoch: null,
        epochValue: DEFAULT_EPOCH_VALUE,
        upstreamVersion: string.Empty,
        revertedUpstreamVersion: null,
        realUpstreamVersion: null,
        revision: null,
        debianRevision: null,
        ubuntuRevision: null,
        format: DotnetDpkgVersionFormat.Empty,
        runtimeVersion: null,
        sdkVersion: null,
        preReleaseVersion: null,
        buildSuffix: null,
        bootstrapArchitecture: null);

    private DotnetDpkgVersion(
        string originalString,
        string? epoch,
        uint epochValue,
        string upstreamVersion,
        string? revertedUpstreamVersion,
        string? realUpstreamVersion,
        string? revision,
        string? debianRevision,
        string? ubuntuRevision,
        DotnetDpkgVersionFormat format,
        DotnetRuntimeVersion? runtimeVersion,
        DotnetSdkVersion? sdkVersion,
        DotnetPreReleaseVersion? preReleaseVersion,
        DpkgMachineArchitecture? bootstrapArchitecture,
        uint? buildSuffix)
        : base(
            originalString,
            epoch,
            epochValue,
            upstreamVersion,
            revertedUpstreamVersion,
            realUpstreamVersion,
            revision,
            debianRevision,
            ubuntuRevision)
    {
        Format = format;
        RuntimeVersion = runtimeVersion;
        SdkVersion = sdkVersion;
        PreReleaseVersion = preReleaseVersion;
        BuildSuffix = buildSuffix;
        BootstrapArchitecture = bootstrapArchitecture;
    }

    public DotnetDpkgVersionFormat Format { get; }

    public DotnetRuntimeVersion? RuntimeVersion { get; }

    public DotnetSdkVersion? SdkVersion { get; }

    public DotnetPreReleaseVersion? PreReleaseVersion { get; }

    public uint? BuildSuffix { get; }

    public DpkgMachineArchitecture? BootstrapArchitecture { get; }

    public bool TryConvertTo(
        DotnetDpkgVersionFormat format,
        out DotnetDpkgVersion? version,
        bool allowDefaultSdkToRuntimeConversion = true,
        int? defaultFeatureBand = DEFAULT_FEATURE_BAND)
    {
        if (Format == format)
        {
            version = this;
            return true;
        }

        if (format.IsEmpty)
        {
            version = Empty;
            return true;
        }

        DotnetSdkVersion? sdkVersion;
        DotnetRuntimeVersion? runtimeVersion;

        if (format.DeclaresRuntimeVersion)
        {
            if (RuntimeVersion is not null)
            {
                runtimeVersion = RuntimeVersion;
            }
            else if (allowDefaultSdkToRuntimeConversion && SdkVersion is not null)
            {
                runtimeVersion = new DotnetRuntimeVersion(SdkVersion);
            }
            else
            {
                version = null;
                return false;
            }

            if (format.IsMalformedDotnet9ReleaseVersion
                && runtimeVersion is not { Major: 9, Minor: 0, Patch: 0 })
            {
                version = null;
                return false;
            }
        }
        else
        {
            runtimeVersion = null;
        }

        if (format.DeclaresSdkVersion)
        {
            if (SdkVersion is not null)
            {
                sdkVersion = SdkVersion;

                if (format.IsLegacyVersion
                    && runtimeVersion is not null
                    && sdkVersion.Patch != runtimeVersion.Patch)
                {
                    version = null;
                    return false;
                }
            }
            else if (defaultFeatureBand.HasValue
                     && RuntimeVersion is not null
                     && DotnetSdkVersion.IsValidFeatureBand(defaultFeatureBand.Value))
            {
                sdkVersion = new DotnetSdkVersion(RuntimeVersion, featureBand: defaultFeatureBand.Value);
            }
            else
            {
                version = null;
                return false;
            }

            if (format.IsMalformedDotnet9ReleaseVersion &&
                (runtimeVersion is not null || sdkVersion is not { Major: 9, Minor: 0, FeatureBand: 100, Patch: 0 }))
            {
                version = null;
                return false;
            }
        }
        else
        {
            sdkVersion = null;
        }

        var versionString = new StringBuilder();

        if (Epoch is not null)
        {
            versionString.Append(Epoch).Append(EPOCH_DELIMITER);
        }

        if (RevertedUpstreamVersion is not null)
        {
            versionString.Append(RevertedUpstreamVersion).Append(REAL_UPSTREAM_VERSION_DELIMITER);
        }

        if (format.IsLegacyVersion || format.IsSdkPackage)
        {
            versionString.Append(sdkVersion);
        }
        else if (format.IsRuntimePackage)
        {
            versionString.Append(runtimeVersion);
        }
        else
        {
            versionString.Append(sdkVersion).Append('-').Append(runtimeVersion);
        }

        if (PreReleaseVersion is not null)
        {
            if (format.IsMalformedDotnet9ReleaseVersion)
            {
                if (PreReleaseVersion is not
                    ({
                        Type: DotnetPreReleaseType.Preview,
                        Revision: >= 1 and <= 7,
                        Metadata: null,
                    }
                    or
                    {
                        Type: DotnetPreReleaseType.ReleaseCandidate,
                        Revision: 1 or 2,
                        Metadata: null,
                    }))
                {
                    version = null;
                    return false;
                }

                versionString.Append(PreReleaseVersion.ToUpstreamString());
            }
            else
            {
                versionString.Append(PreReleaseVersion.ToDpkgString());
            }
        }
        else if (format.IsMalformedDotnet9ReleaseVersion)
        {
            versionString.Append("-rtm");
        }

        if (BuildSuffix.HasValue)
        {
            versionString.Append("+build").Append(BuildSuffix.Value);
        }

        if (BootstrapArchitecture is { Identifier: var arch })
        {
            versionString.Append("~bootstrap+").Append(arch);
        }

        if (Revision is not null)
        {
            versionString.Append(REVISION_DELIMITER).Append(Revision);
        }

        return new DotnetDpkgVersion(
            originalString: versionString.ToString(),
            epoch: Epoch,
            epochValue: EpochValue,
            upstreamVersion:

            );

        fail:
        version = null;
        return false;
    }
}
