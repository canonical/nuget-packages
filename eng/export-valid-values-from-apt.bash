#!/usr/bin/env bash
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

set -e
set -u
set -o pipefail
set -x

apt list 2>/dev/null \
  | awk 'NF>1 {sub(/\/.*/, "", $1); print $1}' \
  | sort -u \
  > "dpkg-names.txt"

apt list 2>/dev/null \
  | awk 'NF>1 {print $2}' \
  | sort -u \
  | python3 "${ROOT_DIR}/eng/sort-dpkg-versions.py" \
  > "dpkg-versions_sorted.txt"
