# Canonical.DistroInfo.Static

[![NuGet](https://img.shields.io/nuget/vpre/Canonical.DistroInfo.Static.svg)](https://www.nuget.org/packages/Canonical.DistroInfo.Static/)
[![License: GPL-3.0-only](https://img.shields.io/badge/license-GPL--3.0--only-blue.svg)]([../../LICENSE](https://github.com/canonical/nuget-packages/blob/main/LICENSE))

> [!IMPORTANT]
> This packages is currently published as a **pre-release** version and the public API may still change. We provide no official support; issues are addressed on a best-effort basis.

Compile-time complement to
[`Canonical.DistroInfo`](https://www.nuget.org/packages/Canonical.DistroInfo/). Provides static
instances of the release data from the distro-info CSV files, eliminating the runtime file
dependency — useful in minimal containers, on non-Debian hosts, and in unit tests that need
deterministic data.

## Installation

```shell
dotnet add package Canonical.DistroInfo.Static --prerelease
```

## Usage

```csharp
using Canonical.DistroInfo.Ubuntu;

// No file access and no async: the data is baked into the assembly.
var noble = UbuntuReleases.NobleNumbat;

Console.WriteLine(noble.Version);       // 24.04 LTS
Console.WriteLine(noble.ShortVersion);  // 24.04
Console.WriteLine(noble.IsLts);         // True

// The full collection exposes the same query API as Canonical.DistroInfo.
var lts = UbuntuReleases.Collection.GetLatestReleasedLts();

if (UbuntuReleases.Collection.TryGetBySeries("noble", out var release))
    Console.WriteLine(release == noble); // True
```

`UbuntuReleases.Collection` is an `UbuntuReleaseInfoCollection`, so every lifecycle query from
`Canonical.DistroInfo` (`GetDevel`, `GetAllStandardSupported`, `GetAllEsmSupported`, …) behaves
identically — only the data source differs.

## Choosing between the two packages

| | `Canonical.DistroInfo` | `Canonical.DistroInfo.Static` |
| --- | --- | --- |
| Data source | distro-info-data CSV files, read at runtime | snapshot embedded at build time |
| Requires `distro-info-data` | yes | no |
| Picks up new releases | automatically, when the system data is updated | only by upgrading this package |
| API shape | `async` loading | direct static access |

Both can be combined: read from disk when the data is available and fall back to the embedded
snapshot otherwise.

## What's in this package

- `UbuntuReleases` — named release constants (e.g. `NobleNumbat`) plus `Collection`.

> [!NOTE]
> Static Debian data (`DebianReleases`) is not populated yet.

## Contributing and support

This library is maintained on a best-effort basis and are not covered by Canonical support agreements. Bug reports and pull requests are welcome via
[GitHub Issues](https://github.com/canonical/nuget-packages/issues) and
[pull requests](https://github.com/canonical/nuget-packages/pulls).

## License

Copyright 2026 Canonical Ltd.

SPDX-License-Identifier: GPL-3.0-only

This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License version 3, as published by the Free Software Foundation.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranties of MERCHANTABILITY, SATISFACTORY QUALITY, or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program.  If not, see http://www.gnu.org/licenses/.
