using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CalendarSyncApp.Model
{
    public class CalendarItem
    {
        public string EmailAddress { get; set; }
        public string Subject { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<string> Attendees { get; set; }

    }
}
