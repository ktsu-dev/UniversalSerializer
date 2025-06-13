# PowerShell script to add missing using statements to C# files

$files = Get-ChildItem -Path "UniversalSerializer" -Filter "*.cs" -Recurse

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw

    # Skip if already has using statements
    if ($content -match "using System;") {
        continue
    }

    # Find the namespace line
    if ($content -match "(namespace [^;]+;)") {
        $namespaceMatch = $matches[1]

        # Common using statements needed
        $usings = @(
            "using System;",
            "using System.Collections.Generic;",
            "using System.Threading;",
            "using System.Threading.Tasks;"
        )

        # Add specific usings based on file content
        if ($content -match "JsonConverter|JsonSerializer") {
            $usings += "using System.Text.Json;"
            $usings += "using System.Text.Json.Serialization;"
        }

        if ($content -match "XmlSerializer|XmlDocument") {
            $usings += "using System.Xml;"
            $usings += "using System.Xml.Serialization;"
        }

        if ($content -match "YamlDotNet") {
            $usings += "using YamlDotNet.Core;"
            $usings += "using YamlDotNet.Serialization;"
        }

        if ($content -match "MessagePack") {
            $usings += "using MessagePack;"
        }

        if ($content -match "Tomlyn") {
            $usings += "using Tomlyn;"
            $usings += "using Tomlyn.Model;"
        }

        if ($content -match "IServiceCollection|ServiceCollectionExtensions") {
            $usings += "using Microsoft.Extensions.DependencyInjection;"
        }

        # Remove duplicates and sort
        $usings = $usings | Sort-Object | Get-Unique

        # Create the replacement
        $usingBlock = ($usings -join "`n") + "`n"
        $replacement = $usingBlock + "`n" + $namespaceMatch

        # Replace in content
        $newContent = $content -replace [regex]::Escape($namespaceMatch), $replacement

        # Write back to file
        Set-Content -Path $file.FullName -Value $newContent -NoNewline

        Write-Host "Updated: $($file.FullName)"
    }
}

Write-Host "Finished updating using statements."
