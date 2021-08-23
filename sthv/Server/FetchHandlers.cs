using System;
using System.Collections.Generic;
using System.Text;
using CitizenFX.Core;
using sthvServer;
using Newtonsoft.Json;

namespace sthvServer
{
	class FetchHandler : BaseScript
	{
		/// <summary>
		/// tuple consists of (isSuccessful, returnObject)
		/// </summary>
		static private Dictionary<string, Func<Player, Shared.BaseFetchClass>> FetchRequestHandlers = new Dictionary<string, Func<Player, Shared.BaseFetchClass>>();

		

		[EventHandler("__sthv__internal:fetchRequest")]
		private void OnFetchRequest([FromSource] Player source, int token, string requestUrl)
		{
			Debug.WriteLine($"Fetch request received. Token: {token}. URL: {requestUrl}. Player: {source.Name}");

			if (!FetchRequestHandlers.TryGetValue(requestUrl, out Func<Player, Shared.BaseFetchClass> func))
			{
				Utilities.logError($"Fetch requestUrl {requestUrl} from {source.Name} (id:{source.Handle}) did not have an associated FetchRequestHandler.");
			}
			else
			{
				var returnObject = func.Invoke(source);
				response(source, token, returnObject);
			}
		}

		/// <summary>
		/// reponse to fetch request, handled in client/fetch/sthvFetch.cs
		/// </summary>
		/// <param name="source">source player</param>
		/// <param name="token">token recieved in request</param>
		/// <param name="isSuccessful">if request was successful</param>
		/// <param name="body">response body</param>
		private void response(Player source, int token, Shared.BaseFetchClass body)
		{
			var jsonBody = JsonConvert.SerializeObject(body);
			source.TriggerEvent("__sthv__internal:fetchResponse", token, jsonBody);
		}

		public void addHandler<T>(Func<Player, Shared.BaseFetchClass> func) where T : Shared.BaseFetchClass
		{
			if (FetchRequestHandlers.ContainsKey(typeof(T).Name))
			{
				throw new Exception("Tried adding duplicate FetchHandlers for type: " + typeof(T).Name);
			}
			FetchRequestHandlers.Add(typeof(T).Name, func);
			Debug.WriteLine("^2Adding fetch handler: " + typeof(T).Name + "^7");
		}
	}
}

