using System;

namespace CalendarSync.SystemCalendar.Dto
{
    public class UserOutlookCalendarEventDto
    {
        public virtual long? SystemEventId { get; set; }
        public virtual long UserId { get; set; }
        public virtual string OutlookEventId { get; set; }
        public virtual string OutlookEventDetails { get; set; }
        public virtual OutlookCalendarEventItem OutlookEventItem { get; set; }
        public virtual DateTime LastChangeDateTime { get; set; }
    }
}
