#requires -Version 7
# Runs Stryker against each of the three cases and reports the exit code of each.
# Every case is expected to fail. Each dies with an unhandled CompilationException, exit code
# -532462766 (0xE0434352).

$root = $PSScriptRoot

dotnet tool restore

$cases = @(
	@{ Name = 'Case 1 - Blazor code-behind (CS0115)'; Path = 'Case1.Blazor\Blazor.Components.Tests'; Log = 'case1-blazor.log' }
	@{ Name = 'Case 2 - WPF app (CS8646 / CS0229)'; Path = 'Case2.WpfApp\Wpf.App.Tests'; Log = 'case2-wpf-app.log' }
	@{ Name = 'Case 3 - WPF library (CS0101 / CS0111)'; Path = 'Case3.WpfLibrary\Wpf.Library.Tests'; Log = 'case3-wpf-library.log' }
)

foreach ($case in $cases)
{
	$folder = Join-Path $root $case.Path
	$log = Join-Path $root $case.Log

	Write-Host ''
	Write-Host "=== $($case.Name) ===" -ForegroundColor Cyan
	Write-Host "    folder: $folder"
	Write-Host "    log:    $log"

	Push-Location $folder
	dotnet stryker -f ..\..\stryker-config.json > $log 2>&1
	$exitCode = $LASTEXITCODE
	Pop-Location

	Write-Host "    exit code: $exitCode"

	Select-String -Path $log -Pattern 'with id: (CS\d{4})' -AllMatches |
		ForEach-Object { $_.Matches.Groups[1].Value } |
		Group-Object |
		Sort-Object Name |
		ForEach-Object { Write-Host ("    {0} x {1}" -f $_.Count, $_.Name) -ForegroundColor Yellow }
}
