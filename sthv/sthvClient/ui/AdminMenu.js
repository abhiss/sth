let Menu = {
    save_success: async function () {
        let saveButton = document.getElementById('admin_menu_save');
        saveButton.style.borderColor = 'limegreen';
        await new Promise(resolve => setTimeout(resolve, 1000));
        saveButton.style.borderColor = 'white';
    }
}

export function OpenMenu() {
    is_menu_open = !is_menu_open;
    if (is_menu_open) open_menu();
    else close_menu();
}

function open_menu() {

}
function close_menu() {

}

document.getElementById('admin_menu_save').onclick = function onAdminMenuSaveClick() {
    console.log('saving admin menu');
    let res = {};
    res['next_respawn_time'] = document.getElementById('ip1').value
    res['next_runner_serverid'] = document.getElementById('ip2').value
    res['next_hunt_length'] = document.getElementById('ip3').value
    res['next_map'] = document.getElementById('ip4').value
    res['is_friendly_fire_allowed'] = (document.getElementById('ip5').value == 'Allowed') ? true : false;
    let json = JSON.stringify(res);
    console.log(json);
    window.sendNuiEvent("admin_menu_save", json);
}