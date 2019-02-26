using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace signalR_Core.Models
{
    public class Message
    {
        public MsgType type { get; set; }
        public string time
        {
            get
            {
                return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        public dynamic data { get; set; }
    }
    public enum MsgType
    {
        /// <summary>
        /// 新增上线
        /// </summary>
        AddUser = 1,

        /// <summary>
        /// 提示其他用户
        /// </summary>
        OtherUser = 11,

        /// <summary>
        /// 自己昵称
        /// </summary>
        CallerName = 12,

        /// <summary>
        /// 用户列表
        /// </summary>
        GetUserList = 2,

        /// <summary>
        /// 移除下线
        /// </summary>
        RemoveUser = 3,

        /// <summary>
        /// 下线通知
        /// </summary>
        RemoveInfo = 31,

        /// <summary>
        /// 所有信息
        /// </summary>
        GetAllMessage = 4,

        /// <summary>
        /// 群组信息
        /// </summary>
        GetGroupMessage = 5,

        /// <summary>
        /// 加入群聊
        /// </summary>
        GroupUserJoin = 51,

        /// <summary>
        /// 退出群聊
        /// </summary>
        GroupUserLeave = 52,

        /// <summary>
        /// 群列表
        /// </summary>
        GroupList = 53,

        /// <summary>
        /// 新增群
        /// </summary>
        GroupAdd = 54,

        /// <summary>
        /// 移除群
        /// </summary>
        GroupRemove = 55,


        /// <summary>
        /// 私密信息
        /// </summary>
        GetPrivateMessage = 6,

        /// <summary>
        /// 自己接收私密信息
        /// </summary>
        CallerPrivateMessage = 61

    }

    public class PushMsg
    {
        /// <summary>
        /// 公共聊天推送，包括用户上下线消息和公共聊天发送的信息
        /// </summary>
        public const string Send = "Send";

        /// <summary>
        /// 个人聊天推送，包括自己发送和到接收的消息
        /// </summary>
        public const string SendUser = "SendUser";

        /// <summary>
        /// 用户列表推送，包括用户上下线的删减列表，用户上线初始化列表
        /// </summary>
        public const string List = "List";

        /// <summary>
        /// 群列表推送，包括新增和解散群，用户上下线初始化群列表
        /// </summary>
        public const string GroupList = "GroupList";

        /// <summary>
        /// 群消息推送，包括成员加群退群，和用户发送的群消息
        /// </summary>
        public const string SendGroup = "SendGroup";
    }
}
