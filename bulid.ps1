# 获取脚本的目录
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# 设置工作目录
Set-Location $scriptDir

Remove-Item -Path "./publish/*" -Force
dotnet publish ./MAZDA_MCTool/MAZDA_MCTool.csproj -c Release -r win-x86 -o ./publish

Set-Location -Path $PSScriptRoot
$assemblyPath = "$PSScriptRoot\publish\MAZDA_MCTool.dll"
$fileVersionString = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($assemblyPath).FileVersion

# 分割版本号字符串
$versionParts = $fileVersionString -split '\.'

# 取前三个部分并组合成新的版本号字符串
$shortVersionString = -join ($versionParts[0..2] -join '.')


vpk pack --framework net8.0-x86-desktop -u MAZDAMCTools -v $shortVersionString  -p ./publish -e "MAZDA_MCTool.exe"
Copy-Item -Path ./Releases/MAZDAMCTools-win-Setup.exe -Destination ./CustomInstaller/Resources/CoreProgram -Force
msbuild ./CustomInstaller/CustomInstaller.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:DeployOnBuild=true /p:OutputPath=bin\Release
Copy-Item -Path ./CustomInstaller/bin/Release/CustomInstaller.exe -Destination ./Releases -Force
Write-Output Done