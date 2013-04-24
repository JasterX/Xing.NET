using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Xing;
using Xing.Entities;
using Xing.OAuth;

namespace Web
{
    public partial class Default : System.Web.UI.Page
    {
        private string AccessToken
        {
            get
            {
                return (string)Session["AccessToken"];
            }
            set { Session["AccessToken"] = value; }
        }

        private InMemoryTokenManager TokenManager
        {
            get
            {
                var tokenManager = (InMemoryTokenManager)Application["TokenManager"];
                if (tokenManager == null)
                {
                    string consumerKey = ConfigurationManager.AppSettings["XingConsumerKey"];
                    string consumerSecret = ConfigurationManager.AppSettings["XingConsumerSecret"];
                    if (string.IsNullOrEmpty(consumerKey) == false)
                    {
                        tokenManager = new InMemoryTokenManager(consumerKey, consumerSecret);
                        Application["TokenManager"] = tokenManager;
                    }
                }

                return tokenManager;
            }
        }

        protected WebOAuthAuthorization Authorization
        {
            get;
            private set;
        }

        protected override void OnLoad(EventArgs e)
        {
            if (ConfigurationManager.AppSettings["XingConsumerKey"] == "xxx" || (ConfigurationManager.AppSettings["XingConsumerSecret"] == "xxx"))
            {
                liResult.Text = "Set XingConsumerKey and XingConsumerSecret in web.config";
            }
            else
            {

                this.Authorization = new WebOAuthAuthorization(this.TokenManager, this.AccessToken);

                if (!IsPostBack)
                {
                    string accessToken = this.Authorization.CompleteAuthorize();
                    if (accessToken != null)
                    {
                        this.AccessToken = accessToken;
                        Response.Redirect(Request.Path);
                    }

                    if (AccessToken == null)
                    {
                        this.Authorization.BeginAuthorize();
                    }
                    else
                    {
                        Test();
                    }
                }
            }

            base.OnLoad(e);
        }

        void Test()
        {
            XingApi api = new XingApi(this.Authorization);
            liResult.Text = api.GetNewtorkFeedSince("me", true, DateTime.Now);
        }
    }
}