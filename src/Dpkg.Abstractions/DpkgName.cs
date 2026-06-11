using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Canonical.Dpkg;

/// <summary>
/// Represents an immutable instance of the name of a debian package.
/// </summary>
public readonly record struct DpkgName : ISpanParsable<DpkgName>
{
    internal DpkgName(string identifier)
    {
        Identifier = identifier;
    }

    /// <summary>
    /// Gets the identifier of the debian package.
    /// </summary>
    public string Identifier { get; }

    /// <inheritdoc />
    public override int GetHashCode() => Identifier.GetHashCode();

    /// <inheritdoc />
    public override string ToString() => Identifier;

    public static implicit operator string(DpkgName dpkgName) => dpkgName.Identifier;
    public static explicit operator DpkgName(string value) => Parse(value, DpkgParsingErrorHandling.ThrowAfterProcessingAll)!.Value;
    public static explicit operator DpkgName(Span<char> value) => Parse(value, DpkgParsingErrorHandling.ThrowAfterProcessingAll)!.Value;

    /// <summary>
    /// Parses a string representation of a debian package name and performs validation.
    /// </summary>
    /// <param name="value">The string representation of the debian package name.</param>
    /// <param name="errorHandling">How the parser should deal with invalid version strings.</param>
    /// <returns>The parsed and validated dpkg name.</returns>
    /// <exception cref="MalformedDpkgNameException">When <paramref name="value"/> is not a valid dpkg name.</exception>
    public static DpkgName? Parse(ReadOnlySpan<char> value, DpkgParsingErrorHandling errorHandling)
    {
        var invalidCharacters = ImmutableList<(char invalidCharacter, int position)>.Empty;

        if (value.IsEmpty)
        {
            return errorHandling is DpkgParsingErrorHandling.ReturnDefault
                ? null
                : throw new MalformedDpkgNameException(
                    message: "Package name is empty.",
                    packageName: value.ToString(),
                    invalidCharacters: invalidCharacters);
        }

        if (!char.IsAsciiLetterLower(value[0]) && !char.IsAsciiDigit(value[0]))
        {
            invalidCharacters = invalidCharacters.Add((value[0], 0));
        }

        for (var position = 1; position < value.Length; ++position)
        {
            char currentCharacter = value[position];

            if (!char.IsAsciiLetterLower(currentCharacter)
                && !char.IsAsciiDigit(currentCharacter)
                && currentCharacter != '-'
                && currentCharacter != '.'
                && currentCharacter != '+')
            {
                if (errorHandling is DpkgParsingErrorHandling.ReturnDefault)
                {
                    return null;
                }
                if (errorHandling is DpkgParsingErrorHandling.ThrowAtFirstError)
                {
                    throw new MalformedDpkgNameException(
                        message: "Package name is invalid.",
                        packageName: value.ToString(),
                        invalidCharacters: [(currentCharacter, position)]);
                }

                invalidCharacters = invalidCharacters.Add((currentCharacter, position));
            }
        }

        if (invalidCharacters.Count > 0)
        {
            return errorHandling == DpkgParsingErrorHandling.ReturnDefault
                ? null
                : throw new MalformedDpkgNameException(
                    message: "Package name contains not allowed characters.",
                    packageName: value.ToString(),
                    invalidCharacters: invalidCharacters);
        }

        return new DpkgName(value.ToString());
    }

    /// <inheritdoc />
    public static DpkgName Parse(string value, IFormatProvider? formatProvider = null)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Parse(value.AsSpan(), DpkgParsingErrorHandling.ThrowAfterProcessingAll)!.Value;
    }

    /// <inheritdoc />
    public static bool TryParse([NotNullWhen(true)] string? value, IFormatProvider? formatProvider, out DpkgName result)
    {
        if (value is null)
        {
            result = default;
            return false;
        }

        var parsed = Parse(value.AsSpan(), DpkgParsingErrorHandling.ReturnDefault);
        result = parsed ?? default;
        return parsed.HasValue;
    }

    /// <inheritdoc />
    public static DpkgName Parse(ReadOnlySpan<char> value, IFormatProvider? formatProvider = null)
    {
        return Parse(value, DpkgParsingErrorHandling.ThrowAfterProcessingAll)!.Value;
    }

    /// <inheritdoc />
    public static bool TryParse(ReadOnlySpan<char> value, IFormatProvider? formatProvider, out DpkgName result)
    {
        var parsed = Parse(value, DpkgParsingErrorHandling.ReturnDefault);
        result = parsed ?? default;
        return parsed.HasValue;
    }

    /// <summary>Tries to parse a string into a value.</summary>
    /// <param name="value">The string to parse.</param>
    /// <param name="result">When this method returns, contains the result of successfully parsing <paramref name="value" /> or an undefined value on failure.</param>
    /// <returns>
    /// <see langword="true" /> if <paramref name="value" /> was successfully parsed; otherwise, <see langword="false" />.</returns>
    public static bool TryParse([NotNullWhen(true)] string? value, out DpkgName result)
    {
        if (value is null)
        {
            result = default;
            return false;
        }

        var parsed = Parse(value.AsSpan(), DpkgParsingErrorHandling.ReturnDefault);
        result = parsed ?? default;
        return parsed.HasValue;
    }

    /// <summary>Tries to parse a span of characters into a value.</summary>
    /// <param name="value">The span of characters to parse.</param>
    /// <param name="result">When this method returns, contains the result of successfully parsing <paramref name="value" />, or an undefined value on failure.</param>
    /// <returns>
    /// <see langword="true" /> if <paramref name="value" /> was successfully parsed; otherwise, <see langword="false" />.</returns>
    public static bool TryParse(ReadOnlySpan<char> value, out DpkgName result)
    {
        var parsed = Parse(value, DpkgParsingErrorHandling.ReturnDefault);
        result = parsed ?? default;
        return parsed.HasValue;
    }
}

public class MalformedDpkgNameException : FormatException
{
    public string PackageName { get; }
    public ImmutableList<(char InvalidCharacter, int Position)> InvalidCharacters { get; }

    public MalformedDpkgNameException(
        string message,
        string packageName,
        ImmutableList<(char InvalidCharacter, int Position)> invalidCharacters)
        : base(message)
    {
        PackageName = packageName;
        InvalidCharacters = invalidCharacters;
    }
}
