using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Canonical.Madison;

public sealed class MadisonClient
{
    private readonly HttpClient _httpClient;
    
    public MadisonClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Uri? DefaultMadisonEndpoint
    {
        get => _httpClient.BaseAddress;
        set => _httpClient.BaseAddress = value;
    }

    public async Task<ImmutableList<MadisonResponseEntry>> QueryAsync(
        MadisonRequest request,
        Uri? madisonEndpoint = null,
        CancellationToken cancellationToken = default)
    {
        var entries = ImmutableList.CreateBuilder<MadisonResponseEntry>();
        await foreach (var entry in QueryAndEnumerateAsync(request, madisonEndpoint, cancellationToken))
        {
            entries.Add(entry);
        }
        return entries.ToImmutableList();
    }
    
    public async IAsyncEnumerable<MadisonResponseEntry> QueryAndEnumerateAsync(
        MadisonRequest request,
        Uri? madisonEndpoint = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var endpoint =
            madisonEndpoint
            ?? DefaultMadisonEndpoint
            ?? throw new ArgumentNullException(
                paramName: nameof(madisonEndpoint), 
                message: "No endpoint specified.");
        
        var requestUri = request.BuildRequestUri(endpoint);
        await using var httpResponseContentStream = await _httpClient.GetStreamAsync(requestUri, cancellationToken);
        using var httpResponseReader = new StreamReader(httpResponseContentStream);
        
        while (true)
        {
            var line = await httpResponseReader.ReadLineAsync(cancellationToken);
            if (line is null) break;
            
            var entry = MadisonResponseEntry.Parse(line, throwOnError: true)!.Value;
            yield return entry;
        }
    }
}