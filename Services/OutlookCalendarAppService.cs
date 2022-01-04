using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CalendarSync.SystemCalendar.Dto;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using CalendarSync.SystemCalendar;

namespace CalendarSync.SystemCalendar
{
    public class OutlookCalendarAppService: IOutlookCalendarAppService
    {
        private const int MaxEventCount = 1000;
        public readonly IServiceCalendarAppService _serviceCalendarAppService;
        private readonly IUserOutlookCalendarEventService _userOutlookCalendarService;
        public readonly IAppConfigurationAccessor _appConfigurationAccessor;
        public readonly IConfigurationRoot _appConfiguration;
        private CalendarConfigurationSettings CalSettingsValue { get; set; }

        public OutlookCalendarAppService(IServiceCalendarAppService serviceCalendarAppService,
            IUserOutlookCalendarEventService userOutlookCalendarService,
            IAppConfigurationAccessor appConfigurationAccessor)
        {
            _serviceCalendarAppService = serviceCalendarAppService;
            _userOutlookCalendarService = userOutlookCalendarService;
            _appConfiguration = appConfigurationAccessor.Configuration;
        }

        private async Task GetAllConfiguration()
        {
            if (string.IsNullOrEmpty(CalSettingsValue?.OutlookEventEndPoint))
            {
                var calendarConfigSettings = await _calendarSetting?.GetAllListAsync();
                if (calendarConfigSettings != null)
                {
                    CalSettingsValue = new CalendarConfigurationSettings();
                    CalSettingsValue = calendarConfigSettings?.FirstOrDefault();
                }
            }
        }

        public async Task<bool> CheckIfOutlookIsAuthorized(long userID)
        {
            var result = await _outlookTokenInfoRepository.FirstOrDefaultAsync(x => x.UserId == userID);

            if (string.IsNullOrEmpty(result?.Email))
                return false;

            var accessToken = await GetAccessToken(result.Email);
            if (string.IsNullOrEmpty(accessToken))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public async Task<bool> DeleteOutlookAccountByUserId(long userID)
        {
            try
            {
                await _outlookTokenInfoRepository.DeleteAsync(x => x.UserId == userID);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<OutlookAuthUrlResponseDto> GetAuthorizationRequestUrl(string state)
        {
            try
            {
                if (string.IsNullOrEmpty(CalSettingsValue?.OutlookEventEndPoint))
                {
                    await GetAllConfiguration();
                }
                string baseAuthUrl = CalSettingsValue?.OutlookAuthorizationEndpoint;
                var param = new Dictionary<string, string>() {
                    { "client_id", CalSettingsValue?.OutlookClientId },
                    { "response_type", "code" },
                    { "prompt", "consent" },
                    { "redirect_uri", CalSettingsValue?.OutlookRedirectURL },
                    { "response_mode", "query" },
                    { "scope", CalSettingsValue?.OutlookScope },
                    { "state", state }
                };

                var newUrl = new Uri(QueryHelpers.AddQueryString(baseAuthUrl, param));

                OutlookAuthUrlResponseDto responseDto = new OutlookAuthUrlResponseDto();
                responseDto.OutlookAuthUrl = newUrl;

                return await Task.FromResult(responseDto);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<string> GetAccessTokenFromCode(OutlookAccessTokenRequestDto requestDto)
        {
            try
            {
                if (string.IsNullOrEmpty(requestDto?.AuthCode))
                    return string.Empty;

                var result = await GetTokenInfoFromCode(requestDto?.AuthCode);

                var outlookUser = await GetOutlookUserInfo(result.access_token);

                OutlookTokenInfoDto outlookTokenInfoDto = new OutlookTokenInfoDto();
                outlookTokenInfoDto.Tokeninfo = JsonConvert.SerializeObject(result);
                outlookTokenInfoDto.RefreshToken = result.refresh_token;
                outlookTokenInfoDto.Email = string.IsNullOrEmpty(outlookUser?.mail) ? outlookUser?.userPrincipalName : outlookUser?.mail;
                outlookTokenInfoDto.Name = outlookUser?.displayName;
                outlookTokenInfoDto.UserId = requestDto.UserId;

                await StoreUserTokenInfo(outlookTokenInfoDto);
                return await Task.FromResult(result.access_token);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> StoreUserTokenInfo(OutlookTokenInfoDto outlookTokenInfoDto)
        {
            try
            {
                if (!string.IsNullOrEmpty(outlookTokenInfoDto?.RefreshToken))
                {
                    var existingUserTokens = await _outlookTokenInfoRepository.GetAll().Where(x => x.UserId == outlookTokenInfoDto.UserId).ToListAsync();

                    if (existingUserTokens != null)
                    {
                        foreach (var existtoken in existingUserTokens)
                        {
                            await _outlookTokenInfoRepository.DeleteAsync(existtoken.Id);
                        }
                    }

                    var outlookTokenInfo = ObjectMapper.Map<OutlookTokenInfo>(outlookTokenInfoDto);

                    if (AbpSession.TenantId != null)
                    {
                        outlookTokenInfo.TenantId = (int?)AbpSession.TenantId;
                    }
                    outlookTokenInfo.UserId = outlookTokenInfoDto.UserId;

                    await _outlookTokenInfoRepository.InsertAsync(outlookTokenInfo);
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return false;
        }

        public async Task<bool> CreateEvents(OutlookEventWithUserInputDto eventInputDto)
        {
            try
            {
                var email = _outlookTokenInfoRepository.GetAllIncluding(x => x.UserFk).Where(x => x.UserFk.EmailAddress == eventInputDto.Email || x.UserId == eventInputDto.UserId).Select(s=>s.Email).FirstOrDefault();
                if (string.IsNullOrEmpty(email))
                {
                    return false;
                }

                string accessToken = await GetAccessToken(email);
                if (string.IsNullOrEmpty(accessToken))
                {
                    return false;
                }
                if (string.IsNullOrEmpty(CalSettingsValue?.OutlookEventEndPoint))
                {
                    await GetAllConfiguration();
                }
                var client = new RestClient(CalSettingsValue?.OutlookEventEndPoint);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Authorization", $"Bearer {accessToken}");
                request.AddHeader("Content-Type", "application/json");

                // Saving Event to Outlook Calendar
                request.AddParameter("application/json", JsonConvert.SerializeObject(eventInputDto.OutlookEventInput), ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    var result = JsonConvert.DeserializeObject<OutlookCalendarEventItem>(response.Content);

                    if (!string.IsNullOrEmpty(result?.id))
                    {
                        ServiceCalendarEventDto serviceCalendar = new ServiceCalendarEventDto();
                        serviceCalendar.CalendarType = CalendarServiceProviderType.Outlook;
                        serviceCalendar.SystemEventId = eventInputDto.SystemEventID;
                        serviceCalendar.ServiceProviderEventId = result.id;
                        serviceCalendar.ServiceProviderEventDetails = JsonConvert.SerializeObject(result);
                        serviceCalendar.UserId = eventInputDto.UserId;
                        await _serviceCalendarAppService.CreateOrUpdateServiceCalendarEvent(serviceCalendar);
                        return true;
                    }
                }
                else
                {
                    throw new Exception(response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return false;
        }

        public async Task<bool> UpdateEventOccurrence(OutlookEventWithUserInputDto calendarInputDto, DateTime searchTime)
        {
            try
            {
                List<ServiceCalendarEventDto> serviceCalendars = await _serviceCalendarAppService.GetServiceCalendarEventBySystemEventId(calendarInputDto.SystemEventID);
                var outlookServiceEvent = serviceCalendars?.FirstOrDefault(sc => sc.CalendarType == CalendarServiceProviderType.Outlook);
                if (!(outlookServiceEvent is null))
                {
                    var outlookEventId = outlookServiceEvent.ServiceProviderEventId;
                    if (!string.IsNullOrEmpty(outlookEventId))
                    {
                        var occurrenceId = await GetOutlookEventOccurrenceId(outlookEventId, calendarInputDto.UserId, searchTime);
                        if (string.IsNullOrEmpty(occurrenceId))
                            return false;
                        var email = _outlookTokenInfoRepository.GetAllIncluding(x => x.UserFk).Where(x => x.UserFk.EmailAddress == calendarInputDto.Email || x.UserId == calendarInputDto.UserId).FirstOrDefault()?.Email;
                        if (string.IsNullOrEmpty(email))
                        {
                            return false;
                        }
                        string accessToken = await GetAccessToken(email);
                        if (string.IsNullOrEmpty(accessToken))
                        {
                            return false;
                        }

                        if (string.IsNullOrEmpty(CalSettingsValue?.OutlookEventEndPoint))
                        {
                            await GetAllConfiguration();
                        }
                        var client = new RestClient(CalSettingsValue?.OutlookEventEndPoint);
                        client.Timeout = -1;
                        calendarInputDto.OutlookEventInput.Recurrence = null;
                        var request = new RestRequest($"/{occurrenceId}", Method.PATCH);
                        request.AddHeader("Authorization", $"Bearer {accessToken}");
                        request.AddHeader("Content-Type", "application/json");
                        request.AddParameter("application/json", JsonConvert.SerializeObject(calendarInputDto.OutlookEventInput), ParameterType.RequestBody);
                        IRestResponse response = client.Execute(request);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var result = JsonConvert.DeserializeObject<Event>(response.Content);
                            if (!string.IsNullOrEmpty(result?.Id))
                            {
                                return true;
                            }
                        }
                        else
                        {
                            throw new Exception(response.ErrorMessage);
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> UpdateEventOccurrenceById(OutlookEventWithUserInputDto calendarInputDto)
        {
            try
            {
                var occurrenceId = calendarInputDto.OutlookEventId;
                if (string.IsNullOrEmpty(occurrenceId))
                    return false;
                var email = _outlookTokenInfoRepository.GetAllIncluding(x => x.UserFk).Where(x => x.UserFk.EmailAddress == calendarInputDto.Email || x.UserId == calendarInputDto.UserId).FirstOrDefault()?.Email;
                if (string.IsNullOrEmpty(email))
                {
                    return false;
                }
                string accessToken = await GetAccessToken(email);
                if (string.IsNullOrEmpty(accessToken))
                {
                    return false;
                }

                if (string.IsNullOrEmpty(CalSettingsValue?.OutlookEventEndPoint))
                {
                    await GetAllConfiguration();
                }
                var client = new RestClient(CalSettingsValue?.OutlookEventEndPoint);
                client.Timeout = -1;
                calendarInputDto.OutlookEventInput.Recurrence = null;
                var request = new RestRequest($"/{occurrenceId}", Method.PATCH);
                request.AddHeader("Authorization", $"Bearer {accessToken}");
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", JsonConvert.SerializeObject(calendarInputDto.OutlookEventInput), ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = JsonConvert.DeserializeObject<Event>(response.Content);
                    if (!string.IsNullOrEmpty(result?.Id))
                    {
                        return true;
                    }
                }
                else
                {
                    throw new Exception(response.ErrorMessage);
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<bool> UpdateEventSeries(OutlookEventWithUserInputDto calendarInputDto)
        {
            try
            {
                List<ServiceCalendarEventDto> serviceCalendars = await _serviceCalendarAppService.GetServiceCalendarEventBySystemEventId(calendarInputDto.SystemEventID);
                var outlookServiceEvent = serviceCalendars?.FirstOrDefault(sc => sc.CalendarType == CalendarServiceProviderType.Outlook);
                if (!(outlookServiceEvent is null) && outlookServiceEvent.CreatorUserId == calendarInputDto.UserId)
                {
                    var email = _outlookTokenInfoRepository.GetAllIncluding(x => x.UserFk).Where(x => x.UserFk.EmailAddress == calendarInputDto.Email || x.UserId == calendarInputDto.UserId).FirstOrDefault()?.Email;
                    if (string.IsNullOrEmpty(email))
                    {
                        return false;
                    }

                    string accessToken = await GetAccessToken(email);
                    if (string.IsNullOrEmpty(accessToken))
                    {
                        return false;
                    }

                    if (string.IsNullOrEmpty(CalSettingsValue?.OutlookEventEndPoint))
                    {
                        await GetAllConfiguration();
                    }

                    var client = new RestClient(CalSettingsValue?.OutlookEventEndPoint);
                    client.Timeout = -1;
                    var outlookEventId = outlookServiceEvent.ServiceProviderEventId;
                    var request = new RestRequest($"/{outlookEventId}", Method.PATCH);
                    request.AddHeader("Authorization", $"Bearer {accessToken}");
                    request.AddHeader("Content-Type", "application/json");
                    request.AddParameter("application/json", JsonConvert.SerializeObject(calendarInputDto.OutlookEventInput), ParameterType.RequestBody);
                    IRestResponse response = client.Execute(request);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var result = JsonConvert.DeserializeObject<Event>(response.Content);
                        if (!string.IsNullOrEmpty(result?.Id))
                        {
                            ServiceCalendarEventDto serviceCalendar = new ServiceCalendarEventDto();
                            serviceCalendar.CalendarType = CalendarServiceProviderType.Outlook;
                            serviceCalendar.SystemEventId = calendarInputDto.SystemEventID;
                            serviceCalendar.ServiceProviderEventId = result?.Id; //outlookServiceEvent.ServiceProviderEventId;
                            serviceCalendar.ServiceProviderEventDetails = JsonConvert.SerializeObject(result);
                            serviceCalendar.UserId = calendarInputDto.UserId;
                            await _serviceCalendarAppService.CreateOrUpdateServiceCalendarEvent(serviceCalendar);
                            return true;
                        }
                    }
                    else
                    {
                        throw new Exception(response.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return false;
        }

        [HttpPost]
        [UnitOfWork(isTransactional: false)]
        public async Task<bool> DeleteOutlookEventBySystemEventId(EventDeleteInputDto deleteInputDto)
        {
            try
            {
                List<ServiceCalendarEventDto> serviceCalendars = await _serviceCalendarAppService.GetServiceCalendarEventBySystemEventId(deleteInputDto.SystemEventID);
                var outlookServiceEvent = serviceCalendars?.WhereIf(deleteInputDto.UserId > 0, x => x.UserId == deleteInputDto.UserId)
                    .FirstOrDefault(sc => sc.CalendarType == CalendarServiceProviderType.Outlook);
                if (!(outlookServiceEvent is null))
                {
                    var email = _outlookTokenInfoRepository.GetAllIncluding(x => x.UserFk).Where(x => x.UserFk.EmailAddress == deleteInputDto.Email || x.UserId == deleteInputDto.UserId).FirstOrDefault()?.Email;
                    if (string.IsNullOrEmpty(email))
                    {
                        return false;
                    }

                    string accessToken = await GetAccessToken(email);
                    if (string.IsNullOrEmpty(accessToken))
                    {
                        return false;
                    }
                    if (string.IsNullOrEmpty(CalSettingsValue?.OutlookEventEndPoint))
                    {
                        await GetAllConfiguration();
                    }
                    var client = new RestClient(CalSettingsValue?.OutlookEventEndPoint);
                    client.Timeout = -1;
                    var outlookEventId = outlookServiceEvent.ServiceProviderEventId;
                    var request = new RestRequest($"/{outlookEventId}", Method.DELETE);
                    request.AddHeader("Authorization", $"Bearer {accessToken}");
                    IRestResponse response = client.Execute(request);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.Gone || response.StatusCode == System.Net.HttpStatusCode.NotFound || response.StatusCode == System.Net.HttpStatusCode.NoContent)
                    {
                        await _serviceCalendarAppService.DeleteServiceCalendarEventByProviderEventID(outlookServiceEvent.ServiceProviderEventId);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return false;
        }

        public async Task<bool> UpdateOutlookEvent(OutlookEventWithUserInputDto calendarInputDto)
        {
            try
            {
                if (!string.IsNullOrEmpty(calendarInputDto?.OutlookEventId))
                {
                    var email = _outlookTokenInfoRepository.GetAllIncluding(x => x.UserFk).Where(x => x.UserFk.EmailAddress == calendarInputDto.Email || x.UserId == calendarInputDto.UserId).FirstOrDefault()?.Email;
                    if (string.IsNullOrEmpty(email))
                    {
                        return false;
                    }

                    string accessToken = await GetAccessToken(email);
                    if (string.IsNullOrEmpty(accessToken))
                    {
                        return false;
                    }
                    if (string.IsNullOrEmpty(CalSettingsValue?.OutlookEventEndPoint))
                    {
                        await GetAllConfiguration();
                    }
                    var client = new RestClient(CalSettingsValue?.OutlookEventEndPoint);
                    client.Timeout = -1;
                    var outlookEventId = calendarInputDto?.OutlookEventId;
                    var request = new RestRequest($"/{outlookEventId}", Method.PATCH);
                    request.AddHeader("Authorization", $"Bearer {accessToken}");
                    request.AddHeader("Content-Type", "application/json");
                    request.AddParameter("application/json", JsonConvert.SerializeObject(calendarInputDto.OutlookEventInput), ParameterType.RequestBody);
                    IRestResponse response = client.Execute(request);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return true;
                    }
                    else
                    {
                        throw new Exception(response.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return false;
        }

        public async Task<bool> DeleteOutlookEventById(EventDeleteInputDto deleteInputDto)
        {
            try
            {
                if (!string.IsNullOrEmpty(deleteInputDto?.ProviderEventID))
                {
                    var email = _outlookTokenInfoRepository.GetAllIncluding(x => x.UserFk).Where(x => x.UserFk.EmailAddress == deleteInputDto.Email || x.UserId == deleteInputDto.UserId).FirstOrDefault()?.Email;
                    if (string.IsNullOrEmpty(email))
                    {
                        return false;
                    }

                    string accessToken = await GetAccessToken(email);
                    if (string.IsNullOrEmpty(accessToken))
                    {
                        return false;
                    }
                    if (string.IsNullOrEmpty(CalSettingsValue?.OutlookEventEndPoint))
                    {
                        await GetAllConfiguration();
                    }
                    var client = new RestClient(CalSettingsValue?.OutlookEventEndPoint);
                    client.Timeout = -1;
                    var outlookEventId = deleteInputDto?.ProviderEventID;
                    var request = new RestRequest($"/{outlookEventId}", Method.DELETE);
                    request.AddHeader("Authorization", $"Bearer {accessToken}");
                    IRestResponse response = client.Execute(request);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.Gone || response.StatusCode == System.Net.HttpStatusCode.NotFound || response.StatusCode == System.Net.HttpStatusCode.NoContent)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return false;
        }

        [HttpPost]
        [UnitOfWork(isTransactional: false)]
        public async Task<bool> DeleteEventOccurrenceSystemEventId(EventDeleteInputDto deleteInputDto, DateTime searchTime)
        {
            try
            {
                List<ServiceCalendarEventDto> serviceCalendars = await _serviceCalendarAppService.GetServiceCalendarEventBySystemEventId(deleteInputDto.SystemEventID);
                var outlookServiceEvent = serviceCalendars?.WhereIf(deleteInputDto.UserId > 0, x => x.UserId == deleteInputDto.UserId)
                    .FirstOrDefault(sc => sc.CalendarType == CalendarServiceProviderType.Outlook);
                if (!(outlookServiceEvent is null))
                {
                    var outlookEventId = outlookServiceEvent.ServiceProviderEventId;
                    var occurrenceId = await GetOutlookEventOccurrenceId(outlookEventId, deleteInputDto.UserId, searchTime);
                    if (string.IsNullOrEmpty(occurrenceId))
                        return false;
                    var email = _outlookTokenInfoRepository.GetAllIncluding(x => x.UserFk).Where(x => x.UserFk.EmailAddress == deleteInputDto.Email || x.UserId == deleteInputDto.UserId).FirstOrDefault()?.Email;
                    if (string.IsNullOrEmpty(email))
                    {
                        return false;
                    }

                    string accessToken = await GetAccessToken(email);
                    if (string.IsNullOrEmpty(accessToken))
                    {
                        return false;
                    }
                    if (string.IsNullOrEmpty(CalSettingsValue?.OutlookEventEndPoint))
                    {
                        await GetAllConfiguration();
                    }
                    var client = new RestClient(CalSettingsValue?.OutlookEventEndPoint);
                    client.Timeout = -1;
                    var request = new RestRequest($"/{occurrenceId}", Method.DELETE);
                    request.AddHeader("Authorization", $"Bearer {accessToken}");
                    IRestResponse response = client.Execute(request);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.Gone || response.StatusCode == System.Net.HttpStatusCode.NotFound || response.StatusCode == System.Net.HttpStatusCode.NoContent)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return false;
        }

        [HttpPost]
        [UnitOfWork(isTransactional: false)]
        public async Task<bool> DeleteEventOccurrenceById(EventDeleteInputDto deleteInputDto)
        {
            try
            {
                var occurrenceId = deleteInputDto.ProviderEventID;
                if (string.IsNullOrEmpty(occurrenceId))
                    return false;
                var email = _outlookTokenInfoRepository.GetAllIncluding(x => x.UserFk).Where(x => x.UserFk.EmailAddress == deleteInputDto.Email || x.UserId == deleteInputDto.UserId).FirstOrDefault()?.Email;
                if (string.IsNullOrEmpty(email))
                {
                    return false;
                }

                string accessToken = await GetAccessToken(email);
                if (string.IsNullOrEmpty(accessToken))
                {
                    return false;
                }
                if (string.IsNullOrEmpty(CalSettingsValue?.OutlookEventEndPoint))
                {
                    await GetAllConfiguration();
                }
                var client = new RestClient(CalSettingsValue?.OutlookEventEndPoint);
                client.Timeout = -1;
                var request = new RestRequest($"/{occurrenceId}", Method.DELETE);
                request.AddHeader("Authorization", $"Bearer {accessToken}");
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.Gone || response.StatusCode == System.Net.HttpStatusCode.NotFound || response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return false;
        }

        public async Task<bool> FetchAndSaveEventsInUserTable(long userID)
        {
            try
            {
                var email = (await _outlookTokenInfoRepository.FirstOrDefaultAsync(x => x.UserId == userID))?.Email;

                if (string.IsNullOrEmpty(email))
                {
                    return false;
                }
                var result = await ExecuteOutlookCalendarApi(email, userID);
                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<ListResultDto<OutlookCalendarEventsOutputDto>> GetOutlookCalendarEventsByUserId(long UserId, DateTime? searchTime)
        {
            try
            {
                var email = (await _outlookTokenInfoRepository.FirstOrDefaultAsync(x => x.UserId == UserId))?.Email;

                string accessToken = await GetAccessToken(email);
                if (string.IsNullOrEmpty(accessToken))
                {
                    return null;
                }
                if (string.IsNullOrEmpty(CalSettingsValue?.OutlookEventEndPoint))
                {
                    await GetAllConfiguration();
                }
                DateTime searchMinTime = DateTime.UtcNow;
                DateTime searchMaxTime = DateTime.UtcNow;
                if (searchTime.HasValue)
                {
                    searchMinTime = searchTime.Value.ToUniversalTime();
                    searchMaxTime = searchMinTime.AddDays(31);
                }
                else
                {
                    searchMinTime = searchTime.Value.ToUniversalTime();
                    searchMaxTime = searchMinTime.AddDays(31);
                }
                var client = new RestClient(_appConfiguration["OutlookEventApi:V1CalendarView"]);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", $"Bearer {accessToken}");
                request.AddParameter("startDateTime", searchMinTime.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                request.AddParameter("endDateTime", searchMaxTime.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                request.AddParameter("top", MaxEventCount);
                IRestResponse response = await client.ExecuteAsync(request);
                var result = JsonConvert.DeserializeObject<CalendarOutlookOutputDto>(response.Content);
                if (result != null)
                {
                    return new ListResultDto<OutlookCalendarEventsOutputDto>(result.value);
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        [HttpPost]
        [UnitOfWork(isTransactional: false)]
        public async Task<List<GetOutlookDeltaEventDto>> GetAllTeamOutlookDeltaEvents(List<long> userIdList, DateTime startSearchDate, DateTime endSearchDate, bool needManhattanCopy)
        {
            try
            {
                var result = new List<GetOutlookDeltaEventDto>();
                foreach (var userId in userIdList)
                {
                    result.Add(await GetAllOutlookDeltaEvents(userId, startSearchDate, endSearchDate, needManhattanCopy));
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<GetOutlookDeltaEventDto> GetAllOutlookDeltaEvents(long userId, DateTime startSearchDate, DateTime endSearchDate, bool needManhattanCopy)
        {
            try
            {
                var email = (await _outlookTokenInfoRepository.FirstOrDefaultAsync(x => x.UserId == userId))?.Email;
                if (string.IsNullOrEmpty(email)) { return null; }
                string accessToken = await GetAccessToken(email);
                if (string.IsNullOrEmpty(accessToken))
                {
                    return null;
                }
                if (string.IsNullOrEmpty(CalSettingsValue?.OutlookEventEndPoint))
                {
                    await GetAllConfiguration();
                }
                var client = new RestClient(_appConfiguration["OutlookEventApi:V1CalendarView"]);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", $"Bearer {accessToken}");
                request.AddParameter("startDateTime", startSearchDate.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                request.AddParameter("endDateTime", endSearchDate.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                request.AddParameter("top", MaxEventCount);
                IRestResponse response = await client.ExecuteAsync(request);
                var result = JsonConvert.DeserializeObject<GetOutlookDeltaEventDto>(response.Content);
                if (result != null)
                {
                    if (!needManhattanCopy)
                    {
                        var filteredList = result?.value?.ToList();
                        foreach (var item in filteredList.ToList())
                        {
                            if (!string.IsNullOrWhiteSpace(item.seriesMasterId))
                            {
                                var isServiceEventExists = await _serviceCalendarAppService.GetServiceCalendarEventByProviderId(item.seriesMasterId);
                                if (isServiceEventExists != null)
                                {
                                    filteredList.Remove(item);
                                }
                            }
                            else
                            {
                                var isServiceEventExists = await _serviceCalendarAppService.GetServiceCalendarEventByProviderId(item.id);
                                if (isServiceEventExists != null)
                                {
                                    filteredList.Remove(item);
                                }
                            }
                        }
                        result.value = filteredList.ToArray();
                    }
                    else
                    {
                        var filteredList = result?.value?.ToList();
                        foreach (var item in filteredList.ToList())
                        {
                            if (!string.IsNullOrWhiteSpace(item.seriesMasterId))
                            {
                                var isServiceEventExists = await _serviceCalendarAppService.GetServiceCalendarEventByProviderId(item.seriesMasterId);
                                if (isServiceEventExists == null)
                                {
                                    filteredList.Remove(item);
                                }
                            }
                            else
                            {
                                var isServiceEventExists = await _serviceCalendarAppService.GetServiceCalendarEventByProviderId(item.id);
                                if (isServiceEventExists == null)
                                {
                                    filteredList.Remove(item);
                                }
                            }
                        }
                        result.value = filteredList.ToArray();
                    }
                    result.UserId = userId;
                    return result;
                }
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public async Task<bool> FetchAndSaveEventsInUserTableByUserIdList(List<long> userIdList)
        {
            try
            {
                foreach (var userID in userIdList)
                {
                    var email = (await _outlookTokenInfoRepository.FirstOrDefaultAsync(x => x.UserId == userID))?.Email;

                    if (!string.IsNullOrEmpty(email))
                    {
                        var result = await ExecuteOutlookCalendarApi(email, userID);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private async Task UpdateUserTokenDetails(string email, OutlookTokenInfo outlookTokenInfo)
        {
            if (!string.IsNullOrEmpty(outlookTokenInfo?.Tokeninfo))
            {
                await _outlookTokenInfoRepository.UpdateAsync(outlookTokenInfo);
            }
        }
        public async Task<string> GetOutlookEventOccurrenceId(string outlookEventId, long userID, DateTime searchTime)
        {
            try
            {
                var email = (await _outlookTokenInfoRepository.FirstOrDefaultAsync(x => x.UserId == userID))?.Email;

                if (string.IsNullOrEmpty(email))
                {
                    return string.Empty;
                }

                string accessToken = await GetAccessToken(email);
                if (string.IsNullOrEmpty(accessToken))
                {
                    return string.Empty;
                }
                if (string.IsNullOrEmpty(CalSettingsValue?.OutlookEventEndPoint))
                {
                    await GetAllConfiguration();
                }

                DateTime searchMinTime = searchTime.ToUniversalTime();
                DateTime searchMaxTime = searchMinTime.AddHours(24);
                var client = new RestClient(_appConfiguration["OutlookEventApi:V1CalendarView"]);
                client.Timeout = -1;
                var request = new RestRequest($"/{outlookEventId}/instances", Method.GET);
                request.AddHeader("Authorization", $"Bearer {accessToken}");
                request.AddParameter("orderby", "start/dateTime asc");
                request.AddParameter("startDateTime", searchMinTime.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                request.AddParameter("endDateTime", searchMaxTime.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                IRestResponse response = await client.ExecuteAsync(request);
                var result = JsonConvert.DeserializeObject<OutlookCalendarEventDto>(response.Content);
                if (result != null)
                {
                    if (result?.value != null && result?.value?.Count > 0)
                    {
                        return result?.value[0].id;
                    }
                }
                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private async Task<bool> ExecuteOutlookCalendarApi(string email, long userID)
        {
            try
            {
                string accessToken = await GetAccessToken(email);
                if (string.IsNullOrEmpty(accessToken))
                {
                    return false;
                }
                if (string.IsNullOrEmpty(CalSettingsValue?.OutlookEventEndPoint))
                {
                    await GetAllConfiguration();
                }
                var client = new RestClient(CalSettingsValue?.OutlookEventEndPoint);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", $"Bearer {accessToken}");
                request.AddParameter("top", MaxEventCount);
                IRestResponse response = await client.ExecuteAsync(request);
                var result = JsonConvert.DeserializeObject<OutlookCalendarEventDto>(response.Content);
                if (result != null)
                {
                    return await _userOutlookCalendarService.SyncOutlookEventsInUserTable(result, userID);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private async Task<OutlookUserInfoDto> GetOutlookUserInfo(string accessToken)
        {
            try
            {
                var client = new RestClient(_appConfiguration["OutlookEventApi:V1BaseUrl"]);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", $"Bearer {accessToken}");
                IRestResponse response = await client.ExecuteAsync(request);
                var result = JsonConvert.DeserializeObject<OutlookUserInfoDto>(response.Content);
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }
        private async Task<string> GetAccessToken(string email)
        {
            var result = await _outlookTokenInfoRepository.FirstOrDefaultAsync(x => x.Email == email);

            if (result is null)
                return string.Empty;

            var tokenInfo = JsonConvert.DeserializeObject<GetTokenInfoFromCodeOutputDto>(result.Tokeninfo);

            if (result.LastRefreshedTime.HasValue && result.LastRefreshedTime.Value.AddMinutes(50) > DateTime.UtcNow && !string.IsNullOrWhiteSpace(tokenInfo?.access_token))
            {
                return tokenInfo.access_token;
            }
            else
            {
                var oldRefreshToken = result.RefreshToken;
                var resultnewTOken = await GetAccessTokenFromRefreshToken(result.RefreshToken);
                tokenInfo = JsonConvert.DeserializeObject<GetTokenInfoFromCodeOutputDto>(resultnewTOken);
                result.Tokeninfo = resultnewTOken;
                if (!string.IsNullOrEmpty(resultnewTOken))
                {
                    result.LastRefreshedTime = DateTime.UtcNow;
                    result.RefreshToken = string.IsNullOrWhiteSpace(result?.RefreshToken) ? oldRefreshToken : result.RefreshToken;
                    await UpdateUserTokenDetails(email, result);
                }
                if (tokenInfo is null)
                {
                    return string.Empty;
                }
            }

            return tokenInfo.access_token;
        }
        private async Task<string> GetAccessTokenFromRefreshToken(string refreshToken)
        {
            try
            {
                if (string.IsNullOrEmpty(CalSettingsValue?.OutlookEventEndPoint))
                {
                    await GetAllConfiguration();
                }
                var client = new RestClient(CalSettingsValue?.OutlookTokenEndpoint);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddParameter("client_id", CalSettingsValue?.OutlookClientId);
                request.AddParameter("client_secret", CalSettingsValue?.OutlookClientSecret);
                request.AddParameter("grant_type", "refresh_token");
                request.AddParameter("refresh_token", refreshToken);
                IRestResponse response = await client.ExecuteAsync(request);
                return response.Content;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        private async Task<GetTokenInfoFromCodeOutputDto> GetTokenInfoFromCode(string code)
        {
            try
            {
                if (string.IsNullOrEmpty(CalSettingsValue?.OutlookEventEndPoint))
                {
                    await GetAllConfiguration();
                }
                var client = new RestClient(CalSettingsValue?.OutlookTokenEndpoint);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddParameter("client_id", CalSettingsValue?.OutlookClientId);
                request.AddParameter("client_secret", CalSettingsValue?.OutlookClientSecret);
                request.AddParameter("code", code);
                request.AddParameter("scope", CalSettingsValue?.OutlookScope);
                request.AddParameter("redirect_uri", CalSettingsValue?.OutlookRedirectURL);
                request.AddParameter("grant_type", "authorization_code");
                IRestResponse response = await client.ExecuteAsync(request);

                var result = JsonConvert.DeserializeObject<GetTokenInfoFromCodeOutputDto>(response.Content);
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<GetOutlookDeltaEventDto> GetAllDeletedOutlookEvents(long userId, DateTime startSearchDate, DateTime endSearchDate)
        {
            try
            {
                var email = (await _outlookTokenInfoRepository.FirstOrDefaultAsync(x => x.UserId == userId))?.Email;
                if (string.IsNullOrEmpty(email)) { return null; }
                string accessToken = await GetAccessToken(email);
                if (string.IsNullOrEmpty(accessToken))
                {
                    return null;
                }
                if (string.IsNullOrEmpty(CalSettingsValue?.OutlookEventEndPoint))
                {
                    await GetAllConfiguration();
                }
                string deltaToken = await GetDeltaToken(email);
                if (string.IsNullOrEmpty(deltaToken))
                {
                    //set deltatoken first time
                    deltaToken = await UpdateDeltaToken(email, null);
                }
                var client = new RestClient(_appConfiguration["OutlookEventApi:V1CalendarViewDelta"]);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", $"Bearer {accessToken}");
                //request.AddParameter("startDateTime", startSearchDate.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                //request.AddParameter("endDateTime", endSearchDate.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                //deltaToken = "R0usmcdvmMuZCBYV0hguCErpBer7CHsreg4dUCXHkayly-BfODKzpy13a3-Bu6-iWxbwQ2XK2pa1ieTtekNYNcNimXw2viN1-AN2pkwE_nFEc3eD70CBIpoXTit9aA5iKXGPliF8AjAiCcCjB5VmuyN1NuIOrCfAapuBALHa2PtuNDtSlD-9oj3vN8W5KHlBf1vPssRWSsialrCqgktOmQ.YvNlw-TR4qvdoYSssSreUI7ode25Lk01evPclWcYsS4";
                request.AddParameter("$deltatoken", deltaToken);
                IRestResponse response = await client.ExecuteAsync(request);
                //to do: update delta token
                response.Content = response.Content.Replace("@removed", "removed");
                var result = JsonConvert.DeserializeObject<GetOutlookDeltaEventDto>(response.Content);
                var newDeltaToken = response.Content.Substring(response.Content.IndexOf("$deltatoken=") + 12);
                newDeltaToken = newDeltaToken.Substring(0, newDeltaToken.IndexOf("\""));
                //to do: update delta token
                await UpdateDeltaToken(email, newDeltaToken);
                if (result != null)
                {
                    var filteredList = result?.value.Where(ev => ev?.removed?.reason == "deleted" || ev?.type == "seriesMaster").ToList();
                    var tempList = new List<GetOutlookDeltaEventDto_Value>();
                    foreach (var item in filteredList.ToList())
                    {
                        if (item?.type == "seriesMaster")
                        {
                            var isServiceEventExists = await _serviceCalendarAppService.GetServiceCalendarEventByProviderId(item.id);
                            if (isServiceEventExists != null)
                            {
                                var betaEventByIdURL = _appConfiguration["OutlookEventApi:BetaEventUrl"];
                                betaEventByIdURL = betaEventByIdURL.Replace("{eventId}", item.id);
                                client = new RestClient(betaEventByIdURL);
                                client.Timeout = -1;
                                request = new RestRequest(Method.GET);
                                request.AddHeader("Authorization", $"Bearer {accessToken}");
                                request.AddParameter("$select", "cancelledOccurrences");
                                response = await client.ExecuteAsync(request);
                                var cancelledEvents = JsonConvert.DeserializeObject<GetOutlookCancelledEvents>(response.Content);
                                foreach (var cancelledItems in cancelledEvents.cancelledOccurrences)
                                {
                                    var start = DateTime.SpecifyKind(DateTime.Parse(cancelledItems.Split(".").Last()), DateTimeKind.Utc);
                                    tempList.Add(new GetOutlookDeltaEventDto_Value { seriesMasterId = cancelledEvents.id, start = new GetOutlookDeltaEventDto_Start { dateTime = start } });
                                }
                            }
                            filteredList.Remove(item);
                        }
                        else
                        {
                            var isServiceEventExists = await _serviceCalendarAppService.GetServiceCalendarEventByProviderId(item.id);
                            if (isServiceEventExists == null)
                            {
                                filteredList.Remove(item);
                            }
                        }
                    }
                    filteredList.AddRange(tempList);
                    result.value = filteredList.ToArray();

                    return result;
                }
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        private async Task<string> GetDeltaToken(string email)
        {
            var result = await _outlookTokenInfoRepository.FirstOrDefaultAsync(x => x.Email == email);

            if (result is null)
                return string.Empty;

            return result.DeltaToken;
        }

        private async Task<string> UpdateDeltaToken(string email, string deltaToken)
        {
            var outlookTokenInfo = await _outlookTokenInfoRepository.FirstOrDefaultAsync(x => x.Email == email);
            if (string.IsNullOrEmpty(deltaToken))
            {
                //first time save
                string accessToken = await GetAccessToken(email);
                if (string.IsNullOrEmpty(accessToken))
                {
                    return null;
                }
                if (string.IsNullOrEmpty(CalSettingsValue?.OutlookEventEndPoint))
                {
                    await GetAllConfiguration();
                }
                var client = new RestClient(_appConfiguration["OutlookEventApi:V1CalendarViewDelta"]);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", $"Bearer {accessToken}");
                request.AddParameter("startDateTime", (new DateTime(2020, 3, 1, 7, 0, 0)).ToString("yyyy-MM-ddTHH:mm:ssZ"));
                request.AddParameter("endDateTime", (new DateTime(2030, 3, 1, 7, 0, 0)).ToString("yyyy-MM-ddTHH:mm:ssZ"));
                IRestResponse response = await client.ExecuteAsync(request);
                while (response.Content.IndexOf("$skiptoken=") != -1)
                {
                    var temp = response.Content.IndexOf("$skiptoken=");
                    var skipToken = response.Content.Substring(response.Content.IndexOf("$skiptoken=") + 11);
                    skipToken = skipToken.Substring(0, skipToken.IndexOf("\""));
                    client = new RestClient(_appConfiguration["OutlookEventApi:V1CalendarViewDelta"]);
                    client.Timeout = -1;
                    request = new RestRequest(Method.GET);
                    request.AddHeader("Authorization", $"Bearer {accessToken}");
                    request.AddParameter("$skipToken", skipToken);
                    response = await client.ExecuteAsync(request);
                }
                var newDeltaToken = response.Content.Substring(response.Content.IndexOf("$deltatoken=") + 12);
                deltaToken = newDeltaToken.Substring(0, newDeltaToken.IndexOf("\""));
                outlookTokenInfo.DeltaToken = deltaToken;
                await _outlookTokenInfoRepository.UpdateAsync(outlookTokenInfo);

            }
            else
            {
                //update new
                outlookTokenInfo.DeltaToken = deltaToken;
                await _outlookTokenInfoRepository.UpdateAsync(outlookTokenInfo);
            }

            return deltaToken;

        }
    }
}
