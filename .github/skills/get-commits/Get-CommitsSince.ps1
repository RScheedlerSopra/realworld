param(
    [Parameter(Mandatory=$true)]
    [string]$SinceDate,
    
    [Parameter(Mandatory=$false)]
    [string]$Branch = "master"
)

# Validate date format
try {
    $dateObj = [DateTime]::Parse($SinceDate)
} catch {
    Write-Error "Invalid date format. Please use a valid date string (e.g., '2024-01-01' or 'January 1, 2024')"
    exit 1
}

# Format date for git log (ISO 8601 format)
$formattedDate = $dateObj.ToString("yyyy-MM-dd")

# Get commits from the specified branch since the date
# Using ||| as field delimiter and null byte as commit separator to handle multi-line descriptions
$commits = git log $Branch --since="$formattedDate" --pretty=format:"%H|||%an|||%ae|||%ad|||%s|||%b%x00" --date=iso

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to retrieve commits from branch '$Branch'"
    exit 1
}

if ([string]::IsNullOrWhiteSpace($commits)) {
    Write-Host "No commits found on branch '$Branch' since $formattedDate"
    return @()
}

# Parse commits into objects
# Split on null byte to separate commits, then split each commit on ||| delimiter
$commitList = $commits -split "`0" | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | ForEach-Object {
    $parts = $_ -split '\|\|\|', 6
    if ($parts.Count -eq 6) {
        [PSCustomObject]@{
            Hash = $parts[0]
            Author = $parts[1]
            Email = $parts[2]
            Date = $parts[3]
            Message = $parts[4]
            Description = $parts[5].Trim()
        }
    }
}

return $commitList
