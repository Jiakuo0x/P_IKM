using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using BestSignDemo.Util;

namespace BestSignDemo.Api
{
    class SSO
    {
        // 查询绑定状态
        public static JObject QueryBindingStatus(string devAccountId)
        {
            string uri = "/api/users/binding-existence";
            string method = "POST";

            Dictionary<string, object> postData = new Dictionary<string, object>()
            {
                { "devAccountId", devAccountId }
            };

            return BestSignHttpClient.Instance.Request(uri, method, postData, null);
        }
    }
}
