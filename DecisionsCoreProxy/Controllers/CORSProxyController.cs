using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Configuration;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace DecisionsCoreProxy.Controllers
{
    [RoutePrefix("api/corsproxy")]
    public class CORSProxyController : ApiController
    {

        [HttpGet]
        [Route("get")]
        public HttpResponseMessage Get(string flowId, string Action, string outputType, string options = null)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                string sessionId = ConfigurationManager.AppSettings["DecisionsSessionId"];
                string URL = ConfigurationManager.AppSettings["DecisionsBaseURL"] + flowId + "&Action=" + Action + "&sessionid=" + ConfigurationManager.AppSettings["DecisionsSessionId"] + "&outputtype=" + outputType;

                // Check to see if we have optional parameters that need passed to Decisions
                if (options != null)
                {
                    // Just in case the options string has double ampersands due to escaping/URL Encoding issues
                    options = options.Replace("&&", "&");
                    URL = URL + "&" + options;
                }

                HttpResponseMessage GetHTTPResponse = new HttpResponseMessage();
                using (var handler = new HttpClientHandler() { })
                using (var GetHTTPClient = new HttpClient(handler))
                {
                    // Add an Accept header for JSON format.
                    GetHTTPClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Get the data from the API
                    GetHTTPResponse = GetHTTPClient.GetAsync(URL).Result;

                    if (GetHTTPResponse.IsSuccessStatusCode)
                    {
                        string json = JsonConvert.DeserializeObject(GetHTTPResponse.Content.ReadAsStringAsync().Result).ToString();
                        JObject jobj = JObject.Parse(json);

                        response = Request.CreateResponse(HttpStatusCode.OK);
                        response.Content = new StringContent(JsonConvert.SerializeObject(jobj));
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        return response;

                    }
                    else
                    {
                        throw new InvalidOperationException(GetHTTPResponse.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return response;
            }
        }

        [HttpPost]
        [Route("post")]
        public HttpResponseMessage Post(JObject data)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            data["sessionid"] = ConfigurationManager.AppSettings["DecisionsSessionId"];

            string flowId = (string)data.SelectToken("flowId");
            string Action = (string)data.SelectToken("Action");
            string URL = ConfigurationManager.AppSettings["DecisionsBaseURL"] + flowId + "&Action=" + Action;

            // We don't want to pass the flowId within the json object on to Decisions as it would be invalid, so we delete it here
            data.Property("flowId").Remove();

            try
            {
                HttpResponseMessage PostHTTPResponse = new HttpResponseMessage();
                using (var handler = new HttpClientHandler() { })
                using (var PostHTTPClient = new HttpClient(handler))
                {
                    // Add an Accept header for JSON format.
                    PostHTTPClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Get the data from the API
                    PostHTTPResponse = PostHTTPClient.PostAsJsonAsync(URL, data).Result;

                    if (PostHTTPResponse.IsSuccessStatusCode)
                    {
                        string json = JsonConvert.DeserializeObject(PostHTTPResponse.Content.ReadAsStringAsync().Result).ToString();
                        JObject jobj = JObject.Parse(json);

                        response = Request.CreateResponse(HttpStatusCode.OK);
                        response.Content = new StringContent(JsonConvert.SerializeObject(jobj));
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        return response;

                    }
                    else
                    {
                        throw new InvalidOperationException(PostHTTPResponse.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return response;
            }
        }
    }
}