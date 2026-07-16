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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Canonical.Common.SourceGeneration;

[Generator]
public sealed class IdentifierSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<StructDeclarationSyntax> structDeclarations =
            context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is StructDeclarationSyntax { BaseList.Types.Count: > 0 },
                transform: static (ctx, _) => GetStructIfImplementsIIdentifier1(ctx))
            .Where(static s => s is not null)!;

        // Pair each candidate with the Compilation so the emitter has full type info.
        IncrementalValueProvider<(Compilation, ImmutableArray<StructDeclarationSyntax>)> compilationAndStructs =
            context.CompilationProvider.Combine(structDeclarations.Collect());

        // Emit one partial file per qualifying struct.
        context.RegisterSourceOutput(compilationAndStructs, static (spc, source) =>
            Execute(source.Item1, source.Item2, spc));
    }

    private static StructDeclarationSyntax? GetStructIfImplementsIIdentifier1(GeneratorSyntaxContext ctx)
    {
        if (ctx.Node is not StructDeclarationSyntax structDeclaration ||
            ctx.SemanticModel.GetDeclaredSymbol(structDeclaration) is not INamedTypeSymbol typeSymbol)
        {
            return null;
        }

        INamedTypeSymbol? identifierInterfaceSymbol =
            ctx.SemanticModel.Compilation
            .GetTypeByMetadataName("Canonical.Common.IIdentifier`1");

        if (identifierInterfaceSymbol is null)
        {
            return null;
        }

        foreach (INamedTypeSymbol interfaceSymbol in typeSymbol.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(interfaceSymbol.OriginalDefinition, identifierInterfaceSymbol))
            {
                return structDeclaration;
            }
        }

        return null;
    }

    private static void Execute(
        Compilation compilation,
        ImmutableArray<StructDeclarationSyntax> structs,
        SourceProductionContext context)
    {
        var processedTypes = new HashSet<string>();

        foreach (var structDeclaration in structs)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            var semanticModel = compilation.GetSemanticModel(structDeclaration.SyntaxTree);

            if (semanticModel.GetDeclaredSymbol(structDeclaration) is not INamedTypeSymbol typeSymbol)
            {
                continue;
            }

            var hintName = $"{typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.g.cs"
                .Replace("global::", string.Empty)
                .Replace('<', '[')
                .Replace('>', ']');

            // skip other partial declarations of same type
            if (!processedTypes.Add(hintName)) continue;

            var source = GenerateSource(typeSymbol);
            context.AddSource(hintName, source);
        }
    }

    private static string GenerateSource(INamedTypeSymbol typeSymbol)
    {
        string typeName = typeSymbol.Name;

        var sourceText = new StringBuilder();

        AppendFileHeader(sourceText);

        sourceText.Append(
            """
            #nullable enable


            """);

        AppendNamespaceDeclaration(sourceText, typeSymbol.ContainingNamespace);

        sourceText.Append(
            $$"""
            public readonly partial struct {{typeName}} : global::System.IComparable<{{typeName}}>, global::System.IEquatable<{{typeName}}>
            {

            """);

        AppendConstructor(sourceText, typeSymbol);
        AppendDeconstructMethod(sourceText);
        AppendGetHashCode(sourceText);
        AppendToString(sourceText);
        AppendEqualsMethods(sourceText, typeName);
        AppendCompareToMethods(sourceText, typeName);
        AppendParseToMethods(sourceText, typeName);
        AppendEqualityOperators(sourceText, typeName);
        AppendConversionOperators(sourceText, typeName);
        AppendCreateParsingExceptionDeclaration(sourceText);

        sourceText.AppendLine("}");

        return sourceText.ToString();
    }

    private static void AppendFileHeader(StringBuilder sourceText)
    {
        sourceText.Append(
            """
            // <auto-generated/>
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


            """);
    }

    private static void AppendNamespaceDeclaration(StringBuilder sourceText, INamespaceSymbol namespaceSymbol)
    {
        if (namespaceSymbol.IsGlobalNamespace) return;

        string namespaceName = namespaceSymbol.ToDisplayString();
        sourceText.Append(
            $"""
            namespace {namespaceName};


            """);

    }

    private static void AppendConstructor(StringBuilder sourceText, INamedTypeSymbol typeSymbol)
    {
        string typeName = typeSymbol.Name;

        var constructors = typeSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(static m => m is { IsImplicitlyDeclared: false, MethodKind: MethodKind.Constructor });

        // ReSharper disable once PossibleMultipleEnumeration
        bool hasParameterlessConstructor = constructors.Any(static c => c.Parameters.IsEmpty);
        // ReSharper disable once PossibleMultipleEnumeration
        bool hasCustomConstructors = constructors.Any(static c => !c.Parameters.IsEmpty);

        sourceText.Append(
            """
                private readonly string _identifier;


            """);

        if (!hasParameterlessConstructor)
        {
            sourceText.Append(
                $$"""
                    public {{typeName}}()
                    {
                        _identifier = string.Empty;
                    }


                """);
        }

        if (!hasCustomConstructors)
        {
            sourceText.Append(
                $$"""
                    internal {{typeName}}(string identifier)
                    {
                        _identifier = identifier;
                    }


                """);
        }

        sourceText.Append(
            """
                [global::System.Diagnostics.Contracts.PureAttribute]
                public string Identifier => _identifier;


            """);
    }

    private static void AppendDeconstructMethod(StringBuilder sourceText)
    {
        sourceText.Append(
            """
                [global::System.Diagnostics.Contracts.PureAttribute]
                public void Deconstruct(out string identifier) => identifier = _identifier;


            """);
    }

    private static void AppendGetHashCode(StringBuilder sourceText)
    {
        sourceText.Append(
            """
                /// <inheritdoc cref="object.GetHashCode()"/>
                [global::System.Diagnostics.Contracts.PureAttribute]
                public override int GetHashCode() => _identifier.GetHashCode();


            """);
    }

    private static void AppendToString(StringBuilder sourceText)
    {
        sourceText.Append(
            """
                /// <inheritdoc cref="object.ToString()"/>
                [global::System.Diagnostics.Contracts.PureAttribute]
                public override string ToString() => _identifier;


            """);
    }

    private static void AppendEqualsMethods(StringBuilder sourceText, string typeName)
    {
        sourceText.Append(
            $$"""
                [global::System.Diagnostics.Contracts.PureAttribute]
                public override bool Equals([global::System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(returnValue: true)] object? other)
                {
                  if (other is null) return false;

                  var otherIdentifier = other switch
                  {
                      {{typeName}} id => id._identifier,
                      string id => id,
                      _ => other.ToString()
                  };

                  return string.Equals(_identifier, otherIdentifier, global::System.StringComparison.Ordinal);
                }

                [global::System.Diagnostics.Contracts.PureAttribute]
                public bool Equals(global::System.ReadOnlySpan<char> other)
                {
                    return other.Equals(_identifier, global::System.StringComparison.Ordinal);
                }

                [global::System.Diagnostics.Contracts.PureAttribute]
                public bool Equals([global::System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(returnValue: true)] string? other)
                {
                    return string.Equals(_identifier, other, global::System.StringComparison.Ordinal);
                }

                [global::System.Diagnostics.Contracts.PureAttribute]
                public bool Equals({{typeName}} other)
                {
                    return string.Equals(_identifier, other._identifier, global::System.StringComparison.Ordinal);
                }

                [global::System.Diagnostics.Contracts.PureAttribute]
                public bool Equals([global::System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(returnValue: true)] {{typeName}}? other)
                {
                    return other.HasValue && string.Equals(_identifier, other.Value._identifier, global::System.StringComparison.Ordinal);
                }


            """);
    }

    private static void AppendCompareToMethods(StringBuilder sourceText, string typeName)
    {
        sourceText.Append(
            $$"""
                public int CompareTo(object? obj)
                {
                    if (obj is null) return 1;

                    var otherIdentifier = obj switch
                    {
                        {{typeName}} id => id._identifier,
                        string id => id,
                        _ => obj.ToString()
                    };

                    return global::Canonical.Common.IdentifierComparer.Default.Compare(_identifier, otherIdentifier);
                }

                public int CompareTo(global::System.ReadOnlySpan<char> other) =>
                    global::Canonical.Common.IdentifierComparer.Default.Compare(_identifier.AsSpan(), other);

                public int CompareTo(string? other) =>
                   global::Canonical.Common.IdentifierComparer.Default.Compare(_identifier, other);

                public int CompareTo({{typeName}} other) =>
                    global::Canonical.Common.IdentifierComparer.Default.Compare(_identifier, other._identifier);

                public int CompareTo({{typeName}}? other) =>
                    other.HasValue
                    ? global::Canonical.Common.IdentifierComparer.Default.Compare(_identifier, other.Value._identifier)
                    : 1;


            """);
    }

    private static void AppendParseToMethods(StringBuilder sourceText, string typeName)
    {
        sourceText.Append(
            $$"""
                [global::System.Diagnostics.Contracts.PureAttribute]
                public static {{typeName}} Parse([global::System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] string? value, global::System.IFormatProvider? provider)
                {
                    global::System.ArgumentNullException.ThrowIfNull(value);
                    return Parse(value.AsSpan());
                }

                [global::System.Diagnostics.Contracts.PureAttribute]
                public static bool TryParse([global::System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] string? value, global::System.IFormatProvider? provider, out {{typeName}} result)
                {
                    if (value is not null) return TryParse(value, out result);

                    result = new();
                    return false;
                }

                [global::System.Diagnostics.Contracts.PureAttribute]
                public static {{typeName}} Parse(global::System.ReadOnlySpan<char> value, global::System.IFormatProvider? provider)
                {
                    return Parse(value);
                }

                [global::System.Diagnostics.Contracts.PureAttribute]
                public static bool TryParse(global::System.ReadOnlySpan<char> value, global::System.IFormatProvider? provider, out {{typeName}} result)
                {
                    return TryParse(value, out result);
                }

                [global::System.Diagnostics.Contracts.PureAttribute]
                public static {{typeName}} Parse(global::System.ReadOnlySpan<char> identifierSpan, bool failFast = false) =>
                    TryParse(identifierSpan, out var identifier, out var annotations, failFast)
                    ? identifier
                    : throw CreateParsingException(identifierSpan, annotations);

                [global::System.Diagnostics.Contracts.PureAttribute]
                public static {{typeName}} Parse(global::System.ReadOnlySpan<char> identifierSpan, out global::System.Collections.Immutable.ImmutableList<global::Canonical.Common.Parsing.ParsingAnnotation> annotations, bool failFast = false) =>
                    TryParse(identifierSpan, out var identifier, out annotations, failFast)
                    ? identifier
                    : throw CreateParsingException(identifierSpan, annotations);

                [global::System.Diagnostics.Contracts.PureAttribute]
                public static bool TryParse(global::System.ReadOnlySpan<char> identifierSpan, out {{typeName}} identifier) =>
                    TryParse(identifierSpan, out identifier, out _, failFast: true);


            """);
    }

    private static void AppendEqualityOperators(StringBuilder sourceText, string typeName)
    {
        sourceText.Append(
            $$"""
                [global::System.Diagnostics.Contracts.PureAttribute]
                public static bool operator ==({{typeName}} a, {{typeName}} b)
                {
                    return string.Equals(a._identifier, b._identifier, global::System.StringComparison.Ordinal);
                }

                [global::System.Diagnostics.Contracts.PureAttribute]
                public static bool operator !=({{typeName}} a, {{typeName}} b)
                {
                    return !string.Equals(a._identifier, b._identifier, global::System.StringComparison.Ordinal);
                }


            """);
    }

    private static void AppendConversionOperators(StringBuilder sourceText, string typeName)
    {
        sourceText.Append(
            $$"""
                public static implicit operator string({{typeName}} id) => id._identifier;

                [return: global::System.Diagnostics.CodeAnalysis.NotNullIfNotNullAttribute(nameof(id))]
                public static implicit operator string?({{typeName}}? id) => id?._identifier;

                public static explicit operator {{typeName}}(global::System.ReadOnlySpan<char> value) => Parse(value);

                public static explicit operator {{typeName}}(string value) => Parse(value.AsSpan());

                [return: global::System.Diagnostics.CodeAnalysis.NotNullIfNotNullAttribute(nameof(value))]
                public static explicit operator {{typeName}}?(string? value) => value is not null ? Parse(value.AsSpan()) : null;


            """);
    }

    private static void AppendCreateParsingExceptionDeclaration(StringBuilder sourceText)
    {
        sourceText.Append(
            """
                private static partial global::Canonical.Common.Parsing.ParsingException CreateParsingException(
                    global::System.ReadOnlySpan<char> identifierSpan,
                    global::System.Collections.Immutable.ImmutableList<global::Canonical.Common.Parsing.ParsingAnnotation> annotations);

            """);
    }
}
