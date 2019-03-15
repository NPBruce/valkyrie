namespace Fabric.Internal.Editor.Net.OAuth
{
	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;
	using System.Net;
	using System.Web;
	using System.IO;
	using System.Text;
	using Fabric.Internal.Editor.Detail;
	
	public sealed class Client
	{
		#region Constants
		private static readonly string TokenEndpoint = "/oauth/token?grant_type=password";
		private static readonly string RefreshTokenEndpoint = "/oauth/token?grant_type=refresh_token";
		private static readonly string RefreshTokenQuery = "&refresh_token=";

		private static readonly string[] DefaultScopes = {
			"organizations",
			"apps"
		};

		private static readonly string DefaultScopeQuery = "&scope=" + string.Join ("%20", DefaultScopes);
		private static readonly string AppId = "c8bbeb4e54bada20aecf3f89edfb74a04588fe86c5368a76388f67e70d945494";
		private static readonly string Secret = "2c7d3d05e8934b182995a30a9aaae369025cca0c139616ce117234050117aa4f";

		private static readonly string AccessTokenKey = "access_token";
		private static readonly string RefreshTokenKey = "refresh_token";
		private static readonly string TokenTypeKey = "token_type";
		private static readonly string ExpiresInKey = "expires_in";
		#endregion

		#region Token
		public sealed class Token
		{
			public readonly string AccessToken;
			public readonly string RefreshToken;
			public readonly string TokenType;
			public readonly long ExpiersIn;

			public Token(string accessToken, string refreshToken, string tokenType, long expiresIn) {
				AccessToken = accessToken;
				RefreshToken = refreshToken;
				TokenType = tokenType;
				ExpiersIn = expiresIn;
			}

			public override string ToString ()
			{
				string s = string.Format (
					"{{\"{0}\":\"{1}\",\"{2}\":\"{3}\",\"{4}\":\"{5}\",\"{6}\":{7}}}",
				    AccessTokenKey, AccessToken,
					RefreshTokenKey, RefreshToken,
					TokenTypeKey, TokenType,
					ExpiresInKey, ExpiersIn
				);

				System.Console.WriteLine ("\ts = " + s);
				return s;
			}
		}
		#endregion

		public readonly string URI;

		public Client(string URI)
		{
			this.URI = URI;
		}

		#region Get
		public Token Get(string username, string password)
		{
			HttpWebRequest request = PrepareRequest (
				URI + TokenEndpoint + DefaultScopeQuery
			);

			using (Stream stream = request.GetRequestStream ()) {
				string entity = ComposeEntity(username, password, AppId, Secret);
				byte[] bytes = Encoding.UTF8.GetBytes (entity.ToString ());
				stream.Write (bytes, 0, bytes.Length);
			}

			using (Stream stream = request.GetResponse ().GetResponseStream ()) {
				return fromStream (stream);
			}
		}
		#endregion

		#region Refresh
		public Token Refresh(Token expired)
		{
			HttpWebRequest request = PrepareRequest (
				URI + RefreshTokenEndpoint + RefreshTokenQuery + expired.RefreshToken
			);

			Fabric.Internal.Editor.Utils.Log ("Refreshing token via " + request.RequestUri.AbsoluteUri.ToString ());
			
			using (Stream stream = request.GetResponse ().GetResponseStream ()) {
				return fromStream (stream);
			}
		}
		#endregion

		private static Token fromStream(Stream stream)
		{
			using (StreamReader reader = new StreamReader (stream)) {
				return parse (reader.ReadToEnd ());
			}
		}

		public static Token parse(string json)
		{
			if (string.IsNullOrEmpty(json))
				return null;

			try {
				Dictionary<string, string> pairs = new Dictionary<string, string>();
				string unwrapped = Strings.Unwrap(json, '{', '}');
				
				foreach (string kvLine in unwrapped.Split(',')) {
					string[] kv = kvLine.Split(':');
					pairs.Add (Strings.Unwrap (kv[0], '"', '"'), Strings.Unwrap (kv[1], '"', '"'));
				}

				return new Token (
					pairs[AccessTokenKey],
					pairs[RefreshTokenKey],
					pairs[TokenTypeKey],
					long.Parse (pairs[ExpiresInKey])
				);
			} catch (System.Exception) {
				return null;
			}
		}

		private static HttpWebRequest PrepareRequest(string URI)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create (URI);
			
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";
			request.Accept = "application/json";

			return request;
		}

		private static string ComposeEntity(string username, string password, string id, string secret)
		{
			return "username=" + System.Uri.EscapeDataString (username) +
				"&password=" + System.Uri.EscapeDataString (password) +
				"&client_id=" + System.Uri.EscapeDataString (id) +
				"&client_secret=" + System.Uri.EscapeDataString (secret);
		}

		#region Sign
		public static HttpWebRequest Sign(HttpWebRequest request, Token token)
		{
			request.Headers.Set ("Authorization", "Bearer " + token.AccessToken);
			return request;
		}
		#endregion
	}
}
