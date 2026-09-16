using System.Reflection;
using CulinaryBlog.API.Endpoints;
using CulinaryBlog.Domain.Common;
using NetArchTest.Rules;

namespace CulinaryBlog.ArchitectureTests;

public sealed class LayerDependencyTests
{
    private const string ApplicationNamespace = "CulinaryBlog.Application";
    private const string InfrastructureNamespace = "CulinaryBlog.Infrastructure";
    private const string ApiNamespace = "CulinaryBlog.API";

    private static readonly Assembly DomainAssembly = typeof(BaseEntity).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(CulinaryBlog.Application.DependencyInjection).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(CulinaryBlog.Infrastructure.DependencyInjection).Assembly;

    [Fact]
    public void Domain_ReferencedAssemblies_AreBaseClassLibraryOnly()
    {
        var nonBclReferences = DomainAssembly.GetReferencedAssemblies()
            .Select(reference => reference.Name!)
            .Where(name => !name.StartsWith("System", StringComparison.Ordinal)
                && name is not ("netstandard" or "mscorlib"))
            .ToList();

        Assert.Empty(nonBclReferences);
    }

    [Fact]
    public void Domain_Types_DoNotDependOnOtherLayers()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, ApiNamespace)
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Application_Types_DoNotDependOnInfrastructureOrFrameworks()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                InfrastructureNamespace,
                ApiNamespace,
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore",
                "Hangfire")
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Infrastructure_Types_DoNotDependOnApi()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Api_EndpointModules_AreSealedAndInEndpointsNamespace()
    {
        var result = Types.InAssembly(typeof(IEndpointModule).Assembly)
            .That()
            .ImplementInterface(typeof(IEndpointModule))
            .Should()
            .BeSealed()
            .And()
            .ResideInNamespaceStartingWith($"{ApiNamespace}.Endpoints")
            .GetResult();

        AssertSuccessful(result);
    }

    private static void AssertSuccessful(TestResult result) =>
        Assert.True(
            result.IsSuccessful,
            $"Vi phạm kiến trúc: {string.Join(", ", result.FailingTypeNames ?? [])}");
}
