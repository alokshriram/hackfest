using System;
using System.Collections.Generic;
using System.Text;
using CalendarSyncApp.Model;
using System.Linq;

namespace CalendarSyncApp.Rules
{
    public class OrganizesManyMeetings : ICalendarRule
    {
        private readonly int threshold = 3;
        int rulePriority;
        public OrganizesManyMeetings(int priority)
        {
            rulePriority = priority;
        }

        public int RulePriority => throw new NotImplementedException();

        public UserRecommendationState MatchesRule(string user, List<CalendarItem> items)
        {
            if (items.Select(a => string.Equals(a.EmailAddress,user)).Count() >threshold)
            {
                return UserRecommendationState.OrganizeManyMeetings;
            }
            return UserRecommendationState.None;
        }
    }
}
