using System;
using System.Collections.Generic;
using System.Text;
using CalendarSyncApp.Model;

namespace CalendarSyncApp.Rules
{
    public class MeetingsOverLunch : ICalendarRule
    {
        private readonly int threshold = 6;
        int rulePriority;
        public MeetingsOverLunch(int priority)
        {
            rulePriority = priority;
        }

        public int RulePriority => throw new NotImplementedException();

        public UserRecommendationState MatchesRule(string user, List<CalendarItem> items)
        {
            return UserRecommendationState.None;
        }
    }
}
