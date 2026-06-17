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

namespace Canonical.Common.Parsing;

/// <summary>
/// The <see cref="FormatException"/> that gets thrown when a string value could not be parsed correctly.
/// </summary>
public class ParsingException : FormatException
{
    /// <summary>
    /// The value that failed to parse.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Additional details containing errors/warnings about why <see cref="Value"/> could not be parsed.
    /// </summary>
    public IImmutableList<ParsingAnnotation> Annotations { get; }

    public ParsingException(
        string? message,
        string value,
        IImmutableList<ParsingAnnotation> annotations,
        Exception? innerException = null) : base(BuildExceptionMessage(message, value, annotations), innerException)
    {
        Value = value;
        Annotations = annotations;
    }

    private static string BuildExceptionMessage(
        string? message,
        string value,
        IImmutableList<ParsingAnnotation> annotations)
    {
        var result = new StringBuilder();

        if (message is null)
        {
            result.AppendLine("Failed to parse value '").Append(value).Append("'.");
        }
        else
        {
            result.Append(message);
        }

        for (int i = 0; i < annotations.Count; ++i)
        {
            result.Append(i == 0 ? "\n\nAnnotations:\n" : "\n");
            annotations[i].ToString(result, isListItem: true);
        }

        result.AppendLine();

        return result.ToString();
    }
}
