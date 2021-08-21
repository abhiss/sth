let wrapper = document.getElementById("map_maker_wrapper");
let save_button = document.getElementById("map_maker_save_button");

let input_tag = document.getElementById("map_maker_car_tag");
let input_team = document.getElementById("map_maker_car_team");

save_button.onclick = function () {
    window.sendNuiEvent("save_map_maker_car", {
        "tag": input_tag.innerText,
        "team": input_team.innerText
    });

    close_menu();
}

export function OpenMapMakerMenu() {
    wrapper.style.display = "grid";
}

function close_menu() {
    input_tag.innerText = "";
    input_team.innerText = "";
    wrapper.style.display = "none";
}


