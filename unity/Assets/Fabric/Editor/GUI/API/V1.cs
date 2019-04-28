namespace Fabric.Internal.Editor.API
{
	using UnityEditor;
	using UnityEngine;
	using System.Collections.Generic;
	using System.Collections;
	using System.Net;
	using System.IO;
	using System;
	using Fabric.Internal.Editor.Model;

	public class V1
	{
		private const int BufferSize = 8192;

		private static readonly byte[] EmptyBytes = new byte[] { };
		private static readonly string OrganizationsEndpoint = "/api/v2/organizations";
		private static readonly string AppsEndpointAndroid = "/platforms/android/apps?include=map_of_available_products";
		private static readonly string AppsEndpointIOS = "/platforms/ios/apps?include=map_of_available_products";

		private Net.OAuth.Client client;
		private Net.OAuth.Client.Token token;
		private string URI;

		public class UnauthorizedException : System.Exception
		{
			public UnauthorizedException(string message) : base (message) {}
		}

		public class ApiException : System.Exception
		{
			public ApiException(string message) : base (message) {}
		}

		public V1(string URI, Net.OAuth.Client client, Net.OAuth.Client.Token token)
		{
			this.URI = URI;
			this.client = client;
			this.token = token;
		}

		public List<Organization> Organizations()
		{
			HttpWebResponse response = HttpGet (
				URI + OrganizationsEndpoint, client, token, 2u
			);

			using (Stream stream = response.GetResponseStream ()) {
				return Parser.ParseOrganizations (stream);
			}
		}

		public List<App> ApplicationsFor(string organizationName, List<Organization> organizations)
		{
			if (organizations == null) {
				Utils.Error ("No organizations exist!");
				return null;
			}

			Organization matching = organizations.Find (l => l.Name.Equals (organizationName));

			if (matching == null) {
				Utils.Error ("Nothing matches {0}", organizationName);
				return null;
			}

			HttpWebResponse response = HttpGet (
				URI + OrganizationsEndpoint + "/" + matching.Id + AppsEndpointAndroid, client, token, 2u
			);

			List<App> apps = new List<App> ();

			using (Stream stream = response.GetResponseStream ()) {
				apps.AddRange (Parser.ParseApps (stream, BuildTarget.Android));
			}

			response = HttpGet (
				URI + OrganizationsEndpoint + "/" + matching.Id + AppsEndpointIOS, client, token, 2u
			);

			using (Stream stream = response.GetResponseStream ()) {
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
				apps.AddRange (Parser.ParseApps (stream, BuildTarget.iPhone));
#else
				apps.AddRange (Parser.ParseApps (stream, BuildTarget.iOS));
#endif
			}

			return apps;
		}

		/**
		 * This function is obsolete. Prefer use of DownloadFile, which does not use WWW internally.
		 */
		public static IEnumerator DownloadTexture(string URI, Action<Texture2D> onComplete)
		{
			WWW www = new WWW (URI);

			while (!www.isDone) {
				yield return null;
			}

			onComplete (String.IsNullOrEmpty (www.error) ? www.texture : null);
		}

		public static byte[] DownloadFile(string URI)
		{
			return DownloadFile (URI, (progress) => {
			}, () => {
				return false;
			});
		}

		public static byte[] DownloadFile(string URI, Action<float> reportProgress, Func<bool> isCancelled)
		{
			WebRequest request = WebRequest.Create (URI);

			request.Method = "GET";
			request.ContentType = "application/x-www-form-urlencoded";

			WebResponse response = request.GetResponse ();

			long contentLength = response.ContentLength;

			int size = -1;
			try {
				size = Convert.ToInt32 (contentLength);
			} catch (OverflowException) {
				throw new ApiException ("File content too large");
			}

			using (Stream stream = response.GetResponseStream ()) {
				return ReadStream (stream, size, reportProgress, isCancelled);
			}
		}

		private static byte[] ReadStream(Stream stream, int contentLength, Action<float> reportProgress, Func<bool> isCancelled)
		{
			if (contentLength < 0) {
				return EmptyBytes;
			}

			byte[] content = new byte[contentLength];

			int bytesRead = 0;
			int bytesReceived = 0;

			while (!isCancelled () && (bytesRead = stream.Read (content, bytesReceived, Math.Min (BufferSize, contentLength - bytesReceived))) > 0) {
				bytesReceived += bytesRead;
				float progress = ((float) bytesReceived / contentLength);
				reportProgress (progress);
			}
			return bytesReceived == contentLength ? content : EmptyBytes;
		}

		public string HttpPost(string endpoint)
		{
			HttpWebResponse response = HttpPost (URI + endpoint, client, token);

			using (Stream stream = response.GetResponseStream ()) {
				using (StreamReader reader = new StreamReader (stream)) {
					return reader.ReadToEnd ();
				}
			}
		}

		private static HttpWebResponse HttpPost(string URI, Net.OAuth.Client client, Net.OAuth.Client.Token token)
		{
			HttpWebRequest request = null;

			try {
				request = WebRequest.Create (URI) as HttpWebRequest;

				request.Method = "POST";
				request.ContentType = "text/plain";
				request.Accept = "application/json";

				Net.OAuth.Client.Sign (request, token);

				return (HttpWebResponse)request.GetResponse ();
			} catch (WebException e) {
				if (e.Response == null) {
					throw;
				}

				HttpStatusCode code = (e.Response as HttpWebResponse).StatusCode;

				if (code == HttpStatusCode.Unauthorized) {
					throw new UnauthorizedException("Response is 401, Unauthorized.");
				}

				throw new ApiException ("Response is unexpected, " + code.ToString ());
			}
		}

		private static HttpWebResponse HttpGet(string URI, Net.OAuth.Client client, Net.OAuth.Client.Token token, uint tries)
		{
			HttpWebRequest request = null;

			try {
				request = (HttpWebRequest)WebRequest.Create (URI);
				
				request.Method = "GET";
				request.ContentType = "application/x-www-form-urlencoded";
				request.Accept = "application/json";

				Net.OAuth.Client.Sign (request, token);

				return (HttpWebResponse)request.GetResponse ();
			} catch (WebException e) {
				if (e.Response == null) {
					throw;
				}

				HttpStatusCode code = (e.Response as HttpWebResponse).StatusCode;

				if (code == HttpStatusCode.Unauthorized) {
					throw new UnauthorizedException ("Response is 401, Unauthorized.");
				}

				throw new ApiException ("Response is unexpected, " + code.ToString ());
			}
		}
	}
}
