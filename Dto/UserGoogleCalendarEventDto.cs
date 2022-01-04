using Google.Apis.Calendar.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarSync.SystemCalendar
{
    public class UserGoogleCalendarEventDto
    {
        public virtual long Id { get; set; }
        public virtual long? SystemEventId { get; set; }
        public virtual long UserId { get; set; }
        public virtual string GoogleEventId { get; set; }
        public virtual string GoogleEventDetails { get; set; }
        public virtual Event GoogleEventItem { get; set; }
        public virtual DateTime? LastChangeDateTime { get; set; }
        public virtual DateTime CreationTime { get; set; }
        public virtual long? CreatorUserId { get; set; }
        public virtual DateTime LastModificationTime { get; set; }
        public virtual long LastModificationUserId { get; set; }
        public virtual bool IsDeleted { get; set; }
        public virtual long DeleterUserId { get; set; }
        public virtual DateTime DeletionTime { get; set; }
    }
}
