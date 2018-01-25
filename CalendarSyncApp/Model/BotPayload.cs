using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CalendarSyncApp.Model
{
    public class BotPayload
    {
        public string UserId { get; set; }

        public List<Suggestion> Suggestions { get; set; }

        public List<ActivityTemplate> Templates { get; set; }
    }

    public class Suggestion
    {
        public string Subject { get; set; }
        public OfficeDate Start { get; set; }
        public OfficeDate End { get; set; }
    }

    public class ActivityTemplate
    {
        public string Id { get; set; }
        public string StrategyText { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
    }

    public class OfficeDate
    {
        public string DateTime { get; set; }
        public string Timezone { get; set; }
    }
}
