// Copyright (C) 2026 Canonical Ltd.
//
// SPDX-License-Identifier: GPL-3.0-only
//
// This program is free software: you can redistribute it and/or modify it under the terms of
// the GNU General Public License version 3, as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY
// WARRANTY; without even the implied warranties of MERCHANTABILITY, SATISFACTORY
// QUALITY, or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License
// for more details.
//
// You should have received a copy of the GNU General Public License along with this
// program.  If not, see http://www.gnu.org/licenses/.

using System.Collections.Immutable;

namespace Canonical.Dpkg;

// ReSharper disable InconsistentNaming
public static class DpkgMachineArchitectures
{
    /// <summary>
    /// A special wildcard architecture specification string that matches all Debian machine architectures and is the most frequently used.
    /// </summary>
    /// <seealso href="https://www.debian.org/doc/debian-policy/ch-controlfields.html#architecture"/>
    public static readonly DpkgMachineArchitecture any = new("any");

    /// <summary>
    /// A special architecture specification string that indicates a source package.
    /// </summary>
    /// <seealso href="https://www.debian.org/doc/debian-policy/ch-controlfields.html#architecture"/>
    public static readonly DpkgMachineArchitecture source = new("source");

    /// <summary>
    /// A special architecture specification string that indicates an architecture-independent package.
    /// </summary>
    /// <seealso href="https://www.debian.org/doc/debian-policy/ch-controlfields.html#architecture"/>
    public static readonly DpkgMachineArchitecture all = new("all");

    // ReSharper disable IdentifierTypo
    // ReSharper disable StringLiteralTypo
    public static readonly DpkgMachineArchitecture armhf = new("armhf");
    public static readonly DpkgMachineArchitecture armel = new("armel");
    public static readonly DpkgMachineArchitecture mipsn32 = new("mipsn32");
    public static readonly DpkgMachineArchitecture mipsn32el = new("mipsn32el");
    public static readonly DpkgMachineArchitecture mipsn32r6 = new("mipsn32r6");
    public static readonly DpkgMachineArchitecture mipsn32r6el = new("mipsn32r6el");
    public static readonly DpkgMachineArchitecture mips64 = new("mips64");
    public static readonly DpkgMachineArchitecture mips64el = new("mips64el");
    public static readonly DpkgMachineArchitecture mips64r6 = new("mips64r6");
    public static readonly DpkgMachineArchitecture mips64r6el = new("mips64r6el");
    public static readonly DpkgMachineArchitecture powerpcspe = new("powerpcspe");
    public static readonly DpkgMachineArchitecture x32 = new("x32");
    public static readonly DpkgMachineArchitecture alpha = new("alpha");
    public static readonly DpkgMachineArchitecture amd64 = new("amd64");
    public static readonly DpkgMachineArchitecture arc = new("arc");
    public static readonly DpkgMachineArchitecture armeb = new("armeb");
    public static readonly DpkgMachineArchitecture arm = new("arm");
    public static readonly DpkgMachineArchitecture arm64 = new("arm64");
    public static readonly DpkgMachineArchitecture hppa = new("hppa");
    public static readonly DpkgMachineArchitecture loong64 = new("loong64");
    public static readonly DpkgMachineArchitecture i386 = new("i386");
    public static readonly DpkgMachineArchitecture ia64 = new("ia64");
    public static readonly DpkgMachineArchitecture m68k = new("m68k");
    public static readonly DpkgMachineArchitecture mips = new("mips");
    public static readonly DpkgMachineArchitecture mipsel = new("mipsel");
    public static readonly DpkgMachineArchitecture mipsr6 = new("mipsr6");
    public static readonly DpkgMachineArchitecture mipsr6el = new("mipsr6el");
    public static readonly DpkgMachineArchitecture nios2 = new("nios2");
    public static readonly DpkgMachineArchitecture or1k = new("or1k");
    public static readonly DpkgMachineArchitecture powerpc = new("powerpc");
    public static readonly DpkgMachineArchitecture powerpcel = new("powerpcel");
    public static readonly DpkgMachineArchitecture ppc64 = new("ppc64");
    public static readonly DpkgMachineArchitecture ppc64el = new("ppc64el");
    public static readonly DpkgMachineArchitecture riscv64 = new("riscv64");
    public static readonly DpkgMachineArchitecture s390 = new("s390");
    public static readonly DpkgMachineArchitecture s390x = new("s390x");
    public static readonly DpkgMachineArchitecture sh3 = new("sh3");
    public static readonly DpkgMachineArchitecture sh3eb = new("sh3eb");
    public static readonly DpkgMachineArchitecture sh4 = new("sh4");
    public static readonly DpkgMachineArchitecture sh4eb = new("sh4eb");
    public static readonly DpkgMachineArchitecture sparc = new("sparc");
    public static readonly DpkgMachineArchitecture sparc64 = new("sparc64");

    public static readonly ImmutableSortedSet<DpkgMachineArchitecture> WellKnown = [
        armhf,
        armel,
        mipsn32,
        mipsn32el,
        mipsn32r6,
        mipsn32r6el,
        mips64,
        mips64el,
        mips64r6,
        mips64r6el,
        powerpcspe,
        x32,
        alpha,
        amd64,
        arc,
        armeb,
        arm,
        arm64,
        hppa,
        loong64,
        i386,
        ia64,
        m68k,
        mips,
        mipsel,
        mipsr6,
        mipsr6el,
        nios2,
        or1k,
        powerpc,
        powerpcel,
        ppc64,
        ppc64el,
        riscv64,
        s390,
        s390x,
        sh3,
        sh3eb,
        sh4,
        sh4eb,
        sparc,
        sparc64,
    ];
}
