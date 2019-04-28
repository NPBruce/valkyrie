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

	internal class AnswersStubImplementation : IAnswers
	{
		/// <summary>
		/// When not using Android or Apple platforms, Unity needs a default behavior for Answers
		/// API Calls. This is a default no-oping behavior.
		/// </summary>
		public AnswersStubImplementation ()
		{
			UnityEngine.Debug.Log ("Answers will no-op because it was initialized for a non-Android, non-Apple platform.");
		}

		public void LogSignUp (string method, bool? success, Dictionary<string, object> customAttributes)
		{
		}

		public void LogLogin (string method, bool? success, Dictionary<string, object> customAttributes)
		{
		}

		public void LogShare (string method, string contentName, string contentType, string contentId, Dictionary<string, object> customAttributes)
		{
		}

		public void LogInvite (string method, Dictionary<string, object> customAttributes)
		{
		}

		public void LogLevelStart (string level, Dictionary<string, object> customAttributes)
		{
		}

		public void LogLevelEnd (string level, double? score, bool? success, Dictionary<string, object> customAttributes)
		{
		}

		public void LogAddToCart (decimal? itemPrice, string currency, string itemName, string itemType, string itemId, Dictionary<string, object> customAttributes)
		{
		}

		public void LogPurchase (decimal? price, string currency, bool? success, string itemName, string itemType, string itemId, Dictionary<string, object> customAttributes)
		{
		}

		public void LogStartCheckout (decimal? totalPrice, string currency, int? itemCount, Dictionary<string, object> customAttributes)
		{
		}

		public void LogRating (int? rating, string contentName, string contentType, string contentId, Dictionary<string, object> customAttributes)
		{
		}

		public void LogContentView (string contentName, string contentType, string contentId, Dictionary<string, object> customAttributes)
		{
		}

		public void LogSearch (string query, Dictionary<string, object> customAttributes)
		{	
		}

		public void LogCustom (string eventName, Dictionary<string, object> customAttributes)
		{
		}
	}
}
