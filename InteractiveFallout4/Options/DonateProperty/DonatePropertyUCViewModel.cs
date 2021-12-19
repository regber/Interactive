using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Net;
using System.IO;

namespace InteractiveFallout4.Options.DonateProperty
{
    public static class DonatePropertyUCViewModel
    {
        public static PropertyPath DonatTestEnabled { get; set; } = new PropertyPath(typeof(Options.Donate).GetProperty("DonatTestEnabled"));

        public static class DonatePay
        {
            public static PropertyPath Enable { get; set; } = new PropertyPath(typeof(Options.Donate.DonatePay).GetProperty("Enable"));
            public static PropertyPath AccessToken { get; set; } = new PropertyPath(typeof(Options.Donate.DonatePay).GetProperty("AccessToken"));
        }
        public static class DonationAlerts
        {
            public static PropertyPath Enable { get; set; } = new PropertyPath(typeof(Options.Donate.DonationAlerts).GetProperty("Enable"));
            public static PropertyPath AccessToken { get; set; } = new PropertyPath(typeof(Options.Donate.DonationAlerts).GetProperty("AccessToken"));
            public static PropertyPath RefreshToken { get; set; } = new PropertyPath(typeof(Options.Donate.DonationAlerts).GetProperty("RefreshToken"));

            private static string _code;
            public static string code 
            { 
                get
                {
                    return _code;
                }
                set
                {
                    _code = value;

                }
            }

            public static ViewModel.Command GetAuthCodeCommand
            {
                get
                {
                    return new ViewModel.Command((obj) =>
                    {
                        System.Diagnostics.Process.Start(@"https://www.donationalerts.com/oauth/authorize?scope=oauth-user-show oauth-donation-index&client_id=8302&redirect_uri=http://127.0.0.1:8000&response_type=code");
                    });
                }
            }
            public static ViewModel.Command GetAuthTokenCommand
            {
                get
                {
                    return new ViewModel.Command((obj) =>
                    {
                        //MessageBox.Show(code);
                        
                        using (WebClient client = new WebClient())
                        {

                            var reqparm = new System.Collections.Specialized.NameValueCollection();
                            reqparm.Add("grant_type", "authorization_code");
                            reqparm.Add("client_id", "8302");
                            reqparm.Add("client_secret", "M0dpeq9v7S13RfEYngZZJOTSqhAYmHBT8JY3wYfu");
                            reqparm.Add("redirect_uri", "http://127.0.0.1:8000");
                            reqparm.Add("code", code);

                            byte[] responsebytes = client.UploadValues("https://www.donationalerts.com/oauth/token", reqparm);
                            string responsebody = Encoding.UTF8.GetString(responsebytes);
                            var rawMessage = Newtonsoft.Json.Linq.JObject.Parse(responsebody.Trim());

                            Options.Donate.DonationAlerts.AccessToken = rawMessage["access_token"].ToString();
                            Options.Donate.DonationAlerts.RefreshToken = rawMessage["refresh_token"].ToString();
                        }
                    });
                }
            }
        }
    }
}
