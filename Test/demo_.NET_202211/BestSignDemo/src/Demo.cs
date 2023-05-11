using System;
using Newtonsoft.Json.Linq;
using BestSignDemo.Api;
using BestSignDemo.Util;
using System.IO;
using System.Collections.Generic;
using BestSignDemo.src.BestSignDemo.Api;

namespace BestSignDemo
{
    class Demo
    {
        static void Main(string[] args)
        {
            BestSignHttpClient.Init();
            Demo demo = new Demo();
            //
            demo.ContractDemo();
            //
        }
        //--------------------- Demo ---------------------//
        // Demo
        void ContractDemo()
        {
            Console.WriteLine("===============================================================");
            Console.WriteLine("查询绑定状态：");
            string devAccountId = "ssodemodevaccountid";
            JObject resultData = SSO.QueryBindingStatus(devAccountId);
            Console.WriteLine(resultData);

            string account = "xxx@bestsign.cn";//account
            string enterpriseName = "xxx";//enterpriseName

            //获取文件 Base64
            string file_base64 = null;
            string fileName = "Demo使用须知.pdf";
            try
            {
                FileStream fs = new FileStream("../../../data/Demo使用须知.pdf", FileMode.Open);
                byte[] bt = new byte[fs.Length];
                fs.Read(bt, 0, bt.Length);
                file_base64 = Convert.ToBase64String(bt);
                fs.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
 
            //多关键字定位
            List<string> keywords = new List<string>();
            keywords.Add("使用须知");
            JObject positions=Contract.MultiCalculatePositions(fileName, file_base64, keywords);
            Console.WriteLine( );

            // 下载合同附页
            Console.WriteLine("===============================================================");
            Console.WriteLine("下载附页：");
            string contractId = "合同编号";
            JObject downloadContractAppendix = Contract.DownloadContractAppendix(contractId, account, enterpriseName);
            Console.WriteLine(downloadContractAppendix);/**/
        }

 
    }

}
