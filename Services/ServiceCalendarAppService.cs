
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;
using CalendarSyncPOC;
using CalendarSync.SystemCalendar.Dto;
using SIRVA.Moving.Sales.SystemCalendar;

namespace CalendarSync.SystemCalendar
{
    public class ServiceCalendarAppService: IServiceCalendarAppService
    {
        private readonly DbContextAzure dbContext;

        public ServiceCalendarAppService(DbContextAzure dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<long> CreateOrUpdateServiceCalendarEvent(ServiceCalendarEventDto input)
        {
            try
            {
                long serviceEventId = 0;
                var serviceCalendar = await dbContext.ServiceCalendarEvent.FirstOrDefaultAsync(x => x.CalendarType == input.CalendarType && x.SystemEventId == input.SystemEventId);
                if (serviceCalendar is null)
                {
                    var serviceEvent = new ServiceCalendarEvent { 
                                        SystemEventId = input.SystemEventId,
                                        UserId = input.UserId,
                                        CalendarType = input.CalendarType,
                                        CreationTime = input.CreationTime,
                                        CreatorUserId = input.CreatorUserId,
                                        ServiceProviderEventDetails = input.ServiceProviderEventDetails,
                                        ServiceProviderEventId = input.ServiceProviderEventId};
                    await dbContext.ServiceCalendarEvent.AddAsync(serviceEvent);
                    await dbContext.SaveChangesAsync();
                    serviceEventId = serviceEvent.Id;
                }
                else
                {
                    serviceCalendar.ServiceProviderEventId = input.ServiceProviderEventId;
                    serviceCalendar.ServiceProviderEventDetails = input.ServiceProviderEventDetails;
                    serviceCalendar.UserId = input.UserId;
                    await dbContext.SaveChangesAsync();
                    serviceEventId = serviceCalendar.Id;
                }
                return serviceEventId;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task AddOrUpdateServiceProviderEvents(object events, long userId)
        {
            try
            {
                // the object is google event
                if (events is Events)
                {
                    var eventLists = events as Events;
                    foreach (Event eventItem in eventLists?.Items)
                    {
                        var serviceCalendarItem = new ServiceCalendarEventDto();
                        serviceCalendarItem.ServiceProviderEventId = eventItem.Id;
                        serviceCalendarItem.CalendarType = CalendarServiceProviderType.Google;
                        serviceCalendarItem.UserId = userId;
                        serviceCalendarItem.ServiceProviderEventDetails = JsonSerializer.Serialize(eventItem);

                        await AddorUpdateServiceCalendar(serviceCalendarItem);
                    }
                }
                // the object is microsoft event
                else if (events is List<Microsoft.Graph.Event>)
                {
                    var eventLists = events as List<Microsoft.Graph.Event>;
                    foreach (Microsoft.Graph.Event eventItem in eventLists)
                    {
                        var serviceCalendarItem = new ServiceCalendarEventDto();
                        serviceCalendarItem.ServiceProviderEventId = eventItem.Id;
                        serviceCalendarItem.CalendarType = CalendarServiceProviderType.Outlook;
                        serviceCalendarItem.UserId = userId;
                        serviceCalendarItem.ServiceProviderEventDetails = JsonSerializer.Serialize(eventItem);

                        await AddorUpdateServiceCalendar(serviceCalendarItem);
                    }
                }

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task DeleteServiceCalendarEventByProviderEventID(string providerEventId)
        {
            try
            {
                await _serviceEventRepository.DeleteAsync(p => p.ServiceProviderEventId == providerEventId);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public async Task DeleteServiceCalendarEventBySystemEventID(long systemEventId)
        {
            try
            {
                var query = await _serviceEventRepository.GetAllListAsync();
                var serviceEventList = await Task.FromResult(query?.Where(x => x.SystemEventId == systemEventId));
                foreach (var item in serviceEventList)
                {
                    await _serviceEventRepository.DeleteAsync(p => p.Id == item.Id);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<List<ServiceCalendarEventDto>> GetAllServiceCalendarEvents()
        {
            try
            {
                var serviceEventList = await _serviceEventRepository.GetAllListAsync();

                var totalCount = await Task.FromResult(serviceEventList.Count());

                return ObjectMapper.Map<List<ServiceCalendarEventDto>>(serviceEventList);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<ServiceCalendarEventDto> GetServiceCalendarEventById(long serviceEventId)
        {
            try
            {
                var serviceEvent = await _serviceEventRepository.FirstOrDefaultAsync(serviceEventId);
                return ObjectMapper.Map<ServiceCalendarEventDto>(serviceEvent);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<List<ServiceCalendarEventDto>> GetServiceCalendarEventBySystemEventId(long systemEventId)
        {
            try
            {
                var serviceEventList = await _serviceEventRepository.GetAll().Where(x => x.SystemEventId == systemEventId).ToListAsync();
                if (serviceEventList != null)
                {
                    return ObjectMapper.Map<List<ServiceCalendarEventDto>>(serviceEventList);
                }
                return new List<ServiceCalendarEventDto>();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<ServiceCalendarEventDto>> GetServiceCalendarEventByUserId(long userId)
        {
            try
            {
                var serviceEventList = await _serviceEventRepository.GetAll().Where(x => x.UserId == userId).ToListAsync();
                if (serviceEventList != null)
                {
                    return ObjectMapper.Map<List<ServiceCalendarEventDto>>(serviceEventList);
                }
                return new List<ServiceCalendarEventDto>();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<ServiceCalendarEventDto>> GetServiceCalendarEventByProvider(CalendarServiceProviderType calType)
        {
            try
            {
                var serviceEventList = await _serviceEventRepository.GetAll().Where(x => x.CalendarType == calType).ToListAsync();
                if (serviceEventList != null)
                {
                    return ObjectMapper.Map<List<ServiceCalendarEventDto>>(serviceEventList);
                }
                return new List<ServiceCalendarEventDto>();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ServiceCalendarEventDto> GetServiceCalendarEventByProviderId(string providerId)
        {
            try
            {
                var serviceEvent = await _serviceEventRepository.FirstOrDefaultAsync(x => x.ServiceProviderEventId == providerId);
                if (serviceEvent != null)
                {
                    return ObjectMapper.Map<ServiceCalendarEventDto>(serviceEvent);
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> CheckIfRecurringEventExistInServiceCalendar(string providerId, long userId, CalendarServiceProviderType calType)
        {
            try
            {
                ServiceCalendarEvent serviceEvent = new ServiceCalendarEvent();
                var serviceEventAll = await _serviceEventRepository.GetAll().Where(x => x.UserId == userId).ToListAsync();
                if(calType == CalendarServiceProviderType.Google)
                {
                    serviceEvent = serviceEventAll.FirstOrDefault(x => string.Concat(x.ServiceProviderEventId.Trim().TakeWhile((c) => c != '_')) == providerId.Trim());
                }
                else
                {
                    serviceEvent = serviceEventAll.FirstOrDefault(x => x.ServiceProviderEventId == providerId);
                }

                //var serviceEvent = await _serviceEventRepository.GetAll()
                //    .Where(x => string.Concat(x.ServiceProviderEventId.Trim().TakeWhile((c) => c != '_')) == providerId.Trim())
                //    .FirstOrDefaultAsync();
                if (serviceEvent != null)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<ServiceCalendarEventDto>> GetServiceCalendarEventByUserIdAndProvider(long userId, CalendarServiceProviderType calType)
        {
            try
            {
                var serviceEventList = await _serviceEventRepository.GetAll().Where(x => x.UserId == userId && x.CalendarType == calType).ToListAsync();
                if (serviceEventList != null)
                {
                    return ObjectMapper.Map<List<ServiceCalendarEventDto>>(serviceEventList);
                }
                return new List<ServiceCalendarEventDto>();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task AddorUpdateServiceCalendar(ServiceCalendarEventDto serviceCalendarItem)
        {
            try
            {
                var query = await _serviceEventRepository.FirstOrDefaultAsync(x => x.ServiceProviderEventId == serviceCalendarItem.ServiceProviderEventId);
                if (query is null)
                {
                    var mappedEvent = ObjectMapper.Map<ServiceCalendarEvent>(serviceCalendarItem);
                    await _serviceEventRepository.InsertAsync(mappedEvent);
                }
                else
                {
                    ObjectMapper.Map(serviceCalendarItem, query);
                    await _serviceEventRepository.UpdateAsync(query);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
