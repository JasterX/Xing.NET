using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json.Linq;
using Xing.Entities;
using Xing.OAuth;

namespace Xing
{
	/// <summary>
	/// A service to access the Xing API's.
	/// </summary>
	public class XingApi
	{
		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="XingApi"/> class.
		/// </summary>
		/// <param name="authorization">The object that can send authorized requests.</param>
		public XingApi(IAuthorization authorization)
		{
			this.Authorization = authorization;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets the object that can send authorized requests.
		/// </summary>
		private IAuthorization Authorization
		{
			get;
			set;
		}
		#endregion

		#region  API

		#region User Profiles

		/// <summary>
		/// Shows a particular user's profile. 
		/// https://dev.xing.com/docs/get/users/:id
		/// </summary>
		/// <param name="userID">required - ID(s) of requested user(s)</param>
		/// <param name="userFields">optional - List of user attributes to return.</param>
		/// <returns>json string</returns>
		public string GetUser(string userID, List<UserFields> userFields = null)
		{
			string url = string.Format("{0}users/{1}", Constants.ApiBaseUrl, userID);
			UriBuilder location = new UriBuilder(url);
			AppendUserFields(ref location, "fields", userFields);
			return MakeGetRequest(location);
		}

		/// <summary>
		/// Shows the profile of the user who has granted access to an API consumer.
		/// https://dev.xing.com/docs/get/users/me
		/// </summary>
		/// <param name="userFields">optional - List of user attributes to return.</param>
		/// <returns>json string</returns>
		public string GetCurrentUser(List<UserFields> userFields = null)
		{
			string url = string.Format("{0}users/me", Constants.ApiBaseUrl);
			UriBuilder location = new UriBuilder(url);
			AppendUserFields(ref location, "fields", userFields);
			return MakeGetRequest(location);
		}

		/// <summary>
		/// Shows minimal profile information of the user that authorized the consumer. 
		/// https://dev.xing.com/docs/get/users/me/id_card
		/// </summary>
		/// <returns>json string</returns>
		public string GetCurrentUserID()
		{
			string url = string.Format("{0}users/me/id_card", Constants.ApiBaseUrl);
			UriBuilder location = new UriBuilder(url);
			return MakeGetRequest(location);
		}

		/// <summary>
		/// Returns the list of users that belong directly to the given email address. 
		/// The users will be returned in the same order as the corresponding email addresses. 
		/// If addresses are invalid or no user was found, the user will be returned with the value null.
		/// https://dev.xing.com/docs/get/users/find_by_emails
		/// </summary>
		/// <param name="email">required</param>
		/// <param name="userFields">optional - List of user attributes to return.</param>
		/// <returns>json string</returns>
		public string FindUserByEmailAddress(string email, List<UserFields> userFields = null)
		{
			return FindUserByEmailAddresses(new List<string>() { email }, userFields);
		}

		/// <summary>
		/// Returns the list of users that belong directly to the given list of email addresses. 
		/// The users will be returned in the same order as the corresponding email addresses. 
		/// If addresses are invalid or no user was found, the user will be returned with the value null.
		/// https://dev.xing.com/docs/get/users/find_by_emails
		/// </summary>
		/// <param name="emails">required</param>
		/// <param name="userFields">optional - List of user attributes to return.</param>
		/// <returns>json string</returns>
		public string FindUserByEmailAddresses(List<string> emails, List<UserFields> userFields = null)
		{
			string url = string.Format("{0}users/find_by_emails", Constants.ApiBaseUrl);
			NameValueCollection col = new NameValueCollection();
			col.Add("emails", String.Join(",", emails.ToArray()));
			UriBuilder location = new UriBuilder(url);
			location = AppendToUri(location, col);
			AppendUserFields(ref location, "user_fields", userFields);
			return MakeGetRequest(location);
		}
		#endregion  

		#region Jobs
		
		/// <summary>
		/// Returns a single job posting. 
		/// When the contact field is present, it contains either a company or a user. 
		/// The company field is not in all cases related to a company profile on XING (please see the links section).
		/// https://dev.xing.com/docs/get/jobs/:id
		/// </summary>
		/// <param name="jobID">required</param>
		/// <param name="userFields">optional - List of user attributes to return.</param>
		/// <returns>json string</returns>
		public string GetJobPosting(string jobID, List<UserFields> userFields = null)
		{
			string url = string.Format("{0}jobs/{1}", Constants.ApiBaseUrl, jobID);
			UriBuilder location = new UriBuilder(url);
			AppendUserFields(ref location, "user_fields", userFields);
			return MakeGetRequest(location);
		}

		/// <summary>
		/// Returns a list of job postings that match the given criteria.
		/// https://dev.xing.com/docs/get/jobs/find
		/// </summary>
		/// <param name="query">required - search query</param>
		/// <param name="limit">optional - Restrict the number of job postings to be returned. This must be a positive number. Default: 10</param>
		/// <param name="offset">optional - Offset. This must be a positive number. Default: 0</param>
		/// <param name="geo">optional - A geo coordinate in the format latitude, longitude, radius. Radius is specified in kilometers. Example: "51.1084,13.6737,100"</param>
		/// <param name="userFields">optional - List of user attributes to return.</param>
		/// <returns>json string</returns>
		public string SearchForJob(string query, int? limit = null, int? offset = null, string geo = null, List<UserFields> userFields = null)
		{
			string url = string.Format("{0}jobs/find", Constants.ApiBaseUrl);
			UriBuilder location = new UriBuilder(url);

			NameValueCollection col = new NameValueCollection();
			col.Add("query", query);
			if (limit != null)
			{
				col.Add("limit", limit.ToString());
			}
			if (offset != null)
			{
				col.Add("offset", offset.ToString());
			}
			if (geo != null)
			{
				col.Add("location", geo);
			}
			location = AppendToUri(location, col);

			AppendUserFields(ref location, "user_fields", userFields);
			return MakeGetRequest(location);
		}

		/// <summary>
		/// Returns a list of job recommendations for the user.
		/// https://dev.xing.com/docs/get/users/:user_id/jobs/recommendations
		/// </summary>
		/// <param name="userID">required - ID of the user starting the conversation</param>
		/// <param name="limit">optional - Restrict the number of job postings to be returned. This must be a positive number. Default: 10</param>
		/// <param name="offset">optional - This must be a positive number. Default: 0</param>
		/// <param name="userFields">optional - List of user attributes to return.</param>
		/// <returns>json string</returns>
		public string GetJobRecommendations(string userID, int? limit = null, int? offset = null, List<UserFields> userFields = null)
		{
			string url = string.Format("{0}users/{1}/jobs/recommendations", Constants.ApiBaseUrl, userID);
			UriBuilder location = new UriBuilder(url);

			NameValueCollection col = new NameValueCollection();
			if (limit != null)
			{
				col.Add("limit", limit.ToString());
			}
			if (offset != null)
			{
				col.Add("offset", offset.ToString());
			}
			location = AppendToUri(location, col);

			AppendUserFields(ref location, "user_fields", userFields);
			return MakeGetRequest(location);
		}

		#endregion

		#region Messages

		/// <summary>
		/// Returns the list of conversations for the given user.
		/// The list is sorted by the updated_at field of each conversation. 
		/// The number of unread conversations in the unread_count response field is limited to 100.
		/// https://dev.xing.com/docs/get/users/:user_id/conversations
		/// </summary>
		/// <param name="userID">required - </param>
		/// <param name="limit">optional - Restrict the number of conversations to be returned. This must be a positive number. Default: 10, Maximum: 100</param>
		/// <param name="offset">optional - This must be a positive number. Default: 0</param>
		/// <param name="numOfLatestMessagedInclude">optional - Number of latest messages to include. Must be non-negative. Default: 0, Maximum: 100</param>
		/// <param name="userFields">optional - List of user attributes to return.</param>
		/// <returns>json string</returns>
		public string GetConversations(string userID, int? limit = null, int? offset = null, int? numOfLatestMessagedInclude = null, List<UserFields> userFields = null)
		{
			string url = string.Format("{0}users/{1}/conversations", Constants.ApiBaseUrl, userID);
			UriBuilder location = new UriBuilder(url);

			NameValueCollection col = new NameValueCollection();
			if (limit != null)
			{
				col.Add("limit", limit.ToString());
			}
			if (offset != null)
			{
				col.Add("offset", offset.ToString());
			}
			if (numOfLatestMessagedInclude != null)
			{
				col.Add("with_latest_messages", numOfLatestMessagedInclude.ToString());
			}
			location = AppendToUri(location, col);

			AppendUserFields(ref location, "user_fields", userFields);
			return MakeGetRequest(location);
		}

		/// <summary>
		/// Starts a conversation by sending the passed message to the recipients.
		/// https://dev.xing.com/docs/post/users/:user_id/conversations
		/// </summary>
		/// <param name="userID">required - ID of the user starting the conversation</param>
		/// <param name="recipientIds">required - There must be at least one recipient. Sender cannot be included.</param>
		/// <param name="subject">required - Subject for conversation. Max. size is 32 UTF-8 characters</param>
		/// <param name="content">required - Message text. Max. size is 16384 UTF-8 characters.</param>
        /// <returns>json string</returns>
		public string CreateConversation(string userID, List<string> recipientIds, string subject, string content)
		{
			string url = string.Format("{0}users/{1}/conversations", Constants.ApiBaseUrl, userID);
			UriBuilder location = new UriBuilder(url);

			NameValueCollection col = new NameValueCollection();
			if (recipientIds != null)
			{
				col.Add("recipient_ids", String.Join(",", recipientIds.ToArray()));
			}
			if (subject != null)
			{
				col.Add("subject", subject);
			}
			if (content != null)
			{
				col.Add("content", content);
			}
			location = AppendToUri(location, col);

			return MakePostRequest(location);
		}

		/// <summary>
        /// Returns a single conversation. 
        /// https://dev.xing.com/docs/get/users/:user_id/conversations/:id
		/// </summary>
        /// <param name="conversationID">required - Conversation ID</param>
        /// <param name="userID">required - ID of the user starting the conversation</param>
        /// <param name="numOfLatestMessagedInclude">optional - List of user attributes to return.</param>
        /// <param name="userFields">optional - Number of latest messages to include. Must be non-negative. Default: 0, Maximum: 100</param>
		/// <returns>json string</returns>
		public string ShowConversation(string conversationID, string userID, int? numOfLatestMessagedInclude = null, List<UserFields> userFields = null)
		{
			string url = string.Format("{0}users/{1}/conversations/{2}", Constants.ApiBaseUrl, userID, conversationID);
			UriBuilder location = new UriBuilder(url);

			NameValueCollection col = new NameValueCollection();
			if (numOfLatestMessagedInclude != null)
			{
				col.Add("with_latest_messages", numOfLatestMessagedInclude.ToString());
			}
			location = AppendToUri(location, col);

			AppendUserFields(ref location, "user_fields", userFields);
			return MakeGetRequest(location);
		}

        /// <summary>
        /// Marks all messages in the conversation as read.
        /// https://dev.xing.com/docs/put/users/:user_id/conversations/:id/read
        /// </summary>
        /// <param name="conversationID">required - ID of the conversation that should be marked as read</param>
        /// <param name="userID">required - ID of the user owning the message</param>
        /// <returns>json string</returns>
		public string MarkConversationAsRead(string conversationID, string userID)
		{
			string url = string.Format("{0}users/{1}/conversations/{2}/read", Constants.ApiBaseUrl, userID, conversationID);
			UriBuilder location = new UriBuilder(url);
			return MakePutRequest(location);
		}

        /// <summary>
        /// Add a contact of yours to a conversation.
        /// https://dev.xing.com/docs/put/users/:user_id/conversations/:conversation_id/participants/:id
        /// </summary>
        /// <param name="conversationID">required - ID of a conversation</param>
        /// <param name="userID">required - ID of the user starting the conversation</param>
        /// <param name="contactID">required - ID of the user that should be invited to the conversation</param>
        /// <returns>json string</returns>
		public string AddContactToConversation(string conversationID, string userID, string contactID)
		{
			string url = string.Format("{0}users/{1}/conversations/{2}/participants/{3}", Constants.ApiBaseUrl, userID, conversationID, contactID);
			UriBuilder location = new UriBuilder(url);
			return MakePutRequest(location);
		}

        /// <summary>
        /// Returns the messages for a conversation.
        /// https://dev.xing.com/docs/get/users/:user_id/conversations/:conversation_id/messages
        /// </summary>
        /// <param name="conversationID">required - ID of the conversation that the message belongs to</param>
        /// <param name="userID">required - ID of the user owning the message</param>
        /// <param name="limit">optional - Restrict the number of conversations to be returned. This must be a positive number. Default: 10</param>
        /// <param name="offset">optional - This must be a positive number. Default: 0</param>
        /// <param name="userFields">optional - List of user attributes to return.</param>
        /// <returns>json string</returns>
        public string ShowConversationMessages(string conversationID, string userID, int? limit = null, int? offset = null, List<UserFields> userFields = null)
		{
			string url = string.Format("{0}users/{1}/conversations/{2}/messages", Constants.ApiBaseUrl, userID, conversationID);
			UriBuilder location = new UriBuilder(url);

			NameValueCollection col = new NameValueCollection();
            if (limit != null)
            {
                col.Add("limit", limit.ToString());
            }
            if (offset != null)
            {
                col.Add("offset", offset.ToString());
            }
			location = AppendToUri(location, col);

			AppendUserFields(ref location, "user_fields", userFields);
			return MakeGetRequest(location);
		}

        /// <summary>
        /// Returns a single message.
        /// https://dev.xing.com/docs/get/users/:user_id/conversations/:conversation_id/messages/:id
        /// </summary>
        /// <param name="conversationID">required - ID of the conversation where the message is created in</param>
        /// <param name="messageID">required - ID of the message</param>
        /// <param name="userID">required - ID of the message sender</param>
        /// <param name="userFields">optional - List of user attributes to be returned.</param>
        /// <returns>json string</returns>
		public string ShowMessage(string conversationID, string messageID, string userID, List<UserFields> userFields = null)
		{
			string url = string.Format("{0}users/{1}/conversations/{2}/messages/{3}", Constants.ApiBaseUrl, userID, conversationID, messageID);
			UriBuilder location = new UriBuilder(url);
			return MakeGetRequest(location);
		}

        /// <summary>
        /// Marks a message in a conversation as read.
        /// https://dev.xing.com/docs/put/users/:user_id/conversations/:conversation_id/messages/:id/read
        /// </summary>
        /// <param name="conversationID">required - ID of the conversation that the message belongs to</param>
        /// <param name="messageID">required - ID of the message that should be marked as read</param>
        /// <param name="userID">required - ID of the user owning the message</param>
        /// <returns>json string</returns>
		public string MarkMessageAsRead(string conversationID, string messageID, string userID)
		{
			string url = string.Format("{0}users/{1}/conversations/{2}/messages/{3}/read", Constants.ApiBaseUrl, userID, conversationID, messageID);
			UriBuilder location = new UriBuilder(url);
			return MakePutRequest(location);
		}

        /// <summary>
        /// Marks a message in a conversation as unread.
        /// https://dev.xing.com/docs/delete/users/:user_id/conversations/:conversation_id/messages/:id/read
        /// </summary>
        /// <param name="conversationID">required - ID of the conversation that the message belongs to</param>
        /// <param name="messageID">required - ID of the message that should be marked as unread</param>
        /// <param name="userID">required - ID of the user owning the message</param>
        /// <returns>json string</returns>
		public string MarkMessageAsUnread(string conversationID, string messageID, string userID)
		{
			string url = string.Format("{0}users/{1}/conversations/{2}/messages/{3}/read", Constants.ApiBaseUrl, userID, conversationID, messageID);
			UriBuilder location = new UriBuilder(url);
			return MakeDeleteRequest(location);
		}

        /// <summary>
        /// Creates a new message in an existing conversation. Premium members are limited to 20 messages to non-contacts per month.
        /// https://dev.xing.com/docs/post/users/:user_id/conversations/:conversation_id/messages
        /// </summary>
        /// <param name="conversationID">required - ID of the conversation where the message is created in</param>
        /// <param name="userID">required - ID of the message sender</param>
        /// <param name="content">required - Message text with max size of 16384 characters</param>
        /// <returns>json string</returns>
		public string CreateMessage(string conversationID, string userID, string content)
		{
			string url = string.Format("{0}users/{1}/conversations/{2}/messages", Constants.ApiBaseUrl, userID, conversationID);
			UriBuilder location = new UriBuilder(url);

			NameValueCollection col = new NameValueCollection();
			if (content != null)
			{
				col.Add("content", content);
			}
			location = AppendToUri(location, col);

			return MakePostRequest(location);
		}

        /// <summary>
        /// Delete a conversation
        /// https://dev.xing.com/docs/delete/users/:user_id/conversations/:id
        /// </summary>
        /// <param name="conversationID">required - ID ot the conversation to delete</param>
        /// <param name="userID">required - ID of the user deleting the conversation</param>
        /// <returns>json string</returns>
		public string DeleteConversation(string conversationID, string userID)
		{
			string url = string.Format("{0}users/{1}/conversations/{2}", Constants.ApiBaseUrl, userID, conversationID);
			UriBuilder location = new UriBuilder(url);
			return MakeDeleteRequest(location);
		}

		#endregion

		#region Status Messages

        /// <summary>
        /// Delete a conversation
        /// https://dev.xing.com/docs/delete/users/:user_id/conversations/:id
        /// </summary>
        /// <param name="userID">required - ID ot the conversation to delete</param>
        /// <param name="message">required - ID of the user deleting the conversation</param>
        /// <returns>json string</returns>
		public string PostStatus(string userID, string message)
		{
			if (message.Length > 420) throw new Exception("The maximum field length is 420 characters.");

			string url = string.Format("{0}users/{1}/status_message", Constants.ApiBaseUrl, userID);
			UriBuilder location = new UriBuilder(url);

			NameValueCollection col = new NameValueCollection();
			if (!string.IsNullOrEmpty(message))
			{
				col.Add("message", message);
			}
			location = AppendToUri(location, col);

			return MakePostRequest(location);
		}

		#endregion

		#region Profile Messages 

        /// <summary>
        /// Get the recent profile message for the user with the given ID.
        /// https://dev.xing.com/docs/get/users/:user_id/profile_message
        /// </summary>
        /// <param name="userID">required</param>
        /// <returns>json string</returns>
		public string GetUserProfileMessage(string userID)
		{
			string url = string.Format("{0}users/{1}/profile_message", Constants.ApiBaseUrl, userID);
			UriBuilder location = new UriBuilder(url);
			return MakeGetRequest(location);
		}

        /// <summary>
        /// Update the profile message for the user with the given ID.
        /// https://dev.xing.com/docs/put/users/:user_id/profile_message
        /// </summary>
        /// <param name="userID">required</param>
        /// <param name="message">required - This is the actual profile message that is being updated. The maximum length is 140 characters. Leave empty to delete the current profile message.</param>
        /// <param name="isPublic">optional - Specifies whether the profile message should be visible to everyone (true) or just a user's direct contacts (false). The default is true. This parameter corresponds to the "only visible to direct contacts" checkbox on a user's profile page.</param>
        /// <returns></returns>
        public string UpdateUserProfileMessage(string userID, string message, bool? isPublic)
		{
			string url = string.Format("{0}users/{1}/profile_message", Constants.ApiBaseUrl, userID);
			UriBuilder location = new UriBuilder(url);

			NameValueCollection col = new NameValueCollection();
			if (isPublic != null)
			{
				col.Add("public", isPublic.ToString());
			}
			location = AppendToUri(location, col);

			return MakePutRequest(location);
		}

		#endregion

		#region Contacts

        /// <summary>
        /// Returns the requested user's contacts. 
        /// https://dev.xing.com/docs/get/users/:user_id/contacts
        /// </summary>
        /// <param name="userID">required - ID of the user whose contacts are to be returned</param>
        /// <param name="limit">optional - Limits the number of contacts to be returned. Must be zero or a positive number. Default: 10, Maximum: 100</param>
        /// <param name="offset">optional - Must be zero or a positive number. Default: 0</param>
        /// <param name="orderby">optional - Field that determines the ascending order of the returned list. Currently only supports "last_name". Defaults to "id"</param>
        /// <param name="userFields">optional - List of user attributes to return.</param>
        /// <returns>json string</returns>
		public string GetContacts(string userID, int? limit = null, int? offset = null, string orderby = null, List<UserFields> userFields = null)
		{
			string url = string.Format("{0}users/{1}/contacts", Constants.ApiBaseUrl, userID);
			UriBuilder location = new UriBuilder(url);

			NameValueCollection col = new NameValueCollection();
			if (limit != null)
			{
				col.Add("limit", limit.ToString());
			}
			if (offset != null)
			{
				col.Add("offset", offset.ToString());
			}
			if (orderby != null)
			{
				col.Add("order_by", orderby);
			}
			location = AppendToUri(location, col);

			AppendUserFields(ref location, "user_fields", userFields);
			return MakeGetRequest(location);
		}

        /// <summary>
        /// Retrieve all tags the user has assigned to a contact.
        /// https://dev.xing.com/docs/get/users/:user_id/contacts/:contact_id/tags
        /// </summary>
        /// <param name="contactID">required - ID of the users contact.</param>
        /// <param name="userID">required - ID of the user who assigned the tags.</param>
        /// <returns>json string</returns>
		public string GetContactTags(string contactID, string userID)
		{
			string url = string.Format("{0}users/{1}/contacts/{2}/tags", Constants.ApiBaseUrl, userID, contactID);
			UriBuilder location = new UriBuilder(url);
			return MakeGetRequest(location);
		}

        /// <summary>
        /// Returns the list of contacts who are direct contacts of both the given and the current user.
        /// https://dev.xing.com/docs/get/users/:user_id/contacts/shared
        /// </summary>
        /// <param name="userID">required - ID of user whose contacts to return</param>
        /// <param name="limit">optional - Limits the number of contacts to be returned. Must be zero or a positive number. Default: 10, Maximum: 100</param>
        /// <param name="offset">optional - Must be zero or a positive number. Default: 0</param>
        /// <param name="orderby">optional - Field that determines the ascending order of the returned list. Currently only supports "last_name". Defaults to "id"</param>
        /// <param name="userFields">optional - List of user attributes to return.</param>
        /// <returns>json string</returns>
		public string GetSharedContacts(string userID, int? limit = null, int? offset = null, string orderby = null, List<UserFields> userFields = null)
		{
			string url = string.Format("{0}users/{1}/contacts", Constants.ApiBaseUrl, userID);
			UriBuilder location = new UriBuilder(url);

			NameValueCollection col = new NameValueCollection();
			if (limit != null)
			{
				col.Add("limit", limit.ToString());
			}
			if (offset != null)
			{
				col.Add("offset", offset.ToString());
			}
			if (orderby != null)
			{
				col.Add("order_by", orderby);
			}
			location = AppendToUri(location, col);

			AppendUserFields(ref location, "user_fields", userFields);
			return MakeGetRequest(location);
		}

		#endregion

		#region Contact Requests

        /// <summary>
        /// Lists all pending incoming contact requests the specified user has received from other users.
        /// https://dev.xing.com/docs/get/users/:user_id/contact_requests
        /// </summary>
        /// <param name="userID">required - ID of the user whose incoming contact requests are to be returned</param>
        /// <param name="limit">optional - Restricts the number of contact requests to be returned. This must be a positive number. Default: 10</param>
        /// <param name="offset">optional - Offset. This must be a positive number. Default: 0</param>
        /// <param name="userFields">optional - List of user attributes to return.</param>
        /// <returns></returns>
		public string GetIncomingContactRequest(string userID, uint? limit = null, uint? offset = null, List<UserFields> userFields = null)
		{
			string url = string.Format("{0}users/{1}/contact_requests", Constants.ApiBaseUrl, userID);
			UriBuilder location = new UriBuilder(url);

			NameValueCollection col = new NameValueCollection();
			if (limit != null)
			{
				col.Add("limit", limit.ToString());
			}
			if (offset != null)
			{
				col.Add("offset", offset.ToString());
			}
			location = AppendToUri(location, col);

			AppendUserFields(ref location, "user_fields", userFields);
			return MakeGetRequest(location);
		}

        /// <summary>
        /// Lists all pending contact requests the specified user has sent.
        /// https://dev.xing.com/docs/get/users/:user_id/contact_requests/sent
        /// </summary>
        /// <param name="userID">required - ID of the user whose sent contact requests are to be returned</param>
        /// <param name="limit">optional - Restricts the number of contact requests to be returned. This must be a positive number. Default: 10</param>
        /// <param name="offset">optional - Offset. This must be a positive number. Default: 0</param>
        /// <returns>json string</returns>
		public string GetSentContactRequest(string userID, uint? limit = null, uint? offset = null)
		{
			string url = string.Format("{0}users/{1}/contact_requests/sent", Constants.ApiBaseUrl, userID);
			UriBuilder location = new UriBuilder(url);

			NameValueCollection col = new NameValueCollection();
			if (limit != null)
			{
				col.Add("limit", limit.ToString());
			}
			if (offset != null)
			{
				col.Add("offset", offset.ToString());
			}
			location = AppendToUri(location, col);

			return MakeGetRequest(location);
		}

        /// <summary>
        /// Initiates a contact request between the current user (sender) and the specified user (recipient).
        /// </summary>
        /// <param name="userID">required - ID of the user receiving the contact request</param>
        /// <param name="message">optional - Message attached to the contact request</param>
        /// <returns>json string</returns>
		public string InitiateContactRequest(string userID, string message = null)
		{
			string url = string.Format("{0}users/{1}/contact_requests", Constants.ApiBaseUrl, userID);
			UriBuilder location = new UriBuilder(url);

			NameValueCollection col = new NameValueCollection();
			if (message != null)
			{
				col.Add("message", message);
			}
			location = AppendToUri(location, col);

			return MakePostRequest(location);
		}

        /// <summary>
        /// Accepts an incoming contact request.
        /// https://dev.xing.com/docs/put/users/:user_id/contact_requests/:id/accept
        /// </summary>
        /// <param name="senderUserID">required - Sender ID</param>
        /// <param name="recipientUserID">required - Recipient ID</param>
        /// <returns>json string</returns>
		public string AcceptContactRequest(string senderUserID, string recipientUserID)
		{
			string url = string.Format("{0}users/{1}/contact_requests/{2}/accept", Constants.ApiBaseUrl, recipientUserID, senderUserID);
			UriBuilder location = new UriBuilder(url);
			return MakePutRequest(location);
		}

        /// <summary>
        /// Denies an incoming contact request or revokes an initiated contact request.
        /// https://dev.xing.com/docs/delete/users/:user_id/contact_requests/:id
        /// </summary>
        /// <param name="senderUserID">required - Sender ID</param>
        /// <param name="recipientUserID">required - Recipient ID</param>
        /// <returns>json string</returns>
		public string DenyContactRequest(string senderUserID, string recipientUserID)
		{
			string url = string.Format("{0}users/{1}/contact_requests/{2}", Constants.ApiBaseUrl, recipientUserID, senderUserID);
			UriBuilder location = new UriBuilder(url);
			return MakeDeleteRequest(location);
		}

		#endregion

		#region Contact Path

        /// <summary>
        /// Get the shortest contact path(s) between a user and any other XING user.
        /// https://dev.xing.com/docs/get/users/:user_id/network/:other_user_id/paths
        /// </summary>
        /// <param name="otherUserID">required - ID of any other XING user</param>
        /// <param name="userID">required - ID of the user whose bookmarks are to be returned</param>
        /// <param name="allPaths">optional - Specifies whether this call returns just one contact path (default) or all contact paths. Possible values are true or false. Default: false</param>
        /// <param name="userFields">optional - List of user attributes to return.</param>
        /// <returns>json string</returns>
		public string GetContactPath(string otherUserID, string userID, bool? allPaths = null, List<UserFields> userFields = null)
		{
			string url = string.Format("{0}users/{1}/network/{2}/paths", Constants.ApiBaseUrl, userID, otherUserID);
			UriBuilder location = new UriBuilder(url);

			NameValueCollection col = new NameValueCollection();
			if (allPaths != null)
			{
				col.Add("all_paths", allPaths.ToString());
			}
			location = AppendToUri(location, col);

			AppendUserFields(ref location, "user_fields", userFields);
			return MakeGetRequest(location);
		}

		#endregion

		#region Bookmarks

        /// <summary>
        /// Returns a list of bookmarked users for the given user_id. This list is sorted by the creation date of the bookmarks.
        /// https://dev.xing.com/docs/get/users/:user_id/bookmarks
        /// </summary>
        /// <param name="userID">required - ID of the user whose bookmarks are to be returned</param>
        /// <param name="limit">optional - Restrict the number of bookmarks to be returned. This must be a positive number. Default: 10</param>
        /// <param name="offset">optional - This must be a positive number. Default: 0</param>
        /// <param name="userFields">optional - List of user attributes to return.</param>
        /// <returns>json string</returns>
		public string GetBookmarks(string userID, uint? limit = null, uint? offset = null, List<UserFields> userFields = null)
		{
			string url = string.Format("{0}users/{1}/bookmarks", Constants.ApiBaseUrl, userID);
			UriBuilder location = new UriBuilder(url);

			NameValueCollection col = new NameValueCollection();
			if (limit != null)
			{
				col.Add("limit", limit.ToString());
			}
			if (offset != null)
			{
				col.Add("offset", offset.ToString());
			}
			location = AppendToUri(location, col);

			AppendUserFields(ref location, "user_fields", userFields);
			return MakeGetRequest(location);
		}

        /// <summary>
        /// Create a bookmark.
        /// https://dev.xing.com/docs/put/users/:user_id/bookmarks/:id
        /// </summary>
        /// <param name="bookmarkedUserId">required - ID of the user to be bookmarked</param>
        /// <param name="creatorUserID">required - ID of the user creating the bookmark</param>
        /// <returns>json string</returns>
		public string CreateBookmark(string bookmarkedUserId, string creatorUserID)
		{
			string url = string.Format("{0}users/{1}/bookmarks/{2}", Constants.ApiBaseUrl, creatorUserID, bookmarkedUserId);
			UriBuilder location = new UriBuilder(url);
			return MakePutRequest(location);
		}

        /// <summary>
        /// Delete a bookmark.
        /// https://dev.xing.com/docs/delete/users/:user_id/bookmarks/:id
        /// </summary>
        /// <param name="bookmarkedUserId">required - ID of the user to be bookmarked</param>
        /// <param name="creatorUserID">required - ID of the user deleting the bookmark</param>
        /// <returns></returns>
		public string DeleteBookmark(string bookmarkedUserId, string creatorUserID)
		{
			string url = string.Format("{0}users/{1}/bookmarks/{2}", Constants.ApiBaseUrl, creatorUserID, bookmarkedUserId);
			UriBuilder location = new UriBuilder(url);
			return MakeDeleteRequest(location);
		}

		#endregion

		#region Network Feed

        /// <summary>
        /// Returns the stream of activities recently performed by the user's network.
        /// https://dev.xing.com/docs/get/users/:user_id/network_feed
        /// </summary>
        /// <param name="userID">required - ID of the user whose contacts' activities are to be returned</param>
        /// <returns>json string</returns>
		public string GetNewtorkFeed(string userID)
		{
			return GetNewtorkFeed(userID, null, null, null, null);
		}

        /// <summary>
        /// Returns the stream of activities recently performed by the user's network.
        /// https://dev.xing.com/docs/get/users/:user_id/network_feed
        /// </summary>
        /// <param name="userID">required - ID of the user whose contacts' activities are to be returned</param>
        /// <param name="aggregate">optional - If set to true (default) similar activities may be combined into one. Set this to false if you don't want any aggregation at all.</param>
        /// <param name="since">optional - Only returns activities that are newer than the specified time.</param>
        /// <param name="userFields">optional - List of user attributes to be returned.</param>
        /// <returns>json string</returns>
		public string GetNewtorkFeedSince(string userID, bool? aggregate = null, DateTime? since = null, List<UserFields> userFields = null)
		{
			return GetNewtorkFeed(userID, aggregate, since, null, userFields);
		}

        /// <summary>
        /// Returns the stream of activities recently performed by the user's network.
        /// https://dev.xing.com/docs/get/users/:user_id/network_feed
        /// </summary>
        /// <param name="userID">required - ID of the user whose contacts' activities are to be returned</param>
        /// <param name="aggregate">optional - If set to true (default) similar activities may be combined into one. Set this to false if you don't want any aggregation at all.</param>
        /// <param name="until">optional - Only returns activities that are older than the specified time.</param>
        /// <param name="userFields">optional - List of user attributes to be returned.</param>
        /// <returns>json string</returns>
		public string GetNewtorkFeedUntil(string userID, bool? aggregate = null, DateTime? until = null, List<UserFields> userFields = null)
		{
			return GetNewtorkFeed(userID, aggregate, null, until, userFields);
		}

        /// <summary>
        /// Returns the stream of activities recently performed by the user's network.
        /// https://dev.xing.com/docs/get/users/:user_id/network_feed
        /// </summary>
        /// <param name="userID">required - ID of the user whose contacts' activities are to be returned</param>
        /// <param name="aggregate">optional - If set to true (default) similar activities may be combined into one. Set this to false if you don't want any aggregation at all.</param>
        /// <param name="since">optional - Only returns activities that are newer than the specified time stamp (ISO 8601). Can't be combined with until!</param>
        /// <param name="until">optional - Only returns activities that are older than the specified time stamp (ISO 8601). Can't be combined with since!</param>
        /// <param name="userFields">optional - List of user attributes to be returned.</param>
        /// <returns>json string</returns>
		private string GetNewtorkFeed(string userID, bool? aggregate, DateTime? since, DateTime? until, List<UserFields> userFields)
		{
			string url = string.Format("{0}users/{1}/network_feed", Constants.ApiBaseUrl, userID);
			UriBuilder location = new UriBuilder(url);

			NameValueCollection col = new NameValueCollection();
			if (aggregate != null)
			{
				col.Add("aggregate", aggregate.ToString());
			}
			if (since != null)
			{
				col.Add("since", GenerateTimestamp(since.Value));
			}
			if (until != null)
			{
				col.Add("until", GenerateTimestamp(until.Value));
			}
			location = AppendToUri(location, col);
			AppendUserFields(ref location, "user_fields", userFields);
			return MakeGetRequest(location);
		}


        /// <summary>
        /// Returns the stream of activities recently performed by the corresponding user.
        /// https://dev.xing.com/docs/get/users/:id/feed
        /// </summary>
        /// <param name="userID">required - ID of user whose feed is to be returned</param>
        /// <returns>json string</returns>
		public string GetUserFeed(string userID)
		{
			return GetUserFeed(userID, null, null, null);
		}

        /// <summary>
        /// Returns the stream of activities recently performed by the corresponding user.
        /// https://dev.xing.com/docs/get/users/:id/feed
        /// </summary>
        /// <param name="userID">required - ID of user whose feed is to be returned</param>
        /// <param name="since">optional - Only returns activities more recent than the specified time.</param>
        /// <param name="userFields">optional - List of user attributes to return</param>
        /// <returns>json string</returns>
		public string GetUserFeedSince(string userID, DateTime? since = null, List<UserFields> userFields = null)
		{
			return GetUserFeed(userID, since, null, userFields);
		}

        /// <summary>
        /// Returns the stream of activities recently performed by the corresponding user.
        /// https://dev.xing.com/docs/get/users/:id/feed
        /// </summary>
        /// <param name="userID">required - ID of user whose feed is to be returned</param>
        /// <param name="until">optional - Only returns activities that are older than the specified time.</param>
        /// <param name="userFields">optional - List of user attributes to return</param>
        /// <returns>json string</returns>
		public string GetUserFeedUntil(string userID, bool? aggregate = null, DateTime? until = null, List<UserFields> userFields = null)
		{
			return GetUserFeed(userID, null, until, userFields);
		}

        /// <summary>
        /// Returns the stream of activities recently performed by the corresponding user.
        /// https://dev.xing.com/docs/get/users/:id/feed
        /// </summary>
        /// <param name="userID">required - ID of user whose feed is to be returned</param>
        /// <param name="since">optional - Only returns activities more recent than the specified time stamp (ISO 8601). Can't be combined with until!</param>
        /// <param name="until">optional - Only returns activities that are older than the specified time stamp (ISO 8601). Can't be combined with since!</param>
        /// <param name="userFields">optional - List of user attributes to return</param>
        /// <returns>json string</returns>
		private string GetUserFeed(string userID, DateTime? since = null, DateTime? until = null, List<UserFields> userFields = null)
		{
			string url = string.Format("{0}users/{1}/feed", Constants.ApiBaseUrl, userID);
			UriBuilder location = new UriBuilder(url);

			NameValueCollection col = new NameValueCollection();
			if (since != null)
			{
				col.Add("since", GenerateTimestamp(since.Value));
			}
			if (until != null)
			{
				col.Add("until", GenerateTimestamp(until.Value));
			}
			location = AppendToUri(location, col);

			AppendUserFields(ref location, "user_fields", userFields);
			return MakeGetRequest(location);

		}

        /// <summary>
        /// Returns a single activity
        /// https://dev.xing.com/docs/get/activities/:id
        /// </summary>
        /// <param name="activityID">required - Activity ID</param>
        /// <param name="userFields">optional - List of user attributes to return</param>
        /// <returns>json string</returns>
		public string GetActivity(string activityID, List<UserFields> userFields = null)
		{
			string url = string.Format("{0}activities/{1}", Constants.ApiBaseUrl, activityID);
			UriBuilder location = new UriBuilder(url);
			AppendUserFields(ref location, "user_fields", userFields);
			return MakeGetRequest(location);
		}

        /// <summary>
        /// Sharing an activity means recommending it to your network. This will then create a new activity for the current user.
        /// https://dev.xing.com/docs/post/activities/:id/share
        /// </summary>
        /// <param name="activityID">required - Activity ID</param>
        /// <param name="text">optional - The text in the message accompanying the share. The maximum field length is 140 characters.</param>
        /// <returns>json string</returns>
		public string ShareActivity(string activityID, string text = null)
		{
			if (text.Length > 140) throw new Exception("The maximum field length is 140 characters.");

			string url = string.Format("{0}activities/{1}", Constants.ApiBaseUrl, activityID);
			UriBuilder location = new UriBuilder(url);

			NameValueCollection col = new NameValueCollection();
			if (!string.IsNullOrEmpty(text))
			{
				col.Add("text", text);
			}
			location = AppendToUri(location, col);

			return MakePostRequest(location);
		}

        /// <summary>
        /// Deletes the activity with the given ID. Users can only delete their own activities.
        /// https://dev.xing.com/docs/delete/activities/:id
        /// </summary>
        /// <param name="activityID">required - Activity ID</param>
        /// <returns>json string</returns>
		public string DeleteActivity(string activityID)
		{
			string url = string.Format("{0}activities/{1}", Constants.ApiBaseUrl, activityID);
			UriBuilder location = new UriBuilder(url);
			return MakeDeleteRequest(location);
		}

        /// <summary>
        /// Returns a list of comments added to the activity with the given activity_id. This list is sorted by the creation date of the comments.
        /// https://dev.xing.com/docs/get/activities/:activity_id/comments
        /// </summary>
        /// <param name="activityID">required - Activity ID</param>
        /// <param name="limit">optional - Restricts the number of comments to be returned. This must be a positive number. Default: 10</param>
        /// <param name="offset">optional - This must be a positive number. Default: 0</param>
        /// <param name="userFields">optional  List of user attributes to return</param>
        /// <returns>json string</returns>
		public string GetComments(string activityID, uint? limit = null, uint? offset = null, List<UserFields> userFields = null)
		{
			string url = string.Format("{0}activities/{1}/comments", Constants.ApiBaseUrl, activityID);
			UriBuilder location = new UriBuilder(url);

			NameValueCollection col = new NameValueCollection();
			if (limit != null)
			{
				col.Add("limit", limit.ToString());
			}
			if (offset != null)
			{
				col.Add("offset", offset.ToString());
			}
			location = AppendToUri(location, col);

			AppendUserFields(ref location, "user_fields", userFields);
			return MakeGetRequest(location);
		}

        /// <summary>
        /// Creates a comment for a certain activity. 
        /// https://dev.xing.com/docs/post/activities/:activity_id/comments
        /// </summary>
        /// <param name="activityID">required - Activity ID</param>
        /// <param name="text">required - Maximum comment text length: 600 characters</param>
        /// <returns>json string</returns>
		public string AddComment(string activityID, string text)
		{
			if (text.Length > 600) throw new Exception("The maximum field length is 600 characters.");

			string url = string.Format("{0}activities/{1}/comments", Constants.ApiBaseUrl, activityID);
			UriBuilder location = new UriBuilder(url);

			NameValueCollection col = new NameValueCollection();
			if (!string.IsNullOrEmpty(text))
			{
				col.Add("text", text);
			}
			location = AppendToUri(location, col);

			return MakePostRequest(location);
		}

        /// <summary>
        /// Deletes a comment for a certain activity. Users can only delete their own comments or comments for activities they own.
        /// https://dev.xing.com/docs/delete/activities/:activity_id/comments/:id
        /// </summary>
        /// <param name="activityID">required - Activity ID</param>
        /// <param name="commentID">required - Comment ID</param>
        /// <returns>json string</returns>
		public string DeleteComment(string activityID, string commentID)
		{
			string url = string.Format("{0}activities/{1}/comments/{2}", Constants.ApiBaseUrl, activityID, commentID);
			UriBuilder location = new UriBuilder(url);
			return MakeDeleteRequest(location);
		}

        /// <summary>
        /// Returns a list of users who liked a certain activity.
        /// https://dev.xing.com/docs/get/activities/:activity_id/likes
        /// </summary>
        /// <param name="activityID">required - Activity ID</param>
        /// <param name="limit">optional - Limits the number of likes to be returned. Must be a positive number. Default: 10</param>
        /// <param name="offset">optional - Must be a positive number. Default: 0</param>
        /// <param name="userFields">optional  List of user attributes to return</param>
        /// <returns>json string</returns>
		public string GetLikes(string activityID, uint? limit = null, uint? offset = null, List<UserFields> userFields = null)
		{
			string url = string.Format("{0}activities/{1}/likes", Constants.ApiBaseUrl, activityID);
			UriBuilder location = new UriBuilder(url);

			NameValueCollection col = new NameValueCollection();
			if (limit != null)
			{
				col.Add("limit", limit.ToString());
			}
			if (offset != null)
			{
				col.Add("offset", offset.ToString());
			}
			location = AppendToUri(location, col);

			AppendUserFields(ref location, "user_fields", userFields);
			return MakeGetRequest(location);
		}

        /// <summary>
        /// Adds the current user to the list of likes for the given activity. 
        /// https://dev.xing.com/docs/put/activities/:activity_id/like
        /// </summary>
        /// <param name="activityID">required - Activity ID</param>
        /// <returns>json string</returns>
		public string Like(string activityID)
		{
			string url = string.Format("{0}activities/{1}/like", Constants.ApiBaseUrl, activityID);
			UriBuilder location = new UriBuilder(url);
			return MakePutRequest(location);
		}

        /// <summary>
        /// Removes a like the current user already added to the given activity.
        /// https://dev.xing.com/docs/delete/activities/:activity_id/like
        /// </summary>
        /// <param name="activityID">required - Activity ID</param>
        /// <returns>json string</returns>
		public string Unlike(string activityID)
		{
			string url = string.Format("{0}activities/{1}/like", Constants.ApiBaseUrl, activityID);
			UriBuilder location = new UriBuilder(url);
			return MakeDeleteRequest(location);
		}

		#endregion

		#region Profile Visits

        /// <summary>
        /// Returns a list of users who recently visited the specified user's profile. 
        /// Entries with a value of null in the user_id attribute indicate anonymous (non-XING) users (e.g. resulting from Google searches).
        /// https://dev.xing.com/docs/get/users/:user_id/visits
        /// </summary>
        /// <param name="userID">required - ID of the user whose profile visits are to be returned</param>
        /// <param name="limit">optional - Restricts the number of profile visits to be returned. This must be a positive number. Default: 10</param>
        /// <param name="offset">optional - This must be a positive number. Default: 0</param>
        /// <param name="since">optional - Only returns visits more recent than the specified time.</param>
        /// <param name="stripHtml">optional - </param>
        /// <returns>json string</returns>
		public string GetProfileVisits(string userID, uint? limit = null, uint? offset = null, DateTime? since = null, bool? stripHtml = null)
		{
			string url = string.Format("{0}users/{1}/network/recommendations", Constants.ApiBaseUrl, userID);
			UriBuilder location = new UriBuilder(url);

			NameValueCollection col = new NameValueCollection();
			if (limit != null)
			{
				col.Add("limit", limit.ToString());
			}
			if (offset != null)
			{
				col.Add("offset", offset.ToString());
			}
			if (since != null)
			{
				col.Add("since", GenerateTimestamp(since.Value));
			}
			if (stripHtml != null)
			{
				col.Add("strip_html", stripHtml.ToString());
			}
			location = AppendToUri(location, col);

			return MakeGetRequest(location);
		}

        /// <summary>
        /// Creates a profile visit. The visiting user will be derived from the user executing the call, and the visit reason derived from the consumer.
        /// https://dev.xing.com/docs/post/users/:user_id/visits
        /// </summary>
        /// <param name="visitedUserID">required - ID of the visited user.</param>
        /// <returns>json string</returns>
		public string CreateProfileVisit(string visitedUserID)
		{
			string url = string.Format("{0}users/{1}/visits", Constants.ApiBaseUrl, visitedUserID);
			UriBuilder location = new UriBuilder(url);
			return MakePostRequest(location);
		}

		#endregion

		#region Recommendations

        /// <summary>
        /// Returns a list of users the specified user might know.
        /// https://dev.xing.com/docs/get/users/:user_id/network/recommendations
        /// </summary>
        /// <param name="userID">required - ID of the user the recommendations are generated for.</param>
        /// <param name="limit">optional - Limit the number of recommendations to be returned. This must be a positive number. Default: 10, Maximum: 100</param>
        /// <param name="offset">optional - This must be a positive number. Default: 0</param>
        /// <param name="similarUserID">optional - User ID of other users for whom interesting users should be returned. The number of returned recommendations will be limited to 4 if no limit is set.</param>
        /// <param name="userFields">optional - List of user attributes to return.</param>
        /// <returns>json string</returns>
		public string GetRecommendations(string userID, uint? limit = null, uint? offset = null, uint? similarUserID = null, List<UserFields> userFields = null)
		{
			string url = string.Format("{0}users/{1}/network/recommendations", Constants.ApiBaseUrl, userID);
			UriBuilder location = new UriBuilder(url);

			NameValueCollection col = new NameValueCollection();
			if (limit != null)
			{
				col.Add("limit", limit.ToString());
			}
			if (offset != null)
			{
				col.Add("offset", offset.ToString());
			}
			if (similarUserID != null)
			{
				col.Add("similar_user_id", similarUserID.ToString());
			}
			location = AppendToUri(location, col);

			AppendUserFields(ref location, "user_fields", userFields);
			return MakeGetRequest(location);
		}

        /// <summary>
        /// Block recommendation for user with given id.
        /// https://dev.xing.com/docs/delete/users/:user_id/network/recommendations/user/:id
        /// </summary>
        /// <param name="blockUserId">required - User ID which should not appear in further recommendations</param>
        /// <param name="userID">required - User ID</param>
        /// <returns></returns>
		public string BlockRecommendation(string blockUserId, string userID)
		{
			string url = string.Format("{0}users/{1}/network/recommendations/user/{2}", Constants.ApiBaseUrl, userID, blockUserId);
			UriBuilder location = new UriBuilder(url);
			return MakeDeleteRequest(location);
		}

		#endregion

		#region Invitations

        /// <summary>
        /// Send invitations via email to contacts who do not have a XING profile. The user is allowed to invite 2000 people per week.
        /// https://dev.xing.com/docs/post/users/invite
        /// </summary>
        /// <param name="emails">required - List of email addresses.</param>
        /// <param name="message">optional - Message that is sent together with the invitation. The maximum length of this message is 150 characters for BASIC users and 600 characters for PREMIUM users. Defaults to the XING standard text for invitations.</param>
        /// <param name="userFields">optional - List of user attributes to be returned.</param>
        /// <returns>json string</returns>
		public string SendInvitations(List<string> emails, string message, List<UserFields> userFields = null)
		{
			string url = string.Format("{0}users/invite", Constants.ApiBaseUrl);
			UriBuilder location = new UriBuilder(url);

			NameValueCollection col = new NameValueCollection();
			if (emails != null)
			{
				col.Add("to_emails", String.Join(",", emails.ToArray()));
			}
			if (message != null)
			{
				col.Add("message", message);
			}
			location = AppendToUri(location, col);

			return MakePutRequest(location);
		}

		#endregion

		#region Geo Locations

        /// <summary>
        /// Update a user's geographical location with a given user_id. This user can then be found using the "get nearby users" call.
        /// https://dev.xing.com/docs/put/users/:user_id/geo_location
        /// </summary>
        /// <param name="accuracy">required - The accuracy of the detected location in meters (e.g. 12.5, 50, 50.0). Must be zero or a positive decimal number (max 3000.0 meters).</param>
        /// <param name="latitude">required - User's geographical latitude (e.g. 53.55555). Valid values range from -90.0 to 90.0 degrees.</param>
        /// <param name="longitude">required - User's geographical longitude (e.g. 9.98765). Valid values range from -180.0 to 180.0 degrees.</param>
        /// <param name="userID">required - ID of user</param>
        /// <param name="ttl">optional - How long the user can be found at this location by other users. Valid values range from 1 to 864000 seconds. Default: 420 (7 minutes).</param>
        /// <returns>json string</returns>
		public string UpdateGeoLocation(decimal accuracy, decimal latitude, decimal longitude, string userID, uint? ttl = null)
		{
			string url = string.Format("{0}users/{1}/geo_location", Constants.ApiBaseUrl, userID);
			UriBuilder location = new UriBuilder(url);

			NameValueCollection col = new NameValueCollection();
			col.Add("accuracy", accuracy.ToString());
			col.Add("latitude", latitude.ToString());
			col.Add("longitude", longitude.ToString());
			if (ttl != null)
			{
				col.Add("ttl", ttl.ToString());
			}
			location = AppendToUri(location, col);

			return MakePostRequest(location);
		}

        /// <summary>
        /// Get the users that are near your current location.
        /// Note: You need to set your location before this call can be executed.
        /// https://dev.xing.com/docs/get/users/:user_id/nearby_users
        /// </summary>
        /// <param name="userID">required - ID of user whose nearby users are to be returned</param>
        /// <param name="age">optional - The maximum age (expressed in seconds) of nearby check-ins to look for. The default and maximum is 420 seconds (i.e. 7 minutes).</param>
        /// <param name="radius">optional - The radius (in meters) within which to search. The default is 50 (meters), max. 3000 (meters). Any higher values are cut off at 3000m.</param>
        /// <param name="userFields">optional - List of user attributes to be returned.</param>
        /// <returns>json string</returns>
		public string GetNearbyUsers(string userID, uint? age = null, uint? radius = null, List<UserFields> userFields = null)
		{
			string url = string.Format("{0}users/{1}/nearby_users ", Constants.ApiBaseUrl, userID);
			UriBuilder location = new UriBuilder(url);

			NameValueCollection col = new NameValueCollection();
			if (age != null)
			{
				col.Add("age", age.ToString());
			}
			if (radius != null)
			{
				col.Add("radius", radius.ToString());
			}
			location = AppendToUri(location, col);

			AppendUserFields(ref location, "user_fields", userFields);
			return MakePutRequest(location);
		}

		#endregion

		#endregion

		#region Private methods

		static string GenerateTimestamp(DateTime dateTime)
		{
			TimeSpan t = (dateTime - new DateTime(1970, 1, 1));

			return ((long)t.TotalMilliseconds).ToString(CultureInfo.InvariantCulture);
		}

		void AppendUserFields(ref UriBuilder location, string queryParamName, List<UserFields> userFields)
		{
			if (userFields != null)
			{
				string fields = string.Empty;
				foreach (UserFields f in userFields)
				{
					fields += f.ToDescriptionString() + ",";
				}
				NameValueCollection col = new NameValueCollection();
				col.Add(queryParamName, fields);
				location = AppendToUri(location, col);
			}
		}

		UriBuilder AppendToUri(UriBuilder location, NameValueCollection values)
		{
			if (values.Count == 0)
			{
				return location;
			}

			StringBuilder sb = new StringBuilder();
			if (string.IsNullOrEmpty(location.Query) == false)
			{
				sb.Append(location.Query.Substring(1));
				sb.Append("&");
			}

			foreach (string key in values.Keys)
			{
				sb.AppendFormat("{0}={1}&", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(values[key]));
			}

			sb.Length--;

			location.Query = sb.ToString();
			return location;
		}

		string MakeGetRequest(UriBuilder location)
		{
			WebRequest webRequest = this.Authorization.InitializeGetRequest(location.Uri);
			string response = ProcessResponse(SendRequest(webRequest));
			return response;
		}

		string MakePostRequest(UriBuilder location)
		{
			WebRequest webRequest = this.Authorization.InitializePostRequest(location.Uri);
			string response = ProcessResponse(SendRequest(webRequest));
			return response;
		}

		string MakeDeleteRequest(UriBuilder location)
		{
			WebRequest webRequest = this.Authorization.InitializeDeleteRequest(location.Uri);
			string response = ProcessResponse(SendRequest(webRequest));
			return response;
		}

		string MakePutRequest(UriBuilder location)
		{
			WebRequest webRequest = this.Authorization.InitializePutRequest(location.Uri);
			string response = ProcessResponse(SendRequest(webRequest));
			return response;
		}

		/// <summary>
		/// Send a <see cref="WebRequest"/> and retrieve the response.
		/// </summary>
		/// <param name="webRequest">The web request to send.</param>
		/// <exception cref="WebException">Thrown in case of a connect failure, name resolution failure, send failure or timeout of the WebException.</exception>
		/// <returns>A <see cref="WebResponse"/> object representing the API response.</returns>
		private WebResponse SendRequest(WebRequest webRequest)
		{
			HttpWebResponse webResponse = null;
			try
			{
				webResponse = (HttpWebResponse)webRequest.GetResponse();
			}
			catch (WebException wex)
			{
				if (wex.Status == WebExceptionStatus.ConnectFailure ||
				  wex.Status == WebExceptionStatus.NameResolutionFailure ||
				  wex.Status == WebExceptionStatus.SendFailure ||
				  wex.Status == WebExceptionStatus.Timeout)
				{
					throw;
				}

				webResponse = (HttpWebResponse)wex.Response;
			}

			return webResponse;
		}

		/// <summary>
		/// Process the API response.
		/// </summary>
		/// <param name="webResponse">The <see cref="WebResponse"/> to process.</param>
		/// <returns>A json string returned by the API.</returns>
		private string ProcessResponse(WebResponse webResponse)
		{
			string response = string.Empty;
			using (var streamReader = new StreamReader(webResponse.GetResponseStream()))
			{
                response = streamReader.ReadToEnd();
			}

            int code = (int)((HttpWebResponse)webResponse).StatusCode;

            if (code >= 400 && code < 500)
            {
                JObject obj = JObject.Parse(response);
                string errorName = obj.SelectToken("error_name").ToString();
                string message = obj.SelectToken("message").ToString();
                throw new Exception(string.Format("{0}: {1}", errorName, message));
            }

			return response;
		}
		#endregion
	}
}
