#if UNITY_IOS && !UNITY_EDITOR
namespace Fabric.Answers.Internal
{
	using UnityEngine;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;
	using System.IO;
	using System.Text.RegularExpressions;
	using Fabric.Internal.ThirdParty.MiniJSON;
	
	internal class AnswersAppleImplementation : IAnswers
	{
		[DllImport("__Internal")]
		private static extern void ANSLogSignUp (string signupMethod, string success, string customAttributes);
		
		[DllImport("__Internal")]
		private static extern void ANSLogLogin (string loginMethod, string success, string customAttributes);
		
		[DllImport("__Internal")]
		private static extern void ANSLogShare (string shareMethod, string contentName, string contentType, string contentId, string customAttributes);
		
		[DllImport("__Internal")]
		private static extern void ANSLogInvite (string inviteMethod, string customAttribute);
		
		[DllImport("__Internal")]
		private static extern void ANSLogPurchase (string purchasePrice, string currency, string success, string itemName, string itemType, string itemId, string customAttributes);
		
		[DllImport("__Internal")]
		private static extern void ANSLogLevelStart (string levelName, string customAttribute);
		
		[DllImport("__Internal")]
		private static extern void ANSLogLevelEnd (string levelName, string levelScore, string success, string customAttribute);
		
		[DllImport("__Internal")]
		private static extern void ANSLogAddToCart (string itemPrice, string currency, string itemName, string itemType, string itemId, string customAttributes);
		
		[DllImport("__Internal")]
		private static extern void ANSLogStartCheckout (string itemPrice, string currency, string itemCount, string customAttributes);
		
		[DllImport("__Internal")]
		private static extern void ANSLogRating (string rating, string contentName, string contentType, string contentId, string customAttributes);
		
		[DllImport("__Internal")]
		private static extern void ANSLogContentView (string contentName, string contentType, string contentId, string customAttributes);
		
		[DllImport("__Internal")]
		private static extern void ANSLogSearch (string query, string customAttributes);
		
		[DllImport("__Internal")]
		private static extern void ANSLogCustom (string customEventName, string customAttributes);

		public AnswersAppleImplementation ()
		{
			
		}
		
		public void LogSignUp (string method, bool? success, Dictionary<string, object> customAttributes)
		{
			ANSLogSignUp (method, boolToString (success), dictionaryToString (customAttributes));
		}
		
		public void LogLogin (string method, bool? success, Dictionary<string, object> customAttributes)
		{
			ANSLogLogin (method, boolToString (success), dictionaryToString (customAttributes));	
		}
		
		public void LogShare (string method, string contentName, string contentType, string contentId, Dictionary<string, object> customAttributes)
		{
			ANSLogShare (method, contentName, contentType, contentId, dictionaryToString (customAttributes));
		}
		
		public void LogInvite (string method, Dictionary<string, object> customAttributes)
		{
			ANSLogInvite (method, dictionaryToString (customAttributes));
		}
		
		public void LogLevelStart (string level, Dictionary<string, object> customAttributes)
		{
			ANSLogLevelStart (level, dictionaryToString (customAttributes));
		}

		public void LogLevelEnd (string level, double? score, bool? success, Dictionary<string, object> customAttributes)
		{
			ANSLogLevelEnd (level, AsStringOrNull (score), boolToString (success), dictionaryToString (customAttributes));
		}

		public void LogAddToCart (decimal? itemPrice, string currency, string itemName, string itemType, string itemId, Dictionary<string, object> customAttributes)
		{
			ANSLogAddToCart (AsStringOrNull (itemPrice), currency, itemName, itemType, itemId, dictionaryToString (customAttributes));
		}

		public void LogPurchase (decimal? price, string currency, bool? success, string itemName, string itemType, string itemId, Dictionary<string, object> customAttributes)
		{
			ANSLogPurchase (AsStringOrNull (price), currency, boolToString (success), itemName, itemType, itemId, dictionaryToString (customAttributes));			
		}
		
		public void LogStartCheckout (decimal? totalPrice, string currency, int? itemCount, Dictionary<string, object> customAttributes)
		{
			ANSLogStartCheckout (AsStringOrNull (totalPrice), currency, AsStringOrNull (itemCount), dictionaryToString (customAttributes));
		}
		
		public void LogRating (int? rating, string contentName, string contentType, string contentId, Dictionary<string, object> customAttributes)
		{
			ANSLogRating (AsStringOrNull (rating), contentName, contentType, contentId, dictionaryToString (customAttributes));
		}
		
		public void LogContentView (string contentName, string contentType, string contentId, Dictionary<string, object> customAttributes)
		{
			ANSLogContentView (contentName, contentType, contentId, dictionaryToString (customAttributes));
		}
		
		public void LogSearch (string query, Dictionary<string, object> customAttributes)
		{	
			ANSLogSearch (query, dictionaryToString (customAttributes));
		}
		
		public void LogCustom (string eventName, Dictionary<string, object> customAttributes)
		{
			ANSLogCustom (eventName, dictionaryToString (customAttributes));			
		}

		private static string AsStringOrNull (object obj)
		{
			if (obj == null) {
				return null;
			}

			return obj.ToString ();
		}

		private static string boolToString (bool? value)
		{
			if (value == null) {
				return null;
			}
			return value.Value ? "true" : "false";
		}

		private static string dictionaryToString (Dictionary<string, object> dictionary)
		{
			if (dictionary == null) {
				return null;
			}
			return Json.Serialize (dictionary);
		}
	}
}
#endif
