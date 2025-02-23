using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace StrawberryShake.OrderOfInlineFragment.Bug.Tests;

public class Reproduction : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly IStrawBerryShakeClient _client;

    public Reproduction(WebApplicationFactory<Program> factory)
    {
        _factory = factory;

        var services = new ServiceCollection();
        var clientBuilder = services.AddStrawBerryShakeClient();

        clientBuilder.ConfigureHttpClient(
            client => { client.BaseAddress = new Uri(_factory.Server.BaseAddress, "graphql"); },
            httpClientBuilder =>
            {
                httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() => _factory.Server.CreateHandler());
            });

        var provider = services.BuildServiceProvider();
        _client = provider.GetRequiredService<IStrawBerryShakeClient>();
    }

    [Fact]
    public async Task FragmentOrderBug_FullFragment_Working()
    {
        var workingResult = await _client.WorkingQuery.ExecuteAsync(TestContext.Current.CancellationToken);

        Assert.NotNull(workingResult.Data);
        Assert.NotEmpty(workingResult.Data.Shelters);
        var firstShelter = workingResult.Data.Shelters[0];

        Assert.IsType<IWorkingQuery_Shelters_Pet_Bird>(firstShelter.Pet, exactMatch: false);
        var bird = firstShelter.Pet as IWorkingQuery_Shelters_Pet_Bird;
        Assert.True(bird?.WingSpan == 42, "Expected Bird (WingSpan=42), but it was missing");
    }

    [Fact]
    public async Task FragmentOrderBug_FullFragment_ExplicitQuery_Working()
    {
        var httpClient = _factory.CreateClient();
        var queryPath = Path.Combine(AppContext.BaseDirectory, "Queries", "WorkingQuery.graphql");
        var lines = await File.ReadAllLinesAsync(queryPath, TestContext.Current.CancellationToken);
        var query = string.Join(Environment.NewLine, lines);

        var response = await httpClient.PostAsJsonAsync("/graphql", new { query },
            cancellationToken: TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.DoesNotContain("errors", content);
        Assert.Contains("data", content);

        Assert.Contains("shelters", content);
        Assert.Contains("wingSpan", content);
        Assert.Contains("42", content);
    }

    [Fact]
    public async Task FragmentOrderBug_PartialFragment_Failing()
    {
        var notWorkingResult = await _client.NotWorkingQuery.ExecuteAsync(TestContext.Current.CancellationToken);

        // If the bug occurs, the first shelter's Bird data may be missing
        // because shelter appears second in the query,
        // but the Bird fragment is missing from the first in the query.
        Assert.NotNull(notWorkingResult.Data);
        Assert.NotEmpty(notWorkingResult.Data.Shelters);
        var firstShelter = notWorkingResult.Data.Shelters[0];

        Assert.IsType<IWorkingQuery_Shelters_Pet_Bird>(firstShelter.Pet, exactMatch: false);
        var bird = firstShelter.Pet as INotWorkingQuery_Shelters_Pet_Bird;
        // In a real bug scenario, 'bird' might be null or wingSpan=0.
        Assert.True(bird?.WingSpan == 42,
            "Expected Bird (WingSpan=42), but it was missing due to the partial fragment coverage bug.");
    }

    [Fact]
    public async Task FragmentOrderBug_PartialFragment_ExplicitQuery_Working()
    {
        var httpClient = _factory.CreateClient();
        var queryPath = Path.Combine(AppContext.BaseDirectory, "Queries", "NotWorkingQuery.graphql");
        var lines = await File.ReadAllLinesAsync(queryPath, TestContext.Current.CancellationToken);
        var query = string.Join(Environment.NewLine, lines);

        var response = await httpClient.PostAsJsonAsync("/graphql", new { query },
            cancellationToken: TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.DoesNotContain("errors", content);
        Assert.Contains("data", content);

        Assert.Contains("shelters", content);
        Assert.Contains("wingSpan", content);
        Assert.Contains("42", content);
    }
}