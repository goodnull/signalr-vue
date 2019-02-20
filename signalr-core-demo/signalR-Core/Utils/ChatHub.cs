using Microsoft.AspNetCore.SignalR;
using signalR_Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace signalR_Core.Utils
{
    public class ChatHub : Hub
    {
        Random ran = new Random();
        /// <summary>
        /// 连接时
        /// </summary>
        /// <returns></returns>
        //public override async Task OnConnectedAsync()
        public async Task Connected(string name)
        {
            //var name = RandomChinese.GetRandomChinese(ran.Next(3, 5));
            //提示所有人
            await Clients.All.SendAsync("Send", new Message
            {
                type = MsgType.AddUser,
                data = new { key = Context.ConnectionId, value = name }
            });
            //自己获得用户列表
            await Clients.Caller.SendAsync("List", new Message
            {
                type = MsgType.GetUserList,
                data = UserListHandler.GetInstance().Where(a => a.Key != Context.ConnectionId).Select(a => new { key = a.Key, value = a.Value }).ToList()
            });

            //自己获得群列表
            await Clients.Caller.SendAsync("GroupList", new Message
            {
                type = MsgType.GroupList,
                data = GroupListHandler.GetInstance().Keys.ToList()
            });


            await Clients.Caller.SendAsync("List", new Message
            {
                type = MsgType.CallerName,
                data = new { key = Context.ConnectionId, value = name }
            });
            //更新其他人用户列表
            await Clients.Others.SendAsync("List", new Message
            {
                type = MsgType.OtherUser,
                data = new { key = Context.ConnectionId, value = name }
            });


            //加到用户列表
            UserListHandler.AddConnectedId(Context.ConnectionId, name);
            //await base.OnConnectedAsync();
        }

        /// <summary>
        /// 断开时
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public override async Task OnDisconnectedAsync(Exception ex)
        {
            var user = UserListHandler.GetInstance().Where(a => a.Key == Context.ConnectionId).ToList()[0];
            //var data = user.Select(a => new { key = a.Key, value = a.Value }).ToList()[0];

            //从群列表中移除该成员，如果该群中人数人为0，则移除该群
            GroupListHandler.RemoveConnectedId(Context.ConnectionId, out var listgroup, out var list);
            foreach (var item in list)//通知群成员已退出群
            {
                await Clients.Group(item).SendAsync("SendGroup", new Message
                {
                    type = MsgType.GroupUserLeave,
                    data = new { key = Context.ConnectionId, value = user.Value, group = item }
                });
            }
            foreach (var item in listgroup)//通知所有人 群已解散
            {
                await Clients.All.SendAsync("GroupList", new Message
                {
                    type = MsgType.GroupRemove,
                    data = item
                });
            }

            //列表下线
            await Clients.All.SendAsync("List", new Message
            {
                type = MsgType.RemoveUser,
                data = new { key = user.Key, value = user.Value }
            });
            //提示下线
            await Clients.All.SendAsync("Send", new Message
            {
                type = MsgType.RemoveInfo,
                data = new { key = user.Key, value = user.Value }
            });
            UserListHandler.GetInstance().Remove(Context.ConnectionId);
            await base.OnDisconnectedAsync(ex);
        }

        /// <summary>
        /// 发送公共信息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task Send(string message)
        {
            var Value = UserListHandler.GetInstance().Where(a => a.Key == Context.ConnectionId).ToList()[0].Value;
            await Clients.All.SendAsync("Send", new Message
            {
                type = MsgType.GetAllMessage,
                data = new { key = Context.ConnectionId, value = Value, msg = message }
            });
        }
        /// <summary>
        /// 发送给特定的人
        /// </summary>
        /// <param name="key"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendPrivate(string key, string message)
        {
            var from = UserListHandler.GetInstance().Where(a => a.Key == Context.ConnectionId).ToList()[0];
            var go = UserListHandler.GetInstance().Where(a => a.Key == key).ToList()[0];
            //提示自己
            await Clients.Caller.SendAsync("SendUser", new Message
            {
                type = MsgType.CallerPrivateMessage,
                data = new { key = Context.ConnectionId, value = go.Value, msg = message, to = from }
            });
            //发送给指定的人
            await Clients.Client(key).SendAsync("SendUser", new Message
            {
                type = MsgType.GetPrivateMessage,
                data = new { key = Context.ConnectionId, value = from.Value, msg = message, to = go }
            });

        }
        /// <summary>
        /// 发送群组信息
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task SendToGroup(string groupName, string message)
        {
            var from = UserListHandler.GetInstance().Where(a => a.Key == Context.ConnectionId).ToList()[0];

            return Clients.Group(groupName).SendAsync("SendGroup", new Message
            {
                type = MsgType.GetGroupMessage,
                data = new { key = Context.ConnectionId, value = from.Value, group = groupName, msg = message }
            });
        }

        /// <summary>
        /// 加入群组
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public async Task JoinGroup(string groupName)
        {
            int i = GroupListHandler.AddConnectedId(Context.ConnectionId, groupName);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            if (i != 0)
            {
                var from = UserListHandler.GetInstance().Where(a => a.Key == Context.ConnectionId).ToList()[0];
                //提醒有人加入群聊
                await Clients.Group(groupName).SendAsync("SendGroup", new Message
                {
                    type = MsgType.GroupUserJoin,
                    data = new { key = Context.ConnectionId, value = from.Value, group = groupName }
                });
            }
            if (i == 2)//新增群
            {
                await Clients.All.SendAsync("GroupList", new Message
                {
                    type = MsgType.GroupAdd,
                    data = groupName
                });
            }
        }

        /// <summary>
        /// 离开群组
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public async Task LeaveGroup(string groupName)
        {
            var from = UserListHandler.GetInstance().Where(a => a.Key == Context.ConnectionId).ToList()[0];

            //从群列表中移除该成员，如果该群中人数人为0，则移除该群
            bool result = GroupListHandler.RemoveConnectedId(Context.ConnectionId, groupName);

            ////提醒有人退出群聊
            await Clients.Group(groupName).SendAsync("SendGroup", new Message
            {
                type = MsgType.GroupUserLeave,
                data = new { key = Context.ConnectionId, value = from.Value, group = groupName }
            });
            //await Clients.Caller.SendAsync("SendGroup", new Message
            //{
            //    type = MsgType.GroupUserLeave,
            //    data = new { key = Context.ConnectionId, value = from.Value, group = groupName }
            //});
            if (result)
            {
                await Clients.All.SendAsync("GroupList", new Message
                {
                    type = MsgType.GroupRemove,
                    data = groupName
                });
            }
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }
}
