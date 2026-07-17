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
using System.Text;
using Microsoft.CodeAnalysis;

using static Microsoft.CodeAnalysis.CSharp.SymbolDisplay;

namespace Canonical.DistroInfo.Data.SourceGeneration;

[Generator]
public sealed class UbuntuDistroInfoSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var ubuntuCsvFile = context.AdditionalTextsProvider
            .Where(additionalText => Path.GetFileName(additionalText.Path).Equals("ubuntu.csv"))
            .Collect();

        context.RegisterSourceOutput(ubuntuCsvFile, GenerateSource);
    }

    private static void GenerateSource(
        SourceProductionContext context,
        ImmutableArray<AdditionalText> ubuntuCsvFiles)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        switch (ubuntuCsvFiles.Length)
        {
            case 0:
                return;
            case > 1:
                context.ReportMultipleUbuntuCsvFilesFound(ubuntuCsvFiles);
                return;
        }

        var sourceText = new StringBuilder();

        string path = ubuntuCsvFiles[0].Path;
        var names = new List<string>();
        foreach ((int lineNumber, var row) in CsvReader.ReadRows(ubuntuCsvFiles[0], context))
        {
            if (!row.TryGetValue("version", out var version) || string.IsNullOrWhiteSpace(version))
            {
                context.ReportRowIsMissingRequiredColumn(lineNumber, path, missingColumn: "version");
                continue;
            }

            bool isLts = false;
            if (version.EndsWith(" LTS"))
            {
                isLts = true;
                version = version.Substring(startIndex: 0, length: version.Length - 4);
            }

            if (!row.TryGetValue("codename", out var codename)|| string.IsNullOrWhiteSpace(codename))
            {
                context.ReportRowIsMissingRequiredColumn(lineNumber, path, missingColumn: "codename");
                continue;
            }

            if (!row.TryGetValue("series", out var series) || string.IsNullOrWhiteSpace(series))
            {
                context.ReportRowIsMissingRequiredColumn(lineNumber, path, missingColumn: "series");
                continue;
            }

            if (!row.TryGetValue("created", out var created) || string.IsNullOrWhiteSpace(created))
            {
                context.ReportRowIsMissingRequiredColumn(lineNumber, path, missingColumn: "created");
                continue;
            }

            if (!row.TryGetValue("release", out var release) || string.IsNullOrWhiteSpace(release))
            {
                context.ReportRowIsMissingRequiredColumn(lineNumber, path, missingColumn: "release");
                continue;
            }

            if (!row.TryGetValue("eol", out var eol) || string.IsNullOrWhiteSpace(eol))
            {
                context.ReportRowIsMissingRequiredColumn(lineNumber, path, missingColumn: "eol");
                continue;
            }

            row.TryGetValue("eol-server", out var eolServer);

            if (eol.Equals(eolServer))
            {
                eolServer = null;
            }

            row.TryGetValue("eol-esm", out var eolEsm);

            string name = codename.Replace(" ", "");
            names.Add(name);

            sourceText.Clear();
            sourceText.Append("Ubuntu ").Append(version);
            if (isLts) sourceText.Append(" LTS");
            sourceText.Append(" (").Append(codename).Append(')');

            context.AddSource(hintName: $"UbuntuReleases.{name}.g.cs", $$"""
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

                namespace Canonical.DistroInfo.Ubuntu;

                public static partial class UbuntuReleases
                {
                    public static UbuntuReleaseInfo {{name}} = new UbuntuReleaseInfo(
                        version: {{FormatLiteral(version, quote: true)}},
                        isLts: {{(isLts ? "true" : "false")}},
                        codename: {{FormatLiteral(codename, quote: true)}},
                        series: {{FormatLiteral(series, quote: true)}},
                        created: {{created.AsDateOnlyLiteral()}},
                        released: {{release.AsDateOnlyLiteral()}},
                        endOfStandardSupport: {{eol.AsDateOnlyLiteral()}},
                        endOfServerStandardSupport: {{eolServer.AsDateOnlyLiteral()}},
                        endOfExpandedSecurityMaintenance: {{eolEsm.AsDateOnlyLiteral()}},
                        endOfLife: {{(eolEsm ?? eolServer ?? eol).AsDateOnlyLiteral()}},
                        stringRepresentation: {{FormatLiteral(sourceText.ToString(), quote: true)}});
                }
                """);
        }

        context.CancellationToken.ThrowIfCancellationRequested();

        sourceText.Clear();
        sourceText.AppendLine($$"""
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

            namespace Canonical.DistroInfo.Ubuntu;

            public static partial class UbuntuReleases
            {
                public static readonly ImmutableArray<UbuntuReleaseInfo> All = [ {{ string.Join(", ", names) }} ];
            }

            """);

        context.AddSource(hintName: "UbuntuReleases.g.cs", sourceText.ToString());
    }
}
