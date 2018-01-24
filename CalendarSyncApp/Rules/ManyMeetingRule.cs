using System;
using System.Collections.Generic;
using System.Text;
using CalendarSyncApp.Model;

namespace CalendarSyncApp.Rules
{
    public class ManyMeetingRule : ICalendarRule
    {
        private readonly int threshold = 6;
        int rulePriority;
        public ManyMeetingRule(int priority)
        {
            rulePriority = priority;
        }

        public int RulePriority => rulePriority;

        public UserRecommendationState MatchesRule(string user, List<CalendarItem> items)
        {
            if(items.Count > threshold)
            {
                return UserRecommendationState.BusyCalendar;
            }
            return UserRecommendationState.None;
        }
    }
}
