using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace signalR_Core.Utils
{
    public class GroupListHandler
    {
        private static Dictionary<string, List<string>> ConnectedIds = null;
        private static readonly object locker = new object();
        private GroupListHandler()
        {

        }
        public static Dictionary<string, List<string>> GetInstance()
        {
            if (ConnectedIds == null)
            {
                lock (locker)
                {
                    if (ConnectedIds == null)
                        ConnectedIds = new Dictionary<string, List<string>>();
                }
            }
            return ConnectedIds;
        }

        /// <summary>
        /// 新增目前使用者至上线清单 防止刷新重复加入
        /// </summary>
        /// <param name="conId"></param>
        /// <param name="clientName"></param>
        public static int AddConnectedId(string conId, string groupName)
        {
            var clentUser = GetInstance().Where(p => p.Key == groupName.Trim());
            if (clentUser != null && clentUser.Count() > 0)
            {
                var users = clentUser.FirstOrDefault().Value;
                var user = users.Where(a => a == conId);
                if (user == null || user.Count() == 0)
                    users.Add(conId);
                else
                    return 0;
                //var result = from pair in clentUser orderby pair.Key select pair;
                //foreach (KeyValuePair<string, string> pair in result)
                //{
                //    GetInstance().Remove(pair.Key);
                //}
            }
            else
            {
                GetInstance().Add(groupName, new List<string>() { conId });
                return 2;
            }
            return 1;
        }

        public static bool RemoveConnectedId(string conId, string groupName)
        {
            var clentUser = GetInstance().Where(p => p.Key == groupName.Trim());
            if (clentUser != null && clentUser.Count() > 0)
            {
                var users = clentUser.FirstOrDefault().Value;
                if (users.Where(a => a == conId) != null)
                {
                    users.Remove(conId);
                    if (users.Count <= 0)
                    {
                        GetInstance().Remove(groupName);
                        return true;
                    }
                }
            }
            return false;
        }

        public static void RemoveConnectedId(string conId, out List<string> listgroup, out List<string> list)
        {
            listgroup = new List<string>();
            list = new List<string>();
            var dic = GetInstance();
            foreach (var key in dic.Keys)
            {
                if (dic[key].Where(a => a == conId) != null)
                {
                    dic[key].Remove(conId);
                    list.Add(key);
                    if (dic[key].Count <= 0)
                    {
                        //dic.Remove(key);
                        listgroup.Add(key);
                    }
                }
            }
            foreach (var item in listgroup)
            {
                dic.Remove(item);
            }


        }
    }
}
