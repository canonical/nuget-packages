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

import sys
import apt_pkg
import functools

def version_compare_with_string_fallback(v1, v2):
    """Compare versions, then by string if equal."""
    cmp = apt_pkg.version_compare(v1, v2)
    if cmp == 0:
        return -1 if v1 < v2 else (1 if v1 > v2 else 0)
    return cmp

def main():
    apt_pkg.init()
    versions = sorted(set(sys.stdin.read().split()), key=functools.cmp_to_key(version_compare_with_string_fallback))
    print('\n'.join(versions))

if __name__ == '__main__':
    main()
