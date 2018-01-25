using CalendarSyncApp.Model;
using CalendarSyncApp.Repositories;
using CalendarSyncApp.Rules;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using service_pulse.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace CalendarSyncApp
{

    class Program
    {
        private static List<ICalendarRule> rules = new List<ICalendarRule>();
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Starting Sync Job ....");
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("App.json")
                .AddEnvironmentVariables()
                .Build();

            rules.Add(new ManyMeetingRule(1));
            rules.Add(new OrganizesManyMeetings(2));
            rules.Add(new MeetingsOverLunch(4));

            ActivityRecommendationRepository activityRepo = new ActivityRecommendationRepository(config);
            activityRepo.GetAllActivityTemplates();


            AppState.TenantId = config["TenantId"];
            AppState.ClientId = config["ClientId"];
            AppState.ClientSecret = config["ClientSecret"];
            AppState.GraphEndPoint = config["GraphEndPoint"];
            AppState.TokenEndPoint = config["TokenEndPoint"];
            AppState.BotEndPoint = config["BotEndPoint"];
            AppState.Token = GetToken();

            List<string> allUsers = GetAllUsersInTenant();

            LoadUsersCalendars(allUsers);

            Console.ReadLine();
        }

        public static string GetToken()
        {
            Console.Write("Getting Access Token ....");
            HttpClient client = new HttpClient();
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, AppState.TokenEndPoint);
            List<KeyValuePair<string, string>> content = new List<KeyValuePair<string, string>>();
            content.Add(new KeyValuePair<string, string>("client_id", AppState.ClientId));
            content.Add(new KeyValuePair<string, string>("client_secret", AppState.ClientSecret));
            content.Add(new KeyValuePair<string, string>("scope", AppState.Scope));
            content.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            message.Content = new FormUrlEncodedContent(content);

            HttpResponseMessage response = client.SendAsync(message).Result;
            string token =response.Content.ReadAsStringAsync().Result;
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                JObject resp = JObject.Parse(token);
                string actualToken = resp.GetValue("access_token").ToString();
                Console.WriteLine(" Got token");
                return actualToken;
            }

            return string.Empty;
        }

        public static List<string> GetAllUsersInTenant()
        {
            Console.WriteLine(" Getting all users in tenant");
            List<string> userPrincipalList = new List<string>();
            string userSuffix = "users?$select=userPrincipalName";
            string userEndPoint = AppState.GraphEndPoint + userSuffix;

            HttpClient client = new HttpClient();
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, userEndPoint);
            message.Headers.Add("Authorization", "Bearer " + AppState.Token);


            HttpResponseMessage response = client.SendAsync(message).Result;
            string userPayload = response.Content.ReadAsStringAsync().Result;
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                JObject resp = JObject.Parse(userPayload);
                IJEnumerable<JToken> users = resp.GetValue("value").AsJEnumerable();

                foreach(JToken user in users)
                {
                    userPrincipalList.Add(user.Value<string>("userPrincipalName"));
                }
            }
            Console.WriteLine($"Found {userPrincipalList.Count()} users");
            return userPrincipalList;
        }

        public static void LoadUsersCalendars(List<string> users)
        {
            string startDate = DateTime.Now.ToString("o");
            string endDate = DateTime.Now.AddDays(7).ToString("o");
            string calendarprefix = $"/calendarview?startdatetime={startDate}&enddatetime={endDate}&$select=start,end,attendees,subject,organizer";
           
            foreach (string user in users)
            {
                string userSubstring = $"users/{user}";
                string fullUrl = AppState.GraphEndPoint + userSubstring + calendarprefix;
                Console.WriteLine($"Getting calendar for {user}");
                HttpClient client = new HttpClient();
                HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, fullUrl);
                message.Headers.Add("Authorization", "Bearer " + AppState.Token);

                HttpResponseMessage response = client.SendAsync(message).Result;
                string userPayload = response.Content.ReadAsStringAsync().Result;
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    List<CalendarItem> meetingList = new List<CalendarItem>();
                    JObject resp = JObject.Parse(userPayload);
                    IJEnumerable<JToken> meetings = resp.GetValue("value").AsJEnumerable();
                    foreach (JToken meeting in meetings)
                    {
                        meetingList.Add(ConvertToCalendarItem(meeting));
                    }

                    Console.WriteLine($"Found {meetingList.Count()} meetings for user");

                    if (meetingList.Count > 0)
                    {
                        UserRecommendationState state = AnalyzeCalendar(user, meetingList);
                        // Call API to get recommended time
                        Console.WriteLine($"Recommendation for user is {state} ");
                        if (state != UserRecommendationState.None)
                        {
                            List<CalendarItem> items = GetRecommendedTimes(user, DateTime.Now, DateTime.Now.AddDays(7), meetingList);
                            Console.WriteLine($"Recommendation for user is {state} ");

                            // Call BOT API
                            CallBotApi(user, items, "LimeBot: Here is a recommendation");
                        }
                    }
                }
            }
        }

        public static List<CalendarItem> GetRecommendedTimes(string user, DateTime start, DateTime end, List<CalendarItem> currentMeetings)
        {
            List<CalendarItem> suggestedTimes = new List<CalendarItem>();
            //string url = AppState.GraphEndPoint + "/users/" + user + "/findMeetingTimes";
            //string meetingRequestPayload = "{  \"attendees\": [    {      \"emailAddress\": {        \"address\": \"{0}\",      },      \"type\": \"Required\"    }  ],  \"timeConstraint\": {    \"timeslots\": [      {        \"start\": {          \"dateTime\": \"{1}\",          \"timeZone\": \"Pacific Standard Time\"        },       \"end\": {          \"dateTime\": \"{2}\",          \"timeZone\": \"Pacific Standard Time\"        }}    ]  },  \"meetingDuration\": \"PT15M\"}";
            TimeSpan span = end.Subtract(start);
            for (int i = 0; i < 3; i++)
            {
                DateTime tempStart = start.AddDays(i).Date.AddHours(8); //Between work hours
                DateTime tempEnd = start.AddDays(i + 1).Date.AddHours(18); //Between work hours
                for(DateTime current =tempStart; current<=tempEnd; current.AddMinutes(30))
                {
                    if(!CalendarHasMeetingConflict(currentMeetings,tempStart,tempEnd))
                    {
                        Console.WriteLine($"Sugegsting times {current} to {current.AddMinutes(30)} ");
                        suggestedTimes.Add(new CalendarItem { StartDate = current, EndDate = current.AddMinutes(30) });
                        break;
                    }
                }
            }

            return suggestedTimes;
        }

        public static bool CalendarHasMeetingConflict(List<CalendarItem> meetingList, DateTime start, DateTime end)
        {
            foreach(CalendarItem item in meetingList)
            {
                if((item.StartDate.ToLocalTime() <start && item.EndDate.ToLocalTime()>start) ||
                   (item.StartDate <end && item.EndDate >end))
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Ideally this comes from the FindMeetingTime APIs but that is currently not enabled for application
        /// level scopes
        /// </summary>
        /// <param name="user"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public static  UserRecommendationState AnalyzeCalendar(string user, List<CalendarItem> items)
        {
            foreach(ICalendarRule rule in rules)
            {
                UserRecommendationState urs = rule.MatchesRule(user, items);
                if(urs != UserRecommendationState.None 
                    //&& !GivenRecommendationsStore.AlreadyRecommended(user,urs)
                    )
                {
                    return urs;
                }
            }
            return UserRecommendationState.None;
        }


        public static void CallBotApi(string user, List<CalendarItem> recommendations, string subject)
        {
            BotPayload payload = new BotPayload();
            payload.UserId = user;
            payload.Suggestions = new List<Suggestion>();
            payload.Suggestions = recommendations.Select(a => new Suggestion
            {
                End = new OfficeDate { DateTime = a.EndDate.ToString("o"), Timezone = "Pacific Standard Time" },
                Start = new OfficeDate { DateTime = a.StartDate.ToString("o"), Timezone = "Pacific Standard Time" },
                Subject = subject

            }).ToList();

            HttpClient client = new HttpClient();
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post,AppState.BotEndPoint);
            string jsonPayload = JsonConvert.SerializeObject(payload);
            requestMessage.Content = new StringContent(jsonPayload,Encoding.UTF8,"application/json");
            HttpResponseMessage response =  client.SendAsync(requestMessage).Result;

        }

        private static CalendarItem ConvertToCalendarItem(JToken token)
        {
            CalendarItem item = new CalendarItem();
            item.Attendees = new List<string>();
            item.Subject = token.SelectToken("subject").Value<string>();
            item.EmailAddress = token.SelectToken("organizer.emailAddress.address").Value<string>();
            item.StartDate = token.SelectToken("start.dateTime").Value<DateTime>();
            item.EndDate = token.SelectToken("end.dateTime").Value<DateTime>();
            if (token.SelectTokens("attendees").Children().Count() >= 1)
            {
                var childern = token.SelectToken("attendees").Children();
                var b = childern.Select(a => a.SelectToken("emailAddress.address").Value<string>());
                item.Attendees = b.ToList();
            }
            return item;
        }
    }
}
