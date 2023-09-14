using Abp.AspNetCore.SignalR.Hubs;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.UI;
using AutoMapper.Mappers;
using Castle.Core;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Timesheet.DomainServices;
using Timesheet.Extension;
using Timesheet.Hubs.Dto;

namespace Timesheet.Hubs
{
    public class TimekeepingHub : AbpHubBase, ISingletonDependency
    {
        private readonly ITimekeepingServices _timekeepingServices;
        private static Dictionary<string, HashSet<string>> groupWithConnectionIds = new Dictionary<string, HashSet<string>>();
        private static HashSet<DateTime> isProcessing = new HashSet<DateTime>();
        private static HashSet<HubConnectionDto> hubConnectionList = new HashSet<HubConnectionDto>();
        public TimekeepingHub(ITimekeepingServices timekeepingServices)
        {
            _timekeepingServices = timekeepingServices;
        }
        public async System.Threading.Tasks.Task SyncData(DateTime date)
        {

            await Groups.AddToGroupAsync(Context.ConnectionId, date.ToString("dd/MM/yyyy"));
            if (groupWithConnectionIds.ContainsKey(date.ToString("dd/MM/yyyy")))
            {
                if (!groupWithConnectionIds[date.ToString("dd/MM/yyyy")].Contains(Context.ConnectionId))
                {
                    groupWithConnectionIds[date.ToString("dd/MM/yyyy")].Add(Context.ConnectionId);
                }
            }
            else
            {
                groupWithConnectionIds.Add(date.ToString("dd/MM/yyyy"), new HashSet<string> { Context.ConnectionId });
            }

            await Clients.Caller.SendAsync("sentRequestSuccess", date.ToString("dd/MM/yyyy"));

            if (!isProcessing.Contains(date))
            {
                isProcessing.Add(date);
                _ = System.Threading.Tasks.Task.Run(() => AddTimekeepingByDayCallBack(date));
            }
        }

        public async System.Threading.Tasks.Task AddTimekeepingByDayCallBack(DateTime date)
        {
            var groupName = date.ToString("dd/MM/yyyy");
            try
            {
                await _timekeepingServices.AddTimekeepingByDay(date);
                await Clients.Group(groupName).SendAsync("syncDataSuccess", groupName);
            }
            catch (Exception e)
            {
                await Clients.Group(groupName).SendAsync("syncDataFailed", groupName);
            }

            isProcessing.Remove(date);

            if (groupWithConnectionIds.ContainsKey(groupName))
            {
                foreach (var connectionId in groupWithConnectionIds[groupName])
                {
                    await Groups.RemoveFromGroupAsync(connectionId, groupName);
                }

                groupWithConnectionIds.Remove(groupName);

            }

        }

        public async override System.Threading.Tasks.Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            var currentUser = AbpSession.UserId.Value;
            HubConnectionDto hubConnection = null;
            hubConnection = hubConnectionList.FirstOrDefault(s => s.UserId == currentUser);
            var listDaysProcessing = new List<string>();

            try
            {
                if (hubConnection != null)
                {
                    hubConnection.IsConnected = true;
                    var oldConnectionId = hubConnection.ConnectionId;
                    try
                    {
                        foreach (KeyValuePair<string, HashSet<string>> pair in groupWithConnectionIds)
                        {
                            if (pair.Value.Contains(oldConnectionId))
                            {
                                listDaysProcessing.Add(pair.Key);
                                pair.Value.Add(Context.ConnectionId);
                                pair.Value.Remove(oldConnectionId);
                                await Groups.AddToGroupAsync(Context.ConnectionId, pair.Key);
                                // If user connects in multiple tabs/browser
                                // await Groups.RemoveFromGroupAsync(oldConnectionId, pair.Key);
                            }

                        }
                        hubConnection.ConnectionId = Context.ConnectionId;

                    }
                    catch (Exception e)
                    {
                        Logger.Error("Cannot find any existing connection when reconnect!");
                    }
                    await Clients.Caller.SendAsync("connectedSuccess", listDaysProcessing);
                }
                else
                {
                    hubConnectionList.Add(new HubConnectionDto { UserId = currentUser, ConnectionId = Context.ConnectionId, IsConnected = true });
                    await Clients.Caller.SendAsync("connectedSuccess", listDaysProcessing);
                }
            }
            catch (Exception e)
            {
                await Clients.Caller.SendAsync("connectedSuccess", listDaysProcessing);
            }
            Logger.Debug("A client connected to MyChatHub: " + Context.ConnectionId);
        }
        public async override System.Threading.Tasks.Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
            Logger.Debug("A client disconnected from MyChatHub: " + Context.ConnectionId);
        }
    }
}
