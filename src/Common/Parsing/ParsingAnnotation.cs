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
/// Represents information that provides further context for a parsing result.
/// </summary>
/// <param name="Identifier">
/// A unique identifier for the type of <see cref="ParsingAnnotation"/>.
/// </param>
/// <param name="Title">
/// A short title describing the <see cref="ParsingAnnotation"/> type
/// without context specific content.
/// </param>
/// <param name="Description">
/// A message that describes the <see cref="ParsingAnnotation"/> type in detail
/// without context specific content and may suggest actions to proceed.
/// </param>
/// <param name="Message">
/// A message that describes the <see cref="ParsingAnnotation"/> instance and
/// may contain context specific content.
/// </param>
/// <param name="Locations">
/// Specific <see cref="Range"/>'s in the provided value where the
/// <see cref="ParsingAnnotation"/> refers to.
/// </param>
/// <param name="HelpLink">
/// An optional hyperlink that provides more detailed information regarding
/// the <see cref="ParsingAnnotation"/>.
/// </param>
public record ParsingAnnotation(
    string Identifier,
    string Title,
    string? Description,
    string Message,
    IImmutableList<Range> Locations,
    Uri? HelpLink)
{
    public override string ToString()
    {
        var message = new StringBuilder();
        ToString(message);
        return message.ToString();
    }

    internal void ToString(StringBuilder value, bool isListItem = false)
    {
        if (isListItem)
        {
            value.Append("- ");
        }

        value.Append('[').Append(Identifier).Append("] ").Append(Title);

        if (Locations.Count > 0)
        {
            value.Append(" (at ").AppendJoin(", ", Locations.Select(l => l.ToLocationString())).Append(')');
        }

        value.Append(": ").Append(Message);

        if (Description is not null)
        {
            value.Append(isListItem ? "\n  " : "\n")
                 .Append(Description);
        }

        if (HelpLink is not null)
        {
            value.Append(isListItem ? "\n  See also: " : "\nSee also: ")
                 .Append(HelpLink);
        }
    }

    public static ParsingAnnotation Create(
        ParsingAnnotationDescriptor descriptor)
    {
        return new ParsingAnnotation(
            Identifier: descriptor.Identifier,
            Title: descriptor.Title,
            Description: descriptor.Description,
            Message: descriptor.MessageFormat,
            Locations: ImmutableList<Range>.Empty,
            HelpLink: descriptor.HelpLink);
    }

    public static ParsingAnnotation Create(
        ParsingAnnotationDescriptor descriptor,
        Range location)
    {
        return new ParsingAnnotation(
            Identifier: descriptor.Identifier,
            Title: descriptor.Title,
            Description: descriptor.Description,
            Message: descriptor.MessageFormat,
            Locations: [ location ],
            HelpLink: descriptor.HelpLink);
    }

    public static ParsingAnnotation Create(
        ParsingAnnotationDescriptor descriptor,
        IImmutableList<Range> locations)
    {
        return new ParsingAnnotation(
            Identifier: descriptor.Identifier,
            Title: descriptor.Title,
            Description: descriptor.Description,
            Message: descriptor.MessageFormat,
            Locations: locations,
            HelpLink: descriptor.HelpLink);
    }

    public static ParsingAnnotation Create(
        ParsingAnnotationDescriptor descriptor,
        params ReadOnlySpan<object?> messageArgs)
    {
        return new ParsingAnnotation(
            Identifier: descriptor.Identifier,
            Title: descriptor.Title,
            Description: descriptor.Description,
            Message: string.Format(descriptor.MessageFormat, messageArgs),
            Locations: ImmutableList<Range>.Empty,
            HelpLink: descriptor.HelpLink);
    }

    public static ParsingAnnotation Create(
        ParsingAnnotationDescriptor descriptor,
        Range location,
        params ReadOnlySpan<object?> messageArgs)
    {
        return new ParsingAnnotation(
            Identifier: descriptor.Identifier,
            Title: descriptor.Title,
            Description: descriptor.Description,
            Message: string.Format(descriptor.MessageFormat, messageArgs),
            Locations: ImmutableList.Create(location),
            HelpLink: descriptor.HelpLink);
    }

    public static ParsingAnnotation Create(
        ParsingAnnotationDescriptor descriptor,
        IImmutableList<Range> locations,
        params ReadOnlySpan<object?> messageArgs)
    {
        return new ParsingAnnotation(
            Identifier: descriptor.Identifier,
            Title: descriptor.Title,
            Description: descriptor.Description,
            Message: string.Format(descriptor.MessageFormat, messageArgs),
            Locations: locations,
            HelpLink: descriptor.HelpLink);
    }
}

/// <summary>
/// A template for a <see cref="ParsingAnnotation"/>.
/// </summary>
/// <param name="Identifier">The value for <see cref="ParsingAnnotation.Identifier"/>.</param>
/// <param name="Title">The value for <see cref="ParsingAnnotation.Title"/>.</param>
/// <param name="Description">The value for <see cref="ParsingAnnotation.Description"/>.</param>
/// <param name="HelpLink">The value for <see cref="ParsingAnnotation.HelpLink"/>.</param>
public sealed record ParsingAnnotationDescriptor(
    string Identifier,
    string Title,
    string MessageFormat,
    string? Description = null,
    Uri? HelpLink = null);

