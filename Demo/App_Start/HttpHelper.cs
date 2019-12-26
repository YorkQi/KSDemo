using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace 爬虫demo
{/// <summary>
 /// Http连接操作帮助类
 /// </summary>
    public class HttpHelper
    {
        private const int ConnectionLimit = 100;
        //编码
        private Encoding _encoding = Encoding.Default;
        //浏览器类型
        private string[] _useragents = new string[]{
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.90 Safari/537.36",
            "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; WOW64; Trident/7.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0)",
            "Mozilla/5.0 (Windows NT 6.1; rv:36.0) Gecko/20100101 Firefox/36.0",
            "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:31.0) Gecko/20130401 Firefox/31.0"
        };

        private String _useragent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.90 Safari/537.36";
        //接受类型
        private String _accept = "text/html, application/xhtml+xml, application/xml, */*";
        //超时时间
        private int _timeout = 30 * 1000;
        //类型
        private string _contenttype = "application/x-www-form-urlencoded";
        //cookies
        private String _cookies = "";
        //cookies
        private CookieCollection _cookiecollection;
        //custom heads
        private Dictionary<string, string> _headers = new Dictionary<string, string>();

        public HttpHelper()
        {
            _headers.Clear();
            //随机一个useragent
            _useragent = _useragents[new Random().Next(0, _useragents.Length)];
            //解决性能问题?
            ServicePointManager.DefaultConnectionLimit = ConnectionLimit;
        }

        public void InitCookie()
        {
            _cookies = "";
            _cookiecollection = null;
            _headers.Clear();
        }

        /// <summary>
        /// 设置当前编码
        /// </summary>
        /// <param name="en"></param>
        public void SetEncoding(Encoding en)
        {
            _encoding = en;
        }

        /// <summary>
        /// 设置UserAgent
        /// </summary>
        /// <param name="ua"></param>
        public void SetUserAgent(String ua)
        {
            _useragent = ua;
        }

        public void RandUserAgent()
        {
            _useragent = _useragents[new Random().Next(0, _useragents.Length)];
        }

        public void SetCookiesString(string c)
        {
            _cookies = c;
        }

        /// <summary>
        /// 设置超时时间
        /// </summary>
        /// <param name="sec"></param>
        public void SetTimeOut(int msec)
        {
            _timeout = msec;
        }

        public void SetContentType(String type)
        {
            _contenttype = type;
        }

        public void SetAccept(String accept)
        {
            _accept = accept;
        }

        /// <summary>
        /// 添加自定义头
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ctx"></param>
        public void AddHeader(String key, String ctx)
        {
            //_headers.Add(key,ctx);
            _headers[key] = ctx;
        }

        /// <summary>
        /// 清空自定义头
        /// </summary>
        public void ClearHeader()
        {
            _headers.Clear();
        }

        /// <summary>
        /// 获取HTTP返回的内容
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private String GetStringFromResponse(HttpWebResponse response)
        {
            String html = "";
            try
            {
                Stream stream = response.GetResponseStream();
                StreamReader sr = new StreamReader(stream, Encoding.UTF8);
                html = sr.ReadToEnd();

                sr.Close();
                stream.Close();
            }
            catch (Exception e)
            {
                Trace.WriteLine("GetStringFromResponse Error: " + e.Message);
            }

            return html;
        }

        /// <summary>
        /// 检测证书
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        private bool CheckCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }

        /// <summary>
        /// 发送GET请求
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public String HttpGet(String url)
        {
            return HttpGet(url, url);
        }


        /// <summary>
        /// 发送GET请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="refer"></param>
        /// <returns></returns>
        public String HttpGet(String url, String refer)
        {
            String html;
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckCertificate);
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.UserAgent = _useragent;
                request.Timeout = _timeout;
                request.ContentType = _contenttype;
                request.Accept = _accept;
                request.Method = "GET";
                request.Referer = refer;
                request.KeepAlive = true;
                request.AllowAutoRedirect = true;
                request.UnsafeAuthenticatedConnectionSharing = true;
                request.CookieContainer = new CookieContainer();
                //据说能提高性能
                //request.Proxy = null;
                if (_cookies != "")
                {
                    _cookiecollection = new CookieCollection();
                    _cookiecollection.Add(ConvertCookieString(_cookies));
                }

                if (_cookiecollection != null)
                {
                    foreach (Cookie c in _cookiecollection)
                    {
                        c.Domain = request.Host;
                    }

                    request.CookieContainer.Add(_cookiecollection);
                }
                foreach (KeyValuePair<String, String> hd in _headers)
                {
                    request.Headers[hd.Key] = hd.Value;
                }

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                html = GetStringFromResponse(response);
                if (request.CookieContainer != null)
                {
                    response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
                }

                if (response.Cookies != null)
                {
                    _cookiecollection = response.Cookies;
                }
                if (response.Headers["Set-Cookie"] != null)
                {
                    string tmpcookie = response.Headers["Set-Cookie"];
                    _cookiecollection.Add(ConvertCookieString(tmpcookie));
                }

                response.Close();
                return html;
            }
            catch (Exception e)
            {
                Trace.WriteLine("HttpGet Error: " + e.Message);
                return String.Empty;
            }
        }

        /// <summary>
        /// 获取MINE文件
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public Byte[] HttpGetMine(String url)
        {
            Byte[] mine = null;
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckCertificate);
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.UserAgent = _useragent;
                request.Timeout = _timeout;
                request.ContentType = _contenttype;
                request.Accept = _accept;
                request.Method = "GET";
                request.Referer = url;
                request.KeepAlive = true;
                request.AllowAutoRedirect = true;
                request.UnsafeAuthenticatedConnectionSharing = true;
                request.CookieContainer = new CookieContainer();
                //据说能提高性能
                request.Proxy = null;
                if (_cookiecollection != null)
                {
                    foreach (Cookie c in _cookiecollection)
                        c.Domain = request.Host;
                    request.CookieContainer.Add(_cookiecollection);
                }

                foreach (KeyValuePair<String, String> hd in _headers)
                {
                    request.Headers[hd.Key] = hd.Value;
                }

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                MemoryStream ms = new MemoryStream();

                byte[] b = new byte[1024];
                while (true)
                {
                    int s = stream.Read(b, 0, b.Length);
                    ms.Write(b, 0, s);
                    if (s == 0 || s < b.Length)
                    {
                        break;
                    }
                }
                mine = ms.ToArray();
                ms.Close();

                if (request.CookieContainer != null)
                {
                    response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
                }

                if (response.Cookies != null)
                {
                    _cookiecollection = response.Cookies;
                }
                if (response.Headers["Set-Cookie"] != null)
                {
                    _cookies = response.Headers["Set-Cookie"];
                }

                stream.Close();
                stream.Dispose();
                response.Close();
                return mine;
            }
            catch (Exception e)
            {
                Trace.WriteLine("HttpGetMine Error: " + e.Message);
                return null;
            }
        }

        /// <summary>
        /// 发送POST请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public String HttpPost(String url, String data)
        {
            return HttpPost(url, data, url, null);
        }

        /// <summary>
        /// 发送POST请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="refer"></param>
        /// <returns></returns>
        public String HttpPost(String url, String data, String refer, string cookie)
        {
            String html;
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckCertificate);
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.UserAgent = _useragent;
                request.Timeout = _timeout;
                request.Referer = refer;
                request.ContentType = _contenttype;
                request.Accept = _accept;
                request.Method = "POST";
                request.KeepAlive = true;
                request.AllowAutoRedirect = true;

                request.CookieContainer = new CookieContainer();
                if (!string.IsNullOrEmpty(cookie))
                {
                    _cookiecollection = this.ConvertCookieString(cookie);
                }
                //据说能提高性能
                request.Proxy = null;

                if (_cookiecollection != null)
                {
                    foreach (Cookie c in _cookiecollection)
                    {
                        c.Domain = request.Host;
                        if (c.Domain.IndexOf(':') > 0)
                            c.Domain = c.Domain.Remove(c.Domain.IndexOf(':'));
                    }
                    request.CookieContainer.Add(_cookiecollection);
                }

                foreach (KeyValuePair<String, String> hd in _headers)
                {
                    request.Headers[hd.Key] = hd.Value;
                }
                byte[] buffer = _encoding.GetBytes(data.Trim());
                request.ContentLength = buffer.Length;
                request.GetRequestStream().Write(buffer, 0, buffer.Length);
                request.GetRequestStream().Close();

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                html = GetStringFromResponse(response);
                if (request.CookieContainer != null)
                {
                    response.Cookies = request.CookieContainer.GetCookies(request.RequestUri);
                }
                if (response.Cookies != null)
                {
                    _cookiecollection = response.Cookies;
                }
                if (response.Headers["Set-Cookie"] != null)
                {
                    string tmpcookie = response.Headers["Set-Cookie"];
                    _cookiecollection.Add(ConvertCookieString(tmpcookie));
                }

                response.Close();
                return html;
            }
            catch (Exception e)
            {
                Trace.WriteLine("HttpPost Error: " + e.Message);
                return String.Empty;
            }
        }


        public string UrlEncode(string str)
        {
            StringBuilder sb = new StringBuilder();
            byte[] byStr = _encoding.GetBytes(str);
            for (int i = 0; i < byStr.Length; i++)
            {
                sb.Append(@"%" + Convert.ToString(byStr[i], 16));
            }

            return (sb.ToString());
        }

        /// <summary>
        /// 转换cookie字符串到CookieCollection
        /// </summary>
        /// <param name="ck"></param>
        /// <returns></returns>
        private CookieCollection ConvertCookieString(string ck)
        {
            CookieCollection cc = new CookieCollection();
            string[] cookiesarray = ck.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < cookiesarray.Length; i++)
            {
                string[] cookiesarray_2 = cookiesarray[i].Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < cookiesarray_2.Length; j++)
                {
                    string[] cookiesarray_3 = cookiesarray_2[j].Trim().Split("=".ToCharArray());
                    if (cookiesarray_3.Length == 2)
                    {
                        string cname = cookiesarray_3[0].Trim();
                        string cvalue = cookiesarray_3[1].Trim();
                        if (cname.ToLower() != "domain" && cname.ToLower() != "path" && cname.ToLower() != "expires")
                        {
                            Cookie c = new Cookie(cname, cvalue);
                            cc.Add(c);
                        }
                    }
                }
            }

            return cc;
        }


        public void DebugCookies()
        {
            Trace.WriteLine("**********************BEGIN COOKIES*************************");
            foreach (Cookie c in _cookiecollection)
            {
                Trace.WriteLine(c.Name + "=" + c.Value);
                Trace.WriteLine("Path=" + c.Path);
                Trace.WriteLine("Domain=" + c.Domain);
            }
            Trace.WriteLine("**********************END COOKIES*************************");
        }




        /// <summary>  
        /// 表单数据项  
        /// </summary>  
        public class FormItemModel
        {
            /// <summary>  
            /// 表单键，request["key"]  
            /// </summary>  
            public string Key { set; get; }
            /// <summary>  
            /// 表单值,上传文件时忽略，request["key"].value  
            /// </summary>  
            public string Value { set; get; }
            /// <summary>  
            /// 是否是文件  
            /// </summary>  
            public bool IsFile
            {
                get
                {
                    if (FileContent == null || FileContent.Length == 0)
                        return false;

                    if (FileContent != null && FileContent.Length > 0 && string.IsNullOrWhiteSpace(FileName))
                        throw new Exception("上传文件时 FileName 属性值不能为空");
                    return true;
                }
            }
            /// <summary>  
            /// 上传文件的类型
            /// </summary>  
            public string FileType { set; get; }
            /// <summary>  
            /// 上传的文件名  
            /// </summary>  
            public string FileName { set; get; }
            /// <summary>  
            /// 上传的文件内容  
            /// </summary>  
            public Stream FileContent { set; get; }
        }





        /// <summary>
        /// 使用Post方法获取字符串结果
        /// </summary>
        /// <param name="url"></param>
        /// <param name="formItems">Post表单内容</param>
        /// <param name="cookieContainer"></param>
        /// <param name="timeOut">默认20秒</param>
        /// <param name="encoding">响应内容的编码类型（默认utf-8）</param>
        /// <returns></returns>
        public string PostForm(string url, List<FormItemModel> formItems, CookieContainer cookieContainer = null, string refererUrl = null, Encoding encoding = null, int timeOut = 20000)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            #region 初始化请求对象
            request.Method = "POST";
            request.Timeout = timeOut;
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            request.KeepAlive = true;
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.57 Safari/537.36";
            if (!string.IsNullOrEmpty(refererUrl))
                request.Referer = refererUrl;
            if (cookieContainer != null)
                request.CookieContainer = cookieContainer;
            #endregion

            string boundary = "----" + DateTime.Now.Ticks.ToString("x");//分隔符
            request.ContentType = string.Format("multipart/form-data; boundary={0}", boundary);
            //请求流
            var postStream = new MemoryStream();
            #region 处理Form表单请求内容
            //是否用Form上传文件
            var formUploadFile = formItems != null && formItems.Count > 0;
            if (formUploadFile)
            {
                //文件数据模板
                string fileFormdataTemplate =
                    "\r\n--" + boundary +
                    "\r\nContent-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"" +
                    "\r\nContent-Type:{2}" +
                    "\r\n\r\n";
                //文本数据模板
                string dataFormdataTemplate =
                    "\r\n--" + boundary +
                    "\r\nContent-Disposition: form-data; name=\"{0}\"" +
                    "\r\n\r\n{1}";
                foreach (var item in formItems)
                {
                    string formdata = null;
                    if (item.IsFile)
                    {
                        //上传文件
                        formdata = string.Format(
                            fileFormdataTemplate,
                            item.Key, //表单键
                            item.FileName,
                            item.FileType);
                    }
                    else
                    {
                        //上传文本
                        formdata = string.Format(
                            dataFormdataTemplate,
                            item.Key,
                            item.Value);
                    }

                    //统一处理
                    byte[] formdataBytes = null;
                    //第一行不需要换行
                    if (postStream.Length == 0)
                        formdataBytes = Encoding.UTF8.GetBytes(formdata.Substring(2, formdata.Length - 2));
                    else
                        formdataBytes = Encoding.UTF8.GetBytes(formdata);
                    postStream.Write(formdataBytes, 0, formdataBytes.Length);

                    //写入文件内容
                    if (item.FileContent != null && item.FileContent.Length > 0)
                    {
                        using (Stream stream = item.FileContent)
                        {
                            byte[] buffer = new byte[1024];
                            int bytesRead = 0;
                            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                postStream.Write(buffer, 0, bytesRead);
                            }
                        }
                    }
                }
                //结尾
                var footer = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
                postStream.Write(footer, 0, footer.Length);

            }
            else
            {
                request.ContentType = "application/x-www-form-urlencoded";
            }
            #endregion

            request.ContentLength = postStream.Length;

            #region 输入二进制流
            if (postStream != null)
            {
                postStream.Position = 0;
                //直接写入流
                Stream requestStream = request.GetRequestStream();


                byte[] buffer = new byte[1024];
                int bytesRead = 0;
                while ((bytesRead = postStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    requestStream.Write(buffer, 0, bytesRead);
                }

                #region debug查询传输数据

                postStream.Seek(0, SeekOrigin.Begin);
                StreamReader sr = new StreamReader(postStream);
                var postStr = sr.ReadToEnd();

                #endregion


                postStream.Close();//关闭文件访问
            }
            #endregion

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (cookieContainer != null)
            {
                response.Cookies = cookieContainer.GetCookies(response.ResponseUri);
            }

            using (Stream responseStream = response.GetResponseStream())
            {
                using (StreamReader myStreamReader = new StreamReader(responseStream, encoding ?? Encoding.UTF8))
                {
                    string retString = myStreamReader.ReadToEnd();
                    return retString;
                }
            }
        }



    }
}