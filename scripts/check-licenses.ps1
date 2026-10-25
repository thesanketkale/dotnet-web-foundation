# scripts/check-licenses.ps1
# Enforces ADR-0003: Dependency License Policy & Elimination of Commercialized Libraries.
# Validates that all NuGet packages declared in Directory.Packages.props carry approved permissive licenses.

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

Write-Host "Auditing package licenses against permissive allow-list (ADR-0003)..." -ForegroundColor Cyan

$allowedLicenses = @{
    'Microsoft.AspNetCore.Identity.EntityFrameworkCore' = 'MIT'
    'Microsoft.EntityFrameworkCore'                      = 'MIT'
    'Microsoft.EntityFrameworkCore.Design'               = 'MIT'
    'Microsoft.EntityFrameworkCore.InMemory'             = 'MIT'
    'Microsoft.EntityFrameworkCore.Relational'           = 'MIT'
    'Microsoft.EntityFrameworkCore.SqlServer'            = 'MIT'
    'Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore' = 'MIT'
    'MySql.EntityFrameworkCore'                          = 'GPL-2.0-with-FOSS-exception'
    'Microsoft.AspNetCore.OpenApi'                       = 'MIT'
    'Microsoft.AspNetCore.Authentication.JwtBearer'      = 'MIT'
    'System.Security.Cryptography.Pkcs'                 = 'MIT'
    'System.Text.Json'                                   = 'MIT'
    'Markdig'                                            = 'BSD-2-Clause'
    'HtmlSanitizer'                                      = 'MIT'
    'Serilog.AspNetCore'                                 = 'Apache-2.0'
    'Serilog.Sinks.Console'                              = 'Apache-2.0'
    'Serilog.Sinks.File'                                 = 'Apache-2.0'
    'OpenTelemetry.Extensions.Hosting'                   = 'Apache-2.0'
    'OpenTelemetry.Instrumentation.AspNetCore'           = 'Apache-2.0'
    'OpenTelemetry.Instrumentation.Http'                 = 'Apache-2.0'
    'OpenTelemetry.Instrumentation.Runtime'              = 'Apache-2.0'
    'OpenTelemetry.Exporter.OpenTelemetryProtocol'       = 'Apache-2.0'
    'OpenTelemetry.Exporter.Console'                     = 'Apache-2.0'
    'Microsoft.AspNetCore.Mvc.Testing'                  = 'MIT'
    'Microsoft.NET.Test.Sdk'                             = 'MIT'
    'xunit'                                              = 'Apache-2.0'
    'xunit.runner.visualstudio'                          = 'MIT'
    'coverlet.collector'                                 = 'MIT'
    'Shouldly'                                           = 'MIT'
    'NetArchTest.Rules'                                  = 'MIT'
    'Testcontainers.MsSql'                               = 'MIT'
    'Testcontainers.MySql'                               = 'MIT'
    'Microsoft.Playwright'                               = 'Apache-2.0'
}

$propsFile = Join-Path $PSScriptRoot '..\Directory.Packages.props'
if (-not (Test-Path $propsFile)) {
    Write-Error "Directory.Packages.props not found at: $propsFile"
    exit 1
}

[xml]$xml = Get-Content $propsFile
$packages = $xml.Project.ItemGroup.PackageVersion

$disallowed = @()
$auditedCount = 0

foreach ($pkg in $packages) {
    $id = $pkg.Include
    $version = $pkg.Version
    $auditedCount++

    if ($allowedLicenses.ContainsKey($id)) {
        $lic = $allowedLicenses[$id]
        Write-Host "  [OK] $id ($version) -> License: $lic" -ForegroundColor Green
    } else {
        Write-Host "  [FAIL] $id ($version) is NOT on the approved license allow-list!" -ForegroundColor Red
        $disallowed += $id
    }
}

if ($disallowed.Count -gt 0) {
    Write-Error "License audit FAILED! Unapproved dependencies detected: $($disallowed -join ', ')"
    exit 1
}

Write-Host "License audit PASSED ($auditedCount packages verified, 0 license violations)." -ForegroundColor Green
exit 0
