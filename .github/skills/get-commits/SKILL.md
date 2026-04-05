---
name: get-commits
description: Get git commits from a branch filtered by date
---
## When to use
Use this skill when you need to retrieve a list of git commits from a specific branch that were made after a certain date. 

## Instructions
1. Use the PowerShell script `Get-CommitsSince.ps1` located in this skill folder
2. Pass the required `$SinceDate` parameter in a valid date format (e.g., "2024-01-01" or "January 1, 2024")
3. Optionally specify a branch name with `-Branch` parameter (defaults to "master")
4. The script returns commit objects with the following properties:
   - Hash: The commit SHA
   - Author: The commit author name
   - Email: The commit author email
   - Date: The commit date in ISO format
   - Message: The commit message
   - Description: The commit description (message body)
## Usage Example
```powershell
# Get all commits from master branch since March 1, 2024
$commits = & .\.github\skills\get-commits\Get-CommitsSince.ps1 -SinceDate "2024-03-01"

# Get commits from a different branch
$commits = & .\.github\skills\get-commits\Get-CommitsSince.ps1 -SinceDate "2024-03-01" -Branch "develop"

# Process the returned commits
$commits | ForEach-Object {
    Write-Host "$($_.Hash.Substring(0,7)) - $($_.Description)"
}
```

## Output Format
The script returns an array of PowerShell objects, each representing a commit with structured properties that can be easily filtered, sorted, or processed further.

## Error Handling
- The script validates the date format and exits with an error if invalid
- It checks git command success and reports errors if the branch doesn't exist or git fails
- Returns an empty array if no commits are found since the specified date
