
[CmdletBinding()]
param (
    [Parameter()]
    [string] $source
)
###Change this
$OutputPath = "C:\Users\abhi-\FiveM\cfx-server-data-master\resources\[local]\sthv\"
Write-Output ($OutputPath +  $OutputSubfolder)

Copy-Item -Path $source -Destination ($OutputPath) -Recurse -Force

###Usage
# -source $(ProjectDir)ui
###Done in post event commands.
# xcopy /y /d "$(TargetPath)" "C:\Users\abhi-\FiveM\cfx-server-data-master\resources\[local]\sthv"