using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using BestSignDemo.Util;
using System;

namespace BestSignDemo.src.BestSignDemo.Api
{
    class Template
    {

        //使用合同模发送合同(老接口)
        public static JObject SendContractByOldTemplate(long templateId, string senderAccount, string senderEntName, List<Dictionary<string, object>> placeHolders, List<Dictionary<string, object>> roles, List<Dictionary<string, object>> textLabels)
        {
            string uri = $"/api/templates/send-contracts-sync";
            string method = "POST";
            //sender
            Dictionary<string, object> sender = new Dictionary<string, object>()
            {
                { "account", senderAccount },
                { "enterpriseName", senderEntName }
            };
            Dictionary<string, object> postData = new Dictionary<string, object>()
                {
                    { "templateId", templateId },
                    { "sender", sender },
                    { "placeHolders", placeHolders },
                    { "roles", roles },
                    { "textLabels", textLabels }
                };
            return BestSignHttpClient.Instance.Request(uri, method, postData, null);
        }

        //使用合同模发送合同(老接口)
        public static JObject SendContractByTemplate(long templateId, string senderAccount, string senderEntName, List<Dictionary<string, object>> roles, List<Dictionary<string, object>> textLabels)
        {
            string uri = $"/api/templates/send-contracts-sync-v2";
            string method = "POST";
            //sender
            Dictionary<string, object> sender = new Dictionary<string, object>()
            {
                { "account", senderAccount },
                { "enterpriseName", senderEntName }
            };
            Dictionary<string, object> postData = new Dictionary<string, object>()
                {
                    { "sender", sender },
                    { "templateId", templateId},
                    { "roles", roles },
                    { "textLabels", textLabels }
                };
            return BestSignHttpClient.Instance.Request(uri, method, postData, null);
        }


    }
}
