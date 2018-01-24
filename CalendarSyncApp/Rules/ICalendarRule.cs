using CalendarSyncApp.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarSyncApp.Rules
{
    public interface ICalendarRule
    {
        int RulePriority { get; }
        UserRecommendationState MatchesRule(string user, List<CalendarItem> items);
    }
}
