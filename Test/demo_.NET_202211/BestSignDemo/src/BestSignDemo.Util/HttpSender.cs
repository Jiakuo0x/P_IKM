using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BestSignDemo.Api;

namespace BestSignDemo.Util
{
    class HttpSender
    {
        const int DEFAULT_CONNECT_TIMEOUT = 5000; //默认连接超时
        const int DEFAULT_IO_TIMEOUT = 60000; //默认读写超时

        const string DEFAULT_CONTENTTYPE = "application/json;charset=utf-8"; //默认ContentType

        public static JObject SendRequest(string url, string method, Dictionary<string, object> postData, Dictionary<string, string> headers)
        {
            HttpWebRequest httpWebRequest = HttpWebRequest.CreateHttp(url);

            httpWebRequest.Timeout = DEFAULT_CONNECT_TIMEOUT;
            httpWebRequest.ReadWriteTimeout = DEFAULT_IO_TIMEOUT;
            httpWebRequest.ContentType = DEFAULT_CONTENTTYPE;

            httpWebRequest.Method = method;

            if (ParamChecker.CheckIsNotEmpty(headers))
            {
                foreach(string key in headers.Keys)
                {
                    string value = headers.GetValueOrDefault(key);
                    if (ParamChecker.CheckIsNotBlank(value))
                    {
                        httpWebRequest.Headers.Add(key, value);
                    }
                }
            }

            JObject request = new JObject
            {
                { "url", url },
                { "method", method }
            };

            if (method.Equals("POST"))
            {
                string requestBody = JsonConvert.SerializeObject(postData);
                request.Add("requestBody", JObject.Parse(requestBody));
                byte[] requestBodyBytes = Encoding.UTF8.GetBytes(requestBody);

                using (Stream requestStream = httpWebRequest.GetRequestStream())
                {
                    requestStream.Write(requestBodyBytes);
                }

                httpWebRequest.ContentLength = requestBodyBytes.Length;
            }

            Console.WriteLine("request:");
            Console.WriteLine(request);

            HttpWebResponse httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse;
            using (Stream responseStream = httpWebResponse.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(responseStream, Encoding.UTF8))
                {
                    JObject response = null;
                    bool isFileDownload = "success".Equals(httpWebResponse.Headers.Get("bestsign-file-download"), StringComparison.OrdinalIgnoreCase);
                    if (isFileDownload) 
                    {
                        byte[] textBytes = ReadFully(responseStream);
                        string content = Convert.ToBase64String(textBytes);   
                        response = new JObject()
                        {
                            { "content-type", httpWebResponse.Headers.Get("content-type") },
                            { "content", content } 
                        };
                    }
                    else
                    {
                        response = JObject.Parse(reader.ReadToEnd());
                    }

                    Console.WriteLine("response:");
                    Console.WriteLine(response);
                    return response;
                }
            }
        }
        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
 
        #region 转换为单字节  java base64
        /// <summary>
        /// 转换为单字节  java base64
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string JavaBase64(string str)
        {
            byte[] by = Encoding.UTF8.GetBytes(str);
            sbyte[] sby = new sbyte[by.Length];
            for (int i = 0; i < by.Length; i++)
            {
                if (by[i] > 127)
                    sby[i] = (sbyte)(by[i] - 256);
                else
                    sby[i] = (sbyte)by[i];
            }
            byte[] newby = (byte[])(object)sby;
            return Convert.ToBase64String(newby);
        }
        #endregion
    }
}