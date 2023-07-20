param(
    [Parameter(Mandatory = $True)]
    [String]
    $DraftFileName
)

$ErrorActionPreference = "Stop"

$tempFolder = New-Item -ItemType Directory -Force -Path ".\temp"
$processingFolder = New-Item -ItemType Directory -Force -Path ".\MtgDraftAnalyser\17lands"

$draftOutputName = "draft_data_public.${DraftFileName}"
$csv = "${draftOutputName}.csv"
$zip = "${csv}.gz"
$json = "${draftOutputName}.json"
$csvZipOutPath = "${tempFolder}\${zip}"
$csvOutPath = "${processingFolder}\${csv}"
$jsonOutPath = "${processingFolder}\${json}"
$jsonZipOutPath = "${processingFolder}\${json}.zip"

$hasFetched = Test-Path -Path $csvZipOutPath
$hasUnzipped = Test-Path -Path $csvOutPath

$7zipExePath = "C:\Program Files\7-Zip\7z.exe"

if (!$hasFetched -and !$hasUnzipped) {
    $zipUri = "https://17lands-public.s3.amazonaws.com/analysis_data/draft_data/${zip}"
    Write-Host "Fetching zip from ${zipUri}"
    Invoke-WebRequest `
        -Uri $zipUri `
        -OutFile $csvZipOutPath
}
else {
    Write-Host "Zip already fetched - skipping"
}

if (!$hasUnzipped) {
    Write-Host "Unzipping ${csvZipOutPath} with 7zip to $processingFolder"
    & ${7zipExePath} x ${csvZipOutPath} "-o${processingFolder}"
    if (-not $?) {
        exit $LASTEXITCODE
    }
}
else {
    Write-Host "File already unzipped - skipping"
}

Write-Host "Removing $tempFolder"
Remove-Item $tempFolder -Recurse

Push-Location ".\MtgDraftAnalyser"
try {
    $hasParsedJson = Test-Path -Path $jsonOutPath
    if (!$hasParsedJson) {
        Write-Host "Parsing ${csv}"
        dotnet run -- $csv
        if (-not $?) {
            exit $LASTEXITCODE
        }
    }
    else {
        Write-Host "${csv} already parsed - skipping"
    }

    $hasZippedJson = Test-Path -Path $jsonZipOutPath
    if (!$hasZippedJson) {
        Write-Host "Zipping ${json}"
        & ${7zipExePath} "a" ${jsonZipOutPath} ${jsonOutPath} "-mx=9"
        if (-not $?) {
            exit $LASTEXITCODE
        }
    }
    else {
        Write-Host "${json} already zipped - skipping"
    }

}
finally {
    Pop-Location
}

Remove-Item $csvOutPath
Write-Host "Done!"






