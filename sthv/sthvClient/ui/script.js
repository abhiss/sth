var timer = 0;

$(document).ready(function() {
	var time = 20;
	//
	$(".players").append($("<li class = 'runner'>").text("another player" + " - - - 5"));
	//	
	
	window.setInterval(function() {
		var countdown = $('#countdown');
        var time = parseInt(countdown.data("time"));
        
		if(time > 0) {
			countdown.data("time", --time);

			var min = parseInt(Math.floor(time / 60) % 60);
			var sec = time % 60;
			countdown.text((min < 10 ? "0"+min : min)+":"+(sec < 10 ? "0"+sec : sec));
		} else if(countdown.is(":visible")) {
			$('.countdown').fadeOut(200);
        }
	}, 1000);

	window.addEventListener('message', function(e) {
		var event = e.data;
		var item = event.EventData;
		
		if(event.EventName === "hunt.countdown"){
			this.console.log("hunt timer registered by nui, time: " + item["Seconds"]);
			startCountdown(item["Message"], item["Seconds"]);
			return;
		}
		if(event.EventName === "sthv:runneropt"){
			this.console.log("showing runner opt screen");
			$(".startscreen").show();
		}
		if(event.EventName === "hunt.testNuiEvent"){
			this.console.log(item["name"] + " " + item["runner"] + " " + item["serverid"])
		}
		if(event.EventName === "sthv:refreshscoreboard"){
			this.console.log("event refreshscoreboard triggered");
		}
		// switch(event.EventName){
		// 	case "hunt.countdown":
		// 		console.log(item);
		// 		startCountdown(item["Message"], item["Seconds"]);
		// 		return;
	
		// }
	});
	//register click events here VVV
	$("#optin").on("click", function(){
		console.log("wants to run");
		sendNuiEvent("nui:returnWantsToRun", {"opt": true});
		$(".startscreen").hide();

		});
	$("#optout").on("click", function(){
		console.log("doesnt not want to run");
		sendNuiEvent("nui:returnWantsToRun", {"opt": false});
		$(".startscreen").hide();
	});

	let serverId = 1;
	let name = "abhi";
	let ping = 16;
	let isAlive = true;
	let isRunner = true;
	AddPlayer(serverId, name, isAlive, ping, isRunner);
	AddPlayer(2, "adumblongplayernamewithtoomanyletters", true, 13, false);
/*
ServerId
Name</th
Alive</t
Ping</th
Runner</
*/

});

function resetsb(){
	document.getElementsByClassName("")
}
function AddPlayer(serverid, playername, isalive, playerping, isrunner){

			if(playername.length > 15){
				playername = playername.slice(0, 15) + "...";
				console.log(playername);

			}
			if(isrunner === true){
				$(".scoreboardtable").append(
					`<tr class = "player">
					<td>${serverid}</td>
					<td>${playername}</td>
					<td>${isalive}</td>
					<td>${playerping}</td>
					<td class= "runner">${isrunner}</td>
					</tr>`);
			}
			else{
				$(".scoreboardtable").append(
					`<tr class = "player">
					<td>${serverid}</td>
					<td>${playername}</td>
					<td>${isalive}</td>
					<td>${playerping}</td>
					<td>${isrunner}</td>
					</tr>`);
			}

	// $(".scoreboardtable").append(`<td>${serverid}</td>`);
	// $(".scoreboardtable").append(`<td>${playername}</td>`);
	// $(".scoreboardtable").append(`<td>${isalive}</td>`);
	// $(".scoreboardtable").append(`<td>${playerping}</td>`);
	// $(".scoreboardtable").append(`<td>${isrunner}</td>`);

};


function startCountdown(msg, time) {
	if(time <= 0) {
		$('.countdown').hide();
		return;
	}
	$('#countdown').data("time", time);
	//$('#countdown-message').text(msg);
	$('.countdown').show(); //should be .timer but i dont want to hide it anyways
};
function sendNuiEvent(name, data={}) {
	$.post("http://sthv/"+name, JSON.stringify(data));
};