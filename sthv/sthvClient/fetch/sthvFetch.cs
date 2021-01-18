using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;


namespace sthv
{
	class sthvFetch : BaseScript
	{
		private static int _tokenCounter;
		public static int NewToken { get
			{
				return (++_tokenCounter);
			}
		}
		private static readonly Dictionary<int, PendingRequest> _pendingRequests = new Dictionary<int, PendingRequest>();

		[EventHandler("__sthv__internal:fetchResponse")]
		private static void onFetchResponse(int token, bool isSuccesss, string body)
		{
			if (!_pendingRequests.TryGetValue(token, out var req)) return;

			req.SetResult(body);

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
		public static async Task<string> DownloadString(string url)
		{
			Debug.WriteLine("making fetch request (DownloadString()), url: [" + url + "]");
			var token = NewToken;
			var req = _pendingRequests[token] = new PendingRequest(token, url);
			TriggerServerEvent("__sthv__internal:fetchRequest", token, url);
			return await req.Task;
		}

		private  class PendingRequest : TaskCompletionSource<string>
		{
			public int Token;
			public string Url;

			public PendingRequest(int token, string url)
			{
				Token = token;
				Url = url;
			}
		}
	}
}
