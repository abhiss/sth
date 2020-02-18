using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace sthvServer
{
	class sthvDiscordController
	{

		static readonly HttpClient httpClient = new HttpClient();
		public static async void VerifyDiscord(string discordId)
		{
			var values = new Dictionary<string, string>
			{
				{"discordId", discordId}
			};
			var content = new FormUrlEncodedContent(values);
			var repsonse = await httpClient.PostAsync("http://localhost:3000", content);

		}
	}
}
