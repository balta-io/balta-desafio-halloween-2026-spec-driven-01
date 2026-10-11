using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PasswordGenerator.Api.Shared.Persistence;
using PasswordGenerator.Tests.Shared;

namespace PasswordGenerator.Tests.Architecture;

public class EndpointsDoNotUseDbContextTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static bool HasDbContextParameter(MethodInfo method) =>
        method.GetParameters().Any(p => typeof(AppDbContext).IsAssignableFrom(p.ParameterType));

    [Fact]
    public void No_endpoint_handler_takes_AppDbContext()
    {
        var handlers = factory.Services
            .GetServices<EndpointDataSource>()
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Where(e => e.RoutePattern.RawText!.StartsWith("/api/"))
            .Select(e => e.Metadata.GetMetadata<MethodInfo>())
            .ToList();

        Assert.Equal(2, handlers.Count);
        Assert.All(handlers, handler => Assert.False(HasDbContextParameter(handler!), handler!.ToString()));
    }

    [Fact]
    public void Rule_detects_a_handler_that_takes_AppDbContext()
    {
        var offending = typeof(EndpointsDoNotUseDbContextTests)
            .GetMethod(nameof(OffendingHandler), BindingFlags.NonPublic | BindingFlags.Static)!;

        Assert.True(HasDbContextParameter(offending));
    }

    private static void OffendingHandler(AppDbContext db) { }
}
