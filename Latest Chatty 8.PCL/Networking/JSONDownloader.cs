﻿using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Latest_Chatty_8.Shared.Networking
{
	/// <summary>
	/// Helper to download JSON objects
	/// </summary>
	public static class JSONDownloader
	{
		#region Public Methods
		/// <summary>
		/// Performs a GET on the URI and parses into a JObject.
		/// </summary>
		/// <param name="uri">The URI.</param>
		/// <returns></returns>
		public static async Task<JObject> DownloadObject(string uri)
		{
			try
			{
				var data = await JSONDownloader.DownloadJSON(uri);
				var payload = JObject.Parse(data);
				return payload;
			}
			catch { System.Diagnostics.Debug.Assert(false); return null; }
		}

		/// <summary>
		/// Performs a GET on the URI and parses into a JArray.
		/// </summary>
		/// <param name="uri">The URI.</param>
		/// <returns></returns>
		public static async Task<JArray> DownloadArray(string uri)
		{
			try
			{
				var data = await JSONDownloader.DownloadJSON(uri);
				var payload = JArray.Parse(data);
				return payload;
			}
			catch { System.Diagnostics.Debug.Assert(false); return null; }
		}

		/// <summary>
		/// Performs a GET on the URI and parses into a JToken
		/// </summary>
		/// <param name="uri">The URI.</param>
		/// <returns></returns>
		public static async Task<JToken> Download(string uri)
		{
			try
			{
				var data = await JSONDownloader.DownloadJSON(uri);
				var payload = JToken.Parse(data);
				return payload;
			}
			catch { System.Diagnostics.Debug.Assert(false); return null; }
		}
		#endregion

		#region Private Methods
		private static async Task<string> DownloadJSON(string uri)
		{
			try
			{
				var handler = new HttpClientHandler();
				if (handler.SupportsAutomaticDecompression)
				{
					handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
					System.Diagnostics.Debug.WriteLine("Starting download with compression for uri {0} ", uri);
				}
				else
				{
					System.Diagnostics.Debug.WriteLine("Starting download for uri {0}", uri);
				}
				//:TODO: Re-enable this??
				//handler.Credentials = CoreServices.Instance.Credentials; //Not sure if this is actually required or not.

				using (var request = new HttpClient(handler, true))
				{
					var response = await request.GetAsync(uri);
					System.Diagnostics.Debug.WriteLine("Got response from uri {0}", uri);
					return await response.Content.ReadAsStringAsync();
				}
			}
			catch (Exception)
			{
				System.Diagnostics.Debug.WriteLine(string.Format("Error getting JSON data for URL {0}", uri));
				return null;
			}
		}
		#endregion
	}
}
