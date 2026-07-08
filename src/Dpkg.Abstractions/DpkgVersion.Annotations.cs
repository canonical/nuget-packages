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
using Canonical.Common.Parsing;

namespace Canonical.Dpkg;

public partial class DpkgVersion
{
    private static readonly ParsingAnnotationDescriptor EmptyVersion = new(
        Identifier: "DPKG-VERSION-001",
        Title: "Empty version",
        MessageFormat: "Dpkg version string is empty.");

    #region Epoch

    private static readonly ParsingAnnotationDescriptor EmptyEpoch = new(
        Identifier: "DPKG-VERSION-002",
        Title: "Empty epoch",
        MessageFormat: $"Dpkg version string has no value before epoch delimiter '{EPOCH_DELIMITER}'.");

    private static readonly ParsingAnnotationDescriptor InvalidEpochCharacters = new(
        Identifier: "DPKG-VERSION-003",
        Title: "Invalid epoch characters",
        MessageFormat: "Epoch value '{0}' contains invalid characters: {1}.",
        Description: "The epoch value has to be an unsigned number.");

    private static readonly ParsingAnnotationDescriptor EpochValueTooLarge = new(
        Identifier: "DPKG-VERSION-004",
        Title: "Epoch value too large",
        MessageFormat: "Numerical value of epoch '{0}' is too large.",
        Description: $"The epoch value has to be between 0 and {int.MaxValue} (inclusive). " +
                     "This is a technical limitation of the dpkg-dev tooling.");
    #endregion

    #region Upstream Version

    private static readonly ParsingAnnotationDescriptor EmptyUpstreamVersion = new(
        Identifier: "DPKG-VERSION-005",
        Title: "Empty upstream version",
        MessageFormat: "Dpkg version does not specify an upstream version.");

    private static readonly ParsingAnnotationDescriptor InvalidUpstreamVersionCharacters = new(
        Identifier: "DPKG-VERSION-006",
        Title: "Invalid upstream version characters",
        MessageFormat: "Upstream version value '{0}' contains invalid characters: {1}.",
        Description: "An upstream version must consist only of lowercase letters (a-z), " +
                     "digits (0-9), plus (+), and minus (-), and colon (:), and tilde (~) signs.");

    private const string REAL_UPSTREAM_VERSION_DELIMITER_DESCRIPTION =
        $"The '{REAL_UPSTREAM_VERSION_DELIMITER}' delimiter within the upstream component of" +
        "a version string is a convention to indicate that the package contains a different " +
        "version. This is for example used to roll back an upload. In this situation the " +
        "version number can not simply be lowered, because the package manager ignores all " +
        "packages with a lower version number than the already installed package version.";

    private static readonly ParsingAnnotationDescriptor MultipleRealUpstreamVersionDelimiter = new(
        Identifier: "DPKG-VERSION-007",
        Title: $"Multiple '{REAL_UPSTREAM_VERSION_DELIMITER}' delimiter",
        MessageFormat: $"Dpkg version string contains multiple '{REAL_UPSTREAM_VERSION_DELIMITER}' delimiter.",
        Description: REAL_UPSTREAM_VERSION_DELIMITER_DESCRIPTION +
                     $" The '{REAL_UPSTREAM_VERSION_DELIMITER}' delimiter should only occur once.");

    private static readonly ParsingAnnotationDescriptor EmptyRevertedUpstreamVersion = new(
        Identifier: "DPKG-VERSION-008",
        Title: "Empty reverted upstream version",
        MessageFormat: "The upstream version '{0}' contains no value before the " +
                       $"'{REAL_UPSTREAM_VERSION_DELIMITER}' delimiter.",
        Description: REAL_UPSTREAM_VERSION_DELIMITER_DESCRIPTION +
                     "If you use this delimiter, you should specify the reverted (old) " +
                     "version before the delimiter.");

    private static readonly ParsingAnnotationDescriptor EmptyRealUpstreamVersion = new(
        Identifier: "DPKG-VERSION-009",
        Title: "Empty real upstream version",
        MessageFormat: "The upstream version '{0}' contains no value after the " +
                       $"'{REAL_UPSTREAM_VERSION_DELIMITER}' delimiter.",
        Description: REAL_UPSTREAM_VERSION_DELIMITER_DESCRIPTION +
                     "If you use this delimiter, you should specify the real (new) " +
                     "version after the delimiter.");
    #endregion

    #region Revision

    private const string UBUNTU_REVISION_DELIMITER_DESCRIPTION =
        $"The '{UBUNTU_REVISION_DELIMITER}' delimiter within the debian revision component of a version string " +
        $"is a convention to indicate changes which are only applied to Ubuntu packages.";

    private static readonly ParsingAnnotationDescriptor EmptyRevision = new(
        Identifier: "DPKG-VERSION-010",
        Title: "Empty revision",
        MessageFormat: "Dpkg version does not specify a revision version.");

    private static readonly ParsingAnnotationDescriptor InvalidRevisionCharacters = new(
        Identifier: "DPKG-VERSION-011",
        Title: "Invalid revision characters",
        MessageFormat: "Revision value '{0}' contains invalid characters: {1}.",
        Description: "A revision must consist only of lowercase letters (a-z), " +
                     "digits (0-9), plus (+), and periods (.) and tilde (~) signs.");

    private static readonly ParsingAnnotationDescriptor MultipleUbuntuRevisionDelimiter = new(
        Identifier: "DPKG-VERSION-012",
        Title: $"Multiple '{UBUNTU_REVISION_DELIMITER}' delimiter",
        MessageFormat: $"Dpkg version string contains multiple '{UBUNTU_REVISION_DELIMITER}' delimiter.",
        Description: UBUNTU_REVISION_DELIMITER_DESCRIPTION + " This delimiter should only occur once.");

    private static readonly ParsingAnnotationDescriptor EmptyDebianRevision = new(
        Identifier: "DPKG-VERSION-013",
        Title: "Empty Debian revision",
        MessageFormat: "The revision '{0}' contains no value before the " +
                       $"'{UBUNTU_REVISION_DELIMITER}' delimiter.",
        Description: UBUNTU_REVISION_DELIMITER_DESCRIPTION +
                     " If you use this delimiter, you should specify the Debian revision" +
                     " before the delimiter. This can be a 0 if there exists no package in Debian yet.");

    private static readonly ParsingAnnotationDescriptor EmptyUbuntuRevision = new(
        Identifier: "DPKG-VERSION-014",
        Title: "Empty Ubuntu revision",
        MessageFormat: "The revision '{0}' contains no value after the " +
                       $"'{UBUNTU_REVISION_DELIMITER}' delimiter.",
        Description: UBUNTU_REVISION_DELIMITER_DESCRIPTION +
                     " If you use this delimiter, you should specify the Ubuntu revision" +
                     " after the delimiter. This can be a 1 if this is your first iteration.");

    #endregion
}

public sealed class MalformedDpkgVersionException : ParsingException
{
    public MalformedDpkgVersionException(
        string value,
        ImmutableList<ParsingAnnotation> annotations)
        : base(message: $"Failed to parse dpkg version '{value}'.", value: value, annotations: annotations)
    {
        HelpLink = "https://manpages.ubuntu.com/manpages/en/man7/deb-version.7.html";
    }
}
