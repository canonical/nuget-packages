# Canonical.DistroInfo

[![NuGet](https://img.shields.io/nuget/vpre/Canonical.DistroInfo.svg)](https://www.nuget.org/packages/Canonical.DistroInfo/)
[![License: GPL-3.0-only](https://img.shields.io/badge/license-GPL--3.0--only-blue.svg)]([../../LICENSE](https://github.com/canonical/nuget-packages/blob/main/LICENSE))

> [!IMPORTANT]
> This packages is currently published as a **pre-release** version and the public API may still change. We provide no official support; issues are addressed on a best-effort basis.

Library for querying Linux distribution release metadata. Reads distro-info-data CSV files at runtime to provide Ubuntu and Debian release information, including support status, ESM/LTS lifecycle queries and well-known identifiers.

## Prerequisites

The CSV files are read at runtime from `/usr/share/distro-info/` by default:

```shell
sudo apt install distro-info-data
```

Pass an explicit path if the data lives elsewhere. If you cannot depend on the data being
installed (for example in a minimal container or on Windows), use
[`Canonical.DistroInfo.Static`](https://www.nuget.org/packages/Canonical.DistroInfo.Static/)
instead, which embeds the same data at compile time.

## Installation

```shell
dotnet add package Canonical.DistroInfo --prerelease
```

## Usage

Query the Ubuntu release lifecycle:

```csharp
using Canonical.DistroInfo.Ubuntu;

var releases = await UbuntuReleaseInfoCollection.ReadFromDistroInfoDataAsync();

Console.WriteLine(releases.GetLatestReleasedLts());
// Ubuntu 26.04 LTS (Resolute Raccoon)

/*
  {
    Series: {resolute}
    Codename: "Resolute Raccoon"
    Version: "26.04 LTS"
    ShortVersion: "26.04"
    IsLts: true
    Created: {09/10/2025}
    Released: {23/04/2026}
    EndOfStandardSupport: {29/05/2031}
    EndOfServerStandardSupport: {29/05/2031}
    EndOfExpandedSecurityMaintenance: {23/04/2036}
    EndOfLegacyMaintenance: {30/04/2041}
    EndOfLife: {30/04/2041}
  }
*/

Console.WriteLine(releases.GetDevel());
// Ubuntu 26.10 (Stonking Stingray)

/*
  {
    Series: {stonking}
    Codename: "Stonking Stingray"
    Version: "26.10"
    ShortVersion: "26.10"
    IsLts: false
    Created: {24/04/2026}
    Released: {15/10/2026}
    EndOfStandardSupport: {15/07/2027}
    EndOfServerStandardSupport: null
    EndOfExpandedSecurityMaintenance: null
    EndOfLegacyMaintenance: null
    EndOfLife: {15/07/2027}
  }
*/

// Everything still receiving standard support today.
foreach (var release in collection.GetAllStandardSupported())
{
    Console.WriteLine(release);
}
// Ubuntu 22.04 LTS (Jammy Jellyfish)
// Ubuntu 24.04 LTS (Noble Numbat)
// Ubuntu 26.04 LTS (Resolute Raccoon)

// Everything on Expanded Security Maintenance (ESM) today.
foreach (var release in collection.GetAllEsmSupported())
{
    Console.WriteLine(release);
}
// Ubuntu 18.04 LTS (Bionic Beaver)
// Ubuntu 20.04 LTS (Focal Fossa

// Everything on Expanded Security Maintenance (ESM) with the legecy add-on today.
foreach (var release in collection.GetAllLegacySupported())
{
    Console.WriteLine(release);
}
// Ubuntu 14.04 LTS (Trusty Tahr)
// Ubuntu 16.04 LTS (Xenial Xerus)

foreach (var release in collection.GetAllSupported())
{
    Console.WriteLine(release);
}
// Ubuntu 14.04 LTS (Trusty Tahr)
// Ubuntu 16.04 LTS (Xenial Xerus)
// Ubuntu 18.04 LTS (Bionic Beaver)
// Ubuntu 20.04 LTS (Focal Fossa)
// Ubuntu 22.04 LTS (Jammy Jellyfish)
// Ubuntu 24.04 LTS (Noble Numbat)
// Ubuntu 26.04 LTS (Resolute Raccoon)

// Look up by series identifier or by an alias such as "devel".
if (releases.TryGetBySeries("noble", out var noble))
    Console.WriteLine($"{noble.Version} ({noble.Codename})");

// Look up by version.
if (releases.TryGetByVersion("22.04", out var jammy))
    Console.WriteLine($"{jammy.Version} ({jammy.Codename})");

// Works with "LTS" suffix too.
if (releases.TryGetByVersion("20.04 LTS", out var focal))
    Console.WriteLine($"{focal.Version} ({focal.Codename})");
```

Every lifecycle query has an overload taking a date, so historical and future states can be evaluated deterministically — which also makes these APIs easy to unit test:

```csharp
var status = noble.GetSupportStatus(new DateOnly(2030, 1, 1));
// UbuntuSupportStatus.ExpandedSecurityMaintenance
```

Debian works the same way, including the familiar suite aliases:

```csharp
using Canonical.DistroInfo.Debian;

var debian = await DebianReleaseInfoCollection.ReadFromDistroInfoDataAsync();

Console.WriteLine(debian.GetStable());
Console.WriteLine(debian.GetTesting());
Console.WriteLine(debian.GetOldStable());
```

## What's in this package

- `DistroReleaseInfoCollection<T>`, `IDistroReleaseInfo` — shared abstractions.
- Ubuntu: `UbuntuReleaseInfo`, `UbuntuReleaseInfoCollection`, `UbuntuSupportStatus`,
  `UbuntuArchitectures`, `UbuntuComponents`, `UbuntuPockets`.
- Debian: `DebianReleaseInfo`, `DebianReleaseInfoCollection`, `DebianSupportStatus`,
  `DebianArchitectures`, `DebianComponents`, `DebianPockets`.

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
