# Week 6 Lab 1: Domain, Business, and Adapter Test Examples

## Purpose

This guide provides example xUnit tests for the Domain, Business, and adapter responsibilities introduced in Lab 1 for:

```text
W5-UC01: View Student Enrolments
```

The examples test the Business use case without requiring:

- Entity Framework Core.
- SQLite.
- MVC.
- REST.
- A running ASP.NET Core application.

They use a fake implementation of the Business-owned output port:

```text
IEnrolmentGateway
```

Adapt the namespaces, constructor parameters, property names, and DTO names to match your implementation.

## Tests Covered

The examples cover the four Business tests requested in Lab 1:

1. Matching Enrolments are returned.
2. Results are ordered chronologically.
3. Enrolments belonging to another Student are excluded.
4. A Student with no Enrolments receives an empty output collection.

# 1. Required Test Project References

The test project needs references to Domain and Business:

```bash
dotnet add PgrStudentManagement.Tests/PgrStudentManagement.Tests.csproj reference PgrStudentManagement.Domain/PgrStudentManagement.Domain.csproj
```

```bash
dotnet add PgrStudentManagement.Tests/PgrStudentManagement.Tests.csproj reference PgrStudentManagement.Business/PgrStudentManagement.Business.csproj
```

Run:

```bash
dotnet restore
dotnet build
dotnet test
```

# 2. Suggested Test Structure

```text
PgrStudentManagement.Tests/
├── Business/
│   └── Students/
│       └── ViewStudentEnrolmentsUseCaseTests.cs
└── TestDoubles/
    └── FakeEnrolmentGateway.cs
```

# 3. Fake Enrolment Gateway

Create:

```text
PgrStudentManagement.Tests/TestDoubles/FakeEnrolmentGateway.cs
```

```csharp
using PgrStudentManagement.Business.Students.Gateway;
using PgrStudentManagement.Domain.Students;

namespace PgrStudentManagement.Tests.TestDoubles;

public class FakeEnrolmentGateway : IEnrolmentGateway
{
    private readonly List<Enrolment> _enrolments = [];

    public string? LastRequestedStudentNumber { get; private set; }

    public int NumberOfCalls { get; private set; }

    public void Add(Enrolment enrolment)
    {
        _enrolments.Add(enrolment);
    }

    public void AddRange(
        IEnumerable<Enrolment> enrolments)
    {
        _enrolments.AddRange(enrolments);
    }

    public Task<IReadOnlyList<Enrolment>>
        GetForStudentAsync(
            string studentNumber,
            CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        LastRequestedStudentNumber = studentNumber;
        NumberOfCalls++;

        IReadOnlyList<Enrolment> result = _enrolments
            .Where(enrolment =>
                string.Equals(
                    enrolment.StudentNumber,
                    studentNumber,
                    StringComparison.OrdinalIgnoreCase))
            .ToList();

        return Task.FromResult(result);
    }
}
```

## Why the Fake Does Not Sort

The fake deliberately returns matching Enrolments in their stored order.

This allows the tests to confirm that the Business use case applies the required earliest-to-latest ordering independently of the persistence technology.

The EF gateway may also order the database query for efficiency, but the observable ordering rule remains the responsibility of Business.

# 4. Complete Use-Case Test Class

Create:

```text
PgrStudentManagement.Tests/Business/Students/
    ViewStudentEnrolmentsUseCaseTests.cs
```

```csharp
using PgrStudentManagement.Business.Students
    .ViewStudentEnrolments;
using PgrStudentManagement.Domain.Students;
using PgrStudentManagement.Tests.TestDoubles;

namespace PgrStudentManagement.Tests.Business.Students;

public class ViewStudentEnrolmentsUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenMatchesExist_ReturnsMatchingEnrolments()
    {
        var gateway = new FakeEnrolmentGateway();

        gateway.AddRange(
        [
            CreateEnrolment(
                enrolmentId: 1,
                studentNumber: "S100001",
                programmeCode: "PHD-COMP",
                startDate: new DateTime(2025, 10, 1)),

            CreateEnrolment(
                enrolmentId: 2,
                studentNumber: "S100001",
                programmeCode: "PHD-DATA",
                startDate: new DateTime(2026, 10, 1))
        ]);

        var useCase =
            new ViewStudentEnrolmentsUseCase(gateway);

        var input =
            new ViewStudentEnrolmentsInput("S100001");

        var output = await useCase.ExecuteAsync(
            input,
            CancellationToken.None);

        Assert.Equal("S100001", output.StudentNumber);
        Assert.Equal(2, output.Enrolments.Count);

        Assert.Contains(
            output.Enrolments,
            enrolment =>
                enrolment.ProgrammeCode == "PHD-COMP");

        Assert.Contains(
            output.Enrolments,
            enrolment =>
                enrolment.ProgrammeCode == "PHD-DATA");

        Assert.Equal(
            "S100001",
            gateway.LastRequestedStudentNumber);

        Assert.Equal(1, gateway.NumberOfCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenGatewayReturnsUnorderedResults_ReturnsChronologicalOrder()
    {
        var gateway = new FakeEnrolmentGateway();

        gateway.AddRange(
        [
            CreateEnrolment(
                enrolmentId: 3,
                studentNumber: "S100001",
                programmeCode: "THIRD",
                startDate: new DateTime(2026, 10, 1)),

            CreateEnrolment(
                enrolmentId: 1,
                studentNumber: "S100001",
                programmeCode: "FIRST",
                startDate: new DateTime(2024, 10, 1)),

            CreateEnrolment(
                enrolmentId: 2,
                studentNumber: "S100001",
                programmeCode: "SECOND",
                startDate: new DateTime(2025, 10, 1))
        ]);

        var useCase =
            new ViewStudentEnrolmentsUseCase(gateway);

        var output = await useCase.ExecuteAsync(
            new ViewStudentEnrolmentsInput("S100001"),
            CancellationToken.None);

        var programmeCodes = output.Enrolments
            .Select(enrolment =>
                enrolment.ProgrammeCode)
            .ToList();

        Assert.Equal(
            ["FIRST", "SECOND", "THIRD"],
            programmeCodes);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOtherStudentHasEnrolments_ExcludesUnrelatedEnrolments()
    {
        var gateway = new FakeEnrolmentGateway();

        gateway.AddRange(
        [
            CreateEnrolment(
                enrolmentId: 1,
                studentNumber: "S100001",
                programmeCode: "INCLUDED",
                startDate: new DateTime(2025, 10, 1)),

            CreateEnrolment(
                enrolmentId: 2,
                studentNumber: "S100002",
                programmeCode: "EXCLUDED",
                startDate: new DateTime(2024, 10, 1))
        ]);

        var useCase =
            new ViewStudentEnrolmentsUseCase(gateway);

        var output = await useCase.ExecuteAsync(
            new ViewStudentEnrolmentsInput("S100001"),
            CancellationToken.None);

        var enrolment = Assert.Single(output.Enrolments);

        Assert.Equal(
            "INCLUDED",
            enrolment.ProgrammeCode);

        Assert.DoesNotContain(
            output.Enrolments,
            item => item.ProgrammeCode == "EXCLUDED");
    }

    [Fact]
    public async Task ExecuteAsync_WhenStudentHasNoEnrolments_ReturnsEmptyCollection()
    {
        var gateway = new FakeEnrolmentGateway();

        gateway.Add(
            CreateEnrolment(
                enrolmentId: 1,
                studentNumber: "S100002",
                programmeCode: "OTHER-STUDENT",
                startDate: new DateTime(2025, 10, 1)));

        var useCase =
            new ViewStudentEnrolmentsUseCase(gateway);

        var output = await useCase.ExecuteAsync(
            new ViewStudentEnrolmentsInput("S100003"),
            CancellationToken.None);

        Assert.Equal("S100003", output.StudentNumber);
        Assert.NotNull(output.Enrolments);
        Assert.Empty(output.Enrolments);
        Assert.Equal(1, gateway.NumberOfCalls);
    }

    private static Enrolment CreateEnrolment(
        int enrolmentId,
        string studentNumber,
        string programmeCode,
        DateTime startDate)
    {
        return new Enrolment
        {
            EnrolmentId = enrolmentId,
            StudentNumber = studentNumber,
            ProgrammeCode = programmeCode,
            StartDate = startDate,
            ExpectedEndDate = startDate.AddYears(3),
            ModeOfStudy = "Full-time",
            Status = EnrolmentStatus.Active
        };
    }
}
```

# 5. Expected Business Implementation

For the ordering test to pass, the Business use case should enforce chronological ordering.

A representative implementation is:

```csharp
using PgrStudentManagement.Business.Students.Gateway;

namespace PgrStudentManagement.Business.Students
    .ViewStudentEnrolments;

public class ViewStudentEnrolmentsUseCase
    : IViewStudentEnrolmentsUseCase
{
    private readonly IEnrolmentGateway _gateway;

    public ViewStudentEnrolmentsUseCase(
        IEnrolmentGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<StudentEnrolmentsOutput>
        ExecuteAsync(
            ViewStudentEnrolmentsInput input,
            CancellationToken cancellationToken)
    {
        var enrolments = await _gateway
            .GetForStudentAsync(
                input.StudentNumber,
                cancellationToken);

        var outputItems = enrolments
            .OrderBy(enrolment =>
                enrolment.StartDate)
            .Select(enrolment =>
                new EnrolmentOutput(
                    enrolment.ProgrammeCode,
                    enrolment.StartDate,
                    enrolment.ExpectedEndDate,
                    enrolment.ModeOfStudy,
                    enrolment.Status.ToString()))
            .ToList();

        return new StudentEnrolmentsOutput(
            input.StudentNumber,
            outputItems);
    }
}
```

This is an example only. Adapt it if your output DTOs are classes rather than records or if your property names differ.

# 6. Run the Tests

Run all tests:

```bash
dotnet test
```

Run only this test class:

```bash
dotnet test \
    --filter \
    FullyQualifiedName~ViewStudentEnrolmentsUseCaseTests
```

Use detailed console output when investigating a failure:

```bash
dotnet test \
    --logger "console;verbosity=detailed"
```

# 7. How to Adapt the Examples

Your implementation may differ from the example in these ways:

- Namespaces.
- Domain property names.
- DTO class names.
- DTO constructor arguments.
- `EnrolmentStatus` values.
- Student identifier normalisation.
- Use of records versus classes.
- Use-case outcome structure.

When adapting the tests, preserve the behaviour being tested rather than copying every name literally.

For example, if your output DTO exposes `CourseCode` instead of `ProgrammeCode`, update the assertions to use `CourseCode`.

# 8. Arrange, Act, Assert Structure

Each test follows the same pattern.

## Arrange

Create:

- A fake gateway.
- Domain test data.
- The Business use case.
- The input DTO.

## Act

Call:

```csharp
await useCase.ExecuteAsync(...)
```

## Assert

Check the observable Business result, such as:

- Returned items.
- Ordering.
- Exclusion of unrelated data.
- Empty collection behaviour.
- The gateway request made by the use case.

# 9. Test Quality Checklist

- [ ] Each test creates its own fake gateway.
- [ ] Tests do not share mutable state.
- [ ] Tests do not require SQLite or Entity Framework Core.
- [ ] Tests do not start ASP.NET Core.
- [ ] Tests use fixed dates rather than the current date.
- [ ] Each test has one clear behavioural purpose.
- [ ] Test names describe the condition and expected result.
- [ ] The ordering test gives the use case deliberately unordered data.
- [ ] The unrelated-data test contains data for at least two Students.
- [ ] The empty-result test expects an empty collection, not `null`.
- [ ] The fake gateway implements the Business-owned output port.
- [ ] The tests remain valid if SQLite is replaced.

# 10. Optional Additional Tests

After completing the required tests, consider adding:

## Passes the Student Number to the Gateway

```csharp
[Fact]
public async Task ExecuteAsync_PassesStudentNumberToGateway()
{
    var gateway = new FakeEnrolmentGateway();
    var useCase =
        new ViewStudentEnrolmentsUseCase(gateway);

    await useCase.ExecuteAsync(
        new ViewStudentEnrolmentsInput("S100004"),
        CancellationToken.None);

    Assert.Equal(
        "S100004",
        gateway.LastRequestedStudentNumber);
}
```

## Uses the Gateway Once

```csharp
[Fact]
public async Task ExecuteAsync_CallsGatewayOnce()
{
    var gateway = new FakeEnrolmentGateway();
    var useCase =
        new ViewStudentEnrolmentsUseCase(gateway);

    await useCase.ExecuteAsync(
        new ViewStudentEnrolmentsInput("S100001"),
        CancellationToken.None);

    Assert.Equal(1, gateway.NumberOfCalls);
}
```

## Honours Cancellation

```csharp
[Fact]
public async Task ExecuteAsync_WhenCancelled_ThrowsOperationCancelledException()
{
    var gateway = new FakeEnrolmentGateway();
    var useCase =
        new ViewStudentEnrolmentsUseCase(gateway);

    using var cancellationSource =
        new CancellationTokenSource();

    cancellationSource.Cancel();

    await Assert.ThrowsAsync<OperationCanceledException>(
        () => useCase.ExecuteAsync(
            new ViewStudentEnrolmentsInput("S100001"),
            cancellationSource.Token));
}
```

These are extensions rather than replacements for the four required Business tests.

# 11. Domain Test Examples

Domain tests should run without Entity Framework Core, MongoDB, MVC, REST, or a running application.

The supplied Domain model may initially contain state rather than rich behaviour. The first example therefore checks that a valid Enrolment retains its business data. The second example checks that the Domain assembly does not reference outer frameworks.

## Suggested Location

```text
PgrStudentManagement.Tests/
└── Domain/
    └── Students/
        └── EnrolmentTests.cs
```

## Example 1: Enrolment Retains Its Business State

```csharp
using PgrStudentManagement.Domain.Students;

namespace PgrStudentManagement.Tests.Domain.Students;

public class EnrolmentTests
{
    [Fact]
    public void Create_WithValidValues_RetainsBusinessState()
    {
        var startDate = new DateTime(2025, 10, 1);
        var expectedEndDate = new DateTime(2028, 9, 30);

        var enrolment = new Enrolment
        {
            EnrolmentId = 12,
            StudentNumber = "S100001",
            ProgrammeCode = "PHD-COMP",
            StartDate = startDate,
            ExpectedEndDate = expectedEndDate,
            ModeOfStudy = "Full-time",
            Status = EnrolmentStatus.Active
        };

        Assert.Equal(12, enrolment.EnrolmentId);
        Assert.Equal("S100001", enrolment.StudentNumber);
        Assert.Equal("PHD-COMP", enrolment.ProgrammeCode);
        Assert.Equal(startDate, enrolment.StartDate);
        Assert.Equal(
            expectedEndDate,
            enrolment.ExpectedEndDate);
        Assert.Equal("Full-time", enrolment.ModeOfStudy);
        Assert.Equal(
            EnrolmentStatus.Active,
            enrolment.Status);
    }
}
```

This is a basic scaffold example. If the Domain model later gains behaviour, replace or supplement this test with tests of business rules rather than testing property assignment alone.

Examples of stronger future Domain tests include:

```text
An Enrolment cannot complete before it starts.
A suspended Enrolment cannot be treated as active.
A completion operation records the completion date.
An expected end date cannot be earlier than the start date.
```

Add such tests only when the corresponding rules exist in the supplied requirements and Domain model.

## Example 2: Domain Does Not Reference Outer Frameworks

Create:

```text
PgrStudentManagement.Tests/Architecture/
    DomainDependencyTests.cs
```

```csharp
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
```

This test illustrates the Dependency Rule. Continue to verify project references with:

```bash
dotnet list PgrStudentManagement.Domain/PgrStudentManagement.Domain.csproj reference
```

Assembly metadata may omit an unused project reference, so the automated test and project-reference inspection complement one another.

# 12. Persistence Adapter Test Example

The Persistence Adapter converts stored records to Domain entities. A mapper unit test can verify that conversion without requiring SQLite.

## Required Test Reference

Add a Persistence project reference if it is not already present:

```bash
dotnet add PgrStudentManagement.Tests/PgrStudentManagement.Tests.csproj reference PgrStudentManagement.Adapters.Persistence/PgrStudentManagement.Adapters.Persistence.csproj
```

## Suggested Location

```text
PgrStudentManagement.Tests/
└── Persistence/
    └── Relational/
        └── EnrolmentPersistenceMapperTests.cs
```

## Example: Maps an EF Record to a Domain Entity

```csharp
using PgrStudentManagement.Adapters.Persistence
    .Relational.Mappers;
using PgrStudentManagement.Adapters.Persistence
    .Relational.Models;
using PgrStudentManagement.Domain.Students;

namespace PgrStudentManagement.Tests.Persistence.Relational;

public class EnrolmentPersistenceMapperTests
{
    [Fact]
    public void ToDomain_MapsAllRequiredFields()
    {
        var record = new EnrolmentRecord
        {
            EnrolmentId = 27,
            StudentNumber = "S100001",
            ProgrammeCode = "PHD-COMP",
            StartDate = new DateTime(2025, 10, 1),
            ExpectedEndDate = new DateTime(2028, 9, 30),
            ModeOfStudy = "Full-time",
            Status = "Active"
        };

        var mapper = new EnrolmentPersistenceMapper();

        var enrolment = mapper.ToDomain(record);

        Assert.Equal(27, enrolment.EnrolmentId);
        Assert.Equal("S100001", enrolment.StudentNumber);
        Assert.Equal("PHD-COMP", enrolment.ProgrammeCode);
        Assert.Equal(
            new DateTime(2025, 10, 1),
            enrolment.StartDate);
        Assert.Equal(
            new DateTime(2028, 9, 30),
            enrolment.ExpectedEndDate);
        Assert.Equal("Full-time", enrolment.ModeOfStudy);
        Assert.Equal(
            EnrolmentStatus.Active,
            enrolment.Status);
    }
}
```

If `EnrolmentRecord.Status` already uses `EnrolmentStatus`, keep the same expected result but remove string parsing from the mapper implementation.

For the read-only guided use case, test only the `ToDomain` operation actually required. Do not add a `ToRecord` test unless the implementation contains a write mapping used by an implemented command.

# 13. Web Adapter Test Examples

The Web Adapter maps a Business output DTO to a presentation-ready MVC view model.

## Required Test Reference

```bash
dotnet add PgrStudentManagement.Tests/PgrStudentManagement.Tests.csproj reference PgrStudentManagement.Adapters.Web/PgrStudentManagement.Adapters.Web.csproj
```

## Suggested Location

```text
PgrStudentManagement.Tests/
└── Web/
    └── Mappers/
        └── StudentEnrolmentsWebMapperTests.cs
```

## Example 1: Maps Business Output to the MVC View Model

```csharp
using PgrStudentManagement.Adapters.Web.Mappers;
using PgrStudentManagement.Business.Students
    .ViewStudentEnrolments;

namespace PgrStudentManagement.Tests.Web.Mappers;

public class StudentEnrolmentsWebMapperTests
{
    [Fact]
    public void ToViewModel_MapsStudentAndEnrolmentDetails()
    {
        var output = new StudentEnrolmentsOutput(
            "S100001",
            [
                new EnrolmentOutput(
                    "PHD-COMP",
                    new DateTime(2025, 10, 1),
                    new DateTime(2028, 9, 30),
                    "Full-time",
                    "Active")
            ]);

        var mapper = new StudentEnrolmentsWebMapper();

        var viewModel = mapper.ToViewModel(output);

        Assert.Equal("S100001", viewModel.StudentNumber);

        var item = Assert.Single(viewModel.Enrolments);

        Assert.Equal("PHD-COMP", item.ProgrammeCode);
        Assert.Equal("Full-time", item.ModeOfStudy);
        Assert.Equal("Active", item.Status);
    }
}
```

The exact date assertions depend on whether the mapper stores dates as `DateTime` values or formatted strings.

## Example 2: Displays a Missing Expected End Date Appropriately

Use this test if the MVC view model contains a display-ready string:

```csharp
[Fact]
public void ToViewModel_WhenExpectedEndDateMissing_UsesNotRecordedText()
{
    var output = new StudentEnrolmentsOutput(
        "S100001",
        [
            new EnrolmentOutput(
                "PHD-COMP",
                new DateTime(2025, 10, 1),
                null,
                "Full-time",
                "Active")
        ]);

    var mapper = new StudentEnrolmentsWebMapper();

    var viewModel = mapper.ToViewModel(output);

    var item = Assert.Single(viewModel.Enrolments);

    Assert.Equal(
        "Not recorded",
        item.ExpectedEndDate);
}
```

If formatting is performed in the Razor view rather than the mapper, move this assertion to an appropriate view or presentation test instead. The test should match the responsibility chosen in the implementation.

# 14. API Adapter Test Examples

The API Adapter maps Business output to an external JSON response model while preserving API-relevant types.

## Required Test Reference

```bash
dotnet add PgrStudentManagement.Tests/PgrStudentManagement.Tests.csproj reference PgrStudentManagement.Adapters.API/PgrStudentManagement.Adapters.API.csproj
```

## Suggested Location

```text
PgrStudentManagement.Tests/
└── API/
    └── Mappers/
        └── StudentEnrolmentsApiMapperTests.cs
```

## Example 1: Maps Business Output to the API Response

```csharp
using PgrStudentManagement.Adapters.API.Mappers;
using PgrStudentManagement.Business.Students
    .ViewStudentEnrolments;

namespace PgrStudentManagement.Tests.API.Mappers;

public class StudentEnrolmentsApiMapperTests
{
    [Fact]
    public void ToResponse_MapsStudentAndEnrolmentDetails()
    {
        var startDate = new DateTime(2025, 10, 1);
        var expectedEndDate = new DateTime(2028, 9, 30);

        var output = new StudentEnrolmentsOutput(
            "S100001",
            [
                new EnrolmentOutput(
                    "PHD-COMP",
                    startDate,
                    expectedEndDate,
                    "Full-time",
                    "Active")
            ]);

        var mapper = new StudentEnrolmentsApiMapper();

        var response = mapper.ToResponse(output);

        Assert.Equal("S100001", response.StudentNumber);

        var item = Assert.Single(response.Enrolments);

        Assert.Equal("PHD-COMP", item.ProgrammeCode);
        Assert.Equal(startDate, item.StartDate);
        Assert.Equal(
            expectedEndDate,
            item.ExpectedEndDate);
        Assert.Equal("Full-time", item.ModeOfStudy);
        Assert.Equal("Active", item.Status);
    }
}
```

## Example 2: Preserves an Empty Collection

```csharp
[Fact]
public void ToResponse_WhenOutputIsEmpty_ReturnsEmptyArrayModel()
{
    var output = new StudentEnrolmentsOutput(
        "S100003",
        []);

    var mapper = new StudentEnrolmentsApiMapper();

    var response = mapper.ToResponse(output);

    Assert.Equal("S100003", response.StudentNumber);
    Assert.NotNull(response.Enrolments);
    Assert.Empty(response.Enrolments);
}
```

This supports the selected REST behaviour:

```text
200 OK with an empty Enrolments array
```

The mapper test verifies the response model. A controller or endpoint test is responsible for verifying the HTTP status code.

# 15. Optional API Controller Test

Use this example only after the API controller exists. It verifies that the controller maps an empty Business result to `200 OK` rather than treating it as an error.

A simple stub input port can be used:

```csharp
using PgrStudentManagement.Business.Students
    .ViewStudentEnrolments;

namespace PgrStudentManagement.Tests.TestDoubles;

public class StubViewStudentEnrolmentsUseCase
    : IViewStudentEnrolmentsUseCase
{
    public StudentEnrolmentsOutput Output { get; set; } =
        new("S100003", []);

    public Task<StudentEnrolmentsOutput> ExecuteAsync(
        ViewStudentEnrolmentsInput input,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Output);
    }
}
```

A representative controller test is:

```csharp
using Microsoft.AspNetCore.Mvc;
using PgrStudentManagement.Adapters.API.Controllers;
using PgrStudentManagement.Adapters.API.Mappers;
using PgrStudentManagement.Adapters.API.Responses;
using PgrStudentManagement.Tests.TestDoubles;

namespace PgrStudentManagement.Tests.API.Controllers;

public class StudentsApiControllerTests
{
    [Fact]
    public async Task GetEnrolments_WhenNoneExist_ReturnsOkWithEmptyCollection()
    {
        var useCase =
            new StubViewStudentEnrolmentsUseCase();

        var mapper =
            new StudentEnrolmentsApiMapper();

        var controller = new StudentsApiController(
            useCase,
            mapper);

        var actionResult = await controller.GetEnrolments(
            "S100003",
            CancellationToken.None);

        var okResult = Assert.IsType<
            OkObjectResult>(actionResult.Result);

        var response = Assert.IsType<
            StudentEnrolmentsResponse>(okResult.Value);

        Assert.Equal(200, okResult.StatusCode);
        Assert.Empty(response.Enrolments);
    }
}
```

Adapt this example if the controller returns `ActionResult<StudentEnrolmentsResponse>` using `return response;`. In that case, the response may appear in `actionResult.Value` rather than an explicit `OkObjectResult`.

# 16. Optional Web Controller Test

Use this example only after the MVC controller exists. It checks that the controller returns the Enrolments view with a mapped view model.

```csharp
using Microsoft.AspNetCore.Mvc;
using PgrStudentManagement.Adapters.Web.Controllers;
using PgrStudentManagement.Adapters.Web.Mappers;
using PgrStudentManagement.Adapters.Web.ViewModels;
using PgrStudentManagement.Business.Students
    .ViewStudentEnrolments;
using PgrStudentManagement.Tests.TestDoubles;

namespace PgrStudentManagement.Tests.Web.Controllers;

public class StudentsControllerTests
{
    [Fact]
    public async Task Enrolments_ReturnsViewWithMappedViewModel()
    {
        var useCase =
            new StubViewStudentEnrolmentsUseCase
            {
                Output = new StudentEnrolmentsOutput(
                    "S100001",
                    [
                        new EnrolmentOutput(
                            "PHD-COMP",
                            new DateTime(2025, 10, 1),
                            new DateTime(2028, 9, 30),
                            "Full-time",
                            "Active")
                    ])
            };

        var mapper =
            new StudentEnrolmentsWebMapper();

        var controller = new StudentsController(
            useCase,
            mapper);

        var result = await controller.Enrolments(
            "S100001",
            CancellationToken.None);

        var viewResult = Assert.IsType<ViewResult>(result);

        var model = Assert.IsType<
            StudentEnrolmentsViewModel>(
                viewResult.Model);

        Assert.Equal("S100001", model.StudentNumber);
        Assert.Single(model.Enrolments);
    }
}
```

If the controller explicitly names the view, also assert:

```csharp
Assert.Equal("Enrolments", viewResult.ViewName);
```

If `ViewName` is `null`, MVC uses the action name by convention, which is also valid.

# 17. Comprehensive Test Structure

After adding all examples, a suitable test-project structure is:

```text
PgrStudentManagement.Tests/
├── Architecture/
│   └── DomainDependencyTests.cs
├── Domain/
│   └── Students/
│       └── EnrolmentTests.cs
├── Business/
│   └── Students/
│       └── ViewStudentEnrolmentsUseCaseTests.cs
├── Persistence/
│   └── Relational/
│       └── EnrolmentPersistenceMapperTests.cs
├── Web/
│   ├── Controllers/
│   │   └── StudentsControllerTests.cs
│   └── Mappers/
│       └── StudentEnrolmentsWebMapperTests.cs
├── API/
│   ├── Controllers/
│   │   └── StudentsApiControllerTests.cs
│   └── Mappers/
│       └── StudentEnrolmentsApiMapperTests.cs
└── TestDoubles/
    ├── FakeEnrolmentGateway.cs
    └── StubViewStudentEnrolmentsUseCase.cs
```

# 18. Responsibility Check

Each test should verify behaviour owned by the project under test.

```text
Domain test
    Business state or domain rule

Business test
    Use-case orchestration, filtering, ordering, and output

Persistence Adapter test
    Persistence-record-to-Domain mapping

Web Adapter test
    Business-output-to-view-model mapping or MVC result

API Adapter test
    Business-output-to-response mapping or HTTP result
```

Avoid testing the same responsibility in every project. For example:

- Chronological ordering belongs to Business.
- `Not recorded` display text belongs to the Web presentation choice.
- JSON response shape belongs to API.
- EF-record conversion belongs to Persistence.

# 19. Updated Test Checklist

- [ ] At least one Domain test exists.
- [ ] Required Business use-case tests pass.
- [ ] The Persistence mapper has a focused mapping test.
- [ ] The Web mapper has a focused presentation-mapping test.
- [ ] The API mapper has a focused response-mapping test.
- [ ] Optional controller tests use a stub input port rather than Persistence.
- [ ] Web and API tests do not construct `DbContext`.
- [ ] Business tests use a fake output port.
- [ ] Domain tests require no outer framework.
- [ ] Tests use deterministic values and fixed dates.
- [ ] Each test verifies responsibility owned by the project under test.

