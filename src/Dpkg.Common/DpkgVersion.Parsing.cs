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
using System.Diagnostics.CodeAnalysis;
using Canonical.Common.Parsing;

namespace Canonical.Dpkg;

[Flags]
public enum DpkgVersionOptions
{
    None = 0,

    /// <summary>
    /// An empty version string will be interpreted as <see cref="DpkgVersion.Empty"/>.
    /// </summary>
    EmptyVersion = 1 << 0,

    /// <summary>
    /// A version string where <see cref="DpkgVersion.UbuntuRevision"/> contains a
    /// <see cref="DpkgVersion.REAL_UPSTREAM_VERSION_DELIMITER"/> can have an empty
    /// <see cref="DpkgVersion.RevertedUpstreamVersion"/> (value before the delimiter).
    /// </summary>
    EmptyRevertedUpstreamVersion = 1 << 1,

    /// <summary>
    /// A version string where <see cref="DpkgVersion.UbuntuRevision"/> contains a
    /// <see cref="DpkgVersion.REAL_UPSTREAM_VERSION_DELIMITER"/> can have an empty
    /// <see cref="DpkgVersion.RealUpstreamVersion"/> (value after the delimiter).
    /// </summary>
    EmptyRealUpstreamVersion = 1 << 2,

    /// <summary>
    /// The <see cref="DpkgVersion.UpstreamVersion"/> part of a version string can contain
    /// multiple <see cref="DpkgVersion.REAL_UPSTREAM_VERSION_DELIMITER"/>.
    /// Only the first delimiter is used to split <see cref="DpkgVersion.RealUpstreamVersion"/>
    /// and <see cref="DpkgVersion.RevertedUpstreamVersion"/>.
    /// </summary>
    MultipleRealUpstreamVersionDelimiter = 1 << 3,

    /// <summary>
    /// A version string where <see cref="DpkgVersion.Revision"/> contains a
    /// <see cref="DpkgVersion.UBUNTU_REVISION_DELIMITER"/> can have an empty
    /// <see cref="DpkgVersion.DebianRevision"/> (value before the delimiter).
    /// </summary>
    EmptyDebianRevision = 1 << 4,

    /// <summary>
    /// A version string where <see cref="DpkgVersion.Revision"/> contains a
    /// <see cref="DpkgVersion.UBUNTU_REVISION_DELIMITER"/> can have an empty
    /// <see cref="DpkgVersion.UbuntuRevision"/> (value after the delimiter).
    /// </summary>
    EmptyUbuntuRevision = 1 << 5,

    /// <summary>
    /// The <see cref="DpkgVersion.Revision"/> part of a version string can contain
    /// multiple <see cref="DpkgVersion.UBUNTU_REVISION_DELIMITER"/>.
    /// Only the first delimiter is used to split <see cref="DpkgVersion.DebianRevision"/>
    /// and <see cref="DpkgVersion.UbuntuRevision"/>.
    /// </summary>
    MultipleUbuntuRevisionDelimiters = 1 << 6,

    /// <summary>
    /// Default behavior of the <see cref="DpkgVersion"/> parsing functions.
    /// </summary>
    Default = None
              | EmptyVersion
              | EmptyRevertedUpstreamVersion
              | EmptyRealUpstreamVersion
              | MultipleRealUpstreamVersionDelimiter
              | EmptyDebianRevision
              | EmptyUbuntuRevision
              | MultipleUbuntuRevisionDelimiters
}

public partial class DpkgVersion :
    ISpanParsable<DpkgVersion>
{
    #region ISpanParsable<DpkgVersion>

    /// <inheritdoc/>
    public static DpkgVersion Parse(
        string versionString,
        IFormatProvider? formatProvider)
    {
        ArgumentNullException.ThrowIfNull(versionString);
        return Parse(versionString);
    }

    /// <inheritdoc/>
    public static bool TryParse(
        [NotNullWhen(returnValue: true)] string? versionString,
        IFormatProvider? formatProvider,
        [NotNullWhen(returnValue: true)] out DpkgVersion? version)
    {
        if (versionString is null)
        {
            version = null;
            return false;
        }

        return TryParse(versionString, out version);
    }

    /// <inheritdoc/>
    public static DpkgVersion Parse(
        ReadOnlySpan<char> versionSpan,
        IFormatProvider? formatProvider)
    {
        return Parse(versionSpan);
    }

    /// <inheritdoc/>
    public static bool TryParse(
        ReadOnlySpan<char> versionSpan,
        IFormatProvider? formatProvider,
        [NotNullWhen(returnValue: true)] out DpkgVersion? version)
    {
        return TryParse(versionSpan, out version);
    }

    #endregion

    public static DpkgVersion Parse(
        ReadOnlySpan<char> versionSpan,
        DpkgVersionOptions options = DpkgVersionOptions.Default,
        bool failFast = false)
    {
        return TryParse(versionSpan, out var version, out var annotations, options, failFast)
            ? version
            : throw new MalformedDpkgVersionException(versionSpan.ToString(), annotations);
    }

    public static DpkgVersion Parse(
        ReadOnlySpan<char> versionSpan,
        // ReSharper disable once OutParameterValueIsAlwaysDiscarded.Global
        out ImmutableList<ParsingAnnotation> annotations,
        DpkgVersionOptions options = DpkgVersionOptions.Default,
        bool failFast = false)
    {
        return TryParse(versionSpan, out var version, out annotations, options, failFast)
            ? version
            : throw new MalformedDpkgVersionException(versionSpan.ToString(), annotations);
    }

    public static bool TryParse(
        ReadOnlySpan<char> versionSpan,
        [NotNullWhen(returnValue: true)] out DpkgVersion? version,
        DpkgVersionOptions options = DpkgVersionOptions.Default)
    {
        // It does not make sense to failFast: false, because the annotations always get discarded.
        return TryParse(versionSpan, out version, out _, options, failFast: true);
    }

    public static bool TryParse(
        ReadOnlySpan<char> versionSpan,
        [NotNullWhen(returnValue: true)] out DpkgVersion? version,
        out ImmutableList<ParsingAnnotation> annotations,
        DpkgVersionOptions options = DpkgVersionOptions.Default,
        bool failFast = false)
    {
        annotations = ImmutableList<ParsingAnnotation>.Empty;

        bool isValid;

        if (versionSpan.Length == 0)
        {
            isValid = (options & DpkgVersionOptions.EmptyVersion) > 0;

            if (isValid)
            {
                version = Empty;
            }
            else
            {
                if (failFast) goto fail;
                version = null;
            }

            annotations += ParsingAnnotation.Create(EmptyVersion);
            return isValid;
        }

        isValid = TryParseEpoch(
            versionSpan,
            out var epochSpan,
            out var epochValue,
            out var upstreamVersionOffset,
            ref annotations,
            failFast);

        if (!isValid && failFast) goto fail;

        if (upstreamVersionOffset >= versionSpan.Length)
        {
            if (failFast) goto fail;
            annotations += ParsingAnnotation.Create(EmptyUpstreamVersion);
            goto fail;
        }

        ImmutableList<Location>.Builder? invalidCharacterLocations = null;
        isValid &= TryParseUpstreamVersionAndRevision(
            versionSpan,
            upstreamVersionOffset,
            out var upstreamVersionSpan,
            out var revisionOffset,
            out var revisionSpan,
            ref annotations,
            ref invalidCharacterLocations,
            failFast);

        if (!isValid && failFast) goto fail;

        isValid &= TrySplitByDelimiter(
            span: upstreamVersionSpan,
            spanStartOffset: upstreamVersionOffset,
            delimiter: REAL_UPSTREAM_VERSION_DELIMITER,
            allowMultipleDelimiter: (options & DpkgVersionOptions.MultipleRealUpstreamVersionDelimiter) > 0,
            allowEmptyFirstPart: (options & DpkgVersionOptions.EmptyRevertedUpstreamVersion) > 0,
            allowEmptySecondPart: (options & DpkgVersionOptions.EmptyRealUpstreamVersion) > 0,
            emptySpanDescriptor: EmptyUpstreamVersion,
            emptyFirstPartDescriptor: EmptyRevertedUpstreamVersion,
            emptySecondPartDescriptor: EmptyRealUpstreamVersion,
            multipleDelimiterDescriptor: MultipleRealUpstreamVersionDelimiter,
            ref annotations,
            out var containsRealUpstreamVersionDelimiter,
            out var revertedUpstreamVersionSpan,
            out var realUpstreamVersionSpan,
            ref invalidCharacterLocations,
            failFast);

        if (!isValid && failFast) goto fail;

        isValid &= TrySplitByDelimiter(
            span: revisionSpan,
            spanStartOffset: revisionOffset,
            delimiter: UBUNTU_REVISION_DELIMITER,
            allowMultipleDelimiter: (options & DpkgVersionOptions.MultipleUbuntuRevisionDelimiters) > 0,
            allowEmptyFirstPart: (options & DpkgVersionOptions.EmptyDebianRevision) > 0,
            allowEmptySecondPart: (options & DpkgVersionOptions.EmptyUbuntuRevision) > 0,
            emptySpanDescriptor: EmptyRevision,
            emptyFirstPartDescriptor: EmptyDebianRevision,
            emptySecondPartDescriptor: EmptyUbuntuRevision,
            multipleDelimiterDescriptor: MultipleUbuntuRevisionDelimiter,
            ref annotations,
            out var containsUbuntuRevisionDelimiter,
            out var debianRevisionSpan,
            out var ubuntuRevisionSpan,
            ref invalidCharacterLocations,
            failFast);

        if (isValid)
        {
            version = new DpkgVersion(
                originalString: versionSpan.ToString(),
                epoch: epochSpan.Length > 0 ? epochSpan.ToString() : null,
                epochValue: epochValue,
                upstreamVersion: upstreamVersionSpan.ToString(),
                revertedUpstreamVersion: containsRealUpstreamVersionDelimiter ? revertedUpstreamVersionSpan.ToString() : null,
                realUpstreamVersion: containsRealUpstreamVersionDelimiter ? realUpstreamVersionSpan.ToString() : null,
                revision: revisionOffset >= 0 ? revisionSpan.ToString() : null,
                debianRevision: revisionOffset >= 0 ? debianRevisionSpan.ToString() : null,
                ubuntuRevision: containsUbuntuRevisionDelimiter ? ubuntuRevisionSpan.ToString() : null);
            return true;
        }
        fail:
        version = null;
        return false;
    }

    private static bool TryParseEpoch(
        ReadOnlySpan<char> versionSpan,
        out ReadOnlySpan<char> epochSpan,
        out uint epochValue,
        out int upstreamVersionOffset,
        ref ImmutableList<ParsingAnnotation> annotations,
        bool failFast)
    {
        epochValue = DEFAULT_EPOCH_VALUE;
        var epochDelimiterIndex = versionSpan.IndexOf(EPOCH_DELIMITER);
        upstreamVersionOffset = epochDelimiterIndex + 1;

        if (epochDelimiterIndex == -1)
        {
            epochSpan = ReadOnlySpan<char>.Empty;
            return true;
        }
        if (epochDelimiterIndex == 0)
        {
            epochSpan = ReadOnlySpan<char>.Empty;
            if (failFast) return false;

            annotations += ParsingAnnotation.Create(EmptyEpoch);
            return false;
        }

        epochSpan = versionSpan[..epochDelimiterIndex];
        var epochValueTooLarge = false;

        List<char>? invalidCharacters = null;
        ImmutableList<Location>.Builder? invalidCharacterLocations = null;
        ulong value = 0ul;
        for (var position = 0; position < epochDelimiterIndex; ++position)
        {
            char currentCharacter = versionSpan[position];

            if (char.IsAsciiDigit(currentCharacter))
            {
                // I think it is computational cheaper to always calculate this than
                // checking every time if it even makes sense to do:
                value = unchecked(value * 10ul + (ulong)(currentCharacter - '0'));
                if (value > MAX_EPOCH_VALUE)
                {
                    if (failFast) return false;
                    epochValueTooLarge = true;
                }
            }
            else
            {
                if (failFast) return false;

                if (invalidCharacters is null)
                {
                    invalidCharacters = [currentCharacter];
                    invalidCharacterLocations = ImmutableList.CreateBuilder<Location>();
                }
                else if (!invalidCharacters.Contains(currentCharacter))
                {
                    invalidCharacters.Add(currentCharacter);
                }
                invalidCharacterLocations!.Add(position);
            }
        }

        if (invalidCharacters is not null)
        {
            annotations += ParsingAnnotation.Create(InvalidEpochCharacters,
                locations: invalidCharacterLocations!.ToImmutable(),
                messageArgs: [epochSpan.ToString(), invalidCharacters.JoinAsCharacterLiteralList()]);
        }
        else if (epochValueTooLarge)
        {
            annotations += ParsingAnnotation.Create(EpochValueTooLarge,
                locations: [ epochSpan.ToLocation() ],
                messageArgs: epochSpan.ToString());
        }
        else
        {
            epochValue = (uint)value;
        }

        return annotations.Count == 0;
    }

    private static bool TryParseUpstreamVersionAndRevision(
        ReadOnlySpan<char> versionSpan,
        int upstreamVersionOffset,
        out ReadOnlySpan<char> upstreamVersionSpan,
        out int revisionOffset,
        out ReadOnlySpan<char> revisionSpan,
        ref ImmutableList<ParsingAnnotation> annotations,
        ref ImmutableList<Location>.Builder? invalidCharacterLocations,
        bool failFast)
    {
        var revisionDelimiterIndex = versionSpan.LastIndexOf(REVISION_DELIMITER);
        List<char>? invalidCharacters = null;

        if (revisionDelimiterIndex == -1)
        {
            revisionOffset = -1;
            revisionSpan = ReadOnlySpan<char>.Empty;
            upstreamVersionSpan = versionSpan[upstreamVersionOffset..];
        }
        else
        {
            upstreamVersionSpan = versionSpan[upstreamVersionOffset..revisionDelimiterIndex];
            revisionOffset = revisionDelimiterIndex + 1;
            revisionSpan = versionSpan[revisionOffset..];
        }

        bool isValid = AllCharactersAreValid(
            span: upstreamVersionSpan,
            spanStartOffset: upstreamVersionOffset,
            isAllowedCharacter: IsAllowedUpstreamVersionCharacter,
            emptyDescriptor: EmptyUpstreamVersion,
            invalidCharacterDescriptor: InvalidUpstreamVersionCharacters,
            failFast,
            annotations: ref annotations,
            invalidCharacters: ref invalidCharacters,
            invalidCharacterLocations: ref invalidCharacterLocations);

        if (revisionDelimiterIndex >= 0)
        {
            if (!isValid && failFast) return false;
            isValid &= AllCharactersAreValid(
                span: revisionSpan,
                spanStartOffset: revisionOffset,
                isAllowedCharacter: IsAllowedRevisionCharacter,
                emptyDescriptor: EmptyRevision,
                invalidCharacterDescriptor: InvalidRevisionCharacters,
                failFast,
                annotations: ref annotations,
                invalidCharacters: ref invalidCharacters,
                invalidCharacterLocations: ref invalidCharacterLocations);
        }

        return isValid;
    }

    private static bool TrySplitByDelimiter(
        ReadOnlySpan<char> span,
        int spanStartOffset,
        string delimiter,
        bool allowMultipleDelimiter,
        bool allowEmptyFirstPart,
        bool allowEmptySecondPart,
        ParsingAnnotationDescriptor emptySpanDescriptor,
        ParsingAnnotationDescriptor emptyFirstPartDescriptor,
        ParsingAnnotationDescriptor emptySecondPartDescriptor,
        ParsingAnnotationDescriptor multipleDelimiterDescriptor,
        ref ImmutableList<ParsingAnnotation> annotations,
        out bool containsDelimiter,
        out ReadOnlySpan<char> firstPartSpan,
        out ReadOnlySpan<char> secondPartSpan,
        ref ImmutableList<Location>.Builder? locations,
        bool failFast)
    {
        if (span.IsEmpty)
        {
            firstPartSpan = span;
            secondPartSpan = ReadOnlySpan<char>.Empty;
            containsDelimiter = false;

            if (spanStartOffset < 0) return true;

            if (failFast) return false;
            annotations += ParsingAnnotation.Create(emptySpanDescriptor,
                location: spanStartOffset);
            return false;
        }

        var ubuntuRevisionDelimiterIndex = span.IndexOf(delimiter);
        if (ubuntuRevisionDelimiterIndex < 0)
        {
            firstPartSpan = span;
            secondPartSpan = ReadOnlySpan<char>.Empty;
            containsDelimiter = false;
            return true;
        }

        bool isValid = true;
        var secondPartOffset = ubuntuRevisionDelimiterIndex + delimiter.Length;
        firstPartSpan = span[..ubuntuRevisionDelimiterIndex];
        secondPartSpan = span[secondPartOffset..];
        containsDelimiter = true;

        if (firstPartSpan.IsEmpty)
        {
            if (!allowEmptyFirstPart)
            {
                if (failFast) return false;
                isValid = false;
            }

            annotations += ParsingAnnotation.Create(emptyFirstPartDescriptor,
                location: spanStartOffset,
                messageArgs: [ span.ToString() ]);
        }

        if (secondPartSpan.IsEmpty)
        {
            if (!allowEmptySecondPart)
            {
                if (failFast) return false;
                isValid = false;
            }

            annotations += ParsingAnnotation.Create(emptySecondPartDescriptor,
                location: secondPartOffset + spanStartOffset,
                messageArgs: [ span.ToString() ]);

            return isValid;
        }

        if (secondPartSpan.Length < delimiter.Length) return isValid;

        for (int position = 0, delimiterPosition = 0; position < secondPartSpan.Length; position++)
        {
            if (secondPartSpan[position] == delimiter[delimiterPosition])
            {
                if (++delimiterPosition < delimiter.Length) continue;
                if (failFast && !allowMultipleDelimiter) return false;

                locations ??= ImmutableList.CreateBuilder<Location>();
                var end = secondPartOffset + position + 1;
                locations.Add(new Location(start: end - delimiter.Length, end));
            }

            delimiterPosition = 0;
        }

        if (locations is { Count: > 0 })
        {
            locations.Insert(0, new Location(start: secondPartOffset - delimiter.Length, end: secondPartOffset));
            annotations += ParsingAnnotation.Create(multipleDelimiterDescriptor,
                locations: locations.ToImmutableList(),
                messageArgs: [ secondPartSpan.ToString() ]);
            locations.Clear();
            return allowMultipleDelimiter && isValid;
        }

        return isValid;
    }

    private static bool AllCharactersAreValid(
        ReadOnlySpan<char> span,
        int spanStartOffset,
        Func<char, bool> isAllowedCharacter,
        ParsingAnnotationDescriptor emptyDescriptor,
        ParsingAnnotationDescriptor invalidCharacterDescriptor,
        bool failFast,
        ref ImmutableList<ParsingAnnotation> annotations,
        ref List<char>? invalidCharacters,
        ref ImmutableList<Location>.Builder? invalidCharacterLocations)
    {
        if (span.IsEmpty)
        {
            if (failFast) return false;
            annotations += ParsingAnnotation.Create(emptyDescriptor,
                location: spanStartOffset);
            return false;
        }

        for (int position = 0; position < span.Length; ++position)
        {
            char currentCharacter = span[position];

            if (!isAllowedCharacter(currentCharacter))
            {
                if (failFast) return false;

                if (invalidCharacters is null)
                {
                    invalidCharacters = [currentCharacter];
                    invalidCharacterLocations ??= ImmutableList.CreateBuilder<Location>();
                }
                else if (!invalidCharacters.Contains(currentCharacter))
                {
                    invalidCharacters.Add(currentCharacter);
                }
                invalidCharacterLocations!.Add(new Location(position + spanStartOffset));
            }
        }

        if (invalidCharacters is { Count: > 0 })
        {
            annotations += ParsingAnnotation.Create(invalidCharacterDescriptor,
                locations: invalidCharacterLocations!.ToImmutable(),
                messageArgs: [span.ToString(), invalidCharacters.JoinAsCharacterLiteralList()]);
            invalidCharacters.Clear();
            invalidCharacterLocations.Clear();
            return false;
        }

        return true;
    }

    private static bool IsAllowedUpstreamVersionCharacter(char character)
        => char.IsAsciiLetterOrDigit(character)
           || character == '.' || character == '+' || character == '-' || character == ':' || character == '~';

    private static bool IsAllowedRevisionCharacter(char character)
        => char.IsAsciiLetterOrDigit(character) || character == '+' || character == '.' || character == '~';
}
