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
        /// <summary>
        /// 连接时
        /// </summary>
        /// <returns></returns>
        //public override async Task OnConnectedAsync()
        public async Task Connected(string name)
        {
            //提示所有人
            await Clients.All.SendAsync(PushMsg.Send, new Message
            {
                type = MsgType.AddUser,
                data = new { connectId = Context.ConnectionId, connectName = name }
            });
            //自己获得用户列表
            await Clients.Caller.SendAsync(PushMsg.List, new Message
            {
                type = MsgType.GetUserList,
                data = UserListHandler.GetInstance().Where(a => a.Key != Context.ConnectionId).Select(a => new { connectId = a.Key, connectName = a.Value }).ToList()
            });

            //自己获得群列表
            await Clients.Caller.SendAsync(PushMsg.GroupList, new Message
            {
                type = MsgType.GroupList,
                data = GroupListHandler.GetInstance().Keys.ToList()
            });

            //自己昵称和id
            await Clients.Caller.SendAsync(PushMsg.List, new Message
            {
                type = MsgType.CallerName,
                data = new { connectId = Context.ConnectionId, connectName = name }
            });
            //更新其他人用户列表
            await Clients.Others.SendAsync(PushMsg.List, new Message
            {
                type = MsgType.OtherUser,
                data = new { connectId = Context.ConnectionId, connectName = name }
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
                await Clients.Group(item).SendAsync(PushMsg.SendGroup, new Message
                {
                    type = MsgType.GroupUserLeave,
                    data = new { connectId = Context.ConnectionId, connectName = user.Value, group = item }
                });
            }
            foreach (var item in listgroup)//通知所有人 群已解散
            {
                await Clients.All.SendAsync(PushMsg.GroupList, new Message
                {
                    type = MsgType.GroupRemove,
                    data = item
                });
            }

            //列表下线
            await Clients.All.SendAsync(PushMsg.List, new Message
            {
                type = MsgType.RemoveUser,
                data = new { connectId = user.Key, connectName = user.Value }
            });
            //提示下线
            await Clients.All.SendAsync(PushMsg.Send, new Message
            {
                type = MsgType.RemoveInfo,
                data = new { connectId = user.Key, value = user.Value }
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
            await Clients.All.SendAsync(PushMsg.Send, new Message
            {
                type = MsgType.GetAllMessage,
                data = new { connectId = Context.ConnectionId, connectName = Value, msg = message }
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
            await Clients.Caller.SendAsync(PushMsg.SendUser, new Message
            {
                type = MsgType.CallerPrivateMessage,
                data = new { connectId = Context.ConnectionId, connectName = go.Value, msg = message, to = from }
            });
            //发送给指定的人
            await Clients.Client(key).SendAsync(PushMsg.SendUser, new Message
            {
                type = MsgType.GetPrivateMessage,
                data = new { connectId = Context.ConnectionId, connectName = from.Value, msg = message, to = go }
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

            return Clients.Group(groupName).SendAsync(PushMsg.SendGroup, new Message
            {
                type = MsgType.GetGroupMessage,
                data = new { connectId = Context.ConnectionId, connectName = from.Value, group = groupName, msg = message }
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
                await Clients.Group(groupName).SendAsync(PushMsg.SendGroup, new Message
                {
                    type = MsgType.GroupUserJoin,
                    data = new { connectId = Context.ConnectionId, connectName = from.Value, group = groupName }
                });
            }
            if (i == 2)//新增群，更新列表
            {
                await Clients.All.SendAsync(PushMsg.GroupList, new Message
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
            await Clients.Group(groupName).SendAsync(PushMsg.SendGroup, new Message
            {
                type = MsgType.GroupUserLeave,
                data = new { connectId = Context.ConnectionId, connectName = from.Value, group = groupName }
            });

            if (result)
            {
                await Clients.All.SendAsync(PushMsg.GroupList, new Message
                {
                    type = MsgType.GroupRemove,
                    data = groupName
                });
            }
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }
}
