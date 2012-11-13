using System;
using System.Web;
using System.Net;
using System.IO;

namespace EchoAPI
{
    public class BackplaneJSON
    {
        #region Properties

        public string PostMessage
        {
            get
            {
                return "[{{" +
                           "\"source\": \"{0}\"," +
                           "\"type\": \"{1}\"," +
                           "\"payload\": {{" +
                               "\"context\": \"{2}\"," +
                               "\"identities\": {{" +
                                   "\"startIndex\": 0," +
                                   "\"itemsPerPage\": 1," +
                                    "\"totalResults\": 1," +
                                   "\"entry\": {{" +
                                       "\"id\": \"{3}\"," +
                                       "\"displayName\": \"{4}\"," +
                                       "\"accounts\": [{{" +
                                           "\"identityUrl\": \"{5}\"," +
                                           "\"username\": \"{6}\"," +
                                           "\"photos\": [{{" +
                                               "\"value\": \"{7}\"," +
                                               "\"type\": \"avatar\"" +
                                           "}}]" +
                                       "}}]" +
                                   "}}" +
                               "}}" +
                           "}}" +
                       "}}]";
            }
            private set {}
        }
        public string Source { get; set; }
        public string Context { get; set; }
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string IdentityUrl { get; set; }
        public string Username { get; set; }
        public string Value { get; set; }

        #endregion

        public string GeneratePostData(string type)
        {
            //Necessary: Source, Id, IdentityUrl
            if (String.IsNullOrEmpty(this.Source) || String.IsNullOrEmpty(this.Id) ||
                String.IsNullOrEmpty(this.IdentityUrl))
            {
                return "";
            }

            return String.Format(PostMessage,
                this.Source,
                type,
                this.Context,
                this.Id,
                this.DisplayName,
                this.IdentityUrl,
                this.Username,
                this.Value
            );
        }
    }

    public class Backplane
    {
        public string Authorization { get; set; }

        #region Constructors

        public Backplane(string busname, string secret)
        {
            this.Authorization = generateAutorization(busname, secret);
        }

        public Backplane(string encodedAutorization)
        {
            this.Authorization = encodedAutorization;
        }

        #endregion

        #region private getters

        private string generateAutorization(string busname, string secret)
        {
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

            return Convert.ToBase64String(encoding.GetBytes(busname + ":" + secret));
        }

        private string getChannelId()
        {
            return HttpContext.Current.Request.Cookies["bp_channel_id"] != null ? HttpContext.Current.Request.Cookies["bp_channel_id"].Value : "";
        }

        #endregion

        #region  Methods

        public bool SendRequest(string postData)
        {
            if (String.IsNullOrEmpty(postData)) 
            {
                //The post message wasn't initialized correctly.
                return false;
            }

            const string ContentType = "application/x-www-form-urlencoded";

            string channelId = getChannelId(),
                   responseString = "";

            bool result = false;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(channelId);

                request.ContentType = ContentType;
                request.Method = "POST";
                request.Headers.Add("Authorization", "Basic " + this.Authorization);

                using (Stream st = request.GetRequestStream())
                {
                    using (StreamWriter stw = new StreamWriter(st))
                    {
                        stw.Write(postData);
                    }
                }

                var response = request.GetResponse();

                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream);
                    responseString = reader.ReadToEnd();
                }

                result = (responseString == "{ \"result\": \"success\" }");
            }
            catch (Exception e)
            {
                //Something went wrong, check the bp_channel_id cookie or the postData.
                result =  false;
            }

            return result;
        }

        public bool Login(string baseUrl, string username, string userAvatar)
        {
            BackplaneJSON parameters = new BackplaneJSON()
            {
                Source = baseUrl,
                Context = baseUrl,
                Id = baseUrl + "/" + username,
                DisplayName = username,
                IdentityUrl = baseUrl + "/" + username,
                Username = username,
                Value = userAvatar,
            };

            return this.Login(parameters);
        }
        public bool Login(BackplaneJSON parameters)
        {
            return this.SendRequest(parameters.GeneratePostData("identity/login"));
        }

        public bool Logout(string baseUrl, string username)
        {
            BackplaneJSON parameters = new BackplaneJSON()
            {
                Source = baseUrl,
                Id = baseUrl + "/" + username,
                IdentityUrl = baseUrl + "/" + username
            };

            return this.Logout(parameters);
        }
        public bool Logout(BackplaneJSON parameters)
        {
            return this.SendRequest(parameters.GeneratePostData("identity/logout"));
        }

        #endregion
    }
}