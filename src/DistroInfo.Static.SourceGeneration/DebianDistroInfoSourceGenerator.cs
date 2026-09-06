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
using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.CSharp.SymbolDisplay;

namespace Canonical.DistroInfo.Static.SourceGeneration;

[Generator]
public sealed class DebianDistroInfoSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var debianCsvFile = context.AdditionalTextsProvider
            .Where(additionalText => Path.GetFileName(additionalText.Path).Equals("debian.csv"))
            .Collect();

        context.RegisterSourceOutput(debianCsvFile, GenerateSource);
    }

    private static void GenerateSource(
        SourceProductionContext context,
        ImmutableArray<AdditionalText> debianCsvFiles)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        switch (debianCsvFiles.Length)
        {
            case 0:
                return;
            case > 1:
                context.ReportMultipleDebianCsvFilesFound(debianCsvFiles);
                return;
        }

        string path = debianCsvFiles[0].Path;
        var names = new List<string>();
        foreach ((int lineNumber, var row) in CsvReader.ReadRows(debianCsvFiles[0], context))
        {
            if (!row.TryGetValue("version", out var version))
            {
                context.ReportRowIsMissingRequiredColumn(lineNumber, path, missingColumn: "version");
                return;
            }
            else if (string.IsNullOrWhiteSpace(version))
            {
                version = null;
            }

            if (!row.TryGetValue("codename", out var codename) || string.IsNullOrWhiteSpace(codename))
            {
                context.ReportRowIsMissingRequiredColumn(lineNumber, path, missingColumn: "codename");
                return;
            }

            if (!row.TryGetValue("series", out var series) || string.IsNullOrWhiteSpace(series))
            {
                context.ReportRowIsMissingRequiredColumn(lineNumber, path, missingColumn: "series");
                return;
            }

            if (!row.TryGetValue("created", out var created) || string.IsNullOrWhiteSpace(created))
            {
                context.ReportRowIsMissingRequiredColumn(lineNumber, path, missingColumn: "created");
                return;
            }

            row.TryGetValue("release", out var release);
            row.TryGetValue("eol", out var eol);
            row.TryGetValue("eol-lts", out var eolLts);
            row.TryGetValue("eol-elts", out var eolELts);

            string name = codename.Replace(" ", "");
            names.Add(name);

            context.AddSource(hintName: $"DebianReleases.{name}.g.cs", $$"""
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

                using Canonical.Apt;

                namespace Canonical.DistroInfo.Debian;

                public static partial class DebianReleases
                {
                    public static DebianReleaseInfo {{name}} = new DebianReleaseInfo(
                        version: {{( version is null ? "null" : FormatLiteral(version, quote: true))}},
                        codename: {{FormatLiteral(codename, quote: true)}},
                        series: AptSeries.Parse({{FormatLiteral(series, quote: true)}}),
                        created: {{created.AsDateOnlyLiteral()}},
                        released: {{release.AsDateOnlyLiteral()}},
                        endOfStandardSupport: {{eol.AsDateOnlyLiteral()}},
                        endOfLongTermSupport: {{eolLts.AsDateOnlyLiteral()}},
                        endOfExtendedLongTermSupport: {{eolELts.AsDateOnlyLiteral()}});
                }
                """);
        }

        context.CancellationToken.ThrowIfCancellationRequested();

        context.AddSource(hintName: "DebianReleases.g.cs", $$"""
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

            namespace Canonical.DistroInfo.Debian;

            public static partial class DebianReleases
            {
                public static readonly ImmutableArray<DebianReleaseInfo> All = [ {{ string.Join(", ", names) }} ];
            }

            """);
    }
}
