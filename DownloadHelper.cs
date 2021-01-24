using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ARMAExtHttp
{
 

    public class DownloadHelper
    {
        public string message = null;

        private int ByteSize = 1024;
   

        /// <summary>
        /// 下载中的后缀，下载完成去掉
        /// </summary>
        private const string Suffix = ".downloading";

        public event Action<int> ShowDownloadPercent;

        /// <summary>
        /// Http方式下载文件
        /// </summary>
        /// <param name="url">http地址</param>
        /// <param name="localfile">本地文件</param>
        /// <returns></returns>
        public string[] DownloadFile(string url, string localfile )
        {
            int ret = 0;
            string localfileReal = localfile;
            string localfileWithSuffix = localfileReal + Suffix;
            double fileSize = 0;
            try
            {
                long startPosition = 0;
                FileStream writeStream = null;

                if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(localfileReal))
                    return new string[] { "1",null};

                //取得远程文件长度
                long remoteFileLength = GetHttpLength(url);
                 fileSize = remoteFileLength;
                fileSize = fileSize / 1024;

  
                if (remoteFileLength == 0)
                    return new string[] { "2", null };

                if (File.Exists(localfileReal))
                    return new string[] { "0", null };

                //判断要下载的文件是否存在
                if (File.Exists(localfileWithSuffix))
                {
                    writeStream = File.OpenWrite(localfileWithSuffix);
                    startPosition = writeStream.Length;
                    if (startPosition > remoteFileLength)
                    {
                        writeStream.Close();
                        File.Delete(localfileWithSuffix);
                        writeStream = new FileStream(localfileWithSuffix, FileMode.Create);
                    }
                    else if (startPosition == remoteFileLength)
                    {
                        DownloadFileOk(localfileReal, localfileWithSuffix);
                        writeStream.Close();
                        return new string[] { "0", null };
                    }
                    else
                        writeStream.Seek(startPosition, SeekOrigin.Begin);
                }
                else
                    writeStream = new FileStream(localfileWithSuffix, FileMode.Create);

                HttpWebRequest req = null;
                HttpWebResponse rsp = null;
                bool blnHttps = true;
                try
                {
                    if (blnHttps)
                    {
                        /*//处理HttpWebRequest访问https有安全证书的问题（ 请求被中止: 未能创建 SSL/TLS 安全通道。）
                        ServicePointManager.ServerCertificateValidationCallback += (s, cert, chain, sslPolicyErrors) => true;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;*/

                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3
                                                               | SecurityProtocolType.Tls
                                                               | (SecurityProtocolType)0x300 //Tls11
                                                               | (SecurityProtocolType)0xC00; //Tls12



                    }


                    req = (HttpWebRequest)WebRequest.Create(url);
   
                    if (startPosition > 0)
                        req.AddRange((int)startPosition);

                    rsp = (HttpWebResponse)req.GetResponse();
                    using (Stream readStream = rsp.GetResponseStream())
                    {
                        byte[] btArray = new byte[ByteSize];
                        long currPostion = startPosition;
                        int contentSize = 0;
                        while ((contentSize = readStream.Read(btArray, 0, btArray.Length)) > 0)
                        {
                            writeStream.Write(btArray, 0, contentSize);
                            currPostion += contentSize;

                            if (ShowDownloadPercent != null)
                                ShowDownloadPercent((int)(currPostion * 100 / remoteFileLength));
                        }
                    }
                }
                catch (Exception)
                {
                    ret = 3;
                }
                finally
                {
                    if (writeStream != null)
                        writeStream.Close();
                    if (rsp != null)
                        rsp.Close();
                    if (req != null)
                        req.Abort();

                    if (ret == 0)
                        DownloadFileOk(localfileReal, localfileWithSuffix);
                }
            }
            catch (Exception)
            {
                ret = 4;
            }
              return new string[] { ret+"", fileSize.ToString("f0") };
        }


        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受     
        }


        /// <summary>
        /// 不做catch处理，需要在外部做
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method">默认GET，空则补充为GET</param>
        /// <param name="contenttype">默认json，空则补充为json</param>
        /// <param name="header">请求头部</param>
        /// <param name="data">请求body内容</param>
        /// <returns></returns>
        public static string Http(string url, string method , string contenttype, Hashtable header = null, string data = null)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = string.IsNullOrEmpty(method) ? "GET" : method;
            request.ContentType = string.IsNullOrEmpty(contenttype) ? "application/text;charset=utf-8" : contenttype;
            if (header != null)
            {
                foreach (var i in header.Keys)
                {
                    request.Headers.Add(i.ToString(), header[i].ToString());
                }
            }
            if (!string.IsNullOrEmpty(data))
            {
                Stream RequestStream = request.GetRequestStream();
                byte[] bytes = Encoding.UTF8.GetBytes(data);
                RequestStream.Write(bytes, 0, bytes.Length);
                RequestStream.Close();
            }
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream ResponseStream = response.GetResponseStream();
            StreamReader StreamReader = new StreamReader(ResponseStream, Encoding.GetEncoding("utf-8"));
            string re = StreamReader.ReadToEnd();
            StreamReader.Close();
            ResponseStream.Close();
            return re;
        }



        /// <summary>
        /// 异步http请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method">默认GET，空则补充为GET</param>
        /// <param name="contenttype">默认json，空则补充为json</param>
        /// <param name="header">请求头部</param>
        /// <param name="data">请求body内容</param>
        /// <returns></returns>

        public static async Task<string> AsyncGetHttp(string url,string method)

        {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            //await异步读取最后的JSON（注意此时gzip已经被自动解压缩了，因为上面的AutomaticDecompression = DecompressionMethods.GZip）
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// 下载完成
        /// </summary>
        private void DownloadFileOk(string localfileReal, string localfileWithSuffix)
        {
            try
            {
                //去掉.downloading后缀
                FileInfo fi = new FileInfo(localfileWithSuffix);
                fi.MoveTo(localfileReal);
            }
            catch (Exception ex)
            {
                Info(ex.ToString());
            }
            finally
            {
                //通知完成
                if (ShowDownloadPercent != null)
                    ShowDownloadPercent(100);
            }
        }

        // 从文件头得到远程文件的长度
        public long GetHttpLength(string url)
        {
            long length = 0;
            HttpWebRequest req = null;
            HttpWebResponse rsp = null;
            try
            {
                req = (HttpWebRequest)HttpWebRequest.Create(url);
                rsp = (HttpWebResponse)req.GetResponse();
                if (rsp.StatusCode == HttpStatusCode.OK)
                    length = rsp.ContentLength;
            }
            catch (Exception ex)
            {
                Info("获取远程文件大小失败！exception：\n" + ex.ToString());
            }
            finally
            {
                if (rsp != null)
                    rsp.Close();
                if (req != null)
                    req.Abort();
            }

            return length;
        }
        private  void Info(string str) {
            Console.WriteLine(str);
            message=str;
        }

    }
}
