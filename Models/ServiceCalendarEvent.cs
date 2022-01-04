using CalendarSyncPOC.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarSync.SystemCalendar
{
    public class ServiceCalendarEvent
    {
        public virtual long Id { get; set; }
        public virtual long SystemEventId { get; set; }

        [Required]
        public virtual long UserId { get; set; }

        [Required]
        public virtual CalendarServiceProviderType CalendarType { get; set; }

        [Required]
        public virtual string ServiceProviderEventId { get; set; }
        public virtual string ServiceProviderEventDetails { get; set; }

        [Required]
        public virtual DateTime CreationTime { get; set; }
        public virtual long? CreatorUserId { get; set; }
        public virtual DateTime LastModificationTime { get; set; }
        public virtual long LastModificationUserId { get; set; }
        public virtual bool IsDeleted { get; set; }
        public virtual long DeleterUserId { get; set; }
        public virtual DateTime DeletionTime { get; set; }
    }
}
