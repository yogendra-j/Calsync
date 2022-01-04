using Google.Apis.Calendar.v3.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarSync.SystemCalendar.Dto
{
    public class GoogleCalendarEventsDto
    {
        public virtual string AccessRole { get; set; }
        public virtual IList<EventReminder> DefaultReminders { get; set; }
        public virtual string Description { get; set; }
        public virtual string ETag { get; set; }
        public virtual IList<Event> Items { get; set; }
        public virtual string Kind { get; set; }
        public virtual string NextPageToken { get; set; }
        public virtual string NextSyncToken { get; set; }
        public virtual string Summary { get; set; }
        public virtual string TimeZone { get; set; }
        public virtual string UpdatedRaw { get; set; }
        public virtual DateTime? Updated { get; set; }
    }


    public class GoogleCalendarEventsOutputDto
    {
        public string kind { get; set; }
        public string summary { get; set; }
        public DateTime updated { get; set; }
        public string timeZone { get; set; }
        public string accessRole { get; set; }
        public Defaultreminder[] defaultReminders { get; set; }
        public string nextPageToken { get; set; }
        public ItemGoogle[] items { get; set; }
        public long? UserId { get; set; }
    }
    
    public class Defaultreminder
    {
        public string method { get; set; }
        public int minutes { get; set; }
    }

    public class ItemGoogle
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public string id { get; set; }
        public string status { get; set; }
        public string htmlLink { get; set; }
        public DateTime created { get; set; }
        //public object created { get; set; }
        public DateTime updated { get; set; }
        public string summary { get; set; }
        public string description { get; set; }
        public Creator creator { get; set; }
        public OrganizerGoogle organizer { get; set; }
        public StartGoogle start { get; set; }
        public EndGoogle end { get; set; }
        public string iCalUID { get; set; }
        public int sequence { get; set; }
        public AttendeeGoogle[] attendees { get; set; }
        public bool guestsCanInviteOthers { get; set; }
        public bool guestsCanSeeOtherGuests { get; set; }
        public Reminders reminders { get; set; }
        public string eventType { get; set; }
        public string location { get; set; }
        public bool endTimeUnspecified { get; set; }
        public string transparency { get; set; }
        public string visibility { get; set; }
        public bool privateCopy { get; set; }
        public Source source { get; set; }
        public string recurringEventId { get; set; }
        public Originalstarttime originalStartTime { get; set; }
    }

    public class Creator
    {
        public string email { get; set; }
        public string displayName { get; set; }
        public bool self { get; set; }
    }

    public class OrganizerGoogle
    {
        public string email { get; set; }
        public string displayName { get; set; }
        public bool self { get; set; }
    }

    public class StartGoogle
    {
        public DateTime dateTime { get; set; }
        public string timeZone { get; set; }
        public string date { get; set; }
    }

    public class EndGoogle
    {
        public DateTime dateTime { get; set; }
        public string timeZone { get; set; }
        public string date { get; set; }
    }

    public class Reminders
    {
        public bool useDefault { get; set; }
        public Override[] overrides { get; set; }
    }

    public class Override
    {
        public string method { get; set; }
        public int minutes { get; set; }
    }

    public class Source
    {
        public string url { get; set; }
        public string title { get; set; }
    }

    public class Originalstarttime
    {
        public DateTime dateTime { get; set; }
        public string timeZone { get; set; }
    }

    public class AttendeeGoogle
    {
        public string email { get; set; }
        public bool self { get; set; }
        public string responseStatus { get; set; }
    }

    public class GoogleDeletedEventsOutputDto
    {
        public string kind { get; set; }
        public string summary { get; set; }
        public DateTime updated { get; set; }
        public string timeZone { get; set; }
        public string accessRole { get; set; }
        public Defaultreminder[] defaultReminders { get; set; }
        public string nextPageToken { get; set; }
        public DeletedItemGoogle[] items { get; set; }
    }

    public class DeletedItemGoogle
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public string id { get; set; }
        public string status { get; set; }
        public string htmlLink { get; set; }
        public object created { get; set; }
        public DateTime updated { get; set; }
        public string summary { get; set; }
        public string description { get; set; }
        public Creator creator { get; set; }
        public OrganizerGoogle organizer { get; set; }
        public StartGoogle start { get; set; }
        public EndGoogle end { get; set; }
        public string iCalUID { get; set; }
        public int sequence { get; set; }
        public AttendeeGoogle[] attendees { get; set; }
        public bool guestsCanInviteOthers { get; set; }
        public bool guestsCanSeeOtherGuests { get; set; }
        public Reminders reminders { get; set; }
        public string eventType { get; set; }
        public string location { get; set; }
        public bool endTimeUnspecified { get; set; }
        public string transparency { get; set; }
        public string visibility { get; set; }
        public bool privateCopy { get; set; }
        public Source source { get; set; }
        public string recurringEventId { get; set; }
        public Originalstarttime originalStartTime { get; set; }
    }


}
