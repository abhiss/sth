
[CmdletBinding()]
param (
    [Parameter()]
    [string] $source,
    [string] $OutputSubfolder
)
$OutputPath = "C:\Users\abhi-\FiveM\cfx-server-data-master\resources\[local]\sthv"
echo ($OutputPath +  $OutputSubfolder)
Copy-Item -Path $source -Destination ($OutputPath +  "\\" + $OutputSubfolder) -Recurse -Force

Write-Output "helper ran successfully"

# -source $(ProjectDir)ui -OutputSubfolder "ui"

# xcopy /y /d "$(TargetPath)" "C:\Users\abhi-\FiveM\cfx-server-data-master\resources\[local]\sthv"