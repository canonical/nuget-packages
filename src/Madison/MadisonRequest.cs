using System.Collections.Immutable;
using System.Text;

namespace Canonical.Madison;

/// <summary>
/// Represents the values of a request to a Madison endpoint.
/// </summary>
/// <param name="PackageNames">
/// Show information packages with these names.
/// </param>
/// <param name="Components">
/// Only show information packages in these components.
/// </param>
/// <param name="Suites">
/// Only show information for packages in these suites.
/// </param>
/// <param name="Architectures">
/// Only show information for packages of these architectures.
/// </param>
/// <param name="IncludeBinaryPackagesOfSourcePackages">
/// Show info for the binary children of the source packages specified in <paramref name="PackageNames"/>.
/// </param>
/// <param name="TreatPackagesNamesAsRegex">
/// Treat the values specified in <paramref name="PackageNames"/> as regular expressions.
/// <remarks>
/// Since this can easily DoS the database (e.g. <c>"."</c>), this option is not supported by the CGI on qa.debian.org and most other madison endpoints.
/// </remarks>
/// </param>
/// <param name="BinaryType">
/// Only show information for packages of this binary type (e.g. <c>"deb"</c>, <c>"udeb"</c>, <c>"ddeb"</c>). The default value is <c>"deb"</c>. 
/// </param>
/// <seealso href="https://manpages.debian.org/unstable/devscripts/rmadison.1.en.html">rmadison(1)</seealso>
public readonly record struct MadisonRequest(
    IImmutableList<string> PackageNames,
    IImmutableList<string>? Components = null,
    IImmutableList<string>? Suites = null,
    IImmutableList<string>? Architectures = null,
    bool IncludeBinaryPackagesOfSourcePackages = false,
    bool TreatPackagesNamesAsRegex = false,
    string? BinaryType = null)
{
    public Uri BuildRequestUri(Uri madisonEndpoint)
    {
        if (PackageNames.Count == 0)
        {
            throw new InvalidOperationException("At lease one package name must be specified.");
        }
        
        var uriBuilder = new UriBuilder(madisonEndpoint);
        var queryBuilder = new StringBuilder(uriBuilder.Query);

        queryBuilder.Append(queryBuilder.Length == 0 ? "?text=on" : "&text=on");
        queryBuilder.Append("&package=").AppendJoin(separator: ',', PackageNames.Select(Uri.EscapeDataString));

        if (Architectures is { Count: > 0 })
        {
            queryBuilder.Append("&a=").AppendJoin(separator: ',', Architectures.Select(Uri.EscapeDataString));
        }
        if (BinaryType is not null)
        {
            queryBuilder.Append("&b=").Append(BinaryType);
        }
        if (Components is { Count: > 0 })
        {
            queryBuilder.Append("&c=").AppendJoin(separator: ',', Components.Select(Uri.EscapeDataString));
        }
        if (TreatPackagesNamesAsRegex)
        {
            queryBuilder.Append("&r=on");
        }
        if (Suites is { Count: > 0 })
        {
            queryBuilder.Append("&s=").AppendJoin(separator: ',', Suites.Select(Uri.EscapeDataString));
        }
        if (IncludeBinaryPackagesOfSourcePackages)
        {
            queryBuilder.Append("&S=on");
        }
        uriBuilder.Query = queryBuilder.ToString();
        return uriBuilder.Uri;
    }
}