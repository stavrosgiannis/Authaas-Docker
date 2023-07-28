# Define the application name, ZIP file path, and the target directory
$appName = "AuthaasDocker"
$zipFile = "AuthaasDocker.zip"
$targetDir = Get-Location

# Close the application
Get-Process | Where-Object { $_.Name -eq $appName } | Stop-Process -Force

# Wait a little to make sure the process has fully stopped
Start-Sleep -Seconds 5


# Extract the ZIP file to the target directory
Expand-Archive -Path $zipFile -DestinationPath $targetDir -Force

# Start the application
Start-Process -FilePath "$targetDir\$appName.exe"