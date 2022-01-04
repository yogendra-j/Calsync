using System.Collections.Generic;
using System.Threading.Tasks;

namespace CalendarSync.SystemCalendar
{
    public interface IUserOutlookCalendarEventService
    {
        Task<List<UserOutlookCalendarEventDto>> GetOutlookEventsFromUserTableByUserID(long userID);
        Task<List<UserOutlookCalendarEventDto>> GetOutlookEventsFromUserTableByUserIDList(List<long> userIdList);
        Task<bool> SyncOutlookEventsInUserTable(OutlookCalendarEventDto outlookEvents, long userID);
    }
}
