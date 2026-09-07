# Canonical NuGet packages

[![CI](https://github.com/canonical/nuget-packages/actions/workflows/ci.yaml/badge.svg)](https://github.com/canonical/nuget-packages/actions/workflows/ci.yaml)
[![License: GPL-3.0-only](https://img.shields.io/badge/license-GPL--3.0--only-blue.svg)](./LICENSE)

Reusable .NET libraries extracted from Canonical projects.

> [!IMPORTANT]
> These packages are currently published as **pre-release** versions and the public API may
> still change. We provide no official support; issues are addressed on a best-effort basis.


## Motivation

[Flamenco](https://github.com/canonical/flamenco) and the [.NET Snap](https://github.com/canonical/dotnet-snap) contain useful functionality that could benefit other projects. This repository extracts some of that functionality and packages it as reusable NuGet libraries and making it easier to consume in future projects. We provide no official support; issues are addressed on a best-effort basis.

## Packages

| Package | Version | Description |
| --- | --- | --- |
| [`Canonical.Dpkg.Common`](./src/Dpkg.Common/README.md) | [![NuGet](https://img.shields.io/nuget/vpre/Canonical.Dpkg.Common.svg)](https://www.nuget.org/packages/Canonical.Dpkg.Common/) | dpkg primitives: package names, machine architectures and `deb-version(7)` versions. |
| [`Canonical.Apt.Common`](./src/Apt.Common/README.md) | [![NuGet](https://img.shields.io/nuget/vpre/Canonical.Apt.Common.svg)](https://www.nuget.org/packages/Canonical.Apt.Common/) | APT identifiers: components, pockets, series and suites. |
| [`Canonical.DistroInfo`](./src/DistroInfo/README.md) | [![NuGet](https://img.shields.io/nuget/vpre/Canonical.DistroInfo.svg)](https://www.nuget.org/packages/Canonical.DistroInfo/) | Ubuntu and Debian release metadata read from distro-info-data at runtime. |
| [`Canonical.DistroInfo.Static`](./src/DistroInfo.Static/README.md) | [![NuGet](https://img.shields.io/nuget/vpre/Canonical.DistroInfo.Static.svg)](https://www.nuget.org/packages/Canonical.DistroInfo.Static/) | Compile-time snapshot of the distro-info data, no runtime file dependency. |
| [`Canonical.Madison`](./src/Madison/README.md) | [![NuGet](https://img.shields.io/nuget/vpre/Canonical.Madison.svg)](https://www.nuget.org/packages/Canonical.Madison/) | Typed HTTP client for the Madison package version lookup service (`rmadison(1)`). |
| [`Canonical.Common`](./src/Common/README.md) | [![NuGet](https://img.shields.io/nuget/vpre/Canonical.Common.svg)](https://www.nuget.org/packages/Canonical.Common/) | Shared building blocks used by the packages above; usually referenced transitively. |

<details>
<summary>Public types per package</summary>

**Canonical.Dpkg.Common**

- `DpkgMachineArchitecture`
- `DpkgMachineArchitectures`
- `DpkgPackageName`
- `DpkgVersion`

**Canonical.Apt.Common**

- `AptComponent`
- `AptPocket`
- `AptSeries`
- `AptSuite`

**Canonical.DistroInfo**

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

**Canonical.DistroInfo.Static**

- `DebianReleases`
- `UbuntuReleases`

**Canonical.Madison**

- `MadisonClient`
- `MadisonRequest`
- `MadisonResponse`
- `WellKnownMadisonEndpoints`

</details>

## Build from source

prerequisite: [install .NET 10 SDK](https://ubuntu.com/developers/docs/howto/dotnet-setup/)

```shell
dotnet pack -c Release -o ./artifacts
```

## Contributing and support

These libraries are maintained on a best-effort basis and are not covered by Canonical support
agreements. Bug reports and pull requests are welcome via
[GitHub Issues](https://github.com/canonical/nuget-packages/issues) and
[pull requests](https://github.com/canonical/nuget-packages/pulls).

## License

Copyright 2026 Canonical Ltd.

SPDX-License-Identifier: GPL-3.0-only

This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License version 3, as published by the Free Software Foundation.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranties of MERCHANTABILITY, SATISFACTORY QUALITY, or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program.  If not, see http://www.gnu.org/licenses/.
