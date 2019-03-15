namespace Fabric.Answers.Internal
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	
	/// <summary>
	/// An interface defining a shared Answers Event's API which must be fulfilled. 
	/// This interface is implemented by both <code>AnswersAndroidImplementation</code> and <code>AnswersAppleImplementation</code>
	/// to properly bridge their respective native SDK's from C#.
	/// </summary>
	internal interface IAnswers
	{
		
		void LogSignUp (string method, bool? success, Dictionary<string, object> customAttributes);
		
		void LogLogin (string method, bool? success, Dictionary<string, object> customAttributes);
		
		void LogShare (string method, string contentName, string contentType, string contentId, Dictionary<string, object> customAttributes);
		
		void LogInvite (string method, Dictionary<string, object> customAttributes);

		void LogLevelStart (string level, Dictionary<string, object> customAttributes);
		
		void LogLevelEnd (string level, double? score, bool? success, Dictionary<string, object> customAttributes);

		void LogPurchase (decimal? price, string currency, bool? success, string itemName, string itemType, string itemId,  Dictionary<string, object> customAttributes);
		
		void LogAddToCart (decimal? itemPrice, string currency, string itemName, string itemType, string itemId, Dictionary<string, object> customAttributes);

		void LogStartCheckout (decimal? totalPrice, string currency, int? itemCount, Dictionary<string, object> customAttributes);
		
		void LogRating (int? rating, string contentName, string contentType, string contentId, Dictionary<string, object> customAttributes);
		
		void LogContentView (string contentName, string contentType, string contentId, Dictionary<string, object> customAttributes);
		
		void LogSearch (string query, Dictionary<string, object> customAttributes);
		
		void LogCustom (string eventName, Dictionary<string, object> customAttributes);
		
	}
}