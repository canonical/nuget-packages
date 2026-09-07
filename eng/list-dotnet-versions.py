#!/usr/bin/env python3
# Copyright (C) 2026 Canonical Ltd.
#
# SPDX-License-Identifier: GPL-3.0-only
#
# This program is free software: you can redistribute it and/or modify it under the terms of
# the GNU General Public License version 3, as published by the Free Software Foundation.
#
# This program is distributed in the hope that it will be useful, but WITHOUT ANY
# WARRANTY; without even the implied warranties of MERCHANTABILITY, SATISFACTORY
# QUALITY, or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License
# for more details.
#
# You should have received a copy of the GNU General Public License along with this
# program.  If not, see http://www.gnu.org/licenses/.

"""
Lists all version strings of the .NET source and binary packages
that were ever published in the
- Ubuntu archive,
- ppa:dotnet/backports and
- ppa:dotnet/previews.

For the source packages `dotnetX` (X in 6..11) the following files are
written (one version string per line, deduplicated and sorted like
eng/sort-dpkg-versions.py):

- dotnet-source-versions.txt  - versions of all dotnet* source packages
- dotnet-binary-versions.txt  - versions of all dotnet* binary packages
- dotnet-runtime-versions.txt - versions of all dotnet-runtime-*.0 binary packages
- dotnet-sdk-versions.txt     - versions of all dotnet-sdk-*.0 binary packages

Dependencies: launchpadlib, python3-apt
"""

import argparse
import functools
import sys

import apt_pkg
from launchpadlib.launchpad import Launchpad

DOTNET_VERSIONS = range(6, 12)


def binary_package_names(dotnet_version):
    """Returns the binary package names of interest for a dotnet version."""
    yield "binary", f"dotnet{dotnet_version}"
    yield "runtime", f"dotnet-runtime-{dotnet_version}.0",
    yield "sdk", f"dotnet-sdk-{dotnet_version}.0",


def version_compare_with_string_fallback(v1, v2):
    """Compare versions, then by string if equal."""
    cmp = apt_pkg.version_compare(v1, v2)
    if cmp == 0:
        return -1 if v1 < v2 else (1 if v1 > v2 else 0)
    return cmp


def sort_versions(versions):
    return sorted(versions, key=functools.cmp_to_key(version_compare_with_string_fallback))


def get_archives(launchpad):
    yield "ubuntu archive", launchpad.distributions["ubuntu"].main_archive
    for ppa_name in ("backports", "previews"):
        yield f"ppa:dotnet/{ppa_name}", launchpad.people["dotnet"].getPPAByName(name=ppa_name)


def collect_versions(launchpad):
    versions = {
        "source": set(),
        "binary": set(),
        "runtime": set(),
        "sdk": set(),
    }

    for archive_description, archive in get_archives(launchpad):
        for dotnet_version in DOTNET_VERSIONS:
            source_package_name = f"dotnet{dotnet_version}"
            print(f"querying {archive_description} for {source_package_name} ...", file=sys.stderr)
            try:
                publications = archive.getPublishedSources(source_name=source_package_name)
                for publication in publications:
                    versions["source"].add(publication.source_package_version)
            except Exception as error:
                print(
                    f"warning: failed to query sources of {source_package_name} "
                    f"in {archive_description}: {error}",
                    file=sys.stderr,
                )
                continue

            for package_type, binary_package_name in binary_package_names(dotnet_version):
                try:
                    publications = archive.getPublishedBinaries(binary_name=binary_package_name, exact_match=True)
                    for publication in publications:
                        versions[package_type].add(publication.binary_package_version)
                except Exception as error:
                    print(
                        f"warning: failed to query binaries of {source_package_name} "
                        f"in {archive_description}: {error}",
                        file=sys.stderr,
                    )

    return versions


def write_versions_file(output_dir, file_name, versions):
    file_path = output_dir / file_name
    if not versions:
        print(f"warning: no versions found for {file_name.removesuffix('.txt')} "
              f"- writing empty file", file=sys.stderr)
    file_path.write_text("".join(f"{version}\n" for version in sort_versions(versions)))
    print(f"wrote {file_path} ({len(versions)} versions)", file=sys.stderr)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "-o", "--output-dir",
        default=".",
        help="directory the version list files are written to (default: current directory)",
    )
    args = parser.parse_args()

    apt_pkg.init()

    launchpad = Launchpad.login_anonymously(
        "list-dotnet-versions", "production", version="devel")

    version_sets = collect_versions(launchpad)

    from pathlib import Path
    output_dir = Path(args.output_dir)
    output_dir.mkdir(parents=True, exist_ok=True)

    for package_type, version_set in version_sets.items():
        write_versions_file(output_dir, f"dotnet-{package_type}-versions.txt", version_set)


if __name__ == "__main__":
    main()
