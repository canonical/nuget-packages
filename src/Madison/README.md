# Canonical.Madison

[![NuGet](https://img.shields.io/nuget/vpre/Canonical.Madison.svg)](https://www.nuget.org/packages/Canonical.Madison/)
[![License: GPL-3.0-only](https://img.shields.io/badge/license-GPL--3.0--only-blue.svg)]([../../LICENSE](https://github.com/canonical/nuget-packages/blob/main/LICENSE))

> [!IMPORTANT]
> This packages is currently published as a **pre-release** version and the public API may still change. We provide no official support; issues are addressed on a best-effort basis.

Typed HTTP client for the Madison package version lookup service — the .NET equivalent of
[`rmadison(1)`](https://manpages.debian.org/unstable/devscripts/rmadison.1.en.html). Query the
Debian and Ubuntu archives for package versions by suite, architecture and component, using
pre-configured endpoints for the Ubuntu and Debian archives, the Debian QA tables, the Debian
NEW queue and more.

## Installation

```shell
dotnet add package Canonical.Madison --prerelease
```

## Usage

```csharp
using System.Collections.Immutable;
using Canonical.Madison;

using var httpClient = new HttpClient
{
    BaseAddress = WellKnownMadisonEndpoints.Ubuntu,
};
var madison = new MadisonClient(httpClient);

var request = new MadisonRequest(
    PackageNames: ImmutableList.Create("dotnet9"),
    Suites: ImmutableList.Create("noble", "noble-updates"),
    Architectures: ImmutableList.Create("amd64"));

foreach (var entry in await madison.QueryAsync(request))
{
    Console.WriteLine($"{entry.PackageName} {entry.Version} {entry.Series}/{entry.Component}");
    Console.WriteLine($"  architectures: {string.Join(", ", entry.Architectures)}");
}
```

Results can also be streamed as they are read, which avoids buffering large responses:

```csharp
await foreach (var entry in madison.QueryAndEnumerateAsync(request, cancellationToken: ct))
{
    Console.WriteLine(entry);
}
```

The endpoint can be supplied per call instead of via `HttpClient.BaseAddress`, which makes it
easy to compare archives with a single client:

```csharp
var inUbuntu = await madison.QueryAsync(request, WellKnownMadisonEndpoints.Ubuntu);
var inDebian = await madison.QueryAsync(request, WellKnownMadisonEndpoints.Debian);
```

### Dependency injection

```csharp
services.AddHttpClient<MadisonClient>(client =>
    client.BaseAddress = WellKnownMadisonEndpoints.Ubuntu);
```

## What's in this package

| Type | Purpose |
| --- | --- |
| `MadisonClient` | Sends queries and parses the response; buffered (`QueryAsync`) or streaming (`QueryAndEnumerateAsync`). |
| `MadisonRequest` | Query parameters: package names, suites, components, architectures, binary type, source/binary expansion and regex matching. |
| `MadisonResponseEntry` | One parsed result row: package name, version, suite, series, component and architectures. Implements `IParsable<T>` / `ISpanParsable<T>`. |
| `WellKnownMadisonEndpoints` | Pre-configured endpoints. |

### Well-known endpoints

`Ubuntu`, `Debian`, `DebianNew`, `DebianQa`, `DebianQaNew`, `DebianQaPorts`,
`DebianQaArchived`, `DebianQaAll`, `DebianQaUbuntu`, `DebianJanitor`,
`UltimateDebianDatabase`.

> [!NOTE]
> `DebianNew` and `DebianQaNew` already pin the queried table/suite in their URI — do not set
> `Suites` yourself when using them.

> [!WARNING]
> `TreatPackagesNamesAsRegex` is rejected by most public endpoints (including `qa.debian.org`)
> because a broad pattern such as `.` can easily overload the backing database.


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
