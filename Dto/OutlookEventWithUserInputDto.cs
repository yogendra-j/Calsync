using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarSync.SystemCalendar.Dto
{
    public class OutlookEventWithUserInputDto
    {
        public string Email { get; set; }
        public string OutlookEventId { get; set; }
        public long UserId { get; set; }
        public long SystemEventID { get; set; }

        public OutlookEventInputDto OutlookEventInput { get; set; }
    }
}
