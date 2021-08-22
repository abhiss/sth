let wrapper = document.getElementById("map_maker_wrapper");
let save_button = document.getElementById("map_maker_save_button");
let save_all_button = document.getElementById("map_maker_save_all_button");
let close_button = document.getElementById("map_maker_close_button");

let input_tag = document.getElementById("map_maker_car_tag");
let input_team = document.getElementById("map_maker_car_team");


save_button.onclick = function () {
    console.log(input_tag, input_team)
    let res = {
        "tag": input_tag.value,
        "team": input_team.value
    };
    window.sendNuiEvent("save_map_maker_car", res)
    close_menu();
}

save_all_button.onclick = function () {
    window.sendNuiEvent("save_map_maker_car_final");
    close_menu();
}

close_button.onclick = function () {
    //dont call close_menu to keep the input text.
    wrapper.style.display = "none";

    window.sendNuiEvent("lose_focus");
}

export function OpenMapMakerMenu() {
    wrapper.style.display = "grid";
}

function close_menu() {
    input_tag.innerText = "";
    input_team.innerText = "";
    wrapper.style.display = "none";
}


