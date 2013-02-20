using System;

namespace Xing
{
    /// <summary>
    /// A Constants class.
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        /// The base url for all the API calls.
        /// </summary>
        public static readonly string ApiBaseUrl = "https://api.xing.com/v1/";

        /// <summary>
        /// The name of the request token resource.
        /// </summary>
        public static readonly string RequestTokenResourceName = "request_token";

        /// <summary>
        /// The name of the authorize method.
        /// </summary>
        public static readonly string AuthorizeTokenMethod = "authorize";

        /// <summary>
        /// The name of the access token resource.
        /// </summary>
        public static readonly string AccessTokenResourceName = "access_token";

    }
}
