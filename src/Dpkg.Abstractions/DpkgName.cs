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
    public static explicit operator DpkgName(string value) => Parse(value, throwOnError: true)!.Value;
    public static explicit operator DpkgName(Span<char> value) => Parse(value, throwOnError: true)!.Value;

    /// <summary>
    /// Parses a string representation of a debian package name and performs validation.
    /// </summary>
    /// <param name="value">The string representation of the debian package name.</param>
    /// <param name="throwOnError"><see langword="true"/> if the method should throw if <paramref name="value"/> is invalid; otherwise return <see langword="null"/>.</param>
    /// <param name="throwOnFirstError"><see langword="true"/> if the method should throw if <paramref name="value"/> is invalid; otherwise return <see langword="null"/>.</param>
    /// <returns>The parsed and validated dpkg name.</returns>
    /// <exception cref="ArgumentException">When <paramref name="throwOnError"/> is <see langword="false"/>, but <paramref name="throwOnFirstError"/> is <see langword="true"/>.</exception>
    /// <exception cref="MalformedDpkgNameException">When <paramref name="value"/> is not a valid dpkg name.</exception>
    public static DpkgName? Parse(ReadOnlySpan<char> value, bool throwOnError, bool throwOnFirstError = false)
    {
        var invalidCharacters = ImmutableList<(char invalidCharacter, int position)>.Empty;
        if (throwOnFirstError && !throwOnError)
        {
            throw new ArgumentException("Parameter throwOnError can not be false when parameter throwOnFirstError is true.", nameof(throwOnFirstError));
        }

        if (value.IsEmpty)
        {
            return throwOnError
                ? throw new MalformedDpkgNameException(
                    message: "Package name is empty.",
                    packageName: value.ToString(),
                    invalidCharacters: invalidCharacters)
                : null;
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
                if (!throwOnError)
                {
                    return null;
                }
                if (throwOnFirstError)
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
            return throwOnError
                ? throw new MalformedDpkgNameException(
                    message: "Package name contains not allowed characters.",
                    packageName: value.ToString(),
                    invalidCharacters: invalidCharacters)
                : null;
        }

        return new DpkgName(value.ToString());
    }

    /// <inheritdoc />
    public static DpkgName Parse(string value, IFormatProvider? formatProvider = null)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Parse(value.AsSpan(), throwOnError: true)!.Value;
    }

    /// <inheritdoc />
    public static bool TryParse([NotNullWhen(true)] string? value, IFormatProvider? formatProvider, out DpkgName result)
    {
        if (value is null)
        {
            result = default;
            return false;
        }

        var parsed = Parse(value.AsSpan(), throwOnError: false);
        result = parsed ?? default;
        return parsed.HasValue;
    }

    /// <inheritdoc />
    public static DpkgName Parse(ReadOnlySpan<char> value, IFormatProvider? formatProvider = null)
    {
        return Parse(value, throwOnError: true)!.Value;
    }

    /// <inheritdoc />
    public static bool TryParse(ReadOnlySpan<char> value, IFormatProvider? formatProvider, out DpkgName result)
    {
        var parsed = Parse(value, throwOnError: false);
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

        var parsed = Parse(value.AsSpan(), throwOnError: false);
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
        var parsed = Parse(value, throwOnError: false);
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
