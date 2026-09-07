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

using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using Canonical.Common.Parsing;
using Canonical.Dpkg;

namespace Canonical.DistroInfo.Ubuntu.Dotnet;

[Flags]
public enum DotnetDpkgVersionStyle
{
    /// <inheritdoc cref="DpkgVersionStyle.None"/>
    None = DpkgVersionStyle.None,

    /// <inheritdoc cref="DpkgVersionStyle.AllowEmpty"/>
    AllowEmpty = DpkgVersionStyle.AllowEmpty,

    /// <inheritdoc cref="DpkgVersionStyle.AllowEmptyRevertedUpstreamVersion"/>
    AllowEmptyRevertedUpstreamVersion = DpkgVersionStyle.AllowEmptyRevertedUpstreamVersion,

    /// <inheritdoc cref="DpkgVersionStyle.AllowEmptyRealUpstreamVersion"/>
    AllowEmptyRealUpstreamVersion = DpkgVersionStyle.AllowEmptyRealUpstreamVersion,

    /// <inheritdoc cref="DpkgVersionStyle.AllowMultipleRealUpstreamVersionDelimiter"/>
    AllowMultipleRealUpstreamVersionDelimiter = DpkgVersionStyle.AllowMultipleRealUpstreamVersionDelimiter,

    /// <inheritdoc cref="DpkgVersionStyle.AllowEmptyDebianRevision"/>
    AllowEmptyDebianRevision = DpkgVersionStyle.AllowEmptyDebianRevision,

    /// <inheritdoc cref="DpkgVersionStyle.AllowEmptyUbuntuRevision"/>
    AllowEmptyUbuntuRevision = DpkgVersionStyle.AllowEmptyUbuntuRevision,

    /// <inheritdoc cref="DpkgVersionStyle.AllowMultipleUbuntuRevisionDelimiters"/>
    AllowMultipleUbuntuRevisionDelimiters = DpkgVersionStyle.AllowMultipleUbuntuRevisionDelimiters,

    /// <summary>
    /// The version
    /// The <see cref="DotnetDpkgVersion.SdkVersion"/> part is not null.
    /// Only the first delimiter is used to split <see cref="DpkgVersion.DebianRevision"/>
    /// and <see cref="DpkgVersion.UbuntuRevision"/>.
    /// </summary>
    /// <remarks></remarks>
    AllowSdkOnlyVersions = 1 << 7,
    AllowRuntimeOnlyVersions = 1 << 8,
    AllowLegacyFormat = 1 << 9,
    AllowMalformedDotnet9ReleaseVersions = 1 << 10,

    AllowNonDotnetVersions = AllowEmptyRealUpstreamVersion,

    /// <inheritdoc cref="DpkgVersionStyle.Default"/>
    Default = None
              | AllowEmpty
              | AllowEmptyRevertedUpstreamVersion
              // | AllowEmptyRealUpstreamVersion
              | AllowMultipleRealUpstreamVersionDelimiter
              | AllowEmptyDebianRevision
              | AllowEmptyUbuntuRevision
              | AllowMultipleUbuntuRevisionDelimiters
              | AllowSdkOnlyVersions
              | AllowRuntimeOnlyVersions
              | AllowLegacyFormat
              | AllowMalformedDotnet9ReleaseVersions
}

partial class DotnetDpkgVersion : ISpanParsable<DotnetDpkgVersion>, ISpanParsable<DpkgVersion>
{
    #region IParsable<DotnetDpkgVersion>

    /// <inheritdoc/>
    public new static DotnetDpkgVersion Parse(string s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out DotnetDpkgVersion result)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region ISpanParsable<DotnetDpkgVersion>

    /// <inheritdoc/>
    public new static DotnetDpkgVersion Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public static bool TryParse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out DotnetDpkgVersion result)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region IParsable<DpkgVersion>

    /// <inheritdoc/>
    static DpkgVersion IParsable<DpkgVersion>.Parse(string s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public new static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out DpkgVersion result)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region ISpanParsable<DpkgVersion>

    /// <inheritdoc/>
    static DpkgVersion ISpanParsable<DpkgVersion>.Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public new static bool TryParse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out DpkgVersion result)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region DpkgVersion.Parsing

    public new static DpkgVersion Parse(
        ReadOnlySpan<char> versionSpan,
        DpkgVersionStyle style = DpkgVersionStyle.Default,
        bool failEarly = false)
    {
        return TryParse(versionSpan, out var version, out var annotations, style, failEarly)
            ? version
            : throw new MalformedDpkgVersionException(versionSpan.ToString(), annotations);
    }

    public new static DpkgVersion Parse(
        ReadOnlySpan<char> versionSpan,
        out ImmutableList<ParsingAnnotation> annotations,
        DpkgVersionStyle style = DpkgVersionStyle.Default,
        bool failEarly = false)
    {
        return TryParse(versionSpan, out var version, out annotations, style, failEarly)
            ? version
            : throw new MalformedDpkgVersionException(versionSpan.ToString(), annotations);
    }

    public new static bool TryParse(
        ReadOnlySpan<char> versionSpan,
        [NotNullWhen(returnValue: true)] out DpkgVersion? version,
        DpkgVersionStyle style = DpkgVersionStyle.Default)
    {
        // It does not make sense to failEarly: false, because the annotations always get discarded.
        return TryParse(versionSpan, out version, out _, style, failEarly: true);
    }

    public new static bool TryParse(
        ReadOnlySpan<char> versionSpan,
        [NotNullWhen(returnValue: true)] out DpkgVersion? version,
        out ImmutableList<ParsingAnnotation> annotations,
        DpkgVersionStyle style = DpkgVersionStyle.Default,
        bool failEarly = false)
    {
        throw new NotImplementedException();
    }

    #endregion

    public static bool TryParse(
        DpkgVersion dpkgVersion,
        [NotNullWhen(returnValue: true)] out DotnetDpkgVersion? dotnetVersion,
        out ImmutableList<ParsingAnnotation> annotations,
        DotnetDpkgVersionStyle style = DotnetDpkgVersionStyle.Default,
        bool failEarly = false)
    {
        annotations = ImmutableList<ParsingAnnotation>.Empty;

        if (dpkgVersion is DotnetDpkgVersion dotnetDpkgVersion)
        {
            dotnetVersion = dotnetDpkgVersion;
            return true;
        }

        if (TryParseUpstreamVersion(
            dpkgVersion.EffectiveUpstreamVersion,
            out var runtimeVersion,
            out var sdkVersion,
            out var preReleaseVersion,
            out var bootstrapArchitecture,
            out var buildSuffix,
            ref annotations,
            failEarly))
        {

        }
    }

    private static bool TryParseUpstreamVersion(
        string upstreamVersion,
        out DotnetDpkgVersionFormat format,
        out DotnetRuntimeVersion? runtimeVersion,
        out DotnetSdkVersion? sdkVersion,
        out DotnetPreReleaseVersion? preReleaseVersion,
        out DpkgMachineArchitecture? bootstrapArchitecture,
        out int? buildSuffix,
        scoped ref ImmutableList<ParsingAnnotation> annotations,
        bool failEarly)
    {
        var match = DotnetDpkgUpstreamVersionRegex().Match(upstreamVersion);
        if (!match.Success)
        {
            if (failEarly) goto fail;
            // TODO: add annotation
            goto fail;
        };

        bool isValid = true;
        int majorValue = int.Parse(match.Groups["Major"].ValueSpan);
        int minorValue = int.Parse(match.Groups["Minor"].ValueSpan);

        sdkVersion = match.Groups["Revision"] switch
        {
            { Success: false } => null,
            { Success: true, ValueSpan: var revisionSpan } =>
                new DotnetSdkVersion(
                    major: majorValue,
                    minor: minorValue,
                    revision: int.Parse(revisionSpan)),
        };

        runtimeVersion = match.Groups["Patch"] switch
        {
            { Success: false } => null,
            { Success: true, ValueSpan: var patchSpan } => new DotnetRuntimeVersion(
                major: majorValue,
                minor: minorValue,
                patch: int.Parse(patchSpan)),
        };

        if (majorValue < 8)
        {
            if (runtimeVersion is null)
            {
                // The regex pattern enforces that SDK version should not be null.
                runtimeVersion = new DotnetRuntimeVersion(sdkVersion!);
            }
            else
            {
                if (failEarly) goto fail;
                // TODO: add annotation
                isValid = false;
            }
        }

        preReleaseVersion = (match.Groups["PreReleaseType"], match.Groups["PreReleaseRevision"], match.Groups["PreReleaseMetadata"])
            switch
            {
                ({Success: false }, {Success: false}, {Success: false}) => null,
                ({Success: true, ValueSpan: var typeSpan}, {Success: true, ValueSpan: var revisionSpan }, var metadata) =>
                    new DotnetPreReleaseVersion()
                    {
                        Type = typeSpan switch
                        {
                            "rc" => DotnetPreReleaseType.ReleaseCandidate,
                            "preview" => DotnetPreReleaseType.Preview,
                            "beta" => DotnetPreReleaseType.Beta,
                            "alpha" => DotnetPreReleaseType.Alpha,
                            _ => throw new UnreachableException($"Unexpected .NET pre-release type '{typeSpan}'."),
                        },
                        Revision = int.Parse(revisionSpan),
                        Metadata = metadata switch
                        {
                            { Success: false } => null,
                            { Success: true } => metadata.Value,
                        },
                    },
                _ => throw new UnreachableException(".NET pre-release version was not parsed fully."),
            };

        if (match.Groups["BootstrapArch"] is { Success: true } bootstrapArchGroup)
        {
            if (bootstrapArchGroup.ValueSpan is "source" or "all" or "any")
            {
                if (failEarly) goto fail;
                // TODO: add annotation
                isValid = false;
                bootstrapArchitecture = null;
            }
            else
            {
                if (DpkgMachineArchitecture.TryParse(
                        bootstrapArchGroup.ValueSpan,
                        out var bootstrapArchValue,
                        out var archParseAnnotations,
                        failEarly))
                {
                    bootstrapArchitecture = bootstrapArchValue;
                }
                else
                {
                    if (failEarly) goto fail;
                    // TODO: add annotation
                    isValid = false;
                    bootstrapArchitecture = null;
                }

                if (archParseAnnotations.Count > 0)
                {
                    annotations += archParseAnnotations.OffsetLocations(offset: bootstrapArchGroup.Index);
                }
            }
        }
        else
        {
            bootstrapArchitecture = null;
        }

        if (match.Groups["BuildSuffix"] is { Success: true, ValueSpan: var buildSuffixSpan })
        {
            if (int.TryParse(buildSuffixSpan, out var buildSuffixValue))
            {
                buildSuffix = buildSuffixValue;
            }
            else
            {
                if (failEarly) goto fail;
                // TODO: add annotation
                isValid = false;
                buildSuffix = null;
            }
        }
        else
        {
            buildSuffix = null;
        }

        return isValid;

        fail:
        format = DotnetDpkgVersionFormat.Empty;
        sdkVersion = null;
        runtimeVersion = null;
        preReleaseVersion = null;
        bootstrapArchitecture = null;
        buildSuffix = null;
        return false;
    }

    private const string DOTNET_DPKG_UPSTREAM_VERSION_PATTERN = """
        \\A
        (
          (
            (?<Major>[1-9]\\d*)\\.(?<Minor>\\d+)\\.
            (
              (
                (
                  (?<Patch>\\d\\d?)
                  |(?<Revision>[1-9]\d\d)
                )
                (~
                  (?<PreReleaseType>rc|preview|beta|alpha)
                  (?<PreReleaseRevision>[1-9]\\d*)
                  (.(?<PreReleaseMetadata>[0-9a-z]+(\\.[0-9a-z]+)*))?
                )?
                (\\+build
                  (?<BuildSuffix>[1-9]\\d*)
                )?
              )
              |(
                (?<Revision>[1-9]\d\d)-\\k<Major>\\.\\k<Minor>\\.(?<Patch>\\d\\d?)
                (~
                  (?<PreReleaseType>rc|preview|beta|alpha)
                  (?<PreReleaseRevision>[1-9]\\d*)
                  (.(?<PreReleaseMetadata>[0-9a-z]+(\\.[0-9a-z]+)*))?
                )?
              )
            )
          )
          |(
            (?<Major>9)\\.(?<Minor>0)\\.((?<Revision>100)|(?<RuntimePatch>0))-
            (?<MalformedDotnet9ReleaseSuffix>
              rtm
              |(?<PreReleaseType>preview)\\.(?<PreReleaseRevision>[1-7])
              |(?<PreReleaseType>rc)\\.(?<PreReleaseRevision>[12])
            )
            (\\+build
              (?<BuildSuffix>[1-9]\\d*)
            )?
          )
        )
        (~bootstrap\\+
          (?<BootstrapArch>[0-9a-z]+)
        )?\\z
        """;

    [GeneratedRegex(
        pattern: DOTNET_DPKG_UPSTREAM_VERSION_PATTERN,
        options: RegexOptions.CultureInvariant
        | RegexOptions.ExplicitCapture
        | RegexOptions.IgnorePatternWhitespace
        | RegexOptions.NonBacktracking)]
    private static partial Regex DotnetDpkgUpstreamVersionRegex();
}

public sealed class DotnetDpkgVersionParsingException : ParsingException
{
    public DotnetDpkgVersionParsingException(
        string value,
        ImmutableList<ParsingAnnotation> annotations)
        : base(message: $"Failed to parse .NET dpkg version '{value}'.", value: value, annotations: annotations)
    {
        HelpLink = "https://github.com/canonical/dotnet/blob/main/specs/FO127.md";
    }
}
