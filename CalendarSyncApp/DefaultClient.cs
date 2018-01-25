namespace service_pulse
{
    using System.Net.Http;
    using service_pulse.Model;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;

    public class DefaultRestClient 
    {
        private static HttpClient client = new HttpClient();

        public HttpResponseMessage GetAsync(string uri)
        {
            return client.GetAsync(uri).Result;
        }

        public HttpResponseMessage GetUrlWithToken(string uri, string token)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            requestMessage.Headers.Add("cache-control", "no-cache");
            requestMessage.Headers.Add("Authorization", "Bearer " + token);
            //requestMessage.Headers.Add("Content-Type", "application/json");
            return client.SendAsync(requestMessage).Result;
        }

        public string GetAuthToken(string uri, TokenRequestModel tokenRequest)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
            requestMessage.Headers.Add("cache-control", "no-cache");
            //requestMessage.Headers.Add("content-type", "application/x-www-form-urlencoded");
            List<KeyValuePair<string, string>> form = new List<KeyValuePair<string, string>>();
            form.Add(new KeyValuePair<string, string>("grant_type", tokenRequest.GrantType));
            form.Add(new KeyValuePair<string, string>("client_id", tokenRequest.ClientId));
            form.Add(new KeyValuePair<string, string>("client_secret", tokenRequest.ClientSecret));
            form.Add(new KeyValuePair<string, string>("username", tokenRequest.Username));
            form.Add(new KeyValuePair<string, string>("password", tokenRequest.Password));
            form.Add(new KeyValuePair<string, string>("scope", tokenRequest.Scopes));
            requestMessage.Content = new FormUrlEncodedContent(form);
            HttpResponseMessage message = client.SendAsync(requestMessage).Result;
            if (message.StatusCode == System.Net.HttpStatusCode.OK)
            {
                JObject json = JObject.Parse(message.Content.ReadAsStringAsync().Result);
                string token = ((JValue)json.GetValue("access_token")).Value.ToString();
                return token;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}