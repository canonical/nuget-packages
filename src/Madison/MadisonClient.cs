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