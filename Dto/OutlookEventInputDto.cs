using Microsoft.Graph;
using System.Collections.Generic;

namespace CalendarSync.SystemCalendar.Dto
{
    public class OutlookEventInputDto
    {
        public string Subject { get; set; }
        //public List<EventAttendee> Attendees { get; set; }
        public EventDateTimeTimeZone Start { get; set; }
        public EventDateTimeTimeZone End { get; set; }
        public EventBody Body { get; set; }
        public EventLocation Location { get; set; }
        public bool IsAllDay { get; set; }
        public EventPatternedRecurrence Recurrence { get; set; }
    }
    public class EventBody
    {
        public string Content { get; set; }
        public string ContentType { get; set; }
    }

    public class EventLocation
    {
        public string DisplayName { get; set; }
    }

    public class EventPatternedRecurrence
    {
        public EventRecurrencePattern Pattern { get; set; }
        public EventRecurrenceRange Range { get; set; }
    }

    public class EventRecurrenceRange
    {
        public string EndDate { get; set; }
        public int? NumberOfOccurrences { get; set; }
        public string RecurrenceTimeZone { get; set; }
        public string StartDate { get; set; }
        public RecurrenceRangeType? Type { get; set; }
    }

    public class EventRecurrencePattern
    {
        public int? DayOfMonth { get; set; }
        public List<DayOfWeek> DaysOfWeek { get; set; }
        public DayOfWeek? FirstDayOfWeek { get; set; }
        public WeekIndex? Index { get; set; }
        public int? Interval { get; set; }
        public int? Month { get; set; }
        public RecurrencePatternType? Type { get; set; }
    }

    public class EventAttendee
    {
        public AttendeeEmailAddress EmailAddress { get; set; }
        public AttendeeType? Type { get; set; }
    }

    public class AttendeeEmailAddress
    {
        public string Address { get; set; }
    }

    public class EventDateTimeTimeZone
    {
        public string DateTime { get; set; }
        public string TimeZone { get; set; }
    }
}