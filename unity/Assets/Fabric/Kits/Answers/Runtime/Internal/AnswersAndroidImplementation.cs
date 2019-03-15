#if UNITY_ANDROID && !UNITY_EDITOR
namespace Fabric.Answers.Internal
{
	using UnityEngine;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;
	using System.IO;
	using System.Text.RegularExpressions;

	internal class AnswersAndroidImplementation : IAnswers
	{
		private AnswersSharedInstanceJavaObject answersSharedInstance;

		public AnswersAndroidImplementation ()
		{
			answersSharedInstance = new AnswersSharedInstanceJavaObject ();
		}

		public void LogSignUp (string method, bool? success, Dictionary<string, object> customAttributes)
		{
			var eventInstance = new AnswersEventInstanceJavaObject ("SignUpEvent", customAttributes);
			eventInstance.PutMethod (method);
			eventInstance.PutSuccess (success);
			answersSharedInstance.Log ("logSignUp", eventInstance);
		}

		public void LogLogin (string method, bool? success, Dictionary<string, object> customAttributes)
		{
			var eventInstance = new AnswersEventInstanceJavaObject ("LoginEvent", customAttributes);
			eventInstance.PutMethod (method);
			eventInstance.PutSuccess (success);
			answersSharedInstance.Log ("logLogin", eventInstance);
		}

		public void LogShare (string method, string contentName, string contentType, string contentId, Dictionary<string, object> customAttributes)
		{
			var eventInstance = new AnswersEventInstanceJavaObject ("ShareEvent", customAttributes);
			
			eventInstance.PutMethod (method);
			eventInstance.PutContentName (contentName);
			eventInstance.PutContentType (contentType);
			eventInstance.PutContentId (contentId);
			
			answersSharedInstance.Log ("logShare", eventInstance);
		}

		public void LogInvite (string method, Dictionary<string, object> customAttributes)
		{
			var eventInstance = new AnswersEventInstanceJavaObject ("InviteEvent", customAttributes);
			eventInstance.PutMethod (method);
			answersSharedInstance.Log ("logInvite", eventInstance);
		}

		public void LogLevelStart (string level, Dictionary<string, object> customAttributes)
		{
			var eventInstance = new AnswersEventInstanceJavaObject ("LevelStartEvent", customAttributes);
			eventInstance.InvokeSafelyAsString ("putLevelName", level);
			answersSharedInstance.Log ("logLevelStart", eventInstance);
		}

		public void LogLevelEnd (string level, double? score, bool? success, Dictionary<string, object> customAttributes)
		{
			var eventInstance = new AnswersEventInstanceJavaObject ("LevelEndEvent", customAttributes);			
			eventInstance.InvokeSafelyAsString ("putLevelName", level);
			eventInstance.InvokeSafelyAsDouble ("putScore", score);
			eventInstance.PutSuccess (success);
			answersSharedInstance.Log ("logLevelEnd", eventInstance);
		}

		public void LogAddToCart (decimal? itemPrice, string currency, string itemName, string itemType, string itemId, Dictionary<string, object> customAttributes)
		{
			var eventInstance = new AnswersEventInstanceJavaObject ("AddToCartEvent", customAttributes);			
			eventInstance.InvokeSafelyAsDecimal ("putItemPrice", itemPrice);
			eventInstance.PutCurrency (currency);
			eventInstance.InvokeSafelyAsString ("putItemName", itemName);
			eventInstance.InvokeSafelyAsString ("putItemId", itemId);
			eventInstance.InvokeSafelyAsString ("putItemType", itemType);
			answersSharedInstance.Log ("logAddToCart", eventInstance);
		}

		public void LogPurchase (decimal? price, string currency, bool? success, string itemName, string itemType, string itemId, Dictionary<string, object> customAttributes)
		{
			var eventInstance = new AnswersEventInstanceJavaObject ("PurchaseEvent", customAttributes);
			
			eventInstance.InvokeSafelyAsDecimal ("putItemPrice", price);
			eventInstance.PutCurrency (currency);
			eventInstance.PutSuccess (success);
			eventInstance.InvokeSafelyAsString ("putItemName", itemName);
			eventInstance.InvokeSafelyAsString ("putItemId", itemId);
			eventInstance.InvokeSafelyAsString ("putItemType", itemType);
			answersSharedInstance.Log ("logPurchase", eventInstance);
		}

		public void LogStartCheckout (decimal? totalPrice, string currency, int? itemCount, Dictionary<string, object> customAttributes)
		{
			var eventInstance = new AnswersEventInstanceJavaObject ("StartCheckoutEvent", customAttributes);			
			eventInstance.InvokeSafelyAsDecimal ("putTotalPrice", totalPrice);
			eventInstance.PutCurrency (currency);
			eventInstance.InvokeSafelyAsInt ("putItemCount", itemCount);
			answersSharedInstance.Log ("logStartCheckout", eventInstance);
		}

		public void LogRating (int? rating, string contentName, string contentType, string contentId, Dictionary<string, object> customAttributes)
		{
			var eventInstance = new AnswersEventInstanceJavaObject ("RatingEvent", customAttributes);			
			
			eventInstance.InvokeSafelyAsInt ("putRating", rating);
			eventInstance.PutContentName (contentName);
			eventInstance.PutContentType (contentType);
			eventInstance.PutContentId (contentId);
			answersSharedInstance.Log ("logRating", eventInstance);
		}

		public void LogContentView (string contentName, string contentType, string contentId, Dictionary<string, object> customAttributes)
		{
			var eventInstance = new AnswersEventInstanceJavaObject ("ContentViewEvent", customAttributes);			
			eventInstance.PutContentName (contentName);
			eventInstance.PutContentType (contentType);
			eventInstance.PutContentId (contentId);
			answersSharedInstance.Log ("logContentView", eventInstance);
		}

		public void LogSearch (string query, Dictionary<string, object> customAttributes)
		{
			var eventInstance = new AnswersEventInstanceJavaObject ("SearchEvent", customAttributes);			
			eventInstance.InvokeSafelyAsString ("putQuery", query);
			answersSharedInstance.Log ("logSearch", eventInstance);
		}

		public void LogCustom (string eventName, Dictionary<string, object> customAttributes)
		{
			var eventInstance = new AnswersEventInstanceJavaObject ("CustomEvent", customAttributes, eventName);		
			answersSharedInstance.Log ("logCustom", eventInstance);
		}
	}

	// A C# representation of an Java AnswersEvent to reduce boilerplate calls
	// across the JNI. Manages serialization and invocation of a number of shared
	// AnswersEvents methods.
	class AnswersEventInstanceJavaObject
	{
		public AndroidJavaObject javaObject;

		public AnswersEventInstanceJavaObject (string eventType, Dictionary<string, object> customAttributes, params string[] args)
		{
			javaObject = new AndroidJavaObject (String.Format ("com.crashlytics.android.answers.{0}", eventType), args);
			
			foreach (var item in customAttributes) {
				var key = item.Key;
				var value = item.Value;

				if (value == null) {
					Debug.Log (String.Format ("[Answers] Expected custom attribute value to be non-null. Received: {0}", value));
				} else if (IsNumericType (value)) {
					javaObject.Call<AndroidJavaObject> ("putCustomAttribute", key, AsDouble (value));
				} else if (value is String) {
					javaObject.Call<AndroidJavaObject> ("putCustomAttribute", key, value);
				} else {
					Debug.Log (String.Format ("[Answers] Expected custom attribute value to be a string or numeric. Received: {0}", value));
				}
			}
		}

		public void PutMethod (string method)
		{
			InvokeSafelyAsString ("putMethod", method);
		}

		public void PutSuccess (bool? success)
		{
			InvokeSafelyAsBoolean ("putSuccess", success);
		}

		public void PutContentName (string contentName)
		{
			InvokeSafelyAsString ("putContentName", contentName);
		}

		public void PutContentType (string contentType)
		{
			InvokeSafelyAsString ("putContentType", contentType);
		}

		public void PutContentId (string contentId)
		{
			InvokeSafelyAsString ("putContentId", contentId);
		}

		public void PutCurrency (string currency)
		{
			InvokeSafelyAsCurrency ("putCurrency", currency);
		}

		public void InvokeSafelyAsCurrency (string methodName, string currency)
		{
			if (currency != null) {
				var currencyClass = new AndroidJavaClass ("java.util.Currency");	
				var currencyObject = currencyClass.CallStatic<AndroidJavaObject> ("getInstance", currency);
				javaObject.Call<AndroidJavaObject> ("putCurrency", currencyObject);
			}
		}

		// Invokes the 'methodName' on 'this' object after
		public void InvokeSafelyAsBoolean (string methodName, bool? arg)
		{
			if (arg != null) {
				javaObject.Call<AndroidJavaObject> (methodName, arg);
			}
		}

		// Invokes the 'methodName' on 'this' object after
		public void InvokeSafelyAsInt (string methodName, int? arg)
		{
			if (arg != null) {
				javaObject.Call<AndroidJavaObject> (methodName, arg);
			}
		}

		// Invokes the 'methodName' on 'this' object after
		public void InvokeSafelyAsString (string methodName, string arg)
		{
			if (arg != null) {
				javaObject.Call<AndroidJavaObject> (methodName, arg);
			}
		}

		// Invokes the 'methodName' on 'this' object after creating a BigDecimal
		// from the "arg"'s ToString
		public void InvokeSafelyAsDecimal (string methodName, object arg)
		{
			if (arg != null) {
				javaObject.Call<AndroidJavaObject> (methodName, new AndroidJavaObject ("java.math.BigDecimal", arg.ToString ()));
			}
		}

		// Invokes the 'methodName' on 'this' object after creating a Double from
		// the "arg"'s ToString
		public void InvokeSafelyAsDouble (string methodName, object arg)
		{
			if (arg != null) {
				javaObject.Call<AndroidJavaObject> (methodName, AsDouble (arg));
			}
		}

		// Returns True if o is a Numeric type
		// Reference: http://stackoverflow.com/a/1750024
		static bool IsNumericType (object o)
		{   
			switch (Type.GetTypeCode (o.GetType ())) {
			case TypeCode.Byte:
			case TypeCode.SByte:
			case TypeCode.UInt16:
			case TypeCode.UInt32:
			case TypeCode.UInt64:
			case TypeCode.Int16:
			case TypeCode.Int32:
			case TypeCode.Int64:
			case TypeCode.Decimal:
			case TypeCode.Double:
			case TypeCode.Single:
				return true;
			default:
				return false;
			}
		}

		static AndroidJavaObject AsDouble (object param)
		{
			return new AndroidJavaObject ("java.lang.Double", param.ToString ());
		}
	}

	class AnswersSharedInstanceJavaObject
	{
		AndroidJavaObject javaObject;

		public AnswersSharedInstanceJavaObject ()
		{
			var answersClass = new AndroidJavaClass ("com.crashlytics.android.answers.Answers");	
			javaObject = answersClass.CallStatic<AndroidJavaObject> ("getInstance");
		}

		public void Log (string methodName, AnswersEventInstanceJavaObject eventInstance)
		{
			javaObject.Call (methodName, eventInstance.javaObject);
		}
	}
}
#endif
