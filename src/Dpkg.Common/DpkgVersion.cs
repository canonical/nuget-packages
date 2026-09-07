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

namespace Canonical.Dpkg;

/// <summary>
/// A representation of the Debian/Ubuntu package version format, as defined in the man page
/// <see href="https://manpages.ubuntu.com/manpages/noble/en/man7/deb-version.7.html">deb-version(7)</see>.
/// </summary>
public partial class DpkgVersion :
    IComparable,
    IComparable<string>,
    IComparable<DpkgVersion>,
    IEquatable<string>,
    IEquatable<DpkgVersion>,
    IFormattable
{
    /// <summary>
    /// The character that indicates the boundary between the epoch and remaining deb version string representation.
    /// </summary>
    public const char EPOCH_DELIMITER = ':';

    /// <summary>
    /// The character that indicates the boundary between the upstream version and debian revision
    /// in a deb version string representation.
    /// </summary>
    public const char REVISION_DELIMITER = '-';

    /// <summary>
    /// The sequence of characters that indicates the boundary between the debian and ubuntu revision (if present)
    /// in a deb version string representation.
    /// </summary>
    public const string UBUNTU_REVISION_DELIMITER = "ubuntu";

    /// <summary>
    /// The sequence of characters that indicates the boundary between the reverted and real upstream version
    /// (if present) in a deb version string representation.
    /// </summary>
    public const string REAL_UPSTREAM_VERSION_DELIMITER = "+really";

    /// <summary>
    /// The default <see cref="Epoch"/> value that can be assumed in case, it is omitted from the string representation.
    /// </summary>
    public const uint DEFAULT_EPOCH_VALUE = 0;

    /// <summary>
    /// The maximum <see cref="Epoch"/> value that can be processed by dpkg-dev tools. It is equal to <see cref="int.MaxValue"/>.
    /// </summary>
    public const uint MAX_EPOCH_VALUE = int.MaxValue;

    /// <summary>
    /// An <see cref="DpkgVersion"/> instance with an empty string representation.
    /// </summary>
    public static readonly DpkgVersion Empty = new DpkgVersion(
        originalString: string.Empty,
        epoch: null,
        epochValue: DEFAULT_EPOCH_VALUE,
        upstreamVersion: string.Empty,
        revertedUpstreamVersion: null,
        realUpstreamVersion: null,
        revision: null,
        debianRevision: null,
        ubuntuRevision: null);

    private readonly string _originalString;
    private int? _hashCode = null;

    /// <summary>
    /// Initializes a new <see cref="DpkgVersion"/> instance using the values provided for initialization.
    /// </summary>
    /// <remarks>
    /// The values provided for initialization are not validated.
    /// Make sure the values conform with the expected formats.
    /// </remarks>
    /// <param name="originalString">The original string representation that was parsed.</param>
    /// <param name="epoch">Value used for the initialization of <see cref="Epoch"/>.</param>
    /// <param name="epochValue">Value used for the initialization of <see cref="EpochValue"/>.</param>
    /// <param name="upstreamVersion">Value used for the initialization of <see cref="UpstreamVersion"/>.</param>
    /// <param name="revertedUpstreamVersion">Value used for the initialization of <see cref="RevertedUpstreamVersion"/>.</param>
    /// <param name="realUpstreamVersion">Value used for the initialization of <see cref="RealUpstreamVersion"/>.</param>
    /// <param name="revision">Value used for the initialization of <see cref="Revision"/>.</param>
    /// <param name="debianRevision">Value used for the initialization of <see cref="DebianRevision"/>.</param>
    /// <param name="ubuntuRevision">Value used for the initialization of <see cref="UbuntuRevision"/>.</param>
    protected DpkgVersion(
        string originalString,
        string? epoch,
        uint epochValue,
        string upstreamVersion,
        string? revertedUpstreamVersion,
        string? realUpstreamVersion,
        string? revision,
        string? debianRevision,
        string? ubuntuRevision)
    {
        _originalString = originalString;
        Epoch = epoch;
        EpochValue = epochValue;
        UpstreamVersion = upstreamVersion;
        RevertedUpstreamVersion = revertedUpstreamVersion;
        RealUpstreamVersion = realUpstreamVersion;
        Revision = revision;
        DebianRevision = debianRevision;
        UbuntuRevision = ubuntuRevision;
    }

    protected static DpkgVersion Create(
        ReadOnlySpan<char> versionSpan,
        ReadOnlySpan<char> epochSpan,
        uint epochValue,
        ReadOnlySpan<char> upstreamVersionSpan,
        int realUpstreamVersionSpanOffset,
        ReadOnlySpan<char> revertedUpstreamVersionSpan,
        ReadOnlySpan<char> realUpstreamVersionSpan,
        int revisionSpanOffset,
        ReadOnlySpan<char> revisionSpan,
        int ubuntuRevisionSpanOffset,
        ReadOnlySpan<char> debianRevisionSpan,
        ReadOnlySpan<char> ubuntuRevisionSpan)
    {
        return new DpkgVersion(
            originalString: versionSpan.ToString(),
            epoch: epochSpan.Length > 0 ? epochSpan.ToString() : null,
            epochValue: epochValue,
            upstreamVersion: upstreamVersionSpan.ToString(),
            revertedUpstreamVersion: realUpstreamVersionSpanOffset > 0 ? revertedUpstreamVersionSpan.ToString() : null,
            realUpstreamVersion: realUpstreamVersionSpanOffset > 0 ? realUpstreamVersionSpan.ToString() : null,
            revision: revisionSpanOffset >= 0 ? revisionSpan.ToString() : null,
            debianRevision: revisionSpanOffset >= 0 ? debianRevisionSpan.ToString() : null,
            ubuntuRevision: ubuntuRevisionSpanOffset >= 0 ? ubuntuRevisionSpan.ToString() : null);
    }

    /// <summary>
    /// A single (generally small) unsigned integer. It may be omitted, in which case <see cref="Epoch"/> will be
    /// <see langword="null"/>. If it is omitted then <see cref="UpstreamVersion"/> may not contain any <c>:</c> (colon).
    /// </summary>
    /// <remarks>
    /// <b>Epochs should be used sparingly!</b>
    /// <br/><br/>
    /// The purpose of epochs is to cope with situations where the upstream version numbering scheme changes
    /// and to allow us to leave behind serious mistakes. If you think that increasing the epoch is the right solution,
    /// you should consult experienced developers for your distribution and get consensus before doing so.
    /// <br/><br/>
    /// Epochs should not be used when a package needs to be rolled back. In that case, use the <c>+really</c>
    /// convention: for example, if you uploaded <c>2.3-3</c>, and now you need to go backwards to upstream <c>2.2</c>,
    /// call your reverting upload something like <c>2.3+really2.2-1</c>. Eventually, when we upload upstream
    /// <c>2.4</c>, the <c>+really</c> part can go away.
    /// </remarks>
    public string? Epoch { get; }

    /// <summary>
    /// Gets numeric value represented by <see cref="Epoch"/>.
    /// </summary>
    /// <remarks>
    /// If <see cref="Epoch"/> is <see langword="null"/>, the value of <see cref="DEFAULT_EPOCH_VALUE"/> is used.
    /// </remarks>
    public uint EpochValue { get; }

    /// <summary>
    /// The main part of the version number.
    /// </summary>
    /// <remarks>
    /// It is usually the version number of the original (“upstream”) package from which the
    /// <c>.deb</c> file has been made, if this is applicable.  Usually this will be in the same
    /// format as that specified by the upstream author(s); however, it may need to be reformatted
    /// to fit into the package management system's format and comparison scheme.
    ///
    /// The upstream-version may contain only alphanumerics (“A-Za-z0-9”) and the characters
    /// <c>.</c> <c>+</c> <c>-</c> <c>:</c> <c>~</c> (full stop, plus, hyphen, colon, tilde)
    /// and should start with a digit. If <see cref="Revision"/> is <see langword="null"/> then
    /// hyphens are not allowed; if <see cref="Epoch"/> is <see langword="null"/> then colons are not allowed.
    /// </remarks>
    public string UpstreamVersion { get; }

    /// <summary>
    /// If <see cref="UpstreamVersion"/> contains a <c>+really</c> delimiter, it will contain the reverted upstream
    /// version (the part of the string before the <c>+really</c> delimiter); otherwise it will be
    /// <see langword="null"/>.
    /// </summary>
    public string? RevertedUpstreamVersion { get; }

    /// <summary>
    /// If <see cref="UpstreamVersion"/> contains a <c>+really</c> delimiter, it will contain the real upstream
    /// version (the part of the string after the <c>+really</c> delimiter); otherwise it will be
    /// <see langword="null"/>.
    /// </summary>
    public string? RealUpstreamVersion { get; }

    /// <summary>
    /// Gets <see cref="RealUpstreamVersion"/> if it is not <see langword="null"/>;
    /// otherwise it gets <see cref="UpstreamVersion"/>.
    /// </summary>
    public string EffectiveUpstreamVersion => RealUpstreamVersion ?? UpstreamVersion;

    /// <summary>
    /// This part of the version number specifies the version of the deb package based on the
    /// <see cref="UpstreamVersion"/>.
    /// Native packages do not have a revision, in which case <see cref="Revision"/> will be <see langword="null"/>.
    /// </summary>
    /// <remarks>
    /// See the man page dpkg-source(1) and the Ubuntu Packaging Guide (packaging.ubuntu.com) for more details
    /// about native packages.
    /// </remarks>
    public string? Revision { get; }

    /// <summary>
    /// If <see cref="Revision"/> is <see langword="null"/> it will be <see langword="null"/>.
    /// If <see cref="Revision"/> contains an <c>ubuntu</c> delimiter, it will contain
    /// the debian revision (the part of the string before the <c>ubuntu</c> delimiter).
    /// If <see cref="Revision"/> does not contain an <c>ubuntu</c> delimiter, it will be equal to
    /// <see cref="Revision"/>.
    /// </summary>
    public string? DebianRevision { get; }

    /// <summary>
    /// If <see cref="Revision"/> is not <see langword="null"/> and contains an<c>ubuntu</c> delimiter, it will contain
    /// the ubuntu revision (the part of the string after the <c>ubuntu</c> delimiter); otherwise it will be
    /// <see langword="null"/>.
    /// </summary>
    public string? UbuntuRevision { get; }

    /// <summary>
    /// Returns a string that represents the current object in the format described in the
    /// <see href="https://manpages.ubuntu.com/manpages/noble/en/man7/deb-version.7.html">deb-version(7)</see> man page.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => _originalString;

    /// <summary>
    /// Formats the value of the current instance using the specified format string.
    /// </summary>
    /// <param name="format">
    /// <para>
    /// The format string controlling how the version is rendered. If <see langword="null"/>, empty,
    /// or <c>"G"</c>, the result is identical to <see cref="ToString()"/>.
    /// </para>
    /// <para>
    /// A format string is a sequence of the following specifiers and literals:
    /// </para>
    /// <list type="table">
    ///   <listheader><term>Specifier</term><description>Output</description></listheader>
    ///   <item>
    ///     <term><c>G</c></term>
    ///     <description>The original version string (same as <see cref="ToString()"/>).</description>
    ///   </item>
    ///   <item>
    ///     <term><c>:</c></term>
    ///     <description>
    ///     The epoch delimiter character (<c>:</c>), but only when <see cref="Epoch"/> is not
    ///     <see langword="null"/>; otherwise nothing is appended.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term><c>-</c></term>
    ///     <description>
    ///     The revision delimiter character (<c>-</c>), but only when <see cref="Revision"/> is not
    ///     <see langword="null"/>; otherwise nothing is appended.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term><c>E</c>, <c>E0</c>-<c>E9</c></term>
    ///     <description>
    ///     <see cref="Epoch"/> as a string, or nothing when <see cref="Epoch"/> is
    ///     <see langword="null"/>. Optionally followed by a single digit <c>0</c>–<c>9</c> that
    ///     specifies a minimum field width; the value is left-padded with zeros to that width
    ///     (when the epoch is present), or nothing is appended (when the epoch is absent).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term><c>e</c></term>
    ///     <description>
    ///     <see cref="EpochValue"/> as a decimal number. Without a digit suffix the numeric value
    ///     is always appended (e.g. <c>0</c> when the epoch is omitted). With a digit suffix
    ///     <c>0</c>–<c>9</c> that specifies a minimum field width: when <see cref="EpochValue"/>
    ///     equals <see cref="DEFAULT_EPOCH_VALUE"/> the field is zero-filled; when
    ///     <see cref="Epoch"/> is <see langword="null"/> and the digit is <c>0</c> nothing is
    ///     appended; otherwise the value is left-padded with zeros to the requested width.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term><c>U</c> / <c>U0</c></term>
    ///     <description><see cref="UpstreamVersion"/>.</description>
    ///   </item>
    ///   <item>
    ///     <term><c>U1</c></term>
    ///     <description>
    ///     <see cref="RevertedUpstreamVersion"/>, or nothing when it is <see langword="null"/>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term><c>U2</c></term>
    ///     <description>
    ///     The <c>+really</c> delimiter, but only when <see cref="RealUpstreamVersion"/> is not
    ///     <see langword="null"/>; otherwise nothing is appended.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term><c>U3</c></term>
    ///     <description>
    ///     <see cref="RealUpstreamVersion"/>, or nothing when it is <see langword="null"/>.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term><c>U4</c></term>
    ///     <description><see cref="EffectiveUpstreamVersion"/>.</description>
    ///   </item>
    ///   <item>
    ///     <term><c>R</c> / <c>R0</c></term>
    ///     <description><see cref="Revision"/>, or nothing when it is <see langword="null"/>.</description>
    ///   </item>
    ///   <item>
    ///     <term><c>R1</c></term>
    ///     <description><see cref="DebianRevision"/>, or nothing when it is <see langword="null"/>.</description>
    ///   </item>
    ///   <item>
    ///     <term><c>R2</c></term>
    ///     <description>
    ///     The <c>ubuntu</c> delimiter, but only when <see cref="UbuntuRevision"/> is not
    ///     <see langword="null"/>; otherwise nothing is appended.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term><c>R3</c></term>
    ///     <description><see cref="UbuntuRevision"/>, or nothing when it is <see langword="null"/>.</description>
    ///   </item>
    ///   <item>
    ///     <term><c>\'</c> / <c>\"</c> … <c>\'</c> / <c>\"</c></term>
    ///     <description>
    ///     Characters enclosed in matching single or double quotes are appended verbatim, allowing
    ///     any character (including specifier characters) to be used as literals.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term><c>\</c><em>c</em></term>
    ///     <description>
    ///     Backslash escape: the character immediately following the backslash is appended
    ///     verbatim.
    ///     </description>
    ///   </item>
    /// </list>
    /// <para>
    /// Any other character in the format string, an unclosed quote, a trailing backslash, or an
    /// unrecognised digit suffix on <c>U</c> or <c>R</c> causes a <see cref="FormatException"/>
    /// to be thrown.
    /// </para>
    /// </param>
    /// <param name="formatProvider">This parameter is currently unused.</param>
    /// <returns>A string representation of the current instance in the requested format.</returns>
    /// <exception cref="FormatException">
    /// <paramref name="format"/> contains an invalid or unrecognised specifier.
    /// </exception>
    /// <example>
    /// <code>
    /// var v = DpkgVersion.Parse("1:2+really1-3ubuntu4");
    ///
    /// v.ToString("\"Version: '\"E:U-R'\\''")         // → "Version: '1:2+really1-3ubuntu4'"  (canonical form)
    /// v.ToString("E3:U-R")        // → "001:2+really1-3ubuntu4" (epoch padded to 3 digits)
    /// v.ToString("U1U2U3")        // → "2+really1"             (reverted + delimiter + real)
    /// v.ToString("R1R2R3")        // → "3ubuntu4"              (debian + literal + ubuntu)
    /// </code>
    /// </example>
    public virtual string ToString(string? format, IFormatProvider? formatProvider = null)
    {
        if (string.IsNullOrEmpty(format) || format == "G") return ToString();

        var version = new StringBuilder();

        char? escapeCharacter = null;
        for (int i = 0; i < format.Length; ++i)
        {
            var currentCharacter = format[i];

            if (currentCharacter == '\\')
            {
                if (++i >= format.Length) goto fail;
                version.Append(format[i]);
            }
            else if (escapeCharacter.HasValue)
            {
                if (currentCharacter == escapeCharacter.Value)
                {
                    escapeCharacter = null;
                }
                else
                {
                    version.Append(currentCharacter);
                }
            }
            else if (currentCharacter is '\'' or '\"')
            {
                escapeCharacter = currentCharacter;
            }
            else if (currentCharacter == EPOCH_DELIMITER)
            {
                if (Epoch is not null)
                {
                    version.Append(EPOCH_DELIMITER);
                }
            }
            else if (currentCharacter == REVISION_DELIMITER)
            {
                if (Revision is not null)
                {
                    version.Append(REVISION_DELIMITER);
                }
            }
            else if (currentCharacter == 'G')
            {
                version.Append(_originalString);
            }
            else if (currentCharacter == 'E')
            {
                if (TryParseNextAsDigit(format, i, out var digits))
                {
                    ++i;

                    if (Epoch is not null)
                    {
                        var epoch = Epoch.PadLeft(digits, '0');
                        version.Append(epoch);
                    }
                }
                else
                {
                    version.Append(Epoch);
                }
            }
            else if (currentCharacter == 'e')
            {
                if (TryParseNextAsDigit(format, i, out var digits))
                {
                    ++i;

                    if (EpochValue == DEFAULT_EPOCH_VALUE)
                    {
                        version.Append('0', digits);
                    }
                    else if (Epoch is not null || digits > 0)
                    {
                        var number = EpochValue.ToString().PadLeft(digits, '0');
                        version.Append(number);
                    }
                }
                else
                {
                    version.Append(EpochValue);
                }
            }
            else if (currentCharacter == 'U')
            {
                if (TryParseNextAsDigit(format, i, out var type))
                {
                    ++i;
                }

                if (type == 0)
                {
                    version.Append(UpstreamVersion);
                }
                else if (type == 1)
                {
                    version.Append(RevertedUpstreamVersion);
                }
                else if (type == 2)
                {
                    if (RealUpstreamVersion is not null)
                    {
                        version.Append(REAL_UPSTREAM_VERSION_DELIMITER);
                    }
                }
                else if (type == 3)
                {
                    version.Append(RealUpstreamVersion);
                }
                else if (type == 4)
                {
                    version.Append(EffectiveUpstreamVersion);
                }
                else
                {
                    goto fail;
                }
            }
            else if (currentCharacter == 'R')
            {
                if (TryParseNextAsDigit(format, i, out var type))
                {
                    ++i;
                }

                if (type == 0)
                {
                    version.Append(Revision);
                }
                else if (type == 1)
                {
                    version.Append(DebianRevision);
                }
                else if (type == 2)
                {
                    if (UbuntuRevision is not null)
                    {
                        version.Append(UBUNTU_REVISION_DELIMITER);
                    }
                }
                else if (type == 3)
                {
                    version.Append(UbuntuRevision);
                }
                else
                {
                    goto fail;
                }
            }
            else
            {
                goto fail;
            }
        }

        if (escapeCharacter.HasValue) goto fail;

        return version.ToString();

        fail:
        throw new FormatException($"The format string '{format}' is not in a correct format.");

        static bool TryParseNextAsDigit(string value, int offset, out int digit)
        {
            digit = 0;

            if (++offset >= value.Length) return false;

            var next = value[offset];
            if (!char.IsDigit(next)) return false;

            digit = next - '0';
            return true;
        }
    }

    /// <inheritdoc cref="Object.GetHashCode()"/>
    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode()
    {
        if (!_hashCode.HasValue)
        {
            var hashCode = new HashCode();
            hashCode.Add(EpochValue);
            AddPart(UpstreamVersion, ref hashCode);
            AddPart(Revision, ref hashCode);

            _hashCode = hashCode.ToHashCode();
        }

        return _hashCode.Value;

        static void AddPart(string? part, ref HashCode hashCode)
        {
            if (part is null) return;

            for (int i = 0; i < part.Length;)
            {
                for (;i < part.Length && !char.IsAsciiDigit(part[i]); ++i)
                {
                    hashCode.Add(part[i]);
                }

                // skip leading zeros
                for (++i; i < part.Length && part[i] == '0'; ++i);

                for (;i < part.Length && char.IsAsciiDigit(part[i]); ++i)
                {
                    hashCode.Add(part[i]);
                }
            }
        }
    }

    public static explicit operator string(DpkgVersion dpkgVersion) => dpkgVersion._originalString;
    public static explicit operator DpkgVersion(string debVersion) => Parse(debVersion, formatProvider: null);
}
