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
public enum DpkgVersionStyle
{
    None = 0,

    /// <summary>
    /// An empty version string will be interpreted as <see cref="DpkgVersion.Empty"/>.
    /// </summary>
    AllowEmpty = 1 << 0,

    /// <summary>
    /// A version string where <see cref="DpkgVersion.UbuntuRevision"/> contains a
    /// <see cref="DpkgVersion.REAL_UPSTREAM_VERSION_DELIMITER"/> can have an empty
    /// <see cref="DpkgVersion.RevertedUpstreamVersion"/> (value before the delimiter).
    /// </summary>
    AllowEmptyRevertedUpstreamVersion = 1 << 1,

    /// <summary>
    /// A version string where <see cref="DpkgVersion.UbuntuRevision"/> contains a
    /// <see cref="DpkgVersion.REAL_UPSTREAM_VERSION_DELIMITER"/> can have an empty
    /// <see cref="DpkgVersion.RealUpstreamVersion"/> (value after the delimiter).
    /// </summary>
    AllowEmptyRealUpstreamVersion = 1 << 2,

    /// <summary>
    /// The <see cref="DpkgVersion.UpstreamVersion"/> part of a version string can contain
    /// multiple <see cref="DpkgVersion.REAL_UPSTREAM_VERSION_DELIMITER"/>.
    /// Only the first delimiter is used to split <see cref="DpkgVersion.RealUpstreamVersion"/>
    /// and <see cref="DpkgVersion.RevertedUpstreamVersion"/>.
    /// </summary>
    AllowMultipleRealUpstreamVersionDelimiter = 1 << 3,

    /// <summary>
    /// A version string where <see cref="DpkgVersion.Revision"/> contains a
    /// <see cref="DpkgVersion.UBUNTU_REVISION_DELIMITER"/> can have an empty
    /// <see cref="DpkgVersion.DebianRevision"/> (value before the delimiter).
    /// </summary>
    AllowEmptyDebianRevision = 1 << 4,

    /// <summary>
    /// A version string where <see cref="DpkgVersion.Revision"/> contains a
    /// <see cref="DpkgVersion.UBUNTU_REVISION_DELIMITER"/> can have an empty
    /// <see cref="DpkgVersion.UbuntuRevision"/> (value after the delimiter).
    /// </summary>
    AllowEmptyUbuntuRevision = 1 << 5,

    /// <summary>
    /// The <see cref="DpkgVersion.Revision"/> part of a version string can contain
    /// multiple <see cref="DpkgVersion.UBUNTU_REVISION_DELIMITER"/>.
    /// Only the first delimiter is used to split <see cref="DpkgVersion.DebianRevision"/>
    /// and <see cref="DpkgVersion.UbuntuRevision"/>.
    /// </summary>
    AllowMultipleUbuntuRevisionDelimiters = 1 << 6,

    /// <summary>
    /// Default behavior of the <see cref="DpkgVersion"/> parsing functions.
    /// </summary>
    Default = None
              | AllowEmpty
              | AllowEmptyRevertedUpstreamVersion
              | AllowEmptyRealUpstreamVersion
              | AllowMultipleRealUpstreamVersionDelimiter
              | AllowEmptyDebianRevision
              | AllowEmptyUbuntuRevision
              | AllowMultipleUbuntuRevisionDelimiters
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
        DpkgVersionStyle style = DpkgVersionStyle.Default,
        bool failEarly = false)
    {
        return TryParse(versionSpan, out var version, out var annotations, style, failEarly)
            ? version
            : throw new MalformedDpkgVersionException(versionSpan.ToString(), annotations);
    }

    public static DpkgVersion Parse(
        ReadOnlySpan<char> versionSpan,
        // ReSharper disable once OutParameterValueIsAlwaysDiscarded.Global
        out ImmutableList<ParsingAnnotation> annotations,
        DpkgVersionStyle style = DpkgVersionStyle.Default,
        bool failEarly = false)
    {
        return TryParse(versionSpan, out var version, out annotations, style, failEarly)
            ? version
            : throw new MalformedDpkgVersionException(versionSpan.ToString(), annotations);
    }

    public static bool TryParse(
        ReadOnlySpan<char> versionSpan,
        [NotNullWhen(returnValue: true)] out DpkgVersion? version,
        DpkgVersionStyle style = DpkgVersionStyle.Default)
    {
        // It does not make sense to failEarly: false, because the annotations always get discarded.
        return TryParse(versionSpan, out version, out _, style, failEarly: true);
    }

    public static bool TryParse(
        ReadOnlySpan<char> versionSpan,
        [NotNullWhen(returnValue: true)] out DpkgVersion? version,
        out ImmutableList<ParsingAnnotation> annotations,
        DpkgVersionStyle style = DpkgVersionStyle.Default,
        bool failEarly = false)
    {
        if (!BaseTryParseCore(
                versionSpan,
                out var epochSpan,
                out var epochValue,
                out _,
                out var upstreamVersionSpan,
                out var realUpstreamVersionSpanOffset,
                out var revertedUpstreamVersionSpan,
                out var realUpstreamVersionSpan,
                out var revisionSpanOffset,
                out var revisionSpan,
                out var ubuntuRevisionSpanOffset,
                out var debianRevisionSpan,
                out var ubuntuRevisionSpan,
                out annotations,
                style,
                failEarly))
        {
            version = null;
            return false;
        }

        version = new DpkgVersion(
            originalString: versionSpan.ToString(),
            epoch: epochSpan.Length > 0 ? epochSpan.ToString() : null,
            epochValue: epochValue,
            upstreamVersion: upstreamVersionSpan.ToString(),
            revertedUpstreamVersion: realUpstreamVersionSpanOffset > 0 ? revertedUpstreamVersionSpan.ToString() : null,
            realUpstreamVersion: realUpstreamVersionSpanOffset > 0 ? realUpstreamVersionSpan.ToString() : null,
            revision: revisionSpanOffset >= 0 ? revisionSpan.ToString() : null,
            debianRevision: revisionSpanOffset >= 0 ? debianRevisionSpan.ToString() : null,
            ubuntuRevision: ubuntuRevisionSpanOffset >= 0 ? ubuntuRevisionSpan.ToString() : null);
        return true;
    }

    protected static bool BaseTryParseCore(
        ReadOnlySpan<char> versionSpan,
        out ReadOnlySpan<char> epochSpan,
        out uint epochValue,
        out int upstreamVersionSpanOffset,
        out ReadOnlySpan<char> upstreamVersionSpan,
        out int realUpstreamVersionSpanOffset,
        out ReadOnlySpan<char> revertedUpstreamVersionSpan,
        out ReadOnlySpan<char> realUpstreamVersionSpan,
        out int revisionSpanOffset,
        out ReadOnlySpan<char> revisionSpan,
        out int ubuntuRevisionSpanOffset,
        out ReadOnlySpan<char> debianRevisionSpan,
        out ReadOnlySpan<char> ubuntuRevisionSpan,
        out ImmutableList<ParsingAnnotation> annotations,
        DpkgVersionStyle style,
        bool failEarly)
    {
        var isValid = true;
        annotations = ImmutableList<ParsingAnnotation>.Empty;

        if (versionSpan.Length == 0)
        {
            isValid = (style & DpkgVersionStyle.AllowEmpty) > 0;

            if (!isValid && failEarly) goto returnEmpty;
            annotations = [ParsingAnnotation.Create(EmptyVersion)];
            goto returnEmpty;
        }

        // re-use list objects if we create one
        var errorBuffers = new ErrorBuffers();

        #region Parse Epoch
        {
            var epochDelimiterIndex = versionSpan.IndexOf(EPOCH_DELIMITER);
            upstreamVersionSpanOffset = epochDelimiterIndex + 1;
            epochSpan = ReadOnlySpan<char>.Empty;
            epochValue = DEFAULT_EPOCH_VALUE;

            if (epochDelimiterIndex == 0)
            {
                isValid = false;
                if (failEarly) goto returnEmpty;
                annotations += ParsingAnnotation.Create(EmptyEpoch, location: epochDelimiterIndex);
            }
            else if (epochDelimiterIndex > 0)
            {
                epochSpan = versionSpan[..epochDelimiterIndex];
                var epochValueTooLarge = false;

                ulong value = 0ul;
                for (var position = 0; position < epochDelimiterIndex; ++position)
                {
                    char currentCharacter = versionSpan[position];

                    if (char.IsAsciiDigit(currentCharacter))
                    {
                        // I think it is computationally cheaper to always calculate this than
                        // checking every time if it even makes sense to do:
                        value = unchecked(value * 10ul + (ulong)(currentCharacter - '0'));
                        if (value > MAX_EPOCH_VALUE)
                        {
                            isValid = false;
                            if (failEarly) goto returnEmpty;
                            epochValueTooLarge = true;
                        }
                    }
                    else
                    {
                        isValid = false;
                        if (failEarly) goto returnEmpty;
                        errorBuffers.AddInvalidCharacter(currentCharacter, position);
                    }
                }

                if (errorBuffers.InvalidCharacters is { Count: > 0 })
                {
                    annotations += ParsingAnnotation.Create(InvalidEpochCharacters,
                        locations: errorBuffers.Locations!.ToImmutable(),
                        messageArgs: [epochSpan.ToString(), errorBuffers.InvalidCharacters.JoinAsCharacterLiteralList()]);
                    errorBuffers.Clear();
                }
                else if (epochValueTooLarge)
                {
                    annotations += ParsingAnnotation.Create(EpochValueTooLarge,
                        locations: [epochSpan.ToLocation()],
                        messageArgs: epochSpan.ToString());
                }
                else
                {
                    epochValue = (uint)value;
                }
            }
        }
        #endregion

        #region Parse Upstream Version & Revision
        if (upstreamVersionSpanOffset >= versionSpan.Length)
        {
            isValid = false;
            if (failEarly) goto returnEmpty;
            annotations += ParsingAnnotation.Create(EmptyUpstreamVersion);
            goto returnEmpty;
        }

        {
            var revisionDelimiterIndex = versionSpan.LastIndexOf(REVISION_DELIMITER);

            if (revisionDelimiterIndex == -1 || revisionDelimiterIndex < upstreamVersionSpanOffset)
            {
                revisionSpanOffset = -1;
                revisionSpan = ReadOnlySpan<char>.Empty;
                upstreamVersionSpan = versionSpan[upstreamVersionSpanOffset..];
            }
            else
            {
                upstreamVersionSpan = versionSpan[upstreamVersionSpanOffset..revisionDelimiterIndex];
                revisionSpanOffset = revisionDelimiterIndex + 1;
                revisionSpan = versionSpan[revisionSpanOffset..];
            }

            isValid &= AllCharactersAreValid(
                span: upstreamVersionSpan,
                spanOffset: upstreamVersionSpanOffset,
                isAllowedCharacter: IsAllowedUpstreamVersionCharacter,
                emptyDescriptor: EmptyUpstreamVersion,
                invalidCharacterDescriptor: InvalidUpstreamVersionCharacters,
                failEarly,
                ref annotations,
                ref errorBuffers);

            if (!isValid && failEarly) goto returnEmpty;

            if (revisionSpanOffset >= 0)
            {
                isValid &= AllCharactersAreValid(
                    span: revisionSpan,
                    spanOffset: revisionSpanOffset,
                    isAllowedCharacter: IsAllowedRevisionCharacter,
                    emptyDescriptor: EmptyRevision,
                    invalidCharacterDescriptor: InvalidRevisionCharacters,
                    failEarly,
                    ref annotations,
                    ref errorBuffers);

                if (!isValid && failEarly) goto returnEmpty;
            }
        }
        #endregion

        isValid &= TrySplitByDelimiter(
            span: upstreamVersionSpan,
            spanOffset: upstreamVersionSpanOffset,
            delimiter: REAL_UPSTREAM_VERSION_DELIMITER,
            allowMultipleDelimiter: (style & DpkgVersionStyle.AllowMultipleRealUpstreamVersionDelimiter) != 0,
            allowEmptyFirstPart: (style & DpkgVersionStyle.AllowEmptyRevertedUpstreamVersion) != 0,
            allowEmptySecondPart: (style & DpkgVersionStyle.AllowEmptyRealUpstreamVersion) != 0,
            emptySpanDescriptor: EmptyUpstreamVersion,
            emptyFirstPartDescriptor: EmptyRevertedUpstreamVersion,
            emptySecondPartDescriptor: EmptyRealUpstreamVersion,
            multipleDelimiterDescriptor: MultipleRealUpstreamVersionDelimiter,
            out realUpstreamVersionSpanOffset,
            out revertedUpstreamVersionSpan,
            out realUpstreamVersionSpan,
            failEarly,
            ref annotations,
            ref errorBuffers.Locations);

        if (!isValid && failEarly) goto returnEmpty;

        isValid &= TrySplitByDelimiter(
            span: revisionSpan,
            spanOffset: revisionSpanOffset,
            delimiter: UBUNTU_REVISION_DELIMITER,
            allowMultipleDelimiter: (style & DpkgVersionStyle.AllowMultipleUbuntuRevisionDelimiters) != 0,
            allowEmptyFirstPart: (style & DpkgVersionStyle.AllowEmptyDebianRevision) != 0,
            allowEmptySecondPart: (style & DpkgVersionStyle.AllowEmptyUbuntuRevision) != 0,
            emptySpanDescriptor: EmptyRevision,
            emptyFirstPartDescriptor: EmptyDebianRevision,
            emptySecondPartDescriptor: EmptyUbuntuRevision,
            multipleDelimiterDescriptor: MultipleUbuntuRevisionDelimiter,
            out ubuntuRevisionSpanOffset,
            out debianRevisionSpan,
            out ubuntuRevisionSpan,
            failEarly,
            ref annotations,
            ref errorBuffers.Locations);

        if (isValid) return true;

    returnEmpty:
        epochSpan = ReadOnlySpan<char>.Empty;
        epochValue = DEFAULT_EPOCH_VALUE;
        upstreamVersionSpanOffset = -1;
        upstreamVersionSpan = ReadOnlySpan<char>.Empty;
        realUpstreamVersionSpanOffset = -1;
        revertedUpstreamVersionSpan = ReadOnlySpan<char>.Empty;
        realUpstreamVersionSpan = ReadOnlySpan<char>.Empty;
        revisionSpanOffset = -1;
        revisionSpan = ReadOnlySpan<char>.Empty;
        ubuntuRevisionSpanOffset = -1;
        debianRevisionSpan = ReadOnlySpan<char>.Empty;
        ubuntuRevisionSpan = ReadOnlySpan<char>.Empty;
        return isValid;
    }

    private static bool TrySplitByDelimiter(
        ReadOnlySpan<char> span,
        int spanOffset,
        string delimiter,
        bool allowMultipleDelimiter,
        bool allowEmptyFirstPart,
        bool allowEmptySecondPart,
        ParsingAnnotationDescriptor emptySpanDescriptor,
        ParsingAnnotationDescriptor emptyFirstPartDescriptor,
        ParsingAnnotationDescriptor emptySecondPartDescriptor,
        ParsingAnnotationDescriptor multipleDelimiterDescriptor,
        out int secondPartSpanOffset,
        out ReadOnlySpan<char> firstPartSpan,
        out ReadOnlySpan<char> secondPartSpan,
        bool failEarly,
        scoped ref ImmutableList<ParsingAnnotation> annotations,
        scoped ref ImmutableList<Location>.Builder? locations)
    {
        if (span.IsEmpty)
        {
            secondPartSpanOffset = -1;
            firstPartSpan = ReadOnlySpan<char>.Empty;
            secondPartSpan = ReadOnlySpan<char>.Empty;

            if (spanOffset < 0) return true;

            if (failEarly) return false;
            annotations += ParsingAnnotation.Create(emptySpanDescriptor, location: spanOffset);
            return false;
        }

        var delimiterIndex = span.IndexOf(delimiter);
        if (delimiterIndex < 0)
        {
            secondPartSpanOffset = -1;
            firstPartSpan = span;
            secondPartSpan = ReadOnlySpan<char>.Empty;
            return true;
        }

        secondPartSpanOffset = delimiterIndex + delimiter.Length;
        firstPartSpan = span[..delimiterIndex];
        secondPartSpan = span[secondPartSpanOffset..];

        bool isValid = true;

        if (firstPartSpan.IsEmpty)
        {
            if (!allowEmptyFirstPart)
            {
                if (failEarly) return false;
                isValid = false;
            }

            annotations += ParsingAnnotation.Create(emptyFirstPartDescriptor,
                location: spanOffset,
                messageArgs: [ span.ToString() ]);
        }

        if (secondPartSpan.IsEmpty)
        {
            if (!allowEmptySecondPart)
            {
                if (failEarly) return false;
                isValid = false;
            }

            annotations += ParsingAnnotation.Create(emptySecondPartDescriptor,
                location: secondPartSpanOffset + spanOffset,
                messageArgs: [ span.ToString() ]);
        }

        if (secondPartSpan.Length < delimiter.Length) return isValid;

        for (int position = 0, delimiterPosition = 0; position < secondPartSpan.Length; position++)
        {
            if (secondPartSpan[position] == delimiter[delimiterPosition])
            {
                if (++delimiterPosition < delimiter.Length) continue;
                if (failEarly && !allowMultipleDelimiter) return false;

                var end = secondPartSpanOffset + position + 1;
                locations ??= ImmutableList.CreateBuilder<Location>();
                locations.Add(new Location(start: end - delimiter.Length, end));
            }

            delimiterPosition = 0;
        }

        if (locations is { Count: > 0 })
        {
            locations.Insert(0, new Location(start: secondPartSpanOffset - delimiter.Length, end: secondPartSpanOffset));
            annotations += ParsingAnnotation.Create(multipleDelimiterDescriptor,
                locations: locations.ToImmutable(),
                messageArgs: [ secondPartSpan.ToString() ]);
            locations.Clear();

            return allowMultipleDelimiter && isValid;
        }

        return isValid;
    }

    private static bool AllCharactersAreValid(
        scoped ReadOnlySpan<char> span,
        int spanOffset,
        Func<char, bool> isAllowedCharacter,
        ParsingAnnotationDescriptor emptyDescriptor,
        ParsingAnnotationDescriptor invalidCharacterDescriptor,
        bool failEarly,
        scoped ref ImmutableList<ParsingAnnotation> annotations,
        scoped ref ErrorBuffers errorBuffers)
    {
        if (span.IsEmpty)
        {
            if (failEarly) return false;
            annotations += ParsingAnnotation.Create(emptyDescriptor, location: spanOffset);
            return false;
        }

        for (int position = 0; position < span.Length; ++position)
        {
            char currentCharacter = span[position];

            if (!isAllowedCharacter(currentCharacter))
            {
                if (failEarly) return false;
                errorBuffers.AddInvalidCharacter(currentCharacter, position + spanOffset);
            }
        }

        if (errorBuffers.InvalidCharacters is { Count: > 0 })
        {
            annotations += ParsingAnnotation.Create(invalidCharacterDescriptor,
                locations: errorBuffers.Locations!.ToImmutable(),
                messageArgs: [span.ToString(), errorBuffers.InvalidCharacters.JoinAsCharacterLiteralList()]);
            errorBuffers.Clear();
            return false;
        }

        return true;
    }

    private static bool IsAllowedUpstreamVersionCharacter(char character)
        => char.IsAsciiLetterOrDigit(character)
           || character == '.' || character == '+' || character == '-' || character == ':' || character == '~';

    private static bool IsAllowedRevisionCharacter(char character)
        => char.IsAsciiLetterOrDigit(character) || character == '+' || character == '.' || character == '~';


    private ref struct ErrorBuffers
    {
        public List<char>? InvalidCharacters;
        public ImmutableList<Location>.Builder? Locations;

        public ErrorBuffers()
        {
            InvalidCharacters = null;
            Locations = null;
        }

        public void AddInvalidCharacter(char character, Location location)
        {
            if (InvalidCharacters is null)
            {
                InvalidCharacters = [character];
            }
            else if (!InvalidCharacters.Contains(character))
            {
                InvalidCharacters.Add(character);
            }

            Locations ??= ImmutableList.CreateBuilder<Location>();
            Locations.Add(location);
        }

        public void Clear()
        {
            InvalidCharacters!.Clear();
            Locations!.Clear();
        }
    }
}
