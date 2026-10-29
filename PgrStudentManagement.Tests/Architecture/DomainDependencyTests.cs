using System.Reflection;
using PgrStudentManagement.Domain.Students;

namespace PgrStudentManagement.Tests.Architecture;

public class DomainDependencyTests
{
    [Fact]
    public void DomainAssembly_DoesNotReferenceOuterFrameworks()
    {
        var assembly = typeof(Enrolment).Assembly;

        var referencedAssemblies = assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .Where(name => name is not null)
            .Cast<string>()
            .ToHashSet(StringComparer.Ordinal);

        Assert.DoesNotContain(
            referencedAssemblies,
            name => name.StartsWith(
                "Microsoft.EntityFrameworkCore",
                StringComparison.Ordinal));

        Assert.DoesNotContain(
            referencedAssemblies,
            name => name.StartsWith(
                "MongoDB",
                StringComparison.Ordinal));

        Assert.DoesNotContain(
            referencedAssemblies,
            name => name.StartsWith(
                "Microsoft.AspNetCore",
                StringComparison.Ordinal));

        Assert.DoesNotContain(
            "PgrStudentManagement.Business",
            referencedAssemblies);

        Assert.DoesNotContain(
            "PgrStudentManagement.Adapters.Persistence",
            referencedAssemblies);

        Assert.DoesNotContain(
            "PgrStudentManagement.Adapters.Web",
            referencedAssemblies);

        Assert.DoesNotContain(
            "PgrStudentManagement.Adapters.API",
            referencedAssemblies);

        Assert.DoesNotContain(
            "PgrStudentManagement.Application",
            referencedAssemblies);
    }
}