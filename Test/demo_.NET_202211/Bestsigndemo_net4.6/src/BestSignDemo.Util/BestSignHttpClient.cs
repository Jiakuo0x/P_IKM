using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BestSignDemo.Util
{
    class BestSignHttpClient
    {
        public static BestSignHttpClient Instance { get; private set; }

        public static void Init()
        {
            if (Instance == null)
            {
                Instance = new BestSignHttpClient
                    (
                        ConfigLoader.GetConfig("serverHost"),
                        ConfigLoader.GetConfig("clientId"),
                        ConfigLoader.GetConfig("clientSecret"),
                        ConfigLoader.GetConfig("privateKey")
                    );
                Console.WriteLine("===============================================================");
                Console.WriteLine("获取Token：");
                Instance.GetToken();
            }
        }


        private readonly string serverHost;
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string privateKey;

        private BestSignHttpClient(string serverHost, string clientId, string clientSecret, string privateKey)
        {
            this.serverHost = serverHost;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.privateKey = privateKey;
        }

        public JObject Request(string uri, string method, Dictionary<string, object> postData, Dictionary<string, object> urlParams)
        {
            JObject response = this.HandleRequest(uri, method, postData, urlParams);
            JObject responseData = this.HandleResponse(response);
            return responseData;
        }

        private JObject HandleRequest(string uri, string method, Dictionary<string, object> postData, Dictionary<string, object> urlParams)
        {
            string requestUri = this.GetUri(uri, urlParams);
            string requestUrl = this.serverHost + requestUri;

            string accessToken = this.GetToken();
            string timestamp = ((DateTime.UtcNow.Ticks - 621355968000000000) / 10000000).ToString();
            string signature = this.GetSignature(requestUri, postData, timestamp);

            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "bestsign-client-id", this.clientId },
                { "bestsign-sign-timestamp", timestamp },
                { "bestsign-signature-type", "RSA256" },
                { "bestsign-signature", signature },
                { "Authorization", "bearer " + accessToken }
            };

            JObject response = HttpSender.SendRequest(requestUrl, method, postData, headers);
            return response;
        }

        private JObject HandleResponse(JObject response)
        {
            string code = response.GetValue("code")?.ToString();
            if (ParamChecker.CheckIsNotBlank(code))
            {
                if (code.Equals("0"))
                {
                    JObject data = response.GetValue("data") as JObject;
                    return data;
                }
                else
                {
                    throw new Exception("Request BestSign Server Failed!");
                }
            }
            else
            {
                return response;
            }
        }

        private string GetUri(string uri, Dictionary<string, object> urlParams)
        {
            if (ParamChecker.CheckIsNotEmpty(urlParams))
            {
                bool isFirstParam = true;
                foreach (KeyValuePair<string, object> item in urlParams)
                {
                    string value = item.Value?.ToString();
                    if (ParamChecker.CheckIsNotBlank(value))
                    {
                        if (isFirstParam)
                        {
                            uri += "?";
                        }
                        else
                        {
                            uri += "&";
                        }
                        uri += item.Key + "=" + CryptUtils.UrlEncode(value);
                        isFirstParam = false;
                    }
                }
            }
            return uri;
        }

        private string accessToken;
        private string GetToken()
        {
            if (!ParamChecker.CheckIsNotBlank(accessToken))
            {
                string path = "/api/oa2/client-credentials/token";
                string method = "POST";

                string url = this.serverHost + path;

                Dictionary<string, object> postData = new Dictionary<string, object>();
                postData.Add("clientId", this.clientId);
                postData.Add("clientSecret", this.clientSecret);

                JObject response = HttpSender.SendRequest(url, method, postData, null);
                JObject responseData = this.HandleResponse(response);
                accessToken = responseData.GetValue("accessToken").ToString();
            }
            return accessToken;
        }

        private string GetSignature(string uri, Dictionary<string, object> postData, string timestamp)
        {
            string requestBodyMD5 = "";
            if (ParamChecker.CheckIsNotEmpty(postData))
            {
                requestBodyMD5 = JsonConvert.SerializeObject(postData);
                requestBodyMD5 = CryptUtils.MD5Hash(requestBodyMD5);
            }
            else
            {
                requestBodyMD5 = CryptUtils.MD5Hash("");
            }

            string signatureString = "bestsign-client-id=" + this.clientId +
                "bestsign-sign-timestamp=" + timestamp +
                "bestsign-signature-type=RSA256" +
                "request-body=" + requestBodyMD5 + 
                "uri=" + uri;

            string signature = CryptUtils.RSA256Sign(signatureString, this.privateKey);
            return signature;
        }
    }
}
