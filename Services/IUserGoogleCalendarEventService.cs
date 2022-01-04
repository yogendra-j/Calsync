using CalendarSync.SystemCalendar;
using CalendarSync.SystemCalendar.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarSync.SystemCalendar
{
    public interface IUserGoogleCalendarEventService
    {
        Task<List<UserGoogleCalendarEventDto>> GetGoogleEventsFromUserTableByUserID(long userID);
        Task<List<UserGoogleCalendarEventDto>> GetGoogleEventsFromUserTableByUserIDList(List<long> userIdList);
        Task<bool> SyncGoogleEventsInUserTable(GoogleCalendarEventsDto googleEvents, long userID);
    }
}
