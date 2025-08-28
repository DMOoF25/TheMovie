# Get the current username
$username = $env:USERNAME

# Define source and destination
$sourceFolder = ".\sampleData"
$destinationFolder = Join-Path $env:LOCALAPPDATA "TheMovie"

# Create the destination folder if it doesn't exist
if (-not (Test-Path -Path $destinationFolder)) {
    New-Item -ItemType Directory -Path $destinationFolder | Out-Null
}

# Get all CSV files in the source folder
$csvFiles = Get-ChildItem -Path $sourceFolder -Filter *.csv

# Copy each file to the destination
foreach ($file in $csvFiles) {
    Copy-Item -Path $file.FullName -Destination $destinationFolder -Force
}

Write-Host "CSV files copied to $destinationFolder for user $username"