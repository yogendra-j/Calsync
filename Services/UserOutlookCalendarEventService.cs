using CalendarSync.SystemCalendar;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIRVA.Moving.Sales.OutlookCalendar
{
    public class UserOutlookCalendarEventService : IUserOutlookCalendarEventService
    {
        private readonly IServiceCalendarAppService _serviceCalendarService;
        private readonly ILeadSurveyAppointmentAppService _leadSurveyAppointmentService;
        public UserOutlookCalendarEventService(IRepository<UserOutlookCalendarEvent, long> userOutlookCalendarService,
            IServiceCalendarAppService serviceCalendarService, ILeadSurveyAppointmentAppService leadSurveyAppointmentService)
        {
            _userOutlookCalendarService = userOutlookCalendarService;
            _serviceCalendarService = serviceCalendarService;
            _leadSurveyAppointmentService = leadSurveyAppointmentService;
        }


        public async Task<bool> SyncOutlookEventsInUserTable(OutlookCalendarEventDto outlookEvents, long userID)
        {
            try
            {
                if (outlookEvents?.value is null)
                    return false;


                var existingServiceEvents = await _serviceCalendarService.GetServiceCalendarEventByUserIdAndProvider(userID, CalendarServiceProviderType.Outlook);
                var existingOutlookEvents = await _userOutlookCalendarService.GetAll().Where(x => x.UserId == userID).ToListAsync();

                if (outlookEvents.value?.Count == 0)
                {
                    foreach (var item in existingOutlookEvents)
                    {
                        await _userOutlookCalendarService.DeleteAsync(item);
                    }
                }
                else
                {
                    var deletedEvents = existingOutlookEvents?.Where(x => !outlookEvents.value.Any(y => y.id == x.OutlookEventId)).ToList();
                    foreach (var item in deletedEvents)
                    {
                        await _userOutlookCalendarService.DeleteAsync(item);
                    }
                    foreach (var evItem in outlookEvents?.value)
                    {
                        UserOutlookCalendarEvent userOutlookEvent = new UserOutlookCalendarEvent();
                        userOutlookEvent = await _userOutlookCalendarService.GetAll().Where(x => x.UserId == userID).FirstOrDefaultAsync(x => x.OutlookEventId == evItem.id);
                        if (userOutlookEvent != null)
                        {
                            if (!userOutlookEvent.LastChangeDateTime.Equals(evItem.lastModifiedDateTime))
                            {
                                userOutlookEvent.OutlookEventDetails = JsonConvert.SerializeObject(evItem);
                                userOutlookEvent.LastChangeDateTime = evItem.lastModifiedDateTime;
                                await _userOutlookCalendarService.UpdateAsync(userOutlookEvent);
                            }
                        }
                        else
                        {
                            var evntSystemID = existingServiceEvents.FirstOrDefault(x => x.ServiceProviderEventId == evItem.id)?.SystemEventId;
                            userOutlookEvent = new UserOutlookCalendarEvent();
                            userOutlookEvent.UserId = userID;
                            userOutlookEvent.SystemEventId = evntSystemID;
                            userOutlookEvent.OutlookEventId = evItem.id;
                            userOutlookEvent.OutlookEventDetails = JsonConvert.SerializeObject(evItem);
                            userOutlookEvent.LastChangeDateTime = evItem.lastModifiedDateTime;
                            await _userOutlookCalendarService.InsertAsync(userOutlookEvent);
                        }
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<UserOutlookCalendarEventDto>> GetOutlookEventsFromUserTableByUserID(long userID)
        {
            try
            {
                var syncedAppointmentIdList = await _leadSurveyAppointmentService.GetSyncedLeadSurveyAppointmentIdListBySurveyorId(userID);
                var query = _userOutlookCalendarService.GetAll().Where(x => x.UserId == userID);
                var existingOutlookEvents = await query?.WhereIf(syncedAppointmentIdList != null && syncedAppointmentIdList?.Count > 0,
                                            x => !syncedAppointmentIdList.Contains(x.OutlookEventId)).ToListAsync();
                var allOutlookEvents = ObjectMapper.Map<List<UserOutlookCalendarEventDto>>(existingOutlookEvents);
                return allOutlookEvents;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<UserOutlookCalendarEventDto>> GetOutlookEventsFromUserTableByUserIDList(List<long> userIdList)
        {
            try
            {
                var existingOutlookEvents = await _userOutlookCalendarService.GetAll()?
                    .Where(x => userIdList.Contains(x.UserId) || userIdList.Contains(x.UserId))
                    .AsNoTracking().ToListAsync();
                List<UserOutlookCalendarEventDto> allUsersOutlookEvents = new List<UserOutlookCalendarEventDto>();
                if (existingOutlookEvents != null)
                {
                    allUsersOutlookEvents = ObjectMapper.Map<List<UserOutlookCalendarEventDto>>(existingOutlookEvents);
                }
                return allUsersOutlookEvents;
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
