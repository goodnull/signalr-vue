using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace signalR_Core.Utils
{
    public class UserListHandler
    {
        private static Dictionary<string, string> ConnectedIds = null;
        private static readonly object locker = new object();
        private UserListHandler()
        {

        }
        public static Dictionary<string, string> GetInstance()
        {
            if (ConnectedIds == null)
            {
                lock (locker)
                {
                    if (ConnectedIds == null)
                        ConnectedIds = new Dictionary<string, string>();
                }
            }
            return ConnectedIds;
        }

        /// <summary>
        /// 新增目前使用者至上线清单 防止刷新重复加入
        /// </summary>
        /// <param name="conId"></param>
        /// <param name="clientName"></param>
        public static void AddConnectedId(string conId, string clientName)
        {
            var clentUser = GetInstance().Where(p => p.Key == conId);
            if (clentUser != null && clentUser.Count() > 0)
            {
                GetInstance().Remove(conId);
                //var result = from pair in clentUser orderby pair.Key select pair;
                //foreach (KeyValuePair<string, string> pair in result)
                //{
                //    GetInstance().Remove(pair.Key);
                //}
            }
            GetInstance().Add(conId, clientName);
        }
    }
}
