using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace CRM.Models
{ 
    public class Sms
    {
        static HttpClient client = new HttpClient();
        private static readonly Uri baseUrl = new Uri("http://smsc.comasys.dk/o/");
        internal string username = WebConfigurationManager.AppSettings["SmsUsername"];
        internal string password = WebConfigurationManager.AppSettings["SmsPassword"];
        public string recipient { get; set; }
        public string message { get; set; }
        public string sender { get; set; }

        public async Task<bool> Send()
        {
            Encoding utf8 = Encoding.UTF8;
            Encoding ascii = Encoding.Default;
            var uriBuilder = new UriBuilder(baseUrl);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["username"] = username;
            query["password"] = password;
            query["recipient"] = recipient;
            query["message"] = message;
            query["sender"] = sender;
            query["enc"] = "utf8";


            uriBuilder.Query = Uri.EscapeUriString(HttpUtility.UrlDecode(query.ToString())); 
            var url = uriBuilder.Uri;

            HttpResponseMessage response = await client.GetAsync(uriBuilder.Uri);
            
            var json = response.Content.ReadAsStringAsync();
            if (json.Result == "Result=\"SUCCESS: Message sent success\"")
            {
                return true;
            }
            else {throw new System.Exception("Error"); }

            
        }
        
    }
}