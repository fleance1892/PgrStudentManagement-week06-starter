# Creating the Week 6 Clean Architecture Projects

## Purpose

This guide explains how to create the six production projects used in the Week 6 Clean Architecture exercises and configure their project references.

The target structure is:

```text
PgrStudentManagement.sln
├── PgrStudentManagement.Domain/
├── PgrStudentManagement.Business/
├── PgrStudentManagement.Adapters.Persistence/
├── PgrStudentManagement.Adapters.Web/
├── PgrStudentManagement.Adapters.API/
├── PgrStudentManagement.Application/
└── PgrStudentManagement.Tests/
```

The production projects compile into separate assemblies, but they are hosted, published, and deployed together as one ASP.NET Core application.

```text
PgrStudentManagement.Application
    -> the only executable project
    -> the composition root
    -> the ASP.NET Core host
```

> If the supplied starter already contains these projects and references, do not recreate them. Use this document to inspect and verify the scaffold.

# 1. Preserve the Existing Week 5 Project

Before changing the solution, confirm that the Week 5 reference implementation builds and runs.

From the solution directory, run:

```bash
dotnet restore
dotnet build
```

Run the existing Week 5 host using its current project name.

Create a source-control checkpoint:

```bash
git status
git add .
git commit -m "Preserve completed Week 5 implementation"
```

Do not delete the existing Week 5 project until the new Application host has been verified.

# 2. Locate the Solution File

Open a terminal in the directory containing the solution file.

List the available solution files.

On PowerShell:

```powershell
Get-ChildItem *.sln, *.slnx
```

On Bash, macOS, Linux, or Git Bash:

```bash
find . -maxdepth 1 -type f \( -name "*.sln" -o -name "*.slnx" \)
```

The commands in this guide use:

```text
PgrStudentManagement.sln
```

If the repository uses a different filename or a `.slnx` file, substitute the actual solution filename in the commands.

If there is exactly one solution file in the current directory, many `dotnet sln` commands can be used without specifying its filename.

# 3. Create the Six Production Projects

Run the following commands from the solution directory.

## Domain Project

Create a class library for pure business concepts and domain rules:

```bash
dotnet new classlib --framework net8.0 --name PgrStudentManagement.Domain
```

## Business Project

Create a class library for use cases, Business DTOs, input ports, and output ports:

```bash
dotnet new classlib --framework net8.0 --name PgrStudentManagement.Business
```

## Persistence Adapter Project

Create a class library for Entity Framework Core, MongoDB, persistence models, gateway implementations, and persistence mappers:

```bash
dotnet new classlib --framework net8.0 --name PgrStudentManagement.Adapters.Persistence
```

## Web Adapter Project

Create a Razor Class Library for MVC controllers, Razor views, MVC view models, web mappers, and static web assets:

```bash
dotnet new razorclasslib --framework net8.0 --name PgrStudentManagement.Adapters.Web --support-pages-and-views
```

## API Adapter Project

Create a class library for REST controllers, request models, response models, and API mappers:

```bash
dotnet new classlib --framework net8.0 --name PgrStudentManagement.Adapters.API
```

## Application Host Project

Create the executable ASP.NET Core host and composition root:

```bash
dotnet new web --framework net8.0 --name PgrStudentManagement.Application
```

# 4. Create or Retain the Test Project

If `PgrStudentManagement.Tests` already exists, retain it.

If it does not exist, create it:

```bash
dotnet new xunit --framework net8.0 --name PgrStudentManagement.Tests
```

The test project is not a production project. It may reference the projects it needs to test, but references should still be added deliberately.

# 5. Remove Generated Placeholder Files

The class-library templates normally create `Class1.cs` files. Delete those placeholder files.

On PowerShell:

```powershell
Remove-Item PgrStudentManagement.Domain/Class1.cs -ErrorAction SilentlyContinue
Remove-Item PgrStudentManagement.Business/Class1.cs -ErrorAction SilentlyContinue
Remove-Item PgrStudentManagement.Adapters.Persistence/Class1.cs -ErrorAction SilentlyContinue
Remove-Item PgrStudentManagement.Adapters.API/Class1.cs -ErrorAction SilentlyContinue
```

On Bash, macOS, Linux, or Git Bash:

```bash
rm -f PgrStudentManagement.Domain/Class1.cs
rm -f PgrStudentManagement.Business/Class1.cs
rm -f PgrStudentManagement.Adapters.Persistence/Class1.cs
rm -f PgrStudentManagement.Adapters.API/Class1.cs
```

Inspect the Razor Class Library before removing generated files. Retain files required by the Razor Class Library SDK, such as `_ViewImports.cshtml`, where present.

# 6. Add the Projects to the Solution

Add each project to the solution:

```bash
dotnet sln PgrStudentManagement.slnx add PgrStudentManagement.Domain/PgrStudentManagement.Domain.csproj
dotnet sln PgrStudentManagement.slnx add PgrStudentManagement.Business/PgrStudentManagement.Business.csproj
dotnet sln PgrStudentManagement.slnx add PgrStudentManagement.Adapters.Persistence/PgrStudentManagement.Adapters.Persistence.csproj
dotnet sln PgrStudentManagement.slnx add PgrStudentManagement.Adapters.Web/PgrStudentManagement.Adapters.Web.csproj
dotnet sln PgrStudentManagement.slnx add PgrStudentManagement.Adapters.API/PgrStudentManagement.Adapters.API.csproj
dotnet sln PgrStudentManagement.slnx add PgrStudentManagement.Application/PgrStudentManagement.Application.csproj
```

If the test project was newly created, add it too:

```bash
dotnet sln PgrStudentManagement.slnx add PgrStudentManagement.Tests/PgrStudentManagement.Tests.csproj
```

Confirm the result:

```bash
dotnet sln PgrStudentManagement.sln list
```

Expected projects include:

```text
PgrStudentManagement.Domain
PgrStudentManagement.Business
PgrStudentManagement.Adapters.Persistence
PgrStudentManagement.Adapters.Web
PgrStudentManagement.Adapters.API
PgrStudentManagement.Application
PgrStudentManagement.Tests
```

# 7. Add the Permitted Production Project References

The intended dependency direction is:

```text
Business
    -> Domain

Persistence
    -> Business
    -> Domain

Web
    -> Business

API
    -> Business

Application
    -> Business
    -> Persistence
    -> Web
    -> API
```

Add the references shown below.

## Business References Domain

```bash
dotnet add PgrStudentManagement.Business/PgrStudentManagement.Business.csproj reference PgrStudentManagement.Domain/PgrStudentManagement.Domain.csproj
```

## Persistence References Business and Domain

```bash
dotnet add PgrStudentManagement.Adapters.Persistence/PgrStudentManagement.Adapters.Persistence.csproj reference PgrStudentManagement.Business/PgrStudentManagement.Business.csproj
```

```bash
dotnet add PgrStudentManagement.Adapters.Persistence/PgrStudentManagement.Adapters.Persistence.csproj reference PgrStudentManagement.Domain/PgrStudentManagement.Domain.csproj
```

## Web References Business

```bash
dotnet add PgrStudentManagement.Adapters.Web/PgrStudentManagement.Adapters.Web.csproj reference PgrStudentManagement.Business/PgrStudentManagement.Business.csproj
```

Do not add a Domain reference to the Web adapter unless the implemented adapter has a specific and justified need for a Domain type.

## API References Business

```bash
dotnet add PgrStudentManagement.Adapters.API/PgrStudentManagement.Adapters.API.csproj reference PgrStudentManagement.Business/PgrStudentManagement.Business.csproj
```

Do not add a Domain reference to the API adapter unless the implemented adapter has a specific and justified need for a Domain type.

## Application References Business and All Adapters

```bash
dotnet add PgrStudentManagement.Application/PgrStudentManagement.Application.csproj reference PgrStudentManagement.Business/PgrStudentManagement.Business.csproj
```

```bash
dotnet add PgrStudentManagement.Application/PgrStudentManagement.Application.csproj reference PgrStudentManagement.Adapters.Persistence/PgrStudentManagement.Adapters.Persistence.csproj
```

```bash
dotnet add PgrStudentManagement.Application/PgrStudentManagement.Application.csproj reference PgrStudentManagement.Adapters.Web/PgrStudentManagement.Adapters.Web.csproj
```

```bash
dotnet add PgrStudentManagement.Application/PgrStudentManagement.Application.csproj reference PgrStudentManagement.Adapters.API/PgrStudentManagement.Adapters.API.csproj
```

The Application project does not normally need a direct Domain reference. Add one only if startup composition has a justified need for a Domain type.

# 8. Add Initial Test Project References

The Business unit tests need access to Business and Domain.

Add:

```bash
dotnet add PgrStudentManagement.Tests/PgrStudentManagement.Tests.csproj reference PgrStudentManagement.Business/PgrStudentManagement.Business.csproj
```

```bash
dotnet add PgrStudentManagement.Tests/PgrStudentManagement.Tests.csproj reference PgrStudentManagement.Domain/PgrStudentManagement.Domain.csproj
```

Add references to Persistence, Web, API, or Application only when a test specifically targets those projects.

For example, add a Persistence reference later if the project includes EF gateway integration tests.

# 9. Configure the API Project for ASP.NET Core Controllers

The API project is a class library, but its controllers require ASP.NET Core types such as `ControllerBase`, `ApiControllerAttribute`, and `ActionResult<T>`.

Open:

```text
PgrStudentManagement.Adapters.API/
    PgrStudentManagement.Adapters.API.csproj
```

Add this framework reference:

```xml
<ItemGroup>
  <FrameworkReference Include="Microsoft.AspNetCore.App" />
</ItemGroup>
```

A minimal API project file should resemble:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\PgrStudentManagement.Business\PgrStudentManagement.Business.csproj" />
  </ItemGroup>

</Project>
```

# 10. Confirm the Web Project Uses the Razor Class Library SDK

Open:

```text
PgrStudentManagement.Adapters.Web/
    PgrStudentManagement.Adapters.Web.csproj
```

Confirm that it begins with:

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">
```

A minimal Web adapter project file should resemble:

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <AddRazorSupportForMvc>true</AddRazorSupportForMvc>
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\PgrStudentManagement.Business\PgrStudentManagement.Business.csproj" />
  </ItemGroup>

</Project>
```

Retain or create the Razor support files required by the implementation, including:

```text
Views/_ViewImports.cshtml
Views/_ViewStart.cshtml
Views/Shared/_Layout.cshtml
```

The exact shared-layout arrangement may be adapted from the Week 5 project.

# 11. Verify the Project Reference Graph

List the references for each project.

## Domain

```bash
dotnet list PgrStudentManagement.Domain/PgrStudentManagement.Domain.csproj reference
```

Expected:

```text
No project references
```

## Business

```bash
dotnet list PgrStudentManagement.Business/PgrStudentManagement.Business.csproj reference
```

Expected:

```text
PgrStudentManagement.Domain
```

## Persistence

```bash
dotnet list PgrStudentManagement.Adapters.Persistence/PgrStudentManagement.Adapters.Persistence.csproj reference
```

Expected:

```text
PgrStudentManagement.Business
PgrStudentManagement.Domain
```

## Web

```bash
dotnet list PgrStudentManagement.Adapters.Web/PgrStudentManagement.Adapters.Web.csproj reference
```

Expected core reference:

```text
PgrStudentManagement.Business
```

## API

```bash
dotnet list PgrStudentManagement.Adapters.API/PgrStudentManagement.Adapters.API.csproj reference
```

Expected core reference:

```text
PgrStudentManagement.Business
```

## Application

```bash
dotnet list PgrStudentManagement.Application/PgrStudentManagement.Application.csproj reference
```

Expected:

```text
PgrStudentManagement.Business
PgrStudentManagement.Adapters.Persistence
PgrStudentManagement.Adapters.Web
PgrStudentManagement.Adapters.API
```

## Tests

```bash
dotnet list PgrStudentManagement.Tests/PgrStudentManagement.Tests.csproj reference
```

Expected initial references:

```text
PgrStudentManagement.Business
PgrStudentManagement.Domain
```

# 12. Check for Prohibited References

The following production references must not exist:

```text
Domain -> another production project

Business -> Persistence
Business -> Web
Business -> API
Business -> Application

Persistence -> Web
Persistence -> API
Persistence -> Application

Web -> Persistence
Web -> API
Web -> Application

API -> Persistence
API -> Web
API -> Application
```

The adapter projects must not reference one another.

The Application project may reference all adapters because it is the outer composition root.

If a prohibited reference exists, remove it:

```bash
dotnet remove <referencing-project.csproj> reference <referenced-project.csproj>
```

For example:

```bash
dotnet remove PgrStudentManagement.Adapters.Web/PgrStudentManagement.Adapters.Web.csproj reference PgrStudentManagement.Adapters.Persistence/PgrStudentManagement.Adapters.Persistence.csproj
```

# 13. Create the Initial Folder Structure

The following folders provide a useful starting point.

## Domain

```text
PgrStudentManagement.Domain/
├── Students/
├── AcademicSupervisors/
└── ResearchActivities/
```

## Business

```text
PgrStudentManagement.Business/
├── Students/
│   ├── Gateway/
│   └── ViewStudentEnrolments/
├── AcademicSupervisors/
└── ResearchActivities/
```

## Persistence Adapter

```text
PgrStudentManagement.Adapters.Persistence/
├── Relational/
│   ├── Models/
│   ├── Configurations/
│   ├── Gateway/
│   ├── Mappers/
│   └── Migrations/
└── Documents/
    ├── Models/
    ├── Gateway/
    └── Mappers/
```

## Web Adapter

```text
PgrStudentManagement.Adapters.Web/
├── Controllers/
├── ViewModels/
├── Mappers/
└── Views/
    ├── Students/
    ├── ResearchActivities/
    └── Shared/
```

## API Adapter

```text
PgrStudentManagement.Adapters.API/
├── Controllers/
├── Requests/
├── Responses/
└── Mappers/
```

## Application Host

```text
PgrStudentManagement.Application/
├── Configuration/
├── appsettings.json
├── appsettings.Development.json
└── Program.cs
```

## Tests

```text
PgrStudentManagement.Tests/
├── Domain/
├── Business/
├── Persistence/
├── Web/
├── API/
├── Architecture/
└── TestDoubles/
```

Empty folders are not retained by Git. Add source files when the folders are needed, or include an intentional placeholder such as `.gitkeep` if the course repository requires the structure to be visible in advance.

# 14. Build and Test the Scaffold

Run:

```bash
dotnet clean
dotnet restore
dotnet build
dotnet test
```

At this stage:

- All projects should restore.
- All projects should compile.
- The test project should run.
- `PgrStudentManagement.Application` should be the only new executable host.

Run the host:

```bash
dotnet run --project PgrStudentManagement.Application
```

The initial host may display only a basic response until MVC and API adapters are registered and the Week 5 functionality is moved.

# 15. Final Verification Checklist

- [ ] All six production projects target `net8.0`.
- [ ] The test project targets `net8.0`.
- [ ] All projects are included in the solution.
- [ ] Domain has no production project references.
- [ ] Business references Domain only.
- [ ] Persistence references Business and Domain.
- [ ] Web references Business and does not reference Persistence or API.
- [ ] API references Business and does not reference Persistence or Web.
- [ ] Application references Business and all three adapter projects.
- [ ] The API project has the ASP.NET Core framework reference.
- [ ] The Web adapter uses the Razor Class Library SDK.
- [ ] The test project initially references Business and Domain.
- [ ] The solution restores and builds.
- [ ] The tests run.
- [ ] `PgrStudentManagement.Application` is the only new executable host.
- [ ] The existing Week 5 implementation has been preserved in source control.

# Dependency Summary

```text
PgrStudentManagement.Domain
    no production project references

PgrStudentManagement.Business
    -> PgrStudentManagement.Domain

PgrStudentManagement.Adapters.Persistence
    -> PgrStudentManagement.Business
    -> PgrStudentManagement.Domain

PgrStudentManagement.Adapters.Web
    -> PgrStudentManagement.Business

PgrStudentManagement.Adapters.API
    -> PgrStudentManagement.Business

PgrStudentManagement.Application
    -> PgrStudentManagement.Business
    -> PgrStudentManagement.Adapters.Persistence
    -> PgrStudentManagement.Adapters.Web
    -> PgrStudentManagement.Adapters.API
```

The central rule is:

```text
Source-code dependencies point towards
Business and Domain.
```
