# Reproduction for stryker-mutator/stryker-net#3813

Three cases in one solution, each a production project plus its test project. Every one of the three
fails under `dotnet-stryker` 4.16.0 with an unhandled `CompilationException`, and each shows a
different compiler error. All three come from the same thing: **Stryker's source-file list for the
mutated project handles one half of an SDK-generated partial class wrongly.** In case 1 the generated
half is missing from the compilation; in cases 2 and 3 it is present twice.

Every project builds and every test passes normally, so the failure is in the Stryker run and not in
the code.

## Environment

| | |
|---|---|
| `dotnet-stryker` | 4.16.0, pinned in `.config/dotnet-tools.json` with `rollForward: false` |
| .NET SDK | 10.0.400 (`dotnet --version`) |
| OS | Windows 11 26200 |
| `test-runner` | `mtp`, set in `stryker-config.json` |
| Test framework | xunit v3 3.2.2, `xunit.runner.visualstudio` 3.1.5 |
| bUnit (case 1) | 2.9.0 |

## How to run

```powershell
dotnet tool restore
dotnet build StrykerRepro3813.sln          # succeeds
dotnet test StrykerRepro3813.sln           # 9 tests, all pass
.\run-all.ps1                              # runs Stryker on all three cases
```

`run-all.ps1` runs each case with its working directory set to the test project's own folder, which
is how we run Stryker in CI, and writes one log per case. To run a single case by hand:

```powershell
cd Case1.Blazor\Blazor.Components.Tests
dotnet stryker -f ..\..\stryker-config.json
```

Logs from our own runs are in `logs/`, including a `-V trace` log for case 2.

## Case 1 - Blazor code-behind, generated half missing (CS0115)

`Case1.Blazor`. A Razor class library (`Microsoft.NET.Sdk.Razor`) with two components in the
code-behind pattern: `TotalsComponent.razor` plus `TotalsComponent.razor.cs`, and the same for
`GreetingComponent`. Each `.razor.cs` is a `sealed partial class` with no base class of its own and
one override of a `ComponentBase` member.

Stryker mutates the `.razor.cs` half. The generated `.razor` half, which is what declares
`ComponentBase` as the base class, is not in the compilation, so every override fails:

```
[WRN] An unidentified mutation in ...\Components\TotalsComponent.razor.cs resulted in a compile
      error (at 18:25) with id: CS0115, message: 'TotalsComponent.OnInitialized()': no suitable
      method found to override
[INF] Safe Mode! Stryker will remove all mutations in OnInitialized and mark them as 'compile error'.
```

Safe Mode engages per method, cannot recover, and the run ends with
`Stryker.Abstractions.Exceptions.CompilationException: Internal error due to compile error.` No
report is written and no mutant is tested.

**This answers the question about the Razor generator.** The run logs this two lines before the first
CS0115:

```
[WRN] Failed to load analyzer 'Microsoft.CodeAnalysis.Razor.Compiler':  (error : ReferencesNewerCompiler, analyzer: All).
[WRN] The analyzer 'Microsoft.CodeAnalysis.Razor.Compiler' references a newer version (5.9.0.0) of
      the compiler than the one used by Stryker.NET.
```

The Razor source generator is refused, does not run, and the generated half never exists.

## Case 2 - WPF application, generated half present twice (CS8646, CS0229)

`Case2.WpfApp`. A `WinExe` with `UseWPF`, an `App.xaml`, a `MainWindow.xaml` and a `TotalsView` user
control. `MainWindow.xaml` names `TotalsView`, a type declared in the same project. That is the part
that matters: naming a local type makes WPF run its two-pass markup compile, which builds the
temporary `_wpftmp` assembly.

Stryker then lists each generated file twice. From the trace log:

```
[DBG] Skipping auto-generated code file: ...\obj\Debug\net10.0-windows\MainWindow.g.cs
[DBG] Skipping auto-generated code file: ...\obj\Debug\net10.0-windows\App.g.cs
[DBG] Skipping auto-generated code file: ...\obj\Debug\net10.0-windows\GeneratedInternalTypeHelper.g.cs
[DBG] Skipping auto-generated code file: ...\obj\Debug\net10.0-windows\TotalsView.g.cs
[DBG] Skipping auto-generated code file: ...\obj\Debug\net10.0-windows\MainWindow.g.cs
[DBG] Skipping auto-generated code file: ...\obj\Debug\net10.0-windows\App.g.cs
[DBG] Skipping auto-generated code file: ...\obj\Debug\net10.0-windows\GeneratedInternalTypeHelper.g.cs
[DBG] Skipping auto-generated code file: ...\obj\Debug\net10.0-windows\TotalsView.g.cs
```

The files are skipped for mutation but still added to the compilation, twice each, so every
declaration in them exists twice:

```
[WRN] ...MainWindow.g.cs ... with id: CS8646, message: 'IComponentConnector.Connect(int, object)'
      is explicitly implemented more than once.
[WRN] ...MainWindow.g.cs ... with id: CS0229, message: Ambiguity between 'MainWindow._contentLoaded'
      and 'MainWindow._contentLoaded'
```

Same ending: unhandled `CompilationException`, no report, no mutant tested. Removing the local-type
reference from `MainWindow.xaml` removes the second markup pass, and the case then completes with a
mutation score of 66.67%, which is how we identified the trigger.

## Case 3 - WPF library, a third compiler error from the same duplication (CS0101, CS0111, CS0579)

`Case3.WpfLibrary`. A `Library` with `UseWPF`, an `App.xaml` kept for resources
(`EnableDefaultApplicationDefinition` set to `false`), and a `UserControl` whose XAML names a value
converter declared in the same project, again forcing the second markup pass.

Here the duplicated file is `GeneratedInternalTypeHelper.g.cs`, and the errors are the
duplicate-declaration set for that file:

```
1 x CS0101   the namespace already contains a definition
5 x CS0111   the type already defines a member with the same parameter types
3 x CS0579   duplicate assembly attribute
```

## Mutant-exclusion patterns have no effect

Each of these was accepted without complaint and changed nothing. The same files were still mutated,
the same errors were reported and the run still died:

| Case | Command | Result |
|---|---|---|
| 1 | `dotnet stryker -f ..\..\stryker-config.json -m "!**/*.razor.cs"` | 3 x CS0115, same crash |
| 2 | `dotnet stryker -f ..\..\stryker-config.json -m "!**/obj/**"` | 4 x CS8646/CS0229, same crash |
| 2 | `dotnet stryker -f ..\..\stryker-config.json -m "!**/*.g.cs"` | 4 x CS8646/CS0229, same crash |

This is the part that removes the obvious workaround. If exclusions worked, a project could skip the
file types Stryker cannot handle and still get a report for everything else.

## Why Nerdbank.GitVersioning is in Directory.Build.props

It is in the real solution, and cases 2 and 3 need it to fail the way the real projects fail. Without
it, the design-time build of the temporary `_wpftmp` project fails first with
`NETSDK1022: Duplicate 'Compile' items were included`, naming exactly the `*.g.cs` files above, and
Stryker stops one stage earlier:

```
[WRN] Analysis of project ..\Wpf.App\Wpf.App.csproj failed for frameworks net10.0-windows.
[INF] Project ...\Wpf.App.csproj analysis failed hence can't be mutated.
Failed to analyze project builds. Stryker cannot continue.
```

That is the same duplication of the generated files reported earlier, so both symptoms are worth
having. Delete `Directory.Build.props` to see it.

## What we would ask for

1. Generated files under `obj/` should not be added to the compilation twice, and arguably should not
   be considered at all.
2. The hand-written half of a generated partial class needs its generated half in the compilation.
3. A compile failure Safe Mode cannot recover from should skip that file and let the run finish, so
   one unsupported file type does not cost the whole project's report.
4. `-m` exclusions should apply to these files, which would give a workaround for all of the above.
