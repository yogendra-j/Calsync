using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarSync.SystemCalendar.Dto
{
    public class OutlookAccessTokenRequestDto
    {
        public string AuthCode { get; set; }
        public long UserId { get; set; }
    }
}
