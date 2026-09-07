# Canonical NuGet packages

Reusable .NET libraries extracted from Canonical projects.

## Motivation

[Flamenco](https://github.com/canonical/flamenco) and the [.NET Snap](https://github.com/canonical/dotnet-snap) contain useful functionality that could benefit other projects. This repository extracts some of that functionality and packages it as reusable NuGet libraries and making it easier to consume in future projects. We provide no official support; issues are addressed on a best-effort basis.

## Libraries

### Canonical.Madison

[![#](https://img.shields.io/nuget/v/Canonical.Madison.svg)](https://www.nuget.org/packages/Canonical.Madison/)

Typed HTTP client for the Madison package version lookup service (.NET equivalent of rmadison(1)). Supports querying Debian and Ubuntu archives for package versions by suite, architecture, and component, with pre-configured endpoints for Ubuntu, Debian, and Debian QA mirrors.

- `MadisonClient`
- `MadisonRequest`
- `MadisonResponse`
- `WellKnownMadisonEndpoints`

### Canonical.Dpkg.Common

[![#](https://img.shields.io/nuget/v/Canonical.Dpkg.Common.svg)](https://www.nuget.org/packages/Canonical.Dpkg.Common/)

Strongly-typed immutable value types, parsers, and comparison logic for core dpkg primitives: package names, machine architectures, and the full deb-version(7) version format.

- `DpkgMachineArchitecture`
- `DpkgMachineArchitectures`
- `DpkgPackageName`
- `DpkgVersion`

### Canonical.Apt.Common

[![#](https://img.shields.io/nuget/v/Canonical.Apt.Common.svg)](https://www.nuget.org/packages/Canonical.Apt.Common/)

Strongly-typed, immutable value types and parsers for APT package manager identifiers.

- `AptComponent`
- `AptPocket`
- `AptSeries`
- `AptSuite`

### Canonical.DistroInfo

[![#](https://img.shields.io/nuget/v/Canonical.DistroInfo.svg)](https://www.nuget.org/packages/Canonical.DistroInfo/)

Library for querying Linux distribution release metadata. Reads distro-info-data CSV files at runtime to provide Ubuntu and Debian release information, including support status, ESM/LTS lifecycle queries and well-known identifiers.

- `DistroReleaseInfoCollection`
- `IDistroReleaseInfo`
- Debian
  - `DebianArchitectures`
  - `DebianComponents`
  - `DebianPockets`
  - `DebianReleaseInfo`
  - `DebianReleaseInfoCollection`
- Ubuntu
  - `UbuntuArchitectures`
  - `UbuntuComponents`
  - `UbuntuPockets`
  - `UbuntuReleaseInfo`
  - `UbuntuReleaseInfoCollection`

### Canonical.DistroInfo.Static

[![#](https://img.shields.io/nuget/v/Canonical.DistroInfo.Static.svg)](https://www.nuget.org/packages/Canonical.DistroInfo.Static/)

Compile-time complement to `Canonical.DistroInfo`. Provides static instances of the release data from the distro-info CSV files, eliminating the runtime file dependency.

- `DebianReleases`
- `UbuntuReleases`


## License

Copyright 2026 Canonical Ltd.

SPDX-License-Identifier: GPL-3.0-only

This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License version 3, as published by the Free Software Foundation.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranties of MERCHANTABILITY, SATISFACTORY QUALITY, or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program.  If not, see http://www.gnu.org/licenses/.
