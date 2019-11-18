//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Threading.Tasks;
//using System.Web;
//using System.Web.Http;
//using Newtonsoft.Json;

//namespace CRM.Models
//{
//    public class CaseTimeRegistration: TimeRegistration
//    {
//        public int CustomerCaseId { get; set; }


//        public CustomerCase CustomerCase { get; set; }

//        public int? CaseAssignmentId { get; set; }
//        public CaseAssignment CaseAssignment { get; set; }

//        public string Description { get; set; }

//        public void Start(string UserId, int customerCaseId)
//        {
//            StartDateTime = DateTime.Now;
//            this.UserId = UserId;
//            IsActive = true;
//            CustomerCaseId = customerCaseId;
//        }

//        public new void Done()
//        {
//            IsActive = false;
//            EndDateTime = DateTime.Now;
//            Interval = EndDateTime - StartDateTime;

//            if (System.Web.Configuration.WebConfigurationManager
//                .AppSettings["CallWebShopUrlTimereg"].ToString() == "1")
//            {
//                try
//                {
//                    _ = CallWebShop();
//                }
//                catch (Exception e)
//                {
//                    Console.WriteLine(e);
//                    throw;
//                }
//            }
//        }
//        private async Task CallWebShop()
//        {
//            Customer c = CustomerCase.Customer;
//            if (c == null)
//                throw new HttpResponseException(System.Net.HttpStatusCode.NotFound);
//            CustomerCase cc = CustomerCase;

//            string base_url = System.Web.Configuration.WebConfigurationManager
//                .AppSettings["WebShopUrlTimereg"].ToString();

//            var encodedProps = new List<string>();
//            var supportedTypes = new List<Type>
//            {
//                typeof(string),
//                typeof(int),
//                typeof(DateTime)
//            };
//            supportedTypes.AddRange(
//                supportedTypes.Where(x => x.IsValueType)
//                    .Select(x => typeof(Nullable<>).MakeGenericType(x)).ToList());

//            foreach (var prop in c.GetType().GetProperties())
//            {
//                Type type = prop.PropertyType;
//                if (supportedTypes.Contains(type))
//                {
//                    object val = prop.GetValue(c);
//                    if (val != null)
//                        encodedProps.Add(prop.Name + "=" + HttpUtility.UrlEncode(val.ToString()));
//                }
//            }
//            foreach (var prop in cc.GetType().GetProperties())
//            {
//                Type type = prop.PropertyType;
//                if (supportedTypes.Contains(type))
//                {
//                    object val = prop.GetValue(cc);
//                    if (val != null)
//                        encodedProps.Add("Case" + prop.Name + "=" + HttpUtility.UrlEncode(val.ToString()));
//                }
//            }
//            string query = string.Join("&", encodedProps);
//            var webShopUrl = base_url + "&" + query;
//            try
//            {
//                HttpClient client = new HttpClient();
//                HttpResponseMessage response = await client.GetAsync(webShopUrl);
//                var jsonString = response.Content.ReadAsStringAsync();
//                dynamic json = JsonConvert.DeserializeObject(await jsonString);
//                externalOrderId = json.externalOrderId;
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e);
//                throw;
//            }
            
//        }
//    }

//}