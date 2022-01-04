using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarSync.SystemCalendar.Dto
{
    public class EventDeleteInputDto
    {
        public string Email { get; set; }
        public long UserId { get; set; }
        public long SystemEventID { get; set; }
        public string ProviderEventID { get; set; }
    }
}
