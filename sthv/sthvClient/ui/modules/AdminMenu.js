let Menu = {
    save_success: async function () {
        let saveButton = document.getElementById('admin_menu_save');
        saveButton.style.borderColor = 'limegreen';
        await new Promise(resolve => setTimeout(resolve, 1000));
        saveButton.style.borderColor = 'white';
    }
}
export function OpenAdminMenu() {
    var x = document.getElementById("admin_menu_wrapper");
        x.style.display = "grid";
    
}
export function CloseMenu() {
    var x = document.getElementById("admin_menu_wrapper");
    x.style.display = "none";

}

function open_menu() {
    document.getElementById("admin_menu_wrapper").hidden = false;
}
function close_menu() {
    document.getElementById("admin_menu_wrapper").hidden = true;
}

document.getElementById('admin_menu_save').onclick = function onAdminMenuSaveClick() {
    console.log('saving admin menu');
    let res = {};
    res['next_respawn_time'] = document.getElementById('respawn_time').value || 0
    res['next_runner_serverid'] = document.getElementById('next_runner').value || 0
    res['next_hunt_length'] = document.getElementById('hunt_length').value || 0
    res['next_map'] = document.getElementById('next_map').value || 0
    res['seconds_between_hints'] = document.getElementById('hints_interval').value || 0
    res['is_friendly_fire_allowed'] = (document.getElementById('friendly_fire').value == 'friendly_fire')
    res['is_cops_enabled'] = (document.getElementById('enable_cops').value == 'Enabled')
    res['end_hunt'] = document.getElementById("end_hunt").checked
    let result = JSON.stringify(res);
    console.log(res);
    $.post("https://sthv/" + "admin_menu_save", result);

    CloseMenu();
    window.sendNuiEvent("admin_menu_close");
}