param(
    [string]$Root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

$scanFolders = @("Repositories", "Data", "Services")
$patterns = @(
    @{
        Name = "Interpolated CommandText"
        Regex = 'CommandText\s*=\s*\$@?"'
    },
    @{
        Name = "Interpolated CommandText append"
        Regex = 'CommandText\s*\+=\s*\$@?"'
    },
    @{
        Name = "Interpolated LIMIT"
        Regex = 'LIMIT\s+\{[^}]+\}'
    },
    @{
        Name = "Interpolated ORDER BY"
        Regex = 'ORDER\s+BY[^\r\n;]*\{[^}]+\}'
    },
    @{
        Name = "Raw SQL clause helper parameter"
        Regex = 'string\s+(whereClause|orderClause|sortClause|filterClause)'
    }
)

$findings = New-Object System.Collections.Generic.List[object]

foreach ($folder in $scanFolders) {
    $path = Join-Path $Root $folder
    if (-not (Test-Path $path)) {
        continue
    }

    Get-ChildItem -Path $path -Filter "*.cs" -Recurse | ForEach-Object {
        $file = $_.FullName
        $lines = Get-Content -Path $file

        for ($i = 0; $i -lt $lines.Count; $i++) {
            foreach ($pattern in $patterns) {
                if ($lines[$i] -match $pattern.Regex) {
                    $findings.Add([pscustomobject]@{
                        Rule = $pattern.Name
                        File = $file.Substring($Root.Length + 1)
                        Line = $i + 1
                        Text = $lines[$i].Trim()
                    })
                }
            }
        }
    }
}

if ($findings.Count -eq 0) {
    Write-Host "SQL safety scan passed. No risky SQL construction patterns found."
    exit 0
}

Write-Host "SQL safety scan found risky SQL construction patterns:"
$findings | Format-Table -AutoSize
exit 1
