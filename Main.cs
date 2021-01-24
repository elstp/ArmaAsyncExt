using System.Text;
using System.Runtime.InteropServices;
using RGiesecke.DllExport;
using System;
using ARMAExtHttp;
using System.Threading.Tasks;
using System.Collections;
using System.Web.SessionState;
using System.Web;
using System.Collections.Generic;
/**
* by 七龙
*/
namespace ArmaAsyncExt
{
    public class ArmaAsyncExt
    {

        public static ExtensionCallback callback;
        public delegate int ExtensionCallback([MarshalAs(UnmanagedType.LPStr)] string name, [MarshalAs(UnmanagedType.LPStr)] string function, [MarshalAs(UnmanagedType.LPStr)] string data);
        public static string results = "WAIT|ready";

#if WIN64
        [DllExport("RVExtensionRegisterCallback", CallingConvention = CallingConvention.Winapi)]
#else
        [DllExport("_RVExtensionRegisterCallback@4", CallingConvention = CallingConvention.Winapi)]
#endif
        public static void RVExtensionRegisterCallback([MarshalAs(UnmanagedType.FunctionPtr)] ExtensionCallback func)
        {
            callback = func;
        }

#if WIN64
        [DllExport("RVExtensionVersion", CallingConvention = CallingConvention.Winapi)]
#else
        [DllExport("_RVExtensionVersion@8", CallingConvention = CallingConvention.Winapi)]
#endif
        public static void RvExtensionVersion(StringBuilder output, int outputSize)
        {
            output.Append("httpExt by 1179163813 v1.0"+ outputSize);
        }

#if WIN64
        [DllExport("RVExtension", CallingConvention = CallingConvention.Winapi)]
#else
        [DllExport("_RVExtension@12", CallingConvention = CallingConvention.Winapi)]
#endif
        public static void RvExtension(StringBuilder output, int outputSize,
            [MarshalAs(UnmanagedType.LPStr)] string function)
        {
            output.Append(function);
        }

#if WIN64
        [DllExport("RVExtensionArgs", CallingConvention = CallingConvention.Winapi)]
#else
        [DllExport("_RVExtensionArgs@20", CallingConvention = CallingConvention.Winapi)]
#endif
        public static int RvExtensionArgs(StringBuilder output, int outputSize,
            [MarshalAs(UnmanagedType.LPStr)] string function,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr, SizeParamIndex = 4)] string[] args, int argCount)
        {

            //请求结果
            if (function.Equals("Result")) {
                if (results.Length > 10240) {
                    output.Append("ERROR|The data is too large");
                    return 0;
                }
                output.Append(results);
                results = "WAIT|ready";
                return 0;
            }

            //http异步请求
            if (function.ToUpper().Equals("GET")) {
                string url = args[0].Trim().Replace("\"", "");
                HttpRequestAsync(url);
                output.Append(results); 
                return 0;
            }


            return 0;
        }


        static void HttpRequestAsync(string url) {
            results = "WAIT|Is requesting";
            new Task(async () =>
            {
                try
                {
                    var result = DownloadHelper.AsyncGetHttp(url, "GET");
                    results = "SUCCESS|"+await result;  //等待返回

                }
                catch (Exception e)
                {
                    results = "ERROR|" + e.Message;
                }
            }).Start();
        }




    }
}
