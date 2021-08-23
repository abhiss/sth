using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using Newtonsoft.Json;

namespace sthv
{
	class sthvFetch : BaseScript
	{
		private static int _tokenCounter;
		public static int NewToken
		{
			get
			{
				return (++_tokenCounter);
			}
		}
		private static readonly Dictionary<int, PendingRequest> _pendingRequests = new Dictionary<int, PendingRequest>();

		[EventHandler("__sthv__internal:fetchResponse")]
		private static void onFetchResponse(int token, string jsonBody)
		//Shared.BaseSharedClass body
		{
			Debug.WriteLine("Got fetch response: " + jsonBody);

			if (!_pendingRequests.TryGetValue(token, out var req)) return;
			
			req.SetResult(jsonBody);

			_pendingRequests.Remove(token);
		}
		[Command("get_unfulfilled_fetch_requests")]
		private void command_get_unfulfilled_fetch_requests()
		{
			Debug.WriteLine("^2== There are " + _pendingRequests.Count + " pending requests ==^7");
			foreach (var req in _pendingRequests)
			{
				Debug.WriteLine($"Request Url: {req.Value.Url}, Token: {req.Value.Token}");
			}
			Debug.WriteLine("\n");
		}
		private static async Task<string> MakeFetchRequest(string url)
		{
			Debug.WriteLine("making fetch request, url: [" + url + "]");
			var token = NewToken;
			var req = _pendingRequests[token] = new PendingRequest(token, url);
			TriggerServerEvent("__sthv__internal:fetchRequest", token, url);
			return await req.Task;
		}

		private class PendingRequest : TaskCompletionSource<string>
		{
			public int Token;
			public string Url;

			public PendingRequest(int token, string url)
			{
				Token = token;
				Url = url;
			}
		}

		public static async Task<T> Get<T>(string _ ) {
			string url = typeof(T).Name;
			string jsonResult = await MakeFetchRequest(url);
			T res = JsonConvert.DeserializeObject<T>(jsonResult);

			return res;
		}

	}
}
