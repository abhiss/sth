﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
namespace sthvServer
{
	class sthvDiscordController : BaseScript
	{
		static readonly Dictionary<int, PendingRequest> _pendingRequests = new Dictionary<int, PendingRequest>();
		static readonly string _discordUrl = "http://localhost:3000";
		public sthvDiscordController()
		{
			//test
			//API.RegisterCommand("test", new Action<int, List<object>, string>((src, args, raw) =>{
			//	MovePlayerToVc("661327633602314290", fivemDead);		
			//}), true);
			EventHandlers["__cfx_internal:httpResponse"] += new Action<int, int, string, object>(OnHttpResponse);
		}
		private void OnHttpResponse(int token, int statusCode, string body, dynamic headers)
		{
			if (!_pendingRequests.TryGetValue(token, out var req)) return;

			if (statusCode == 200)
			{
				req.SetResult(body);
			}
			else if (statusCode == 0)
			{
				Debug.WriteLine("^2DISCORD SERVER RETURNED STATUS CODE 0! (OFFLINE)");
			}
			else
				req.SetException(new Exception("Server returned status code: " + statusCode));

			_pendingRequests.Remove(token);
		}
		public string pcVoice = "pc-voice";
		public string fivemHunters = "fivem-hunters";
		public string fivemRunner = "fivem-runner";
		public string fivemDead = "fivem-dead";
		
		/// <summary>returns list of discordId's of members in channel. Empty list if channel is empty.</summary>
		public async Task<string[]> GetPlayersInChannel(string channelName)
		{
			var requestBody = new{
				name = "GetPlayersInChannel",
				data = new {
					channel = channelName 
				}
			};
			var response = await UploadString(_discordUrl, JsonConvert.SerializeObject(requestBody));
			Debug.WriteLine("GetPlayersInChannel response: " + response);
			var output = JsonConvert.DeserializeObject<string[]>(response);
			return output;
		}
		public async Task<bool> GetIsPlayerInGuild (string discordid)
		{
			var requestBody = new
			{
				name = "GetIsPlayerInGuild",
				data = new
				{
					id = discordid
				}
			};
			var response = await UploadString(_discordUrl, JsonConvert.SerializeObject(requestBody));
			Debug.WriteLine("GetIsPlayerInGuild response: " + response);
			var output = JsonConvert.DeserializeObject<bool>(response);
			Debug.WriteLine(output.ToString());
			return output;
		}
		/// <summary>
		/// returns true if player was already in the target voice channel.
		/// </summary>
		public async Task<bool> MovePlayerToVc(string discordid, string channelName)
		{
			var requestBody = new
			{
				name = "MovePlayerToVc",
				data = new
				{
					id = discordid,
					channel = channelName
				}
			};
			var response = await UploadString(_discordUrl, JsonConvert.SerializeObject(requestBody));
			Debug.WriteLine("MovePlayerToVc response: " + response);
			var output = JsonConvert.DeserializeObject<bool>(response);
			Debug.WriteLine(output.ToString());
			return output;
		}
		private async Task<string> DownloadString(string url)
		{
			var args = new Dictionary<string, object> {
				{"url", url}
			};
			var argsJson = JsonConvert.SerializeObject(args);
			var id = API.PerformHttpRequestInternal(argsJson, argsJson.Length);
			var req = _pendingRequests[id] = new PendingRequest(id);
			return await req.Task;
		}

		private async Task<string> UploadString(string url, string body)
		{
			var args = new Dictionary<string, object> {
				{"url", url},
				{"method", "POST"},
				{"data", body},
				{"headers", new Dictionary<string, string> {{"Content-Type", "application/json"}}}
			};
			var argsJson = JsonConvert.SerializeObject(args);
			var id = API.PerformHttpRequestInternal(argsJson, argsJson.Length);
			var req = _pendingRequests[id] = new PendingRequest(id);
			return await req.Task;
		}

	}
	class PendingRequest : TaskCompletionSource<string>
	{
		public int Token;
		public PendingRequest( int token ) {
			Token = token;
		}
	}
}