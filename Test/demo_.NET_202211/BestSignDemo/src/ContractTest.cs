using BestSignDemo.Api;
using BestSignDemo.src.BestSignDemo.Api;
using BestSignDemo.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace BestSignDemo
{
    [TestClass]
    public class ContractTest
    {
        string account = "xxx@bestsign.cn";  //account
        string enterpriseName = "xxx";//enterpriseName

        //下载单份合同
        [TestMethod]
        public void TestDownloadOneContract()
        {
            Console.WriteLine("=== 下载单份合同 ============================================================");
            JObject result = Contract.DownloadOneContract("合同编号", "pdf");
            //Console.WriteLine(contract);
            byte[] byteArray = Convert.FromBase64String(result["content"].ToString());
            string fileName1 = "../../../data/contract1.pdf";
            if (Contract.ByteToFile(byteArray, fileName1))
            {
                Console.WriteLine("下载成功 ！ ");
            }
            Assert.IsNotNull(result);
        }
 
        //下载多份合同
        [TestMethod]
        public void TestDownloadMultipleContract()
        {
            Console.WriteLine("=== 下载多份合同 ============================================================");
            JObject result = Contract.DownloadMultipleContract(new List<string> { "合同编号1", "合同编号2" }); ;
            Console.WriteLine(result.Value<string>("content"));

            byte[] byteArray = Convert.FromBase64String(result.Value<string>("content"));// Encoding.UTF8.GetBytes(result.Value<string>("content"));//Convert.FromBase64String(result.Value<string>("content"));;//System.Text.Encoding.Default.GetBytes(result.Value<string>("content")) 
            string fileName1 = "../../../data/contract1.zip";
            if (Contract.ByteToFile(byteArray, fileName1))
            {
                Console.WriteLine("下载多份合同 ！ ");
            }
            Assert.IsNotNull(result);
        }

        //下载合同附页
        [TestMethod]
        public void TestDownloadContractAppendix()
        {
            Console.WriteLine("=== 下载合同附页 ============================================================");
             
            JObject result = Contract.DownloadContractAppendix("合同编号", account, enterpriseName);
            //Console.WriteLine(contract);
            byte[] byteArray = Convert.FromBase64String(result["content"].ToString());
            string fileName1 = "../../../data/contractAppendix1.pdf";
            if (Contract.ByteToFile(byteArray, fileName1))
            {
                Console.WriteLine("下载成功 ！ ");
            }
            Assert.IsNotNull(result);
        }
        //发送合同 ： 使用合同模发送合同(新接口)
        [TestMethod]
        public void TestSendContractByTemplate()
        {
            Console.WriteLine("=== 使用合同模发送合同(新接口) ============================================================");
            long templateId = 3141198758050893829;

            List<Dictionary<string, object>> roles = new List<Dictionary<string, object>>();

            Dictionary<string, object> _userInfo1 = new Dictionary<string, object>()
            {
                { "userAccount", "xxx@bestsign.cn" },//userAccount
                { "enterpriseName", "xxx"}//enterpriseName
            };
            Dictionary<string, object> _role1 = new Dictionary<string, object>()
            {
                { "roleId", xxx },//roleId
                { "userInfo", _userInfo1 }
            };
            roles.Add(_role1);

            Dictionary<string, object> _userInfo2 = new Dictionary<string, object>()
            {
                { "userAccount", "zhiying_deng@bestsign.com" }
            };
            Dictionary<string, object> _role2 = new Dictionary<string, object>()
            {
                { "roleId", xxx },//roleId
                { "userInfo", _userInfo2 }
            };
            roles.Add(_role2);

            List<Dictionary<string, object>> textLabels = new List<Dictionary<string, object>>();
            Dictionary<string, object> textLabel = new Dictionary<string, object>()
            {
                { "name", "同意声明"},
                {"value", "我同意" }
            };
            textLabels.Add(textLabel);

            JObject result = Template.SendContractByTemplate(templateId, account, enterpriseName, roles, textLabels);
            Console.WriteLine(result);

            Assert.IsNotNull(result);
        }

        //发送合同 ： 使用合同模发送合同(老接口)
        [TestMethod]
        public void TestSendContractByOldTemplate()
        {
            long templateId = xxx;//templateId
            Console.WriteLine("=== 使用合同模发送合同(老接口) ============================================================");
 
            List<Dictionary<string, object>> placeHolders = new List<Dictionary<string, object>>();
            placeHolders.Add(new Dictionary<string, object>()
            {
                {"userAccount", "zhiying_deng@bestsign.com"}
            });

            List<Dictionary<string, object>> roles = new List<Dictionary<string, object>>();
            Dictionary<string, object> _role1 = new Dictionary<string, object>()
            {
                { "roleId", 2256418821600444420 },
                { "userType", "ENTERPRISE"},
                { "ifProxyClaimer", true},
                { "userInfo", new Dictionary<string, object>()
                        {
                            { "userAccount", "xxx@bestsign.cn" },//userAccount
                            { "enterpriseName", "xxx"}//enterpriseName
                        }
                }
            };
            roles.Add(_role1);

            List<Dictionary<string, object>> textLabels = new List<Dictionary<string, object>>();
            Dictionary<string, object> textLabel = new Dictionary<string, object>()
            {
                { "name", "同意声明"},
                {"value", "我同意" }
            };
            textLabels.Add(textLabel);
            JObject result = Template.SendContractByOldTemplate(templateId, account, enterpriseName, placeHolders, roles, textLabels);
            Console.WriteLine(result);
            Assert.IsNotNull(result);
        }
        //发送合同 ： 不使用合同模板 
        [TestMethod]
        public void TestSendContract()
        {
            Console.WriteLine("=== 不使用合同模板 ============================================================");
            string contractTitle = ".net 创建并发送待合同_测试老模板api";
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
            List<Dictionary<string, object>> receivers = new List<Dictionary<string, object>>();
            //receiver1
            Dictionary<string, object> receiver1 = new Dictionary<string, object>()
            {
                { "userName", "xxx" },//userName
                { "receiverType", "SIGNER" },
                { "userType", "ENTERPRISE" },
                { "enterpriseName", "xxx"},//enterpriseName
                { "userAccount", "xxx" },//userAccount
                { "routeOrder", "0" }
            };
            List<Dictionary<string, object>> labels1 = new List<Dictionary<string, object>>();
            //label1
            Dictionary<string, object> _label1 = new Dictionary<string, object>()
            {
                { "x", "0.1"},
                { "y", "0.2" },
                { "type", "SEAL" },
                { "pageNumber", "1"},
                { "documentOrder", "0" }
            };
            labels1.Add(_label1);
            receiver1.Add("labels", labels1);
            receivers.Add(receiver1);

            //receiver2
            Dictionary<string, object> receiver2 = new Dictionary<string, object>()
            {
                { "userName", "xxx" },//userName
                { "receiverType", "SIGNER" },
                { "userType", "ENTERPRISE" },
                { "enterpriseName", "xxx"},//enterpriseName
                { "userAccount", "xxx@bestsign.cn" },//userAccount
                { "routeOrder", "0" }
            };
            List<Dictionary<string, object>> labels2 = new List<Dictionary<string, object>>();
            //label2
            Dictionary<string, object> _label2 = new Dictionary<string, object>()
            {
                { "x", "0.1"},
                { "y", "0.1" },
                { "type", "SEAL" },
                { "pageNumber", "1"},
                { "documentOrder", "0" }
            };
            labels2.Add(_label2);
            receiver2.Add("labels", labels2);
            receivers.Add(receiver2);

            DateTime signDeadline = DateTime.Now.AddDays(10);
            JObject contractInfo = Contract.SendContract(contractTitle, account, enterpriseName, fileName, file_base64, receivers, signDeadline);
            Console.WriteLine(contractInfo);
            Assert.IsNotNull(contractInfo);
        }

        //查询合同详情
        [TestMethod]
        public void TestContractsOverview()
        {
            JObject contractInfo = Contract.ContractsOverview("合同编号");
            Console.WriteLine(contractInfo );
            Assert.IsNotNull(contractInfo);
        }

        //多关键字定位
        [TestMethod]
        public void TestMultiCalculatePositions()
        {
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

            List<string> keywords = new List<string>();
            keywords.Add("使用须知");
            JObject positions = Contract.MultiCalculatePositions(fileName, file_base64, keywords);
            Console.WriteLine();
            Assert.IsNotNull(positions);
        }
 



        [TestInitialize()]
        public void Init()
        {
            BestSignHttpClient.Init();
        }
    }
}
