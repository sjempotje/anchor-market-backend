using NetArchTest.Rules;
using Xunit;

namespace AnchorMarket.ArchitectureTests;

public class DependencyRuleTests
{
    private static readonly string[] InfrastructureNamespaces = ["AnchorMarket.Infrastructure"];
    private static readonly string[] ApplicationNamespaces = ["AnchorMarket.Application"];

    [Fact]
    public void Domain_Should_Not_Reference_Infrastructure()
    {
        var result = Types.InAssembly(typeof(AnchorMarket.Domain.Entities.Market).Assembly)
            .ShouldNot()
            .HaveDependencyOn("AnchorMarket.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, "Domain layer must not reference Infrastructure.");
    }

    [Fact]
    public void Domain_Should_Not_Reference_Application()
    {
        var result = Types.InAssembly(typeof(AnchorMarket.Domain.Entities.Market).Assembly)
            .ShouldNot()
            .HaveDependencyOn("AnchorMarket.Application")
            .GetResult();

        Assert.True(result.IsSuccessful, "Domain layer must not reference Application.");
    }

    [Fact]
    public void Application_Should_Not_Reference_Infrastructure()
    {
        var result = Types.InAssembly(typeof(AnchorMarket.Application.Features.GroupMarkets.Queries.GetGroupMarketByIdQueryHandler).Assembly)
            .ShouldNot()
            .HaveDependencyOn("AnchorMarket.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, "Application layer must not reference Infrastructure.");
    }
}
