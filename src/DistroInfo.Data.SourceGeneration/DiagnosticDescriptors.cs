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

namespace Canonical.DistroInfo.Data.SourceGeneration;

internal static class DiagnosticDescriptors
{
    private const string CATEGORY = "DistroInfoDataSourceGenerator";

    public static readonly DiagnosticDescriptor MultipleUbuntuCsvFilesFound = new (
        id: "DID001",
        category: CATEGORY,
        title: "multiple distro-info ubuntu.csv files found",
        messageFormat: "More than one distro-info ubuntu.csv file were found",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MultipleDebianCsvFilesFound = new (
        id: "DID002",
        category: CATEGORY,
        title: "multiple distro-info debian.csv files found",
        messageFormat: "More than one distro-info debian.csv file were found",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor CsvUnreadable = new (
        id: "DID003",
        category: CATEGORY,
        title: "CSV file is unreadable",
        messageFormat: "The CSV file '{0}' could not be read",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UndefinedColumn = new (
        id: "DID004",
        category: CATEGORY,
        title: "CSV column with undefined name",
        messageFormat: "Row {0} of '{1}' contains more columns than defined in the header",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor RowIsMissingRequiredColumn = new (
        id: "DID005",
        category: CATEGORY,
        title: "CSV row is missing required column",
        messageFormat: "Row {0} of '{1}' is missing a value for the required column '{2}'",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}

internal static class DiagnosticsHelper
{
    public static void ReportMultipleUbuntuCsvFilesFound(this SourceProductionContext context, ImmutableArray<AdditionalText> csvFiles)
    {
        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.MultipleUbuntuCsvFilesFound,
            location: csvFiles.First().GetLocation(),
            additionalLocations: csvFiles.Skip(1).Select(file => file.GetLocation())));
    }

    public static void ReportMultipleDebianCsvFilesFound(this SourceProductionContext context, ImmutableArray<AdditionalText> csvFiles)
    {
        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.MultipleDebianCsvFilesFound,
            location: csvFiles.First().GetLocation(),
            additionalLocations: csvFiles.Skip(1).Select(file => file.GetLocation())));
    }

    public static void ReportCsvUnreadable(this SourceProductionContext context, string filePath)
    {
        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.CsvUnreadable,
            Location.None,
            filePath));
    }

    public static void ReportUndefinedColumn(this SourceProductionContext context, string filePath, int lineNumber)
    {
        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.UndefinedColumn,
            Location.None,
            lineNumber, filePath));
    }

    public static void ReportRowIsMissingRequiredColumn(
        this SourceProductionContext context,
        int lineNumber,
        string filePath,
        string missingColumn)
    {
        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.RowIsMissingRequiredColumn ,
            Location.None,
            lineNumber, filePath, missingColumn));
    }
}
