# Canonical.Dpkg.Common

[![NuGet](https://img.shields.io/nuget/vpre/Canonical.Dpkg.Common.svg)](https://www.nuget.org/packages/Canonical.Dpkg.Common/)
[![License: GPL-3.0-only](https://img.shields.io/badge/license-GPL--3.0--only-blue.svg)]([../../LICENSE](https://github.com/canonical/nuget-packages/blob/main/LICENSE))

> [!IMPORTANT]
> This packages is currently published as a **pre-release** version and the public API may still change. We provide no official support; issues are addressed on a best-effort basis.

Strongly-typed immutable value types, parsers, and comparison logic for core dpkg primitives: package names, machine architectures, and the full deb-version(7) version format.

## Installation

```shell
dotnet add package Canonical.Dpkg.Common --prerelease
```

## Usage

Compare two Debian package versions using `deb-version(7)` ordering:

```csharp
using Canonical.Dpkg;

var installed = DpkgVersion.Parse("1:2.3.4-1ubuntu2");
var candidate = DpkgVersion.Parse("1:2.3.4-1ubuntu10");

Console.WriteLine(installed < candidate); // True
```

Parse dpkg version strings and access detailed information:

```csharp
using Canonical.Dpkg;

var candidate = DpkgVersion.Parse("1:2.3.4+really3.0-1ubuntu10");

DpkgVersion.Parse("8.0.100-8.0.0~rc1+really7.0.100-7.0.0~beta1~bootstrap+amd64-0ubuntu1")
// {
//    Epoch: null
//    EpochValue: 0
//    UpstreamVersion: "8.0.100-8.0.0~rc1+really7.0.100-7.0.0~beta1~bootstrap+amd64"
//    RevertedUpstreamVersion: "8.0.100-8.0.0~rc1"
//    RealUpstreamVersion: "7.0.100-7.0.0~beta1~bootstrap+amd64"
//    EffectiveUpstreamVersion: "7.0.100-7.0.0~beta1~bootstrap+amd64"
//    Revision: "0ubuntu1"
//    DebianRevision: "0"
//    UbuntuRevision: "1"
// }
```

Parse/Validate dpkg package names:

```
// Validating parse without exceptions.
if (DpkgPackageName.TryParse("dotnet-sdk-10.0", out var name))
    Console.WriteLine(name);
```

Type-safe well known architectures, e.g.:

```
var architecture = DpkgMachineArchitectures.Amd64;
```

## What's in this package

| Type | Purpose |
| --- | --- |
| `DpkgVersion` | Parsing, formatting and `deb-version(7)`-compliant comparison of package versions. |
| `DpkgPackageName` | Validated dpkg package name. |
| `DpkgMachineArchitecture` | Validated dpkg machine architecture. |
| `DpkgMachineArchitectures` | Well-known machine architecture values. |


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
