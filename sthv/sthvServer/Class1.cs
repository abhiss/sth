using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CitizenFX.Core;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Owin;
using static CitizenFX.Core.Native.API;

namespace sthvServer
{
	class Class1
	{
		public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
		{
			SetHttpHandler(new Action<dynamic, dynamic>(async (req, res) =>
			{
				var resourceName = GetCurrentResourceName();

				var bodyStream = (req.method != "GET" && req.method != "HEAD")
						? await GetBodyStream(req)
							: Stream.Null;

				var oldSc = SynchronizationContext.Current;
				SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

				var cts = new CancellationTokenSource();
				req.setCancelHandler(new Action(() =>
				{
					cts.Cancel();
				}));

				await Task.Factory.StartNew(async () =>
				{
					var owinEnvironment = new Dictionary<string, object>();
					owinEnvironment["owin.RequestBody"] = bodyStream;

					var headers = new HeaderDictionary();

					foreach (var headerPair in req.headers)
					{
						headers.Add(headerPair.Key, new string[] { headerPair.Value.ToString() });
					}

					owinEnvironment["owin.RequestHeaders"] = headers;

					owinEnvironment["owin.RequestMethod"] = req.method;
					owinEnvironment["owin.RequestPath"] = req.path.Split('?')[0];
					owinEnvironment["owin.RequestPathBase"] = "/" + resourceName;
					owinEnvironment["owin.RequestProtocol"] = "HTTP/1.0";
					owinEnvironment["owin.RequestQueryString"] = (req.path.Contains('?')) ? req.path.Split('?', 2)[1] : "";
					owinEnvironment["owin.RequestScheme"] = "http";

					var outStream = new HttpOutStream(owinEnvironment, res);
					owinEnvironment["owin.ResponseBody"] = outStream;

					var outHeaders = new Dictionary<string, string[]>();
					owinEnvironment["owin.ResponseHeaders"] = outHeaders;

					owinEnvironment["owin.CallCancelled"] = cts.Token;
					owinEnvironment["owin.Version"] = "1.0";

					var ofc = new FxOwinFeatureCollection(owinEnvironment);
					var context = application.CreateContext(new FeatureCollection(ofc));

					try
					{
						await application.ProcessRequestAsync(context);
						await ofc.InvokeOnStarting();
					}
					catch (Exception ex)
					{
						Debug.WriteLine($"Exception while handling request. {ex}");

						await ofc.InvokeOnCompleted();

						application.DisposeContext(context, ex);

						var errorText = Encoding.UTF8.GetBytes("Error.");

						owinEnvironment["owin.ResponseStatusCode"] = 500;
						await outStream.WriteAsync(errorText, 0, errorText.Length);
						await outStream.EndStream();

						return;
					}

					application.DisposeContext(context, null);

					await outStream.EndStream();

					await ofc.InvokeOnCompleted();
				}, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());

				SynchronizationContext.SetSynchronizationContext(oldSc);
			}));

			return Task.CompletedTask;
		}
	}
}
