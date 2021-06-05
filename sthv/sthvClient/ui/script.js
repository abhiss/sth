import {ModuleLoadTest, addToastNotification} from "./modules/toastNotify.js";
import {  } from "./modules/AdminMenu.js";


console.log(ModuleLoadTest);


var timer = 0;
var letPlay = false;
window.sthv = {}

$(document).ready(function () {
	var time = 20;
	let spectatedPlayerNameBeingShown = "error"
	//document.querySelector(".scoreboard").style.display = 'none';//

	//gsap stuff
	let dur1 = 1
	gsap.from('.intro-g2', { duration: dur1, opacity: 0, ease: 'back.out(.5)' })
	gsap.from('.intro-mid', { duration: dur1, opacity: 0, ease: 'back.out(.5)' })
	addToastNotification("Welcome to Survive the Hunt.", 2000);
	// var tl = gsap.timeline({onComplete: ()=> {t1.reverse()}});
	// tl.to("#toast_message_wrapper", {opacity: 100, duration: 1000}, '+=1');
	
	//t1.play();

	window.setInterval(function () {
		var countdown = $('#countdown');
		var time = parseInt(countdown.data("time"));

		if (time > 0) {
			countdown.data("time", --time);

			var min = parseInt(Math.floor(time / 60) % 60);
			var sec = time % 60;
			countdown.text((min < 10 ? "0" + min : min) + ":" + (sec < 10 ? "0" + sec : sec));
		} else if (countdown.is(":visible")) {
			$('.countdown').fadeOut(200);
		}

	}, 1000);
	window.addEventListener('message', function (e) {
		//this.console.log(JSON.stringify(e.data));
		var event = e.data;
		var item = event.EventData;
		if (event.EventName === "hunttimer") {
			this.console.log("hunt timer registered by nui, time: " + item["Seconds"]);
			startCountdown(item["Message"], item["Seconds"]);
			return;
		}
		if(event.EventName == "sthv:showToastNotification"){
			//console.log("GOT MESSAGE FOR TOASTNOTIF ")
			if(item.display_time) addToastNotification(item.message, item.display_time)
			else addToastNotification(item.message);
		}
		if (event.EventName === "sthv:runneropt") {
			console.log("show: " + item)
			if (item) {
				this.console.log("showing runner opt screen");
				$(".startscreen").show();
				setTimeout(() => {
					$(".startscreen").hide();
				}, 10000);
			}
			else {
				$(".startscreen").hide();
			}
		}
		if (event.EventName === "sthvnui:updateAlive") {
			//this.console.log(`^1 sthvnui:updateAlive triggered, serverid= ${item["serverid"]}, isalive = ${item["isalive"]}`);
			let serverid = item["serverid"];
			let targetelem = document.getElementById(`player${serverid}`).childNodes[5];
			if (targetelem) {
				if (item["isalive"]) {


				}
				else {
					document.querySelector(".player" + serverid).innerHTML += ('<img src="assets/dead.png" width="21vh" style="filter:invert(100%); margin: 0vh 0vh -0.28vh 0vh">')
				}
			}
		}
		if (event.EventName === "sthv.showsb") {
			let show = item["data"];
			//this.console.log("showing sb:" + item["data"]);
			if (show) {
				$(".scoreboardtable").css("visibility", "visible");
			}
			else {
				setTimeout(() => {
					$(".scoreboardtable").css("visibility", "hidden");
				}, 0);
			}
		}
		if (event.EventName === "sthv:updatesb") {
			resetsb();
			item.forEach(element => {
				//this.console.log(`^1 list of stuff name = ${element["name"]}, isrunner = ${element["runner"]}, serverid = ${element["serverid"]}, isalive = ${element["alive"]}, isinhel = ${element["isinheli"]}`);
				AddPlayer(element["serverid"], element["name"], element["alive"], element["runner"], element["isinheli"]);

			});
		}

		if (event.EventName === "sthvui:spectatorinfo") {
			let spectatedPlayer = item["nameOfSpectatedPlayer"]
			//this.console.log("recieved spectator info, spectator id: " + spectatedPlayer);
			if (spectatedPlayer == "") { $(".spectatingText").remove() }
			else if (spectatedPlayer != spectatedPlayerNameBeingShown) {
				$(".spectatingText").remove();
				$(".spectatingplayer").append("<span class = 'spectatingText'><span>Spectating: </span> <span> " + spectatedPlayer + "</span></span>");
			}
		}
		if (event.EventName == "sthvui:introoff") {
			gsap.to(".intro-g2", { duration: 1, opacity: 0, ease: 'power2' })
			gsap.to(".intro-mid", { duration: 1, opacity: 0, ease: 'power2' })
			setTimeout(() => {
				$('intro').hide();
			}, 1000);
		}
		if (event.EventName == "sthv:discordVerification") {
			let hasDiscord = item["has_discord"];
			let inSth = item["is_in_sth"];
			let inVc = item["is_in_vc"];
			let isDiscordOnline = item['is_discord_online']

			if (hasDiscord) {
				document.getElementById("hasdiscord").innerHTML += '<img src="assets/check.png" alt="true">';
			}
			else document.getElementById("hasdiscord").innerHTML += '<img src="assets/cross.png" alt="true">';
			if (inSth || !isDiscordOnline) {
				if (!isDiscordOnline) {
					document.getElementById('validatingWithDiscord').innerHTML = '<p><img width="2%" src="./assets/err.png"> Discord validation failed for technical reasons. <br>You can still play. :)</p>'
					document.getElementById("insth").innerHTML += '<img src="assets/cross.png" alt="true">';
				}
				else document.getElementById("insth").innerHTML += '<img src="assets/check.png" alt="true">';

				const playbtn_elm = document.getElementById('playbtn')
				playbtn_elm.classList.add('intro-selectable');
				playbtn_elm.click();
				
				letPlay = true;

			}
			else if (!inSth && isDiscordOnline) { //discord works but person isnt in the discord server.
				document.getElementById("insth").innerHTML += '<img src="assets/cross.png" alt="true">';
				$('.hide').removeClass('hide')
				document.getElementById('playbtn').classList.remove('intro-selectable');
				letPlay = false;
			}
			if (inVc) document.getElementById("invc").innerHTML += '<img src="assets/check.png" alt="true">';
			else {
				document.getElementById("invc").innerHTML += '<img src="assets/cross.png" alt="true">';

				if (isDiscordOnline) {
					document.querySelector('.intro-mid').innerHTML += '<p><img width="2%" src="./assets/err.png"> hunters not in pc-voice dont get guns at spawn <img width="2%" src="./assets/err.png"></p>'
				}
			}
		}
		// switch(event.EventName){
		// 	case "hunt.countdown":
		// 		console.log(item);
		// 		startCountdown(item["Message"], item["Seconds"]);
		// 		return;
		// }
	});
	//register click events here
	document.querySelector("#optin").addEventListener("click", function () {
		console.log("wants to run");
		sendNuiEvent("nui:returnWantsToRun", { "opt": true });
		$(".startscreen").hide();

	});
	document.querySelector("#optout").addEventListener("click", function () {
		$(".startscreen").hide();
	});

	document.getElementById("playbtn").addEventListener('click', function () {
		if (letPlay) {
			gsap.to(".intro-g2", { duration: 1, opacity: 0, ease: 'power2' })
			gsap.to(".intro-mid", { duration: 1, opacity: 0, ease: 'power2' })
			setTimeout(() => {
				$('intro').hide();
			}, 1000);
			sendNuiEvent("sthv:requestspawn")
		}
		else {
			console.log('letplay = false');
		}
	});
	document.querySelector("#discordbtn").addEventListener('click', function () {
	});
	document.querySelector("#rulesbtn").addEventListener('click', function () {
	});
	document.querySelector("#settingsbtn").addEventListener('click', function () {
	});


	let serverId = 1;
	let name = "normalname";
	let ping = 16;
	let isAlive = true;
	let isRunner = true;
	for (var i = 0; i < 10; i++) {
		AddPlayer(i, name, isAlive, isRunner);
	}

	AddPlayer(i, "adumblongplayernamewithtoomanyletters", false, false);
	/*
	ServerId
	Name</th
	Alive</t
	Ping</th
	Runner</
	*/

});

function resetsb() {
	$(".playerintable").remove();
}
function AddPlayer(serverid, playername, isalive, isrunner, isinheli) {

	if (playername.length > 15) {
		playername = playername.slice(0, 15) + "...";
	}
	{
		$(".scoreboardtable").append(
			`<tr class="playerintable player${serverid}">
		<td id="id${serverid}">${serverid}</td>
		<td>${playername}</td>
		</tr>`);
	}

	if (isinheli) {
		console.log("player" + serverid + "is in heli");
		document.querySelector(".player" + serverid).innerHTML += ('<img src="assets/heliicon.png" alt="heli" width="30vh" style="margin: 0vh 0vh -.52vh -1vh;">')
	}
	if (isrunner) {
		document.querySelector(".player" + serverid).innerHTML += ('<img src="assets/target.png" width="23vh" style="filter:invert(100%); margin: 0vh 0vh -0.25vh 0vh">')
	}
	if (!isalive) {

		document.querySelector(".player" + serverid).innerHTML += ('<img src="assets/dead.png" width="21vh" style="filter:invert(100%); margin: 0vh 0vh -0.28vh 0vh">')
	}

	// $(".scoreboardtable").append(`<td>${serverid}</td>`);
	// $(".scoreboardtable").append(`<td>${playername}</td>`);
	// $(".scoreboardtable").append(`<td>${isalive}</td>`);
	// $(".scoreboardtable").append(`<td>${playerping}</td>`);
	// $(".scoreboardtable").append(`<td>${isrunner}</td>`);

};


function startCountdown(msg, time) {
	if (time <= 0) {
		$('.countdown').hide();
		return;
	}
	$('#countdown').data("time", time);
	//$('#countdown-message').text(msg);
	$('.countdown').show(); //should be .timer but i dont want to hide it anyways
};
window.sendNuiEvent = function(name, data = {}) {
	$.post("https://sthv/" + name, JSON.stringify(data));
};

function addPosPercentToTween(originalTween, addend, isX) {
	if (isX) {
		let i = (Number(originalTween.vars.x.slice(0, -1)) + addend).toString() + '%';
		console.log(i)
		return i
	}
	else {
		let i = (Number(originalTween.vars.y.slice(0, -1)) + addend).toString() + '%';
		console.log(i)
		return i
	}


}