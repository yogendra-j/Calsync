using CalendarSync.SystemCalendar.Dto;
using CalendarSyncPOC.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CalendarSync.SystemCalendar
{
    public interface IServiceCalendarAppService
    {
        Task<long> CreateOrUpdateServiceCalendarEvent(ServiceCalendarEventDto input);
        Task AddOrUpdateServiceProviderEvents(object events, long userId);
        Task<ServiceCalendarEventDto> GetServiceCalendarEventById(long serviceEventId);
        Task<List<ServiceCalendarEventDto>> GetAllServiceCalendarEvents();
        Task<List<ServiceCalendarEventDto>> GetServiceCalendarEventBySystemEventId(long systemEventId);
        Task<List<ServiceCalendarEventDto>> GetServiceCalendarEventByUserId(long userId);
        Task<ServiceCalendarEventDto> GetServiceCalendarEventByProviderId(string providerId);
        Task<List<ServiceCalendarEventDto>> GetServiceCalendarEventByProvider(CalendarServiceProviderType calType);
        Task<List<ServiceCalendarEventDto>> GetServiceCalendarEventByUserIdAndProvider(long userId, CalendarServiceProviderType calType);
        Task DeleteServiceCalendarEventByProviderEventID(string providerEventId);
        Task DeleteServiceCalendarEventBySystemEventID(long systemEventId);

        Task<bool> CheckIfRecurringEventExistInServiceCalendar(string providerId, long userId, CalendarServiceProviderType calType);
    }
}
