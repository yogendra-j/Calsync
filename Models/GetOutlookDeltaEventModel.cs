using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarSyncPOC.Models
{
    public class GetOutlookDeltaEventDto
    {
        public string odatacontext { get; set; }
        public string odatanextLink { get; set; }
        public GetOutlookDeltaEventDto_Value[] value { get; set; }
        public long? UserId { get; set; }
    }

    public class GetOutlookDeltaEventDto_Value
    {
        public string odatatype { get; set; }
        public string odataetag { get; set; }
        public DateTime createdDateTime { get; set; }
        public DateTime lastModifiedDateTime { get; set; }
        public string changeKey { get; set; }
        public object[] categories { get; set; }
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
        public bool IsRoomRequested { get; set; }
        public string AutoRoomBookingStatus { get; set; }
        public bool responseRequested { get; set; }
        public string seriesMasterId { get; set; }
        public string showAs { get; set; }
        public string type { get; set; }
        public string webLink { get; set; }
        public object onlineMeetingUrl { get; set; }
        public bool isOnlineMeeting { get; set; }
        public string onlineMeetingProvider { get; set; }
        public bool allowNewTimeProposals { get; set; }
        public string OccurrenceId { get; set; }
        public bool isDraft { get; set; }
        public bool hideAttendees { get; set; }
        public object[] CalendarEventClassifications { get; set; }
        public object AutoRoomBookingOptions { get; set; }
        public string id { get; set; }
        public GetOutlookDeltaEventDto_Removed removed { get; set; }
        public GetOutlookDeltaEventDto_Responsestatus responseStatus { get; set; }
        public GetOutlookDeltaEventDto_Body body { get; set; }
        public GetOutlookDeltaEventDto_Start start { get; set; }
        public GetOutlookDeltaEventDto_End end { get; set; }
        public GetOutlookDeltaEventDto_Location location { get; set; }
        public GetOutlookDeltaEventDto_Location1[] locations { get; set; }
        public GetOutlookDeltaEventDto_Recurrence recurrence { get; set; }
        public GetOutlookDeltaEventDto_Attendee[] attendees { get; set; }
        public GetOutlookDeltaEventDto_Organizer organizer { get; set; }
        public GetOutlookDeltaEventDto_Onlinemeeting onlineMeeting { get; set; }
        public DateTime originalStart { get; set; }
    }

    public class GetOutlookDeltaEventDto_Responsestatus
    {
        public string response { get; set; }
        public DateTime time { get; set; }
    }

    public class GetOutlookDeltaEventDto_Body
    {
        public string contentType { get; set; }
        public string content { get; set; }
    }

    public class GetOutlookDeltaEventDto_Start
    {
        public DateTime dateTime { get; set; }
        public string timeZone { get; set; }
    }

    public class GetOutlookDeltaEventDto_End
    {
        public DateTime dateTime { get; set; }
        public string timeZone { get; set; }
    }

    public class GetOutlookDeltaEventDto_Location
    {
        public string displayName { get; set; }
        public string locationType { get; set; }
        public string uniqueIdType { get; set; }
        public GetOutlookDeltaEventDto_Address address { get; set; }
        public GetOutlookDeltaEventDto_Coordinates coordinates { get; set; }
        public string uniqueId { get; set; }
    }

    public class GetOutlookDeltaEventDto_Address
    {
    }

    public class GetOutlookDeltaEventDto_Coordinates
    {
    }

    public class GetOutlookDeltaEventDto_Recurrence
    {
        public GetOutlookDeltaEventDto_Pattern pattern { get; set; }
        public GetOutlookDeltaEventDto_Range range { get; set; }
    }

    public class GetOutlookDeltaEventDto_Pattern
    {
        public string type { get; set; }
        public int interval { get; set; }
        public int month { get; set; }
        public int dayOfMonth { get; set; }
        public string[] daysOfWeek { get; set; }
        public string firstDayOfWeek { get; set; }
        public string index { get; set; }
    }

    public class GetOutlookDeltaEventDto_Range
    {
        public string type { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string recurrenceTimeZone { get; set; }
        public int numberOfOccurrences { get; set; }
    }

    public class GetOutlookDeltaEventDto_Organizer
    {
        public GetOutlookDeltaEventDto_Emailaddress emailAddress { get; set; }
    }

    public class GetOutlookDeltaEventDto_Emailaddress
    {
        public string name { get; set; }
        public string address { get; set; }
    }

    public class GetOutlookDeltaEventDto_Onlinemeeting
    {
        public string joinUrl { get; set; }
    }

    public class GetOutlookDeltaEventDto_Location1
    {
        public string displayName { get; set; }
        public string locationType { get; set; }
        public string uniqueId { get; set; }
        public string uniqueIdType { get; set; }
    }

    public class GetOutlookDeltaEventDto_Attendee
    {
        public string type { get; set; }
        public GetOutlookDeltaEventDto_Status status { get; set; }
        public GetOutlookDeltaEventDto_Emailaddress1 emailAddress { get; set; }
    }

    public class GetOutlookDeltaEventDto_Status
    {
        public string response { get; set; }
        public DateTime time { get; set; }
    }

    public class GetOutlookDeltaEventDto_Emailaddress1
    {
        public string name { get; set; }
        public string address { get; set; }
    }
    public class GetOutlookDeltaEventDto_Removed
    {
        public string reason { get; set; }
    }
    public class GetOutlookCancelledEvents
    {
        public string id { get; set; }
        public string[] cancelledOccurrences { get; set; }
    }
}
