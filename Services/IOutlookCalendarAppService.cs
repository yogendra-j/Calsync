using CalendarSync.SystemCalendar.Dto;
using System;
using System.Threading.Tasks;

namespace CalendarSync.SystemCalendar
{
    public interface IOutlookCalendarAppService
    {
        Task<bool> CheckIfOutlookIsAuthorized(long userID);
        Task<bool> DeleteOutlookAccountByUserId(long userID);
        Task<string> GetAccessTokenFromCode(OutlookAccessTokenRequestDto requestDto);
        Task<OutlookAuthUrlResponseDto> GetAuthorizationRequestUrl(string state);
        Task<bool> StoreUserTokenInfo(OutlookTokenInfoDto outlookTokenInfoDto);

        Task<bool> CreateEvents(OutlookEventWithUserInputDto eventInputDto);
        Task<bool> UpdateEventOccurrence(OutlookEventWithUserInputDto calendarInputDto, DateTime searchTime);
        Task<bool> UpdateEventOccurrenceById(OutlookEventWithUserInputDto calendarInputDto);
        Task<bool> UpdateEventSeries(OutlookEventWithUserInputDto calendarInputDto);
        Task<bool> DeleteOutlookEventBySystemEventId(EventDeleteInputDto deleteInputDto);
        Task<bool> UpdateOutlookEvent(OutlookEventWithUserInputDto calendarInputDto);
        Task<bool> DeleteOutlookEventById(EventDeleteInputDto deleteInputDto);
        Task<bool> DeleteEventOccurrenceSystemEventId(EventDeleteInputDto deleteInputDto, DateTime searchTime);
        Task<bool> DeleteEventOccurrenceById(EventDeleteInputDto deleteInputDto);
        Task<string> GetOutlookEventOccurrenceId(string outlookEventId, long userID, DateTime searchTime);
        Task<GetOutlookDeltaEventDto> GetAllOutlookDeltaEvents(long userId, DateTime startSearchDate, DateTime endSearchDate, bool needManhattanCopy);

        Task<bool> FetchAndSaveEventsInUserTable(long userID);
        Task<bool> FetchAndSaveEventsInUserTableByUserIdList(System.Collections.Generic.List<long> userIdList);
        Task<GetOutlookDeltaEventDto> GetAllDeletedOutlookEvents(long userId, DateTime startSearchDate, DateTime endSearchDate);
    }
}