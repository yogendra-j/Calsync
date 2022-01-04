using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarSync.SystemCalendar.Dto
{
    public class OutlookCalendarEventDto
    {
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }
        [JsonProperty("value")]
        public List<OutlookCalendarEventItem> value { get; set; }
    }

    public class ResponseStatus
    {
        public string response { get; set; }
        public DateTime time { get; set; }
    }

    public class Body
    {
        public string contentType { get; set; }
        public string content { get; set; }
    }

    public class Start
    {
        public DateTime dateTime { get; set; }
        public string timeZone { get; set; }
    }

    public class End
    {
        public DateTime dateTime { get; set; }
        public string timeZone { get; set; }
    }

    public class Address
    {
    }

    public class Coordinates
    {
    }

    public class Location
    {
        public string displayName { get; set; }
        public string locationType { get; set; }
        public string uniqueIdType { get; set; }
        public Address address { get; set; }
        public Coordinates coordinates { get; set; }
    }

    public class Pattern
    {
        public string type { get; set; }
        public int interval { get; set; }
        public int month { get; set; }
        public int dayOfMonth { get; set; }
        public List<string> daysOfWeek { get; set; }
        public string firstDayOfWeek { get; set; }
        public string index { get; set; }
    }

    public class Range
    {
        public string type { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string recurrenceTimeZone { get; set; }
        public int numberOfOccurrences { get; set; }
    }

    public class Recurrence
    {
        public Pattern pattern { get; set; }
        public Range range { get; set; }
    }

    public class EmailAddress
    {
        public string name { get; set; }
        public string address { get; set; }
    }

    public class Organizer
    {
        public EmailAddress emailAddress { get; set; }
    }



    public class OutlookCalendarEventItem
    {
        [JsonProperty("@odata.etag")]
        public string OdataEtag { get; set; }
        public string id { get; set; }
        public DateTime createdDateTime { get; set; }
        public DateTime lastModifiedDateTime { get; set; }
        public string changeKey { get; set; }
        public List<object> categories { get; set; }
        public string transactionId { get; set; }
        public string originalStartTimeZone { get; set; }
        public string originalEndTimeZone { get; set; }
        public string iCalUId { get; set; }
        public int reminderMinutesBeforeStart { get; set; }
        public bool isReminderOn { get; set; }
        public bool hasAttachments { get; set; }
        public string subject { get; set; }
        public string bodyPreview { get; set; }
        public string importance { get; set; }
        public string sensitivity { get; set; }
        public bool isAllDay { get; set; }
        public bool isCancelled { get; set; }
        public bool isOrganizer { get; set; }
        public bool responseRequested { get; set; }
        public object seriesMasterId { get; set; }
        public string showAs { get; set; }
        public string type { get; set; }
        public string webLink { get; set; }
        public object onlineMeetingUrl { get; set; }
        public bool isOnlineMeeting { get; set; }
        public string onlineMeetingProvider { get; set; }
        public bool allowNewTimeProposals { get; set; }
        public bool isDraft { get; set; }
        public bool hideAttendees { get; set; }
        public ResponseStatus responseStatus { get; set; }
        public Body body { get; set; }
        public Start start { get; set; }
        public End end { get; set; }
        public Location location { get; set; }
        public List<object> locations { get; set; }
        public Recurrence recurrence { get; set; }
        public List<object> attendees { get; set; }
        public Organizer organizer { get; set; }
        public object onlineMeeting { get; set; }

        [JsonProperty("calendar@odata.associationLink")]
        public string CalendarOdataAssociationLink { get; set; }

        [JsonProperty("calendar@odata.navigationLink")]
        public string CalendarOdataNavigationLink { get; set; }
    }



    public class CalendarOutlookOutputDto
    {
        public string odatacontext { get; set; }
        public OutlookCalendarEventsOutputDto[] value { get; set; }
        public string odatanextLink { get; set; }
    }

    public class OutlookCalendarEventsOutputDto
    {
        public string odataetag { get; set; }
        public string id { get; set; }
        public DateTime createdDateTime { get; set; }
        public DateTime lastModifiedDateTime { get; set; }
        public string changeKey { get; set; }
        public string[] categories { get; set; }
        public object transactionId { get; set; }
        public string originalStartTimeZone { get; set; }
        public string originalEndTimeZone { get; set; }
        public string iCalUId { get; set; }
        public int reminderMinutesBeforeStart { get; set; }
        public bool isReminderOn { get; set; }
        public bool hasAttachments { get; set; }
        public string subject { get; set; }
        public string bodyPreview { get; set; }
        public string importance { get; set; }
        public string sensitivity { get; set; }
        public bool isAllDay { get; set; }
        public bool isCancelled { get; set; }
        public bool isOrganizer { get; set; }
        public bool responseRequested { get; set; }
        public string seriesMasterId { get; set; }
        public string showAs { get; set; }
        public string type { get; set; }
        public string webLink { get; set; }
        public object onlineMeetingUrl { get; set; }
        public bool isOnlineMeeting { get; set; }
        public string onlineMeetingProvider { get; set; }
        public bool allowNewTimeProposals { get; set; }
        public string occurrenceId { get; set; }
        public bool isDraft { get; set; }
        public bool hideAttendees { get; set; }
        public Responsestatus responseStatus { get; set; }
        public Body body { get; set; }
        public Start start { get; set; }
        public End end { get; set; }
        public Location location { get; set; }
        public object[] locations { get; set; }
        public object recurrence { get; set; }
        public Attendee[] attendees { get; set; }
        public Organizer organizer { get; set; }
        public Onlinemeeting onlineMeeting { get; set; }
    }

    public class Responsestatus
    {
        public string response { get; set; }
        public DateTime time { get; set; }
    }

    public class Emailaddress
    {
        public string name { get; set; }
        public string address { get; set; }
    }

    public class Onlinemeeting
    {
        public string joinUrl { get; set; }
    }

    public class Attendee
    {
        public string type { get; set; }
        public Status status { get; set; }
        public Emailaddress emailAddress { get; set; }
    }

    public class Status
    {
        public string response { get; set; }
        public DateTime time { get; set; }
    }

    //public class OutlookCalendarEventItems
    //{
    //    public string Id { get; set; }
    //    public string ODataType { get; set; }
    //    public IDictionary<string, object> AdditionalData { get; set; }

    //    public virtual IEnumerable<string> Categories { get; set; }
    //    public virtual string ChangeKey { get; set; }
    //    public virtual DateTimeOffset? CreatedDateTime { get; set; }
    //    public virtual DateTimeOffset? LastModifiedDateTime { get; set; }
    //    public virtual DateTimeOffset? OriginalStart { get; set; }
    //    public virtual string OriginalStartTimeZone { get; set; }

    //    public virtual PatternedRecurrence Recurrence { get; set; }

    //    public virtual int? ReminderMinutesBeforeStart { get; set; }

    //    public virtual bool? ResponseRequested { get; set; }

    //    public virtual ResponseStatus ResponseStatus { get; set; }

    //    public virtual Sensitivity? Sensitivity { get; set; }

    //    public virtual string SeriesMasterId { get; set; }

    //    public virtual string OriginalEndTimeZone { get; set; }

    //    public virtual FreeBusyStatus? ShowAs { get; set; }

    //    public virtual string Subject { get; set; }

    //    public virtual string TransactionId { get; set; }

    //    public virtual EventType? Type { get; set; }

    //    public virtual string WebLink { get; set; }

    //    public virtual EventAttachmentsCollectionPage Attachments { get; set; }

    //    /* Todo: Commented following three propeties as causing Swagger Undefined errors */

    //    //public Calendar Calendar { get; set; }

    //    //public EventExtensionsCollectionPage Extensions { get; set; }

    //    //public EventInstancesCollectionPage Instances { get; set; }

    //    public virtual DateTimeTimeZone Start { get; set; }

    //    public virtual EventMultiValueExtendedPropertiesCollectionPage MultiValueExtendedProperties { get; set; }

    //    public virtual Recipient Organizer { get; set; }

    //    public virtual OnlineMeetingProviderType? OnlineMeetingProvider { get; set; }

    //    public virtual bool? AllowNewTimeProposals { get; set; }

    //    public virtual IEnumerable<Attendee> Attendees { get; set; }

    //    public virtual ItemBody Body { get; set; }

    //    public virtual string BodyPreview { get; set; }

    //    public virtual DateTimeTimeZone End { get; set; }

    //    public bool? HasAttachments { get; set; }

    //    public virtual bool? HideAttendees { get; set; }

    //    public virtual string ICalUId { get; set; }

    //    public virtual string OnlineMeetingUrl { get; set; }

    //    public virtual Importance? Importance { get; set; }

    //    public virtual bool? IsCancelled { get; set; }

    //    public virtual bool? IsDraft { get; set; }

    //    public virtual bool? IsOnlineMeeting { get; set; }

    //    public virtual bool? IsOrganizer { get; set; }

    //    public virtual bool? IsReminderOn { get; set; }

    //    public virtual Location Location { get; set; }

    //    public virtual IEnumerable<Location> Locations { get; set; }

    //    public virtual OnlineMeetingInfo OnlineMeeting { get; set; }

    //    public virtual bool? IsAllDay { get; set; }

    //    public virtual EventSingleValueExtendedPropertiesCollectionPage SingleValueExtendedProperties { get; set; }
    //}

}

