using Foundation.Domain.Common;
using NetArchTest.Rules;
using Shouldly;
using Xunit;

namespace Foundation.ArchitectureTests;

public class ArchitectureTests
{
    private static readonly System.Reflection.Assembly DomainAssembly = typeof(BaseEntity).Assembly;
    private static readonly System.Reflection.Assembly ApplicationAssembly = typeof(Foundation.Application.DependencyInjection).Assembly;
    private static readonly System.Reflection.Assembly InfrastructureAssembly = typeof(Foundation.Infrastructure.DependencyInjection).Assembly;
    private static readonly System.Reflection.Assembly WebAssembly = typeof(Foundation.Web.Program).Assembly;

    [Fact]
    public void Domain_Should_Not_Have_Dependency_On_Other_Projects()
    {
        var otherProjects = new[]
        {
            "Foundation.Application",
            "Foundation.Infrastructure",
            "Foundation.Web"
        };

        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(otherProjects)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Domain layer should not depend on other layers: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_AspNetCore()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.AspNetCore")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Domain layer must have zero framework dependencies on Microsoft.AspNetCore: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure_Or_Web()
    {
        var forbidden = new[]
        {
            "Foundation.Infrastructure",
            "Foundation.Web"
        };

        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbidden)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Application layer should not depend on Infrastructure or Web: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Web()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn("Foundation.Web")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Infrastructure layer should not depend on Web: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
