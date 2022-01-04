using CalendarSyncPOC.Enums;

namespace CalendarSync.SystemCalendar.Dto
{
    public class ServiceCalendarEventDto
    {
        public long SystemEventId { get; set; }
        public long UserId { get; set; }
        public CalendarServiceProviderType CalendarType { get; set; }

        public string ServiceProviderEventId { get; set; }
        public string ServiceProviderEventDetails { get; set; }
        public System.DateTime CreationTime { get; set; }
        public long? CreatorUserId { get; set; }
    }
}
