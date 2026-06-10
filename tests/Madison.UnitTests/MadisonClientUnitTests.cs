using VerifyTests.Http;

namespace Canonical.Madison.UnitTests;

using System.Collections.Immutable;
using VerifyTests;

public class MadisonClientUnitTests
{
    [Fact]
    public async Task QueryAsync_WithValidResponse_ReturnsAllEntries()
    {
        var mockResponse = """
         dotnet8 | 8.0.100-8.0.0~rc1-0ubuntu1     | mantic/universe | source, amd64, arm64
         dotnet8 | 8.0.100-8.0.0~rc1-0ubuntu1~oracular1 | oracular/universe | source, amd64, arm64
         """;

        var httpMessageHandler = new MockHttpMessageHandler(mockResponse);
        var httpClient = new HttpClient(httpMessageHandler)
        {
            BaseAddress = new Uri("https://api.example.com/")
        };
        var client = new MadisonClient(httpClient);

        var request = new MadisonRequest(
            PackageNames: ["dotnet8"]);

        var entries = await client.QueryAsync(request);

        Assert.NotEmpty(entries);
        Assert.Equal(2, entries.Count);
        Assert.Equal("dotnet8", entries[0].PackageName);
        Assert.Equal("dotnet8", entries[1].PackageName);
    }

    [Fact]
    public async Task QueryAsync_WithEmptyResponse_ReturnsEmptyList()
    {
        var mockResponse = "";

        var httpMessageHandler = new MockHttpMessageHandler(mockResponse);
        var httpClient = new HttpClient(httpMessageHandler)
        {
            BaseAddress = new Uri("https://api.example.com/")
        };
        var client = new MadisonClient(httpClient);

        var request = new MadisonRequest(
            PackageNames: ["nonexistent"]);

        var entries = await client.QueryAsync(request);

        Assert.Empty(entries);
    }

    [Fact]
    public async Task QueryAsync_WithMultipleArchitectures_ParsesCorrectly()
    {
        var mockResponse = """
         gcc | 11.2.0-1 | jammy/main | source, amd64, i386, ppc64el, armhf, s390x
         """;

        var httpMessageHandler = new MockHttpMessageHandler(mockResponse);
        var httpClient = new HttpClient(httpMessageHandler)
        {
            BaseAddress = new Uri("https://api.example.com/")
        };
        var client = new MadisonClient(httpClient);

        var request = new MadisonRequest(
            PackageNames: ["gcc"]);

        var entries = await client.QueryAsync(request);

        var entry = Assert.Single(entries);
        Assert.Equal(["source", "amd64", "i386", "ppc64el", "armhf", "s390x"], entry.Architectures);
    }

    [Fact]
    public async Task QueryAsync_WithoutDefaultEndpoint_ThrowsArgumentNullException()
    {
        var mockResponse = "";

        var httpMessageHandler = new MockHttpMessageHandler(mockResponse);
        var httpClient = new HttpClient(httpMessageHandler);
        var client = new MadisonClient(httpClient);

        var request = new MadisonRequest(
            PackageNames: ["package"]);

        var ex = await Assert.ThrowsAsync<ArgumentNullException>(
            () => client.QueryAsync(request));

        Assert.Equal("madisonEndpoint", ex.ParamName);
    }

    [Fact]
    public async Task QueryAsync_WithExplicitEndpoint_UsesProvidedEndpoint()
    {
        var mockResponse = """
         package | 1.0.0 | jammy/main | amd64
         """;

        var httpMessageHandler = new MockHttpMessageHandler(mockResponse);
        var httpClient = new HttpClient(httpMessageHandler);
        var client = new MadisonClient(httpClient);

        var request = new MadisonRequest(
            PackageNames: ["package"]);
        var endpoint = new Uri("https://api.example.com/");

        var entries = await client.QueryAsync(request, madisonEndpoint: endpoint);

        var entry = Assert.Single(entries);
        Assert.Equal("package", entry.PackageName);
    }

    [Fact]
    public async Task QueryAndEnumerateAsync_WithValidResponse_YieldsEntries()
    {
        var mockResponse = """
         dotnet8 | 8.0.100 | mantic/universe | amd64
         gcc | 11.2.0-1 | jammy/main | amd64
         """;

        var httpMessageHandler = new MockHttpMessageHandler(mockResponse);
        var httpClient = new HttpClient(httpMessageHandler)
        {
            BaseAddress = new Uri("https://api.example.com/")
        };
        var client = new MadisonClient(httpClient);

        var request = new MadisonRequest(
            PackageNames: ["dotnet8", "gcc"]);

        var entries = new List<MadisonResponseEntry>();
        await foreach (var entry in client.QueryAndEnumerateAsync(request))
        {
            entries.Add(entry);
        }

        Assert.Equal(2, entries.Count);
        Assert.Equal("dotnet8", entries[0].PackageName);
        Assert.Equal("gcc", entries[1].PackageName);
    }

    [Fact]
    public async Task QueryAsync_WithVariousComponents_ParsesCorrectly()
    {
        var mockResponse = """
         package1 | 1.0 | jammy/main | amd64
         package2 | 1.0 | jammy/universe | amd64
         package3 | 1.0 | jammy/restricted | amd64
         package4 | 1.0 | jammy/multiverse | amd64
         """;

        var httpMessageHandler = new MockHttpMessageHandler(mockResponse);
        var httpClient = new HttpClient(httpMessageHandler)
        {
            BaseAddress = new Uri("https://api.example.com/")
        };
        var client = new MadisonClient(httpClient);

        var request = new MadisonRequest(
            PackageNames: ["package1", "package2", "package3", "package4"]);

        var entries = await client.QueryAsync(request);

        Assert.Equal("main", entries[0].Component);
        Assert.Equal("universe", entries[1].Component);
        Assert.Equal("restricted", entries[2].Component);
        Assert.Equal("multiverse", entries[3].Component);
    }

    [Fact]
    public async Task QueryAsync_WithComplexVersionStrings_ParsesCorrectly()
    {
        var mockResponse = """
         python3.11 | 3.11.9-1 | jammy/main | amd64
         golang-go | 2:1.18~2ubuntu1 | jammy/universe | amd64
         cmake | 3.22.3-3ubuntu1 | jammy/main | amd64
         """;

        var httpMessageHandler = new MockHttpMessageHandler(mockResponse);
        var httpClient = new HttpClient(httpMessageHandler)
        {
            BaseAddress = new Uri("https://api.example.com/")
        };
        var client = new MadisonClient(httpClient);

        var request = new MadisonRequest(
            PackageNames: ["python3.11", "golang-go", "cmake"]);

        var entries = await client.QueryAsync(request);

        Assert.Equal("3.11.9-1", entries[0].Version);
        Assert.Equal("2:1.18~2ubuntu1", entries[1].Version);
        Assert.Equal("3.22.3-3ubuntu1", entries[2].Version);
    }

    [Fact]
    public async Task QueryAsync_WithCancellation_CancelsProperly()
    {
        var mockResponse = """
         package | 1.0 | jammy/main | amd64
         """;

        var httpMessageHandler = new MockHttpMessageHandler(mockResponse);
        var httpClient = new HttpClient(httpMessageHandler)
        {
            BaseAddress = new Uri("https://api.example.com/")
        };
        var client = new MadisonClient(httpClient);

        var request = new MadisonRequest(
            PackageNames: ["package"]);

        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => client.QueryAsync(request, cancellationToken: cts.Token));
    }

    [Fact]
    public async Task QueryAsync_WithWhitespaceVariations_ParsesCorrectly()
    {
        var mockResponse = """
             dotnet8   |   8.0.100     |     mantic/universe     |     amd64, arm64
         """;

        var httpMessageHandler = new MockHttpMessageHandler(mockResponse);
        var httpClient = new HttpClient(httpMessageHandler)
        {
            BaseAddress = new Uri("https://api.example.com/")
        };
        var client = new MadisonClient(httpClient);

        var request = new MadisonRequest(
            PackageNames: ["dotnet8"]);

        var entries = await client.QueryAsync(request);

        var entry = Assert.Single(entries);
        Assert.Equal("dotnet8", entry.PackageName);
        Assert.Equal("8.0.100", entry.Version);
        Assert.Equal("mantic", entry.Series);
        Assert.Equal("universe", entry.Component);
    }

    [Fact]
    public async Task DefaultMadisonEndpoint_CanBeSetAndUsed()
    {
        var mockResponse = """
         package | 1.0 | jammy/main | amd64
         """;

        var httpMessageHandler = new MockHttpMessageHandler(mockResponse);
        var httpClient = new HttpClient(httpMessageHandler);
        var client = new MadisonClient(httpClient);

        var endpoint = new Uri("https://api.example.com/");
        client.DefaultMadisonEndpoint = endpoint;

        Assert.Equal(endpoint, client.DefaultMadisonEndpoint);

        var request = new MadisonRequest(
            PackageNames: ["package"]);

        var entries = await client.QueryAsync(request);

        var entry = Assert.Single(entries);
        Assert.Equal("package", entry.PackageName);
    }

    [Fact]
    public async Task QueryAsync_WithSpecialCharactersInPackageNames_BuildsCorrectUri()
    {
        var mockResponse = """
         lib32-gcc | 11.2.0 | jammy/main | amd64
         """;

        var httpMessageHandler = new MockHttpMessageHandler(mockResponse);
        var httpClient = new HttpClient(httpMessageHandler)
        {
            BaseAddress = new Uri("https://api.example.com/")
        };
        var client = new MadisonClient(httpClient);

        var request = new MadisonRequest(
            PackageNames: ["lib32-gcc"]);

        var entries = await client.QueryAsync(request);

        var entry = Assert.Single(entries);
        Assert.Equal("lib32-gcc", entry.PackageName);
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _responseContent;

        public MockHttpMessageHandler(string responseContent)
        {
            _responseContent = responseContent;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(_responseContent)
            };

            return Task.FromResult(response);
        }
    }
}
