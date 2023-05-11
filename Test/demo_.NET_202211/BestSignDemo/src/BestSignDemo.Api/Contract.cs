using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using BestSignDemo.Util;
using System;
using System.IO;

namespace BestSignDemo.Api
{
    class Contract
    {
        // 下载多份合同
        public static JObject DownloadMultipleContract(List<string> contractIds)
        {
            string uri = $"/api/contracts/download-file";
            string method = "POST";

            Dictionary<string, object> postData = new Dictionary<string, object>()
            {
                { "contractIds", contractIds } ,
                { "encodeByBase64", false }
            };
            return BestSignHttpClient.Instance.Request(uri, method, postData, null);
        }

        // 下载单份合同
        public static JObject DownloadOneContract(string contractId, string fileType)
        {
            string uri = $"/api/contracts/download-file";
            string method = "POST";

            Dictionary<string, object> postData = new Dictionary<string, object>()
            {
                { "contractIds", new List<string> { contractId } },
                { "fileType", fileType }
            };
            return BestSignHttpClient.Instance.Request(uri, method, postData, null);
        }
        public static bool ByteToFile(byte[] byteArray, string fileName)
        {
            bool result = false;
            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    fs.Write(byteArray, 0, byteArray.Length);
                    result = true;
                }
            }
            catch (Exception  e1)
            {
                Console.WriteLine("Exception caught: {0}", e1);
                result = false;
            }
            return result;
        }
        // 下载附页
        public static JObject DownloadContractAppendix(string contractId, string account, string enterpriseName)
        {
            string uri = $"/api/contracts/{contractId}/appendix-file";
            string method = "GET";

            Dictionary<string, object> urlParams = new Dictionary<string, object>()
            {
                { "account", account },
                { "enterpriseName", enterpriseName }
            };

            return BestSignHttpClient.Instance.Request(uri, method, null, urlParams);
        }

        //不使用合同模板
        public static JObject SendContract(string contractTitle, string senderAccount, string senderEntName, string fileName, string fileDate,List<Dictionary<string, object>> receivers, DateTime signDeadline)
        {
            string uri = $"/api/contracts";
            string method = "POST";
            //sender
            Dictionary<string, object> sender = new Dictionary<string, object>()
            {
                { "account", senderAccount },
                { "enterpriseName", senderEntName }
            };

            //documents
            List<Dictionary<string, object>> documents = new List<Dictionary<string, object>>();
            Dictionary<string, object> document = new Dictionary<string, object>()
            {
                { "content", fileDate },
                { "fileName", fileName },
                { "order", "0" }
            };
            documents.Add(document);
 
            Dictionary<string, object> postData = new Dictionary<string, object>()
            {
                { "contractTitle", contractTitle },
                { "sender", sender },
                { "documents", documents },
                { "receivers", receivers },
                { "signDeadline", signDeadline },
                { "signOrdered", "false" }
            };
            return BestSignHttpClient.Instance.Request(uri, method, postData, null);
        }

        //查询合同详情
        public static JObject ContractsOverview(string contractId)
        {
            Console.WriteLine("=== 查询合同详情 ============================================================");
            string uri = $"/api/contracts/overview/{contractId}";
            string method = "GET";
            return BestSignHttpClient.Instance.Request(uri, method, null, null);
        }

        //多关键字定位
        public static JObject MultiCalculatePositions(string fileName, string fileDate, List<string> keywords)
        {
            Console.WriteLine("=== 多关键字定位 ============================================================");
            string uri = $"/api/contracts/multi-calculate-positions";
            string method = "POST";
            //documents
            List<Dictionary<string, object>> documents = new List<Dictionary<string, object>>();
            Dictionary<string, object> document = new Dictionary<string, object>()
            {
                { "content", fileDate },
                { "fileName", fileName },
                { "order", "0" }
            };
            documents.Add(document);
            Dictionary<string, object> postData = new Dictionary<string, object>()
                {
                    { "documents", documents },
                    { "keywords", keywords }
                };
    
            return BestSignHttpClient.Instance.Request(uri, method, postData, null);
        }
    }
}
