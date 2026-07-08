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

using System.Reflection;
using Xunit.Sdk;

namespace Canonical.Dpkg.UnitTests;

public class FileDataAttribute : DataAttribute
{
    private readonly string _filePath;

    public FileDataAttribute(string filePath)
    {
        _filePath = filePath;
    }

    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        var path = Path.IsPathRooted(_filePath)
            ? _filePath
            : Path.Combine(AppContext.BaseDirectory, _filePath);

        foreach (var line in File.ReadLines(path))
        {
            yield return new object[] { line };
        }
    }
}
