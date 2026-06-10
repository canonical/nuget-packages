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

namespace Canonical.Madison;

public static class MadisonResponseExtensions
{
    public static IEnumerable<IGrouping<string,MadisonResponseEntry>> GroupByPackageName(
        this IEnumerable<MadisonResponseEntry> packageReleaseEntries)
    {
        return packageReleaseEntries.GroupBy(entry => entry.PackageName);
    }
    
    public static IEnumerable<IGrouping<string,MadisonResponseEntry>> GroupBySuite(
        this IEnumerable<MadisonResponseEntry> packageReleaseEntries)
    {
        return packageReleaseEntries.GroupBy(entry => entry.Suite);
    }
    
    public static IEnumerable<IGrouping<string,MadisonResponseEntry>> GroupBySeries(
        this IEnumerable<MadisonResponseEntry> packageReleaseEntries)
    {
        return packageReleaseEntries.GroupBy(entry => entry.Series);
    }
    
    public static IEnumerable<IGrouping<string,MadisonResponseEntry>> GroupByComponent(
        this IEnumerable<MadisonResponseEntry> packageReleaseEntries)
    {
        return packageReleaseEntries.GroupBy(entry => entry.Component);
    }
    
    public static IEnumerable<IGrouping<string,MadisonResponseEntry>> GroupByArchitecture(
        this IEnumerable<MadisonResponseEntry> packageReleaseEntries)
    {
        return packageReleaseEntries
            .SelectMany(entry => entry.Architectures.Select(arch => (arch, entry)))
            .GroupBy(x => x.arch, x => x.entry);
    }
}