namespace Fabric.Answers
{
	using UnityEngine;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using Fabric.Answers.Internal;

	public class Answers : MonoBehaviour
	{
		private static IAnswers implementation;

		private static IAnswers Implementation {
			get {
				if (implementation == null) {
#if UNITY_IOS && !UNITY_EDITOR
					implementation = new AnswersAppleImplementation();
#elif UNITY_ANDROID && !UNITY_EDITOR
					implementation = new AnswersAndroidImplementation();
#else
					implementation = new AnswersStubImplementation ();
#endif
				}
				return implementation;
			}
		}

		/// <summary>
		/// Log a Sign Up event to see users signing up for your app in real-time, understand how
		/// many users are signing up with different methods and their success rate signing up.
		///
		/// <param name="method">The method by which a user logged in, e.g. Twitter or Digits.</param>
		/// <param name="success">The ultimate success or failure of the login.</param>
		/// <param name="customAttributes">A dictionary of custom attributes to associate with this purchase.</param>
		/// </summary>
		public static void LogSignUp (string method = null, bool? success = null, Dictionary<string, object> customAttributes = null)
		{
			if (customAttributes == null) {
				customAttributes = new Dictionary<string,object> ();
			}
			Implementation.LogSignUp (method: method, success: success, customAttributes: customAttributes);
		}

		/// <summary>
		/// Log a Log In event to see users logging into your app in real-time, understand how many
		/// users are logging in with different methods and their success rate logging into your app.
		///
		/// <param name="method">The method by which a user logged in, e.g. email, Twitter or Digits.</param>
		/// <param name="success">The ultimate success or failure of the login.</param>
		/// <param name="customAttributes">A dictionary of custom attributes to associate with this purchase.</param>
		/// </summary>
		public static void LogLogin (string method = null, bool? success = null, Dictionary<string, object> customAttributes = null)
		{
			if (customAttributes == null) {
				customAttributes = new Dictionary<string,object> ();
			}
			Implementation.LogLogin (method: method, success: success, customAttributes: customAttributes);
		}

		/// <summary>
		/// Log a Share event to see users sharing from your app in real-time, letting you
		/// understand what content they're sharing from the type or genre down to the specific id.
		///
		/// <param name="method">The method by which a user shared, e.g. email, Twitter, SMS.</param>
		/// <param name="contentName">The human readable name for this piece of content.</param>
		/// <param name="contentType">The type of content shared.</param>
		/// <param name="contentId">The unique identifier for this piece of content. Useful for finding the top shared item.</param>
		/// <param name="customAttributes">A dictionary of custom attributes to associate with this event.</param>
		/// </summary>
		public static void LogShare (string method = null, string contentName = null, string contentType = null, string contentId = null, Dictionary<string, object> customAttributes = null)
		{
			if (customAttributes == null) {
				customAttributes = new Dictionary<string,object> ();
			}
			Implementation.LogShare (method: method, contentName: contentName, contentType: contentType, contentId: contentId, customAttributes: customAttributes);
		}

		/// <summary>
		/// Log an Invite Event to track how users are inviting other users into
		/// your application.
		///
		/// <param name="inviteMethod">The method of invitation, e.g. GameCenter, Twitter, email.</param>
		/// <param name="customAttributes">A dictionary of custom attributes to associate with this event.</param>
		/// </summary>
		public static void LogInvite (string method = null, Dictionary<string, object> customAttributes = null)
		{
			if (customAttributes == null) {
				customAttributes = new Dictionary<string,object> ();
			}
			Implementation.LogInvite (method: method, customAttributes: customAttributes);
		}

		/// <summary>
		/// Log a Level Start Event to track where users are in your game.
		///
		/// <param name="level">The level name.</param>
		/// <param name="customAttributes">A dictionary of custom attributes to associate with this level start event.</param>
		/// </summary>
		public static void LogLevelStart (string level = null, Dictionary<string, object> customAttributes = null)
		{
			if (customAttributes == null) {
				customAttributes = new Dictionary<string,object> ();
			}
			Implementation.LogLevelStart (level: level, customAttributes: customAttributes);
		}

		/// <summary>
		/// Log a Level End event to track how users are completing levels in your game.
		///
		/// <param name="level">The name of the level completed, E.G. "1" or "Training".</param>
		/// <param name="score">The score the user completed the level with.</param>
		/// <param name="success">A boolean representing whether or not the level was completed succesfully.</param>
		/// <param name="customAttributes">A dictionary of custom attributes to associate with this event.</param>
		/// </summary>
		public static void LogLevelEnd (string level = null, double? score = null, bool? success = null, Dictionary<string, object> customAttributes = null)
		{
			if (customAttributes == null) {
				customAttributes = new Dictionary<string,object> ();
			}
			Implementation.LogLevelEnd (level: level, score: score, success: success, customAttributes: customAttributes);
		}

		/// <summary>
		/// Log an Add to Cart event to see users adding items to a shopping cart in real-time, understand how
		/// many users start the purchase flow, see which items are most popular, and track plenty of other important
		/// purchase-related metrics.
		///
		/// <param name="itemPrice">The purchased item's price.</param>
		/// <param name="currency">The ISO4217 currency code. Example: USD</param>
		/// <param name="itemName">The human-readable form of the item's name. Example:</param>
		/// <param name="itemType">The type, or genre of the item. Example: Song</param>
		/// <param name="itemId">The machine-readable, unique item identifier Example: SKU</param>
		/// <param name="customAttributes">A dictionary of custom attributes to associate with this event.</param>
		/// </summary>
		public static void LogAddToCart (decimal? itemPrice = null, string currency = null, string itemName = null, string itemType = null, string itemId = null, Dictionary<string, object> customAttributes = null)
		{
			if (customAttributes == null) {
				customAttributes = new Dictionary<string,object> ();
			}
			Implementation.LogAddToCart (itemPrice: itemPrice, currency: currency, itemName: itemName, itemType: itemType, itemId: itemId, customAttributes: customAttributes);
		}

		/// <summary>
		/// Log a Purchase event to see your revenue in real-time, understand how many users are making purchases, see which
		/// items are most popular, and track plenty of other important purchase-related metrics.
		///
		/// <param name="price">The purchased item's price.</param>
		/// <param name="currency">The ISO4217 currency code. Example: USD</param>
		/// <param name="success">Was the purchase succesful or unsuccesful.</param>
		/// <param name="itemName">The human-readable form of the item's name. Example:</param>
		/// <param name="itemType">The type, or genre of the item. Example: Song</param>
		/// <param name="itemId">The machine-readable, unique item identifier Example: SKU</param>
		/// <param name="customAttributes">A dictionary of custom attributes to associate with this purchase.</param>
		/// </summary>
		public static void LogPurchase (decimal? price = null, string currency = null, bool? success = null, string itemName = null, string itemType = null, string itemId = null, Dictionary<string, object> customAttributes = null)
		{
			if (customAttributes == null) {
				customAttributes = new Dictionary<string,object> ();
			}
			Implementation.LogPurchase (price: price, currency: currency, success: success, itemName: itemName, itemType: itemType, itemId: itemId, customAttributes: customAttributes);
		}

		/// <summary>
		/// Log a Start Checkout event to see users moving through the purchase funnel in real-time, understand how many
		/// users are doing this and how much they're spending per checkout, and see how it related to other important
		/// purchase-related metrics.
		///
		/// <param name="totalPrice">The total price of the cart.</param>
		/// <param name="currency">The ISO4217 currency code. Example: USD</param>
		/// <param name="itemCount">The number of items in the cart.</param>
		/// <param name="customAttributes">A dictionary of custom attributes to associate with this purchase.</param>
		/// </summary>
		public static void LogStartCheckout (decimal? totalPrice = null, string currency = null, int? itemCount = null, Dictionary<string, object> customAttributes = null)
		{
			if (customAttributes == null) {
				customAttributes = new Dictionary<string,object> ();
			}
			Implementation.LogStartCheckout (totalPrice: totalPrice, currency: currency, itemCount: itemCount, customAttributes: customAttributes);
		}

		/// <summary>
		/// Log a Rating event to see users rating content within your app in real-time and understand what
		/// content is most engaging, from the type or genre down to the specific id.
		///
		/// <param name="rating">The integer rating given by the user.</param>
		/// <param name="contentName">The human readable name for this piece of content.</param>
		/// <param name="contentType">The type of content shared.</param>
		/// <param name="contentId">The unique identifier for this piece of content. Useful for finding the top shared item.</param>
		/// <param name="customAttributes">A dictionary of custom attributes to associate with this event.</param>
		/// </summary>
		public static void LogRating (int? rating = null, string contentName = null, string contentType = null, string contentId = null, Dictionary<string, object> customAttributes = null)
		{
			if (customAttributes == null) {
				customAttributes = new Dictionary<string,object> ();
			}
			Implementation.LogRating (rating: rating, contentName: contentName, contentType: contentType, contentId: contentId, customAttributes: customAttributes);
		}

		/// <summary>
		/// Log a Content View event to see users viewing content within your app in real-time and
		/// understand what content is most engaging, from the type or genre down to the specific id.
		///
		/// <param name="contentName">The human readable name for this piece of content.</param>
		/// <param name="contentType">The type of content shared.</param>
		/// <param name="contentId">The unique identifier for this piece of content. Useful for finding the top shared item.</param>
		/// <param name="customAttributes">A dictionary of custom attributes to associate with this event.</param>
		/// </summary>
		public static void LogContentView (string contentName = null, string contentType = null, string contentId = null, Dictionary<string, object> customAttributes = null)
		{
			if (customAttributes == null) {
				customAttributes = new Dictionary<string,object> ();
			}
			Implementation.LogContentView (contentName: contentName, contentType: contentType, contentId: contentId, customAttributes: customAttributes);
		}

		/// <summary>
		/// Log a Search event allows you to see users searching within your app in real-time and understand
		/// exactly what they're searching for.
		///
		/// <param name="query">The user's query.</param>
		/// <param name="customAttributes">A dictionary of custom attributes to associate with this event.</param>
		/// </summary>
		public static void LogSearch (string query = null, Dictionary<string, object> customAttributes = null)
		{
			if (customAttributes == null) {
				customAttributes = new Dictionary<string,object> ();
			}
			Implementation.LogSearch (query: query, customAttributes: customAttributes);
		}

		/// <summary>
		/// Log a Custom Event to see user actions that are uniquely important for your app in real-time, to see how often
		/// they're performing these actions with breakdowns by different categories you add. Use a human-readable name for
		/// the name of the event, since this is how the event will appear in Answers.
		///
		/// <param name="eventName">The human-readable name for the event.</param>
		/// <param name="customAttributes">A dictionary of custom attributes to associate with this purchase. Attribute keys
		///                              must be <code>string</code> and values must be numbers or <code>string</code>.</param>
		/// <remarks>                    How we treat numbers:
		///                              We will provide information about the distribution of values over time.
		///
		///                              How we treat <code>string</code>:
		///                              Strings are used as categorical data, allowing comparison across different category values.
		///                              Strings are limited to a maximum length of 100 characters, attributes over this length will be
		///                              truncated.
		///
		///                              When tracking the Tweet views to better understand user engagement, sending the tweet's length
		///                              and the type of media present in the tweet allows you to track how tweet length and the type of media influence
		///                              engagement.
		/// </remarks>
		/// </summary>
		public static void LogCustom (string eventName, Dictionary<string, object> customAttributes = null)
		{
			if (eventName == null) {
				UnityEngine.Debug.Log ("Answers' Custom Events require event names. Skipping this event because its name is null.");
				return;
			}

			if (customAttributes == null) {
				customAttributes = new Dictionary<string,object> ();
			}
			
			Implementation.LogCustom (eventName: eventName, customAttributes: customAttributes);
		}
	}
}
