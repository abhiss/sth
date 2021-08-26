fx_version 'cerulean'
game 'gta5'

author 'abhi'
description 'A Survive the Hunt gamemode.'
version '1.0.0'
resource_type 'gametype' { name = 'Survive the Hunt' }

loadscreen_manual_shutdown "yes"

client_scripts{
    'sthvClient.net.dll'
}
server_script 'sthvServer.net.dll'
ui_page 'ui/index.html'
loadscreen 'loadingscreen/loadingscreen.html'
files {
    'loadingscreen/*',
    'ui/index.html',
    'Newtonsoft.Json.dll',
    'System.ValueTuple.dll',
    'ui/style.css',
    'ui/script.js',
    'ui/assets/*',
    'ui/modules/*.js'
}

