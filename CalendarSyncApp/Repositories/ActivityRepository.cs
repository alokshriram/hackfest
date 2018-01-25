using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using service_pulse.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace service_pulse.Repository
{
    public class ActivityRecommendationRepository 
    {
        private DefaultRestClient client;
        private static Dictionary<string, List<string>> templatedActivities = new Dictionary<string, List<string>>();
        private static int templateCount = 0;
        private TokenRequestModel tokenRequestModel = new TokenRequestModel();
        private readonly string granteType = "password";
        private readonly string scope = "apiaccess openid";
        private string tokenEndPoint;
        private string templateEndPoint;

        public ActivityRecommendationRepository(IConfiguration configuration) : this(configuration, new DefaultRestClient())
        {
        }

        public ActivityRecommendationRepository(IConfiguration configuration,  DefaultRestClient restClient)
        {
            
            tokenRequestModel.Scopes = this.scope;
            tokenRequestModel.GrantType = this.granteType;
            this.tokenEndPoint = "https://api.limeade.info/identity/connect/token";
            this.templateEndPoint = "http://activitytemplatelibrarywebappdevelopment.azurewebsites.net/templates";
            this.client = restClient;
        }

        public string GetActivityRecommendation(string userId)
        {
            if (templatedActivities.Count == 0)
            {
                GetAllActivityTemplates();
            }

            return string.Empty;
        }

        public void GetAllActivityTemplates()
        {
            string token = this.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                HttpResponseMessage response = this.client.GetUrlWithToken(this.templateEndPoint, token);
                JArray templates = JArray.Parse(response.Content.ReadAsStringAsync().Result);
                int i = 0;
                foreach (JObject item in templates)
                {
                    string id = item.GetValue("id").ToString();
                    JToken allDimensions  = item.GetValue("dimensions");
                    foreach(JToken dimension in allDimensions )
                    {
                        string dimensionName = dimension.Value<string>();
                        if(!templatedActivities.ContainsKey(dimensionName))
                        {
                            templatedActivities.Add(dimensionName, new List<string>());
                        }
                        templatedActivities[dimensionName].Add(item.ToString());
                    }
                    //tempActivities.Add(i, id);
                    i++;
                }

            }
        }

        private string GetToken()
        {
            return this.client.GetAuthToken(this.tokenEndPoint, this.tokenRequestModel);
        }
    }
}