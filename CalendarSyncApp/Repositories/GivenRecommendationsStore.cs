using CalendarSyncApp.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarSyncApp.Repositories
{
    public static class GivenRecommendationsStore
    {
        private static Dictionary<string, List<UserRecommendationState>> givenRecommendations = new Dictionary<string, List<UserRecommendationState>>();

        public static void RecordRecommendation(string user, UserRecommendationState state)
        {
            if(!givenRecommendations.ContainsKey(user))
            {
                givenRecommendations.Add(user, new List<UserRecommendationState>());
            }
            givenRecommendations[user].Add(state);
        }

        public static bool AlreadyRecommended(string user, UserRecommendationState state)
        {
            return givenRecommendations[user].Contains(state);
        }
    }
}
