using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Canonical.Madison;

public readonly record struct MadisonResponseEntry(
    string PackageName,
    string Version,
    string Suite,
    string Series,
    string Component,
    ImmutableList<string> Architectures) 
    : IParsable<MadisonResponseEntry>, ISpanParsable<MadisonResponseEntry>
{
    public static MadisonResponseEntry Parse(string value, IFormatProvider? formatProvider = null)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        return Parse(value, throwOnError: true)!.Value;
    }

    public static bool TryParse([NotNullWhen(true)] string? value, out MadisonResponseEntry result)
    {
        return TryParse(value, formatProvider: null, out result);
    }
    
    public static bool TryParse([NotNullWhen(true)] string? value, IFormatProvider? formatProvider, out MadisonResponseEntry result)
    {
        if (value is null)
        {
            result = default;
            return false;
        }
        
        return TryParse(value.AsSpan(), formatProvider, out result);
    }
    
    public static MadisonResponseEntry Parse(ReadOnlySpan<char> value, IFormatProvider? formatProvider = null)
    {
        return Parse(value, throwOnError: true)!.Value;
    }

    public static bool TryParse(ReadOnlySpan<char> value, out MadisonResponseEntry result)
    {
        var parsedValue = Parse(value, throwOnError: false);
        if (parsedValue.HasValue)
        {
            result = parsedValue.Value;
            return true;
        }

        result = default;
        return false;
    }
    
    public static bool TryParse(ReadOnlySpan<char> value, IFormatProvider? formatProvider, out MadisonResponseEntry result)
    {
        var parsedValue = Parse(value, throwOnError: false);
        if (parsedValue.HasValue)
        {
            result = parsedValue.Value;
            return true;
        }

        result = default;
        return false;
    }
    
    internal static MadisonResponseEntry? Parse(ReadOnlySpan<char> line, bool throwOnError)
    {
        // for reference, a line looks something like this:
        // " dotnet8 | 8.0.100-8.0.0~rc1-0ubuntu1     | mantic/universe | source, amd64, arm64  "

        Span<(int Start, int End)> valueRanges = stackalloc (int, int)[4];
        
        int start = 0, end, valueIndex = 0;
        for (int position = 0; position <= line.Length; ++position)
        {
            if (position == line.Length || line[position] == '|')
            {
                if (valueIndex >= valueRanges.Length)
                {
                    return throwOnError
                        ? throw new FormatException("A row of the madison response contains too many values.")
                        : null;
                }
                
                end = position - 1;

                // trim spaces at end and start of value
                while (start <= end && char.IsWhiteSpace(line[start])) ++start;
                while (end >= start && char.IsWhiteSpace(line[end])) --end;
                
                if (end < start)
                {
                    return throwOnError 
                        ? throw new FormatException("A row of the madison response contains at least one empty value.") 
                        : null;
                }

                valueRanges[valueIndex++] = (start, end);
                start = position + 1;
            }
        }

        if (valueIndex != valueRanges.Length)
        {
            return throwOnError
                ? throw new FormatException("A row of the madison response contains too few values.")
                : null;
        }
        
        (start, end) = valueRanges[0];
        string packageName = line.Slice(start, length: end - start + 1).ToString();
        
        (start, end) = valueRanges[1];
        string version = line.Slice(start, length: end - start + 1).ToString();
        
        (start, end) = valueRanges[2];
        var suiteSpan = line.Slice(start, length: end - start + 1);
        string suite = suiteSpan.ToString();
        string series;
        string component;
        int separatorIndex = suiteSpan.IndexOf('/');

        if (separatorIndex < 0)
        {
            component = string.Empty;
            series = suite;
        }
        else
        {
            if (separatorIndex == 0)
            {
                return throwOnError
                    ? throw new FormatException($"The suite '{suiteSpan}' does not have a value before the '/'")
                    : null;
            }
            if (separatorIndex + 1 == suiteSpan.Length)
            {
                return throwOnError
                    ? throw new FormatException($"The suite '{suiteSpan}' does not have a value after the '/'")
                    : null;
            }
            
            series = suiteSpan.Slice(start: 0, length: separatorIndex).ToString();
            component = suiteSpan.Slice(start: separatorIndex + 1).ToString();
        }
        
        (start, end) = valueRanges[3];
        var archListSpan = line.Slice(start, length: end - start + 1);
        var architectures = ImmutableList.CreateBuilder<string>();
        for (int position = start = 0; position <= archListSpan.Length; ++position)
        {
            if (position >= archListSpan.Length || archListSpan[position] == ',')
            {
                end = position - 1;

                var archSpan = archListSpan.Slice(start, length: end - start + 1);
                architectures.Add(archSpan.ToString());
                
                position++;
                while (position < archListSpan.Length && char.IsWhiteSpace(archListSpan[position])) ++position;
                start = position;
            }
        }
        
        return new MadisonResponseEntry(
            PackageName: packageName, 
            Version: version, 
            Suite: suite, 
            Series: series, 
            Component: component, 
            Architectures: architectures.ToImmutable());
    }
}
