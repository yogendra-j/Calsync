using System;

namespace CalendarSync.SystemCalendar.Dto
{
    public class UserOutlookCalendarEventDto
    {
        public virtual long Id { get; set; }
        public virtual long? SystemEventId { get; set; }
        public virtual long UserId { get; set; }
        public virtual string OutlookEventId { get; set; }
        public virtual string OutlookEventDetails { get; set; }
        public virtual OutlookCalendarEventItem OutlookEventItem { get; set; }
        public virtual DateTime LastChangeDateTime { get; set; }
        public virtual DateTime CreationTime { get; set; }
        public virtual long? CreatorUserId { get; set; }
        public virtual DateTime LastModificationTime { get; set; }
        public virtual long LastModificationUserId { get; set; }
        public virtual bool IsDeleted { get; set; }
        public virtual long DeleterUserId { get; set; }
        public virtual DateTime DeletionTime { get; set; }
    }
}
