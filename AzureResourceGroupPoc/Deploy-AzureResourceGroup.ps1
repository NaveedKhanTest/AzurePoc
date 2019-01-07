#Requires -Version 3.0

Param(
    [string] [Parameter(Mandatory=$true)] $ResourceGroupLocation,
    [string] $ResourceGroupName = 'AzurePoc-2',
    [switch] $UploadArtifacts,
    [string] $StorageAccountName,
    [string] $StorageContainerName = $ResourceGroupName.ToLowerInvariant() + '-stageartifacts',
    [string] $TemplateFile = 'WebSite.json',
    [string] $TemplateParametersFile = 'WebSite.parameters.json',
    [string] $ArtifactStagingDirectory = '.',
    [string] $DSCSourceFolder = 'DSC',
    [switch] $ValidateOnly
)

try {
    [Microsoft.Azure.Common.Authentication.AzureSession]::ClientFactory.AddUserAgent("VSAzureTools-$UI$($host.name)".replace(' ','_'), '3.0.0')
} catch { }

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3

function Format-ValidationOutput {
    param ($ValidationOutput, [int] $Depth = 0)
    Set-StrictMode -Off
    return @($ValidationOutput | Where-Object { $_ -ne $null } | ForEach-Object { @('  ' * $Depth + ': ' + $_.Message) + @(Format-ValidationOutput @($_.Details) ($Depth + 1)) })
}

$OptionalParameters = New-Object -TypeName Hashtable
$TemplateFile = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($PSScriptRoot, $TemplateFile))
$TemplateParametersFile = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($PSScriptRoot, $TemplateParametersFile))

if ($UploadArtifacts) {
    # Convert relative paths to absolute paths if needed
    $ArtifactStagingDirectory = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($PSScriptRoot, $ArtifactStagingDirectory))
    $DSCSourceFolder = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($PSScriptRoot, $DSCSourceFolder))

    # Parse the parameter file and update the values of artifacts location and artifacts location SAS token if they are present
    $JsonParameters = Get-Content $TemplateParametersFile -Raw | ConvertFrom-Json
    if (($JsonParameters | Get-Member -Type NoteProperty 'parameters') -ne $null) {
        $JsonParameters = $JsonParameters.parameters
    }
    $ArtifactsLocationName = '_artifactsLocation'
    $ArtifactsLocationSasTokenName = '_artifactsLocationSasToken'
    $OptionalParameters[$ArtifactsLocationName] = $JsonParameters | Select -Expand $ArtifactsLocationName -ErrorAction Ignore | Select -Expand 'value' -ErrorAction Ignore
    $OptionalParameters[$ArtifactsLocationSasTokenName] = $JsonParameters | Select -Expand $ArtifactsLocationSasTokenName -ErrorAction Ignore | Select -Expand 'value' -ErrorAction Ignore

    # Create DSC configuration archive
    if (Test-Path $DSCSourceFolder) {
        $DSCSourceFilePaths = @(Get-ChildItem $DSCSourceFolder -File -Filter '*.ps1' | ForEach-Object -Process {$_.FullName})
        foreach ($DSCSourceFilePath in $DSCSourceFilePaths) {
            $DSCArchiveFilePath = $DSCSourceFilePath.Substring(0, $DSCSourceFilePath.Length - 4) + '.zip'
            Publish-AzureRmVMDscConfiguration $DSCSourceFilePath -OutputArchivePath $DSCArchiveFilePath -Force -Verbose
        }
    }

    # Create a storage account name if none was provided
    if ($StorageAccountName -eq '') {
        $StorageAccountName = 'stage' + ((Get-AzureRmContext).Subscription.SubscriptionId).Replace('-', '').substring(0, 19)
    }

    $StorageAccount = (Get-AzureRmStorageAccount | Where-Object{$_.StorageAccountName -eq $StorageAccountName})

    # Create the storage account if it doesn't already exist
    if ($StorageAccount -eq $null) {
        $StorageResourceGroupName = 'ARM_Deploy_Staging'
        New-AzureRmResourceGroup -Location "$ResourceGroupLocation" -Name $StorageResourceGroupName -Force
        $StorageAccount = New-AzureRmStorageAccount -StorageAccountName $StorageAccountName -Type 'Standard_LRS' -ResourceGroupName $StorageResourceGroupName -Location "$ResourceGroupLocation"
    }

    # Generate the value for artifacts location if it is not provided in the parameter file
    if ($OptionalParameters[$ArtifactsLocationName] -eq $null) {
        $OptionalParameters[$ArtifactsLocationName] = $StorageAccount.Context.BlobEndPoint + $StorageContainerName
    }

    # Copy files from the local storage staging location to the storage account container
    New-AzureStorageContainer -Name $StorageContainerName -Context $StorageAccount.Context -ErrorAction SilentlyContinue *>&1

    $ArtifactFilePaths = Get-ChildItem $ArtifactStagingDirectory -Recurse -File | ForEach-Object -Process {$_.FullName}
    foreach ($SourcePath in $ArtifactFilePaths) {
        Set-AzureStorageBlobContent -File $SourcePath -Blob $SourcePath.Substring($ArtifactStagingDirectory.length + 1) `
            -Container $StorageContainerName -Context $StorageAccount.Context -Force
    }

    # Generate a 4 hour SAS token for the artifacts location if one was not provided in the parameters file
    if ($OptionalParameters[$ArtifactsLocationSasTokenName] -eq $null) {
        $OptionalParameters[$ArtifactsLocationSasTokenName] = ConvertTo-SecureString -AsPlainText -Force `
            (New-AzureStorageContainerSASToken -Container $StorageContainerName -Context $StorageAccount.Context -Permission r -ExpiryTime (Get-Date).AddHours(4))
    }
}

# Create or update the resource group using the specified template file and template parameters file
 Write-Output '', ' Going to run New-AzureRmResourceGroup....' 
 # Creates an Azure resource group 
 New-AzureRmResourceGroup -Name 'AzurePoc002' -Location 'Central US' 
 #New-AzureRmResourceGroup -Name $ResourceGroupName -Location $ResourceGroupLocation -Verbose -Force
 Write-Output '', ' Finished runing New-AzureRmResourceGroup....'
if ($ValidateOnly) {
    $ErrorMessages = Format-ValidationOutput (Test-AzureRmResourceGroupDeployment -ResourceGroupName $ResourceGroupName `
                                                                                  -TemplateFile $TemplateFile `
                                                                                  -TemplateParameterFile $TemplateParametersFile `
                                                                                  @OptionalParameters)
    if ($ErrorMessages) {
        Write-Output '', 'Validation returned the following errors:', @($ErrorMessages), '', 'Template is invalid.'
    }
    else {
        Write-Output '', 'Template is valid.'
    }
}
else {
    New-AzureRmResourceGroupDeployment -Name ((Get-ChildItem $TemplateFile).BaseName + '-' + ((Get-Date).ToUniversalTime()).ToString('MMdd-HHmm')) `
                                       -ResourceGroupName $ResourceGroupName `
                                       -TemplateFile $TemplateFile `
                                       -TemplateParameterFile $TemplateParametersFile `
                                       @OptionalParameters `
                                       -Force -Verbose `
                                       -ErrorVariable ErrorMessages
    if ($ErrorMessages) {
        Write-Output '', 'Template deployment returned the following errors:', @(@($ErrorMessages) | ForEach-Object { $_.Exception.Message.TrimEnd("`r`n") })
    }
}
# SIG # Begin signature block
# MIIFmgYJKoZIhvcNAQcCoIIFizCCBYcCAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQU0RsDjT7q+okFF4Aqo0i6jkFN
# SaGgggMmMIIDIjCCAgqgAwIBAgIQR+FckLzMU4VF0wJaPzynfzANBgkqhkiG9w0B
# AQUFADApMScwJQYDVQQDDB5JVFNQIFByb2plY3QgVGVhbSBjb2RlIFNpZ25pbmcw
# HhcNMTgwODAxMDAzMzIwWhcNMjMwODAxMDA0MzIwWjApMScwJQYDVQQDDB5JVFNQ
# IFByb2plY3QgVGVhbSBjb2RlIFNpZ25pbmcwggEiMA0GCSqGSIb3DQEBAQUAA4IB
# DwAwggEKAoIBAQCxQF2bfNRMKdF2mHmlRtscB98hU4pzMDXOL2YGxvaXBaBsUH44
# /EeOkTEWPXY3qb3xOm4Q9Ydu2hTtFckChRx+ol/wqIHaTdkYGS8jVthMbD8V6V3n
# PaFixSaEslBQ+uvVyRGTWBtzOJabgoQ5EI5n0/IiFR/0O5UaN5BvFfs1J/SFx4WJ
# o9LQSdkp6/0mbJHqrjTkR0xFNQZDR/d6Zk3hLY2ROZJdpcBONEpudwl0QrdORGM7
# c83qg5m0OtMD9BrMboIKLWnXL6HDg71RI8e20t0Bdnu6WbEtliS9LY91BIp5u19Q
# xCS6YoDDPsoLCl/uxiX/WsrOtP+1gOFpKuKFAgMBAAGjRjBEMA4GA1UdDwEB/wQE
# AwIHgDATBgNVHSUEDDAKBggrBgEFBQcDAzAdBgNVHQ4EFgQUPTAEm45dFFdaUBAB
# As4T3wi7yGAwDQYJKoZIhvcNAQEFBQADggEBAKOshYnuYAkw1QrqNJIB+cIAoc3o
# Rn1K5UxdD1/ZZPOrJ/8TaocNOXYyG9r/yaNYcDd5PSYRTB/XMfFGmkpT3kPYlW5v
# vVhWveX67XKQoeHrt2z1Fgmm1B0bv4WD66J6CXkNKAtcAMnFckCcFhkqC8HdcjT7
# 60Cm4WjbnZ1fIGvlcPzs6zEq/6/evvUo3T+mBhCp5e9ye/0k++kvFa9arXMK91nG
# mkuX+6ys6DNtF+NGa6SHeZ7c0Za+Lzqyxham0/2xnSXtcpUDb5tHyFjxnTWgY9lo
# xXTEQOW4NopSoqOaHaKscraczQhS1/bjos16IjKtgCCs2y1gOktT3sUbWHcxggHe
# MIIB2gIBATA9MCkxJzAlBgNVBAMMHklUU1AgUHJvamVjdCBUZWFtIGNvZGUgU2ln
# bmluZwIQR+FckLzMU4VF0wJaPzynfzAJBgUrDgMCGgUAoHgwGAYKKwYBBAGCNwIB
# DDEKMAigAoAAoQKAADAZBgkqhkiG9w0BCQMxDAYKKwYBBAGCNwIBBDAcBgorBgEE
# AYI3AgELMQ4wDAYKKwYBBAGCNwIBFTAjBgkqhkiG9w0BCQQxFgQUi1jaWKiIW9H4
# PVSWetrd7wB4oagwDQYJKoZIhvcNAQEBBQAEggEAGfhQpHHhJq1wUUhZj7QkXBZa
# WNuktOxkIl7PvntTmBl2r/EQz2UG1HS2IVzh+Cg8FjwTo3Ga10I3mltAnBcqSK6W
# mLhpHKvT4l3s+ED5h2pKCDL07ffKDXChm6uepUSv34GEDHIxxxNJSz13W6GwQTc8
# /K7TD/oR7ZFJPyDF4Gsv7XSoJRTTBzFQR7uXzEDEiAd8DZgT3n45H7n5LEUvxVe5
# 84d+ytkPKAUwipZM4a0EVLjBprV+2M3bFNAQ54hM1tntW763SqlYkTROqw1565cs
# i/Vgvdn7ay5UQrI3QFAA/wpsSbNcZ9+4h1uvNivhao9HmMMNY26dgYuUlK+rpg==
# SIG # End signature block
