using System;
using System.Collections.Generic;
using System.Text;
using CitizenFX.Core;


namespace sthvServer
{
	class sthvFetchHandler : BaseScript
	{
		[EventHandler("__sthv__internal:fetchRequest")]
		private void OnFetchRequest([FromSource] Player source, int token, string requestUrl)
		{
			Debug.WriteLine($"Fetch request received. Token: {token}. URL: {requestUrl}. Player: {source.Name}");
			switch (requestUrl)
			{
				case "RequestSpawn":
					
					break;
				case "ping":
					response(source, token, true, "pong");

					break;
				default:
					throw new Exception("sthvFetchHandlers requestUrl found no handler.");
			}
		}

		/// <summary>
		/// reponse to fetch request, handled in client/fetch/sthvFetch.cs
		/// </summary>
		/// <param name="source">source player</param>
		/// <param name="token">token recieved in request</param>
		/// <param name="isSuccessful">if request was successful</param>
		/// <param name="body">possibly jsoned response body</param>
		private void response(Player source, int token, bool isSuccessful, string body)
		{
			source.TriggerLatentEvent("__sthv__internal:fetchResponse", 1000, token, isSuccessful, body);
		}
	}
}
