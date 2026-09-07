# Canonical.Apt.Common

[![NuGet](https://img.shields.io/nuget/vpre/Canonical.Apt.Common.svg)](https://www.nuget.org/packages/Canonical.Apt.Common/)
[![License: GPL-3.0-only](https://img.shields.io/badge/license-GPL--3.0--only-blue.svg)]([../../LICENSE](https://github.com/canonical/nuget-packages/blob/main/LICENSE))

> [!IMPORTANT]
> This packages is currently published as a **pre-release** version and the public API may still change. We provide no official support; issues are addressed on a best-effort basis.

Strongly-typed, immutable value types and parsers for APT package manager identifiers, so that
suites, series, pockets and components can be passed around as validated values instead of
`string`.

## Installation

```shell
dotnet add package Canonical.Apt.Common --prerelease
```

## Usage

```csharp
using Canonical.Apt;

// "noble-updates" == series "noble" + pocket "updates"
var suite = AptSuite.Parse("noble-updates");

Console.WriteLine(suite.Series);
Console.WriteLine(suite.Pocket);

if (AptComponent.TryParse("main", out var component))
    Console.WriteLine(component);
```

## What's in this package

| Type | Purpose |
| --- | --- |
| `AptSeries` | Release series identifier (e.g. `noble`, `bookworm`). |
| `AptPocket` | Archive pocket (e.g. `release`, `updates`, `security`, `proposed`, `backports`). |
| `AptSuite` | Combination of a series and a pocket (e.g. `noble-updates`). |
| `AptComponent` | Archive component (e.g. `main`, `universe`, `restricted`, `multiverse`). |

## Related packages

- [`Canonical.Dpkg.Common`](https://www.nuget.org/packages/Canonical.Dpkg.Common/) — dpkg
  package names, architectures and versions.
- [`Canonical.DistroInfo`](https://www.nuget.org/packages/Canonical.DistroInfo/) — Ubuntu and
  Debian release metadata built on top of these identifiers.

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
