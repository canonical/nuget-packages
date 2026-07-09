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
    IEquatable<DpkgVersion>
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
